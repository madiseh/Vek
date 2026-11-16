#!/usr/bin/env python3
"""
Autonomous Feature Migration Pipeline Orchestrator for Vek.
Coordinates clean-room feature migrations inspired by modern UI specifications.
Manages feature queue, two-way specification (.md) status synchronization,
worktree isolation, quality gate verification, and PR readiness.
"""

import argparse
import datetime
import json
import os
import re
import subprocess
import sys
from pathlib import Path

# Add script directory to sys.path for local discovery import
SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from discovery import discover_next_feature

ROOT_DIR = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT_DIR / "migration"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"
SPECS_DIR = MIGRATION_DIR / "specifications"
REPORTS_DIR = MIGRATION_DIR / "reports"


def load_backlog():
    if not BACKLOG_PATH.exists():
        print(f"Error: Backlog not found at {BACKLOG_PATH}")
        sys.exit(1)
    with open(BACKLOG_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def save_backlog(data):
    with open(BACKLOG_PATH, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
        f.write("\n")


def parse_spec_metadata(spec_path):
    """Parses metadata from a specification markdown file."""
    if not spec_path.exists():
        return {}
    content = spec_path.read_text(encoding="utf-8")
    meta = {}

    m_status = re.search(r"-\s+\*\*Status\*\*:\s*([^\r\n]+)", content, re.IGNORECASE)
    if m_status:
        meta["status"] = m_status.group(1).strip().lower()

    m_pr = re.search(r"-\s+\*\*Pull Request\*\*:\s*([^\r\n]+)", content, re.IGNORECASE)
    if m_pr and m_pr.group(1).strip() and not m_pr.group(1).strip().lower().startswith("none"):
        meta["pr_url"] = m_pr.group(1).strip()

    m_completed = re.search(r"-\s+\*\*Completed At\*\*:\s*([^\r\n]+)", content, re.IGNORECASE)
    if m_completed and m_completed.group(1).strip() and not m_completed.group(1).strip().lower().startswith("none"):
        meta["completed_at"] = m_completed.group(1).strip()

    m_deps = re.search(r"-\s+\*\*Dependencies\*\*:\s*([^\r\n]+)", content, re.IGNORECASE)
    if m_deps:
        raw_deps = m_deps.group(1).strip()
        try:
            meta["dependencies"] = json.loads(raw_deps)
        except Exception:
            meta["dependencies"] = [d.strip().strip("'\"") for d in raw_deps.strip("[]").split(",") if d.strip()]

    return meta


def update_spec_metadata(spec_path, status=None, pr_url=None, completed_at=None, started_at=None):
    """Updates metadata in a specification markdown file in-place with consistent ordering."""
    if not spec_path.exists():
        return False
    content = spec_path.read_text(encoding="utf-8")

    m = re.search(r"(## 1\. Metadata\r?\n)(.*?)(---)", content, re.DOTALL)
    if not m:
        return False

    meta_body = m.group(2)
    lines = [line.strip() for line in meta_body.splitlines() if line.strip()]
    fields = {}
    for line in lines:
        match = re.match(r"-\s+\*\*([^*]+)\*\*:\s*(.*)", line)
        if match:
            fields[match.group(1)] = match.group(2)

    if status:
        fields["Status"] = status.title() if status != "in_progress" else "In Progress"
    if pr_url is not None:
        if pr_url:
            fields["Pull Request"] = pr_url
        else:
            fields.pop("Pull Request", None)
    if completed_at is not None:
        if completed_at:
            fields["Completed At"] = completed_at
        else:
            fields.pop("Completed At", None)
    if started_at is not None:
        if started_at:
            fields["Started At"] = started_at
        else:
            fields.pop("Started At", None)

    preferred_order = [
        "Feature ID", "Feature Slug", "Module", "Status",
        "Priority", "Dependencies", "Pull Request", "Started At", "Completed At"
    ]
    new_meta_lines = []
    for key in preferred_order:
        if key in fields:
            new_meta_lines.append(f"- **{key}**: {fields.pop(key)}")
    for key, val in fields.items():
        new_meta_lines.append(f"- **{key}**: {val}")

    new_meta_body = "\n".join(new_meta_lines) + "\n\n"
    content = content[:m.start(2)] + new_meta_body + content[m.start(3):]
    spec_path.write_text(content, encoding="utf-8")
    return True


def sync_all_features(backlog):
    """Synchronizes feature statuses between specification .md files and backlog.json."""
    completed_ids = set()
    changed = False

    # 1. Reconcile markdown metadata with backlog entries
    for feature in backlog["features"]:
        spec_path = MIGRATION_DIR / feature["specification"]
        meta = parse_spec_metadata(spec_path)

        # Check if specification MD marks it completed
        if meta.get("status") in ("completed", "done"):
            if feature.get("status") != "completed":
                feature["status"] = "completed"
                changed = True
            if meta.get("pr_url") and not feature.get("pr_url"):
                feature["pr_url"] = meta["pr_url"]
                changed = True
            if meta.get("completed_at") and not feature.get("completed_at"):
                feature["completed_at"] = meta["completed_at"]
                changed = True

        # If backlog has completed status, make sure MD is in sync
        if feature.get("status") == "completed":
            completed_ids.add(feature["id"])
            if meta.get("status") != "completed" or (feature.get("pr_url") and not meta.get("pr_url")):
                update_spec_metadata(
                    spec_path,
                    status="completed",
                    pr_url=feature.get("pr_url"),
                    completed_at=feature.get("completed_at")
                )

    # 2. Promote backlog features whose dependencies are satisfied to 'ready'
    for feature in backlog["features"]:
        if feature.get("status") == "backlog":
            deps = feature.get("dependencies", [])
            if all(dep in completed_ids for dep in deps):
                feature["status"] = "ready"
                spec_path = MIGRATION_DIR / feature["specification"]
                update_spec_metadata(spec_path, status="ready")
                changed = True

    if changed:
        save_backlog(backlog)

    return backlog


def get_next_ready_feature(backlog):
    sync_all_features(backlog)
    completed_ids = {f["id"] for f in backlog["features"] if f.get("status") == "completed"}

    for feature in sorted(backlog["features"], key=lambda x: x.get("priority", 99)):
        if feature.get("status") == "ready":
            deps = feature.get("dependencies", [])
            if all(dep in completed_ids for dep in deps):
                return feature
            else:
                unmet = [d for d in deps if d not in completed_ids]
                print(f"Skipping feature {feature['id']} ({feature['title']}): unmet dependencies {unmet}")
    return None


def set_feature_status(feature_id, new_status, pr_url=None):
    """Sets a feature status in both backlog.json and its specification .md file."""
    backlog = load_backlog()
    feature = next((f for f in backlog["features"] if f["id"] == feature_id), None)
    if not feature:
        print(f"Error: Feature with ID '{feature_id}' not found.")
        sys.exit(1)

    now_iso = datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    feature["status"] = new_status
    spec_path = MIGRATION_DIR / feature["specification"]

    if new_status == "completed":
        feature["completed_at"] = now_iso
        if pr_url:
            feature["pr_url"] = pr_url
        update_spec_metadata(spec_path, status="completed", pr_url=pr_url, completed_at=now_iso)
    elif new_status == "in_progress":
        feature["started_at"] = now_iso
        update_spec_metadata(spec_path, status="in_progress", started_at=now_iso)
    elif new_status == "ready":
        update_spec_metadata(spec_path, status="ready")
    elif new_status == "backlog":
        update_spec_metadata(spec_path, status="backlog")

    save_backlog(backlog)
    sync_all_features(backlog)
    print(f"[Orchestrator] Feature {feature_id} ({feature['slug']}) status updated to '{new_status}'.")
    return feature


def list_features(backlog):
    """Prints a formatted table of all 30 features and their migration statuses."""
    sync_all_features(backlog)
    features = sorted(backlog["features"], key=lambda x: int(x.get("id", 999)))

    print("\n" + "=" * 105)
    print(f"{'ID':<5} | {'Status':<13} | {'Prio':<4} | {'Feature Title':<45} | {'Deps':<10} | {'PR / Evidence'}")
    print("=" * 105)

    counts = {"completed": 0, "in_progress": 0, "ready": 0, "backlog": 0}

    for f in features:
        st = f.get("status", "backlog")
        counts[st] = counts.get(st, 0) + 1

        st_tag = {
            "completed": "[COMPLETED]  ",
            "in_progress": "[IN PROGRESS]",
            "ready": "[READY]      ",
            "backlog": "[BACKLOG]    "
        }.get(st, f"[{st.upper()}]")

        deps_str = ",".join(f.get("dependencies", [])) or "-"
        pr_str = f.get("pr_url", "-")
        title_disp = f['title'][:44]

        print(f"{f['id']:<5} | {st_tag:<13} | {f.get('priority', 99):<4} | {title_disp:<45} | {deps_str:<10} | {pr_str}")

    print("=" * 105)
    print(f"Total: {len(features)} | Completed: {counts['completed']} | In Progress: {counts['in_progress']} | Ready: {counts['ready']} | Backlog: {counts['backlog']}\n")


def pick_and_start_feature(feature_id=None):
    """Picks a specific feature or the next ready one, marks it in_progress, and displays implementation guide."""
    backlog = load_backlog()
    sync_all_features(backlog)

    completed_ids = {f["id"] for f in backlog["features"] if f.get("status") == "completed"}
    feature = None

    if feature_id:
        feature = next((f for f in backlog["features"] if f["id"] == feature_id), None)
        if not feature:
            print(f"Error: Feature '{feature_id}' not found in backlog.")
            sys.exit(1)
        deps = feature.get("dependencies", [])
        unmet = [d for d in deps if d not in completed_ids]
        if unmet:
            print(f"Error: Cannot start feature {feature_id} ({feature['title']}). Unmet dependencies: {unmet}")
            print("Please complete the required dependencies first.")
            sys.exit(1)
    else:
        feature = get_next_ready_feature(backlog)

    if not feature:
        print("[Orchestrator] No ready features found in backlog.")
        sys.exit(0)

    # Transition feature to in_progress
    set_feature_status(feature["id"], "in_progress")

    branch_prefix = backlog["pipeline"]["feature_branch_prefix"]
    branch_name = f"{branch_prefix}{feature['id']}-{feature['slug']}"

    print(f"\n=======================================================")
    print(f" TASK STARTED: [{feature['id']}] {feature['title']}")
    print(f" Status:        IN PROGRESS")
    print(f" Module:        {feature['module']}")
    print(f" Specification: migration/{feature['specification']}")
    print(f" Target Branch: {branch_name}")
    print(f"=======================================================\n")
    print("Actionable Next Steps:")
    print(f"1. Checkout isolated worktree/branch: git checkout -b {branch_name}")
    print(f"2. Review specification contract: migration/{feature['specification']}")
    print("3. Implement feature in Project A following Vek architecture rules.")
    print("4. Verify quality gates: python migration/scripts/orchestrator.py --verify")
    print(f"5. Complete task: python migration/scripts/orchestrator.py --complete {feature['id']} --pr <pr_url>\n")
    return feature


def run_cmd(cmd, cwd=ROOT_DIR):
    print(f"Executing: {cmd}")
    res = subprocess.run(cmd, shell=True, cwd=cwd, capture_output=True, text=True)
    if res.returncode != 0:
        print(f"Command failed (exit {res.returncode}):\n{res.stderr}\n{res.stdout}")
    return res.returncode == 0, res.stdout, res.stderr


def verify_quality_gates(worktree_dir=ROOT_DIR):
    print("\n--- Running CI Quality Gates ---")

    # 1. Build
    build_ok, out, err = run_cmd("dotnet build src/Vek/Vek.csproj -f net10.0 -p:SkipBunCompile=true --nologo", cwd=worktree_dir)
    if not build_ok:
        return False, f"Build failed:\n{err or out}"

    # 2. Test
    test_ok, out, err = run_cmd("dotnet test src/Vek.UnitTests/Vek.UnitTests.csproj -p:SkipBunCompile=true -f net10.0 --output Normal --no-ansi", cwd=worktree_dir)
    if not test_ok:
        return False, f"Tests failed:\n{err or out}"

    # 3. Format
    fmt_ok, out, err = run_cmd("cd src && dotnet format whitespace --folder --verify-no-changes", cwd=worktree_dir)
    if not fmt_ok:
        return False, f"Formatting verification failed:\n{err or out}"

    # 4. Brand Cleanliness Check (pathspec exclusion on workflow)
    forbidden = "".join(["dev", "express"])
    clean_ok, out, err = run_cmd(f'git grep -i -E "{forbidden}" -- ":!.github/workflows/*"', cwd=worktree_dir)
    if clean_ok and out.strip():
        return False, f"Brand anonymity violation detected:\n{out}"

    print("\n[Quality Gates] All checks passed successfully (Build, Test, Format, Brand Cleanliness)!\n")
    return True, "All Quality Gates Passed (Build, Test, Format, Brand Cleanliness)."


def main():
    parser = argparse.ArgumentParser(description="Autonomous Feature Migration Pipeline Orchestrator")
    parser.add_argument("--list", "--status", action="store_true", help="List all 30 features and their migration status")
    parser.add_argument("--pick", "--run", dest="pick_id", help="Pick and start a specific feature ID (e.g. 003)")
    parser.add_argument("--complete", dest="complete_id", help="Mark a feature as completed (e.g. --complete 003)")
    parser.add_argument("--pr", dest="pr_url", help="Pull request URL to record when completing a feature")
    parser.add_argument("--sync", action="store_true", help="Synchronize specification MD files with backlog.json")
    parser.add_argument("--verify", action="store_true", help="Run CI quality gates on the working tree")
    parser.add_argument("--discover", action="store_true", help="Force discovery and planning of next feature gap")
    args = parser.parse_args()

    backlog = load_backlog()

    if args.list:
        list_features(backlog)
        return

    if args.sync:
        sync_all_features(backlog)
        print("[Orchestrator] All specification markdown files and backlog.json synchronized.")
        list_features(backlog)
        return

    if args.complete_id:
        set_feature_status(args.complete_id, "completed", pr_url=args.pr_url)
        return

    if args.verify:
        ok, msg = verify_quality_gates(ROOT_DIR)
        if not ok:
            print(f"[Quality Gate Error] {msg}")
            sys.exit(1)
        return

    if args.pick_id:
        pick_and_start_feature(args.pick_id)
        return

    # Default action: pick next ready feature
    pick_and_start_feature()


if __name__ == "__main__":
    main()
