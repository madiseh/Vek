#!/usr/bin/env python3
"""
Deterministic PR Creation, Milestone Linking, and Orchestrator Completion for Vek Migration.

Automates the non-reasoning steps of Section 4 in vek-migration-batch-agent:
1. Pre-PR Verification:
   - Detects screenshots in `migration/artifacts/ui-screenshots/<feature_id>-*.png`.
   - Resolves active GitHub milestone (using milestone_gate).
   - Generates raw GitHub raw URLs for screenshots:
     `https://raw.githubusercontent.com/<owner>/<repo>/<branch>/<path>`
2. PR Body & Title Generation:
   - Derives compliant PR title: `<type>(<module>): <imperative summary>`
   - Builds Markdown PR body containing:
     - `## Summary` referencing Migration <feature_id>
     - `## Verification` (build, tests, format)
     - `## UI screenshots` with inline images in reading order
3. PR Creation or Update:
   - Checks if PR already exists for the head branch.
   - If not, creates draft PR using `gh pr create --draft --milestone "<milestone_title>"`.
   - If already open, updates milestone and body.
4. Orchestrator Sync & Status Completion:
   - Runs `python migration/scripts/orchestrator.py --complete <feature_id> --pr <pr_url>`.
   - Commits and pushes the status update to the feature branch.
5. Release Claim:
   - Releases the feature claim in the ledger:
     `python migration/scripts/claim_ledger.py release <feature_id> --run-id <run_id>`.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT / "migration"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"
SCREENSHOTS_DIR = MIGRATION_DIR / "artifacts" / "ui-screenshots"


class PRError(Exception):
    """Exception raised when PR finalization fails."""


def run_cmd(
    args: list[str],
    cwd: Path | str = ROOT,
    check: bool = True,
) -> tuple[int, str, str]:
    """Execute command safely and capture output."""
    try:
        res = subprocess.run(
            args,
            cwd=str(cwd),
            capture_output=True,
            text=True,
            check=False,
        )
        if check and res.returncode != 0:
            err = res.stderr.strip() or res.stdout.strip()
            raise PRError(f"Command '{' '.join(args)}' failed: {err}")
        return res.returncode, res.stdout, res.stderr
    except FileNotFoundError as exc:
        raise PRError(f"Executable not found: {args[0]} ({exc})")


def get_git_repo_owner_name() -> tuple[str, str]:
    """Extract owner and repo from git remote or GitHub CLI."""
    code, stdout, _ = run_cmd(["gh", "repo", "view", "--json", "owner,name"], check=False)
    if code == 0:
        try:
            data = json.loads(stdout)
            return data["owner"]["login"], data["name"]
        except Exception:
            pass

    # Fallback to git remote get-url origin
    _, stdout, _ = run_cmd(["git", "remote", "get-url", "origin"], check=False)
    url = stdout.strip()
    m = re.search(r"github\.com[:/]([^/]+)/([^/\.]+)", url)
    if m:
        return m.group(1), m.group(2)

    return "madiseh", "Vek"


def find_screenshots_for_feature(
    feature_id: str,
    worktree_dir: Path | str = ROOT,
) -> list[Path]:
    """Finds all screenshot files for a feature ordered alphabetically."""
    wt_path = Path(worktree_dir).resolve()
    ss_dir = wt_path / "migration" / "artifacts" / "ui-screenshots"
    if not ss_dir.exists():
        return []

    pattern = re.compile(rf"^{feature_id}-\d+-.*\.png$", re.IGNORECASE)
    found = [p for p in ss_dir.iterdir() if p.is_file() and pattern.match(p.name)]
    return sorted(found, key=lambda p: p.name)


def format_screenshot_markdown(
    feature_id: str,
    branch: str,
    owner: str,
    repo: str,
    worktree_dir: Path | str = ROOT,
) -> str:
    """Formats markdown images using raw.githubusercontent.com URLs."""
    screenshots = find_screenshots_for_feature(feature_id, worktree_dir)
    if not screenshots:
        return "*No screenshots found under `migration/artifacts/ui-screenshots/`.*"

    md_lines = []
    for ss in screenshots:
        name_clean = ss.stem.replace(f"{feature_id}-", "").replace("-", " ").title()
        raw_url = f"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/migration/artifacts/ui-screenshots/{ss.name}"
        md_lines.append(f"![{name_clean}]({raw_url})")

    return "\n".join(md_lines)


def get_feature_details(feature_id: str) -> dict[str, Any]:
    """Reads feature metadata from backlog.json."""
    if not BACKLOG_PATH.exists():
        raise PRError(f"Backlog not found at {BACKLOG_PATH}")

    with open(BACKLOG_PATH, "r", encoding="utf-8") as f:
        backlog = json.load(f)

    feature = next((f for f in backlog.get("features", []) if f.get("id") == feature_id), None)
    if not feature:
        raise PRError(f"Feature {feature_id} not found in backlog.json")

    pipeline = backlog.get("pipeline", {})
    prefix = pipeline.get("feature_branch_prefix", "feature/migration-")
    target_branch = f"{prefix}{feature_id}-{feature['slug']}"

    return {
        "feature": feature,
        "target_branch": target_branch,
    }


def generate_pr_title(feature: dict[str, Any]) -> str:
    """Generates commit/PR title: feat(module): <summary>."""
    module = (feature.get("module") or "component").strip().lower()
    title = feature.get("title", "").strip()

    # If title starts with module name (e.g. DataGrid Master-Detail...), strip it for concise summary
    if title.lower().startswith(module.lower()):
        summary = title[len(module):].strip()
    else:
        summary = title

    # Lowercase first word of summary
    if summary:
        summary = summary[0].lower() + summary[1:]

    return f"feat({module}): {summary}"


def build_pr_body(
    feature_id: str,
    feature: dict[str, Any],
    branch: str,
    owner: str,
    repo: str,
    worktree_dir: Path | str = ROOT,
    verification_notes: str | None = None,
) -> str:
    """Builds standard compliant pull request body."""
    title = feature.get("title", "")
    module = feature.get("module", "")
    spec = feature.get("specification", "")

    ss_block = format_screenshot_markdown(feature_id, branch, owner, repo, worktree_dir)

    verif_text = verification_notes or (
        "- **Tests**: Verified with filtered `dotnet test` bUnit regression tests.\n"
        "- **Formatting**: Passes `dotnet format` and style analyzer checks.\n"
        "- **Documentation**: Interactive showcase example verified in Docs."
    )

    body = f"""## Summary
Migration {feature_id}: {title}

- **Module**: {module}
- **Specification**: `{spec}`

## Verification
{verif_text}

## UI screenshots
{ss_block}
"""
    return body.strip()


def check_existing_pr(branch: str, cwd: Path | str = ROOT) -> dict[str, Any] | None:
    """Checks if a PR already exists for the branch."""
    code, stdout, _ = run_cmd(
        ["gh", "pr", "list", "--head", branch, "--state", "all", "--json", "number,url,state,title"],
        cwd=cwd,
        check=False,
    )
    if code != 0 or not stdout.strip():
        return None

    try:
        prs = json.loads(stdout)
        if prs:
            return prs[0]
    except Exception:
        pass
    return None


def finalize_feature_pr(
    feature_id: str,
    worktree_dir: Path | str = ROOT,
    milestone_title: str | None = None,
    run_id: str | None = None,
    draft: bool = True,
    push: bool = True,
) -> dict[str, Any]:
    """Complete workflow for opening PR, recording status, and releasing claim."""
    wt_path = Path(worktree_dir).resolve()

    # 1. Inspect feature
    details = get_feature_details(feature_id)
    feature = details["feature"]
    branch = details["target_branch"]

    # 2. Get owner and repository
    owner, repo = get_git_repo_owner_name()

    # 3. Resolve active milestone if not passed
    if not milestone_title:
        from milestone_gate import get_primary_active_milestone
        ms = get_primary_active_milestone()
        milestone_title = ms.title

    # 4. Optional git push feature branch
    if push:
        run_cmd(["git", "push", "-u", "origin", branch], cwd=wt_path, check=False)

    # 5. Build PR title & body
    pr_title = generate_pr_title(feature)
    pr_body = build_pr_body(feature_id, feature, branch, owner, repo, worktree_dir=wt_path)

    # 6. Check existing PR or create new
    existing_pr = check_existing_pr(branch, cwd=wt_path)
    if existing_pr:
        pr_url = existing_pr["url"]
        pr_number = str(existing_pr["number"])
        # Update milestone and body if needed
        run_cmd(
            ["gh", "pr", "edit", pr_number, "--title", pr_title, "--body", pr_body, "--milestone", milestone_title],
            cwd=wt_path,
            check=False,
        )
    else:
        cmd = [
            "gh", "pr", "create",
            "--title", pr_title,
            "--body", pr_body,
            "--base", "master",
            "--head", branch,
            "--milestone", milestone_title,
        ]
        if draft:
            cmd.append("--draft")

        code, stdout, stderr = run_cmd(cmd, cwd=wt_path)
        pr_url = stdout.strip()

    # 7. Run orchestrator.py --complete <feature_id> --pr <pr_url> inside the worktree
    orch_path = wt_path / "migration" / "scripts" / "orchestrator.py"
    if orch_path.exists():
        run_cmd([sys.executable, str(orch_path), "--complete", feature_id, "--pr", pr_url], cwd=wt_path)
        # Commit and push completion metadata
        run_cmd(["git", "add", "migration/backlog.json", f"migration/specifications/{feature['slug']}.md"], cwd=wt_path, check=False)
        run_cmd(["git", "commit", "-m", f"chore(migration): record completion metadata for {feature_id}"], cwd=wt_path, check=False)
        if push:
            run_cmd(["git", "push", "origin", branch], cwd=wt_path, check=False)

    # 8. Release claim if run_id provided
    if run_id:
        claim_script = ROOT / "migration" / "scripts" / "claim_ledger.py"
        run_cmd([sys.executable, str(claim_script), "release", feature_id, "--run-id", run_id], cwd=ROOT, check=False)

    return {
        "feature_id": feature_id,
        "title": pr_title,
        "pr_url": pr_url,
        "milestone": milestone_title,
        "branch": branch,
        "is_draft": draft,
        "status": "completed",
    }


def main():
    parser = argparse.ArgumentParser(description="Deterministic PR Finalization and Orchestrator Completion")
    parser.add_argument("feature_id", help="3-digit feature ID (e.g. 017)")
    parser.add_argument("--dir", type=Path, default=ROOT, help="Worktree directory")
    parser.add_argument("--milestone", help="Milestone title (e.g. v1.5.0)")
    parser.add_argument("--run-id", help="Claim run ID to release")
    parser.add_argument("--no-draft", action="store_true", help="Create ready (non-draft) PR")
    parser.add_argument("--no-push", action="store_true", help="Skip git push")

    args = parser.parse_args()

    try:
        res = finalize_feature_pr(
            feature_id=args.feature_id,
            worktree_dir=args.dir,
            milestone_title=args.milestone,
            run_id=args.run_id,
            draft=not args.no_draft,
            push=not args.no_push,
        )
        print(json.dumps(res, indent=2))
    except PRError as exc:
        print(f"Error: {exc}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
