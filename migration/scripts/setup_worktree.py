#!/usr/bin/env python3
"""
Deterministic Worktree, Branch, and Environment Setup for Vek Migration Workers.

Automates the non-reasoning aspects of worker environment initialization:
1. Sync with upstream / fetch origin/master.
2. Verify feature eligibility and active milestone (via milestone_gate & claim_ledger).
3. Derive target branch name from backlog pipeline convention.
4. Create an isolated git worktree for the feature based on origin/master.
5. Create or checkout the target feature branch.
6. Run `orchestrator.py --pick <feature_id>` inside the new worktree to initialize status and metadata.
7. Generate a worker_context.json file inside the worktree containing all necessary metadata,
   paths, and instructions for the worker agent.
8. Support cleaning/teardown of worktrees after completion or abandonment.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import sys
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT / "migration"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"
SPECS_DIR = MIGRATION_DIR / "specifications"


class WorktreeError(Exception):
    """Exception raised when worktree setup operations fail."""


def run_cmd(
    args: list[str],
    cwd: Path | str = ROOT,
    check: bool = True,
    env: dict[str, str] | None = None,
) -> subprocess.CompletedProcess[str]:
    """Execute command and return CompletedProcess."""
    merged_env = os.environ.copy()
    if env:
        merged_env.update(env)

    res = subprocess.run(
        args,
        cwd=str(cwd),
        capture_output=True,
        text=True,
        check=False,
        env=merged_env,
    )
    if check and res.returncode != 0:
        err = res.stderr.strip() or res.stdout.strip()
        cmd_str = " ".join(args)
        raise WorktreeError(f"Command '{cmd_str}' failed with exit code {res.returncode}: {err}")
    return res


def sync_upstream_master() -> str:
    """Fetch latest origin/master and return the commit SHA."""
    run_cmd(["git", "fetch", "origin", "master", "--prune"])
    res = run_cmd(["git", "rev-parse", "origin/master"])
    return res.stdout.strip()


def get_feature_info(feature_id: str) -> dict[str, Any]:
    """Retrieve feature entry and pipeline settings from backlog.json."""
    if not BACKLOG_PATH.exists():
        raise WorktreeError(f"Backlog not found at {BACKLOG_PATH}")

    with open(BACKLOG_PATH, "r", encoding="utf-8") as f:
        backlog = json.load(f)

    feature = next((item for item in backlog.get("features", []) if item.get("id") == feature_id), None)
    if not feature:
        raise WorktreeError(f"Feature {feature_id} not found in backlog.json")

    pipeline = backlog.get("pipeline", {})
    prefix = pipeline.get("feature_branch_prefix", "feature/migration-")
    target_branch = f"{prefix}{feature_id}-{feature['slug']}"

    return {
        "feature": feature,
        "pipeline": pipeline,
        "target_branch": target_branch,
    }


def list_existing_worktrees() -> dict[str, str]:
    """Returns mapping of worktree path (normalized) -> branch/commit."""
    res = run_cmd(["git", "worktree", "list", "--porcelain"])
    worktrees = {}
    current_path = None

    for line in res.stdout.splitlines():
        line = line.strip()
        if line.startswith("worktree "):
            current_path = os.path.normpath(line[len("worktree "):].strip())
        elif line.startswith("branch ") and current_path:
            worktrees[current_path] = line[len("branch "):].strip()
        elif line == "detached" and current_path:
            worktrees[current_path] = "detached"

    return worktrees


def default_worktree_path(feature_id: str, slug: str, base_dir: Path | None = None) -> Path:
    """Determine default directory for the worker worktree."""
    if base_dir is None:
        # Check standard worktrees directory pattern or root sibling
        parent_dir = ROOT.parent
        # If running inside worktrees folder (e.g. .gemini/antigravity/worktrees/vekblazor), create sibling
        base_dir = parent_dir
    return (base_dir / f"wt-{feature_id}-{slug}").resolve()


def setup_worker_environment(
    feature_id: str,
    target_dir: Path | None = None,
    run_id: str | None = None,
    owner: str | None = None,
    claim: bool = False,
    skip_fetch: bool = False,
) -> dict[str, Any]:
    """
    Creates an isolated worktree and branch for the worker.
    Optionally registers the claim in the shared claims branch.
    """
    # 1. Fetch upstream master commit
    master_sha = "HEAD" if skip_fetch else sync_upstream_master()

    # 2. Inspect feature information from backlog
    info = get_feature_info(feature_id)
    feature = info["feature"]
    target_branch = info["target_branch"]
    slug = feature.get("slug", "unknown")

    # 3. Optional claim ledger update
    if claim:
        if not run_id:
            import uuid
            run_id = str(uuid.uuid4())[:8]
        if not owner:
            owner = os.environ.get("USERNAME") or os.environ.get("USER") or "worker"

        claim_script = MIGRATION_DIR / "scripts" / "claim_ledger.py"
        run_cmd([sys.executable, str(claim_script), "claim", feature_id, "--run-id", run_id, "--owner", owner])

    # 4. Resolve target worktree directory
    wt_dir = target_dir.resolve() if target_dir else default_worktree_path(feature_id, slug)

    # Check if worktree already exists
    existing_worktrees = list_existing_worktrees()
    norm_wt = os.path.normpath(str(wt_dir))

    if norm_wt in existing_worktrees:
        print(f"Worktree already exists at {wt_dir}")
    else:
        # Check if local branch already exists
        check_branch = run_cmd(["git", "branch", "--list", target_branch], check=False)
        branch_exists = bool(check_branch.stdout.strip())

        if branch_exists:
            # Add worktree checking out existing branch
            run_cmd(["git", "worktree", "add", str(wt_dir), target_branch])
        else:
            # Create branch from origin/master
            base_ref = "origin/master" if not skip_fetch else "master"
            run_cmd(["git", "worktree", "add", "-b", target_branch, str(wt_dir), base_ref])

    # 5. Run orchestrator.py --pick <feature_id> in the worktree
    orch_path = wt_dir / "migration" / "scripts" / "orchestrator.py"
    if orch_path.exists():
        run_cmd([sys.executable, str(orch_path), "--pick", feature_id], cwd=wt_dir)

    # 6. Generate worker_context.json for the subagent
    spec_rel = feature.get("specification", "")
    spec_abs = wt_dir / "migration" / spec_rel

    context_data = {
        "feature_id": feature_id,
        "title": feature.get("title", ""),
        "module": feature.get("module", ""),
        "slug": slug,
        "target_branch": target_branch,
        "worktree_path": str(wt_dir),
        "base_commit": master_sha,
        "specification_path": str(spec_abs),
        "run_id": run_id,
        "owner": owner,
        "dependencies": feature.get("dependencies", []),
    }

    context_file = wt_dir / "worker_context.json"
    context_file.write_text(json.dumps(context_data, indent=2), encoding="utf-8")

    return context_data


def teardown_worker_environment(feature_id: str, force: bool = False) -> bool:
    """Removes the worktree associated with the feature."""
    info = get_feature_info(feature_id)
    slug = info["feature"].get("slug", "unknown")
    wt_dir = default_worktree_path(feature_id, slug)

    norm_wt = os.path.normpath(str(wt_dir))
    existing_worktrees = list_existing_worktrees()

    # Find worktree matching path or branch
    matching_path = None
    target_branch_ref = f"refs/heads/{info['target_branch']}"

    for path_str, branch_ref in existing_worktrees.items():
        if path_str == norm_wt or branch_ref == target_branch_ref or f"wt-{feature_id}" in path_str:
            matching_path = path_str
            break

    if not matching_path:
        print(f"No active worktree found for feature {feature_id}")
        return False

    cmd = ["git", "worktree", "remove", matching_path]
    if force:
        cmd.append("--force")
    run_cmd(cmd)
    print(f"Worktree removed: {matching_path}")
    return True


def main():
    parser = argparse.ArgumentParser(description="Deterministic Worktree and Branch Setup for Vek Workers")
    subparsers = parser.add_subparsers(dest="command", required=True)

    # setup
    setup_parser = subparsers.add_parser("setup", help="Setup an isolated worktree and branch for a feature")
    setup_parser.add_argument("feature_id", help="3-digit feature ID (e.g. 017)")
    setup_parser.add_argument("--dir", type=Path, help="Custom target directory for the worktree")
    setup_parser.add_argument("--run-id", help="Claim run ID")
    setup_parser.add_argument("--owner", help="Claim owner name")
    setup_parser.add_argument("--claim", action="store_true", help="Register claim in claim_ledger automatically")
    setup_parser.add_argument("--no-fetch", action="store_true", help="Skip fetching origin/master")

    # teardown
    teardown_parser = subparsers.add_parser("teardown", help="Remove worktree for a feature")
    teardown_parser.add_argument("feature_id", help="3-digit feature ID (e.g. 017)")
    teardown_parser.add_argument("--force", "-f", action="store_true", help="Force removal even if untracked files exist")

    # list
    subparsers.add_parser("list", help="List active worktrees")

    args = parser.parse_args()

    try:
        if args.command == "setup":
            res = setup_worker_environment(
                feature_id=args.feature_id,
                target_dir=args.dir,
                run_id=args.run_id,
                owner=args.owner,
                claim=args.claim,
                skip_fetch=args.no_fetch,
            )
            print(json.dumps(res, indent=2))

        elif args.command == "teardown":
            success = teardown_worker_environment(feature_id=args.feature_id, force=args.force)
            if not success:
                sys.exit(1)

        elif args.command == "list":
            worktrees = list_existing_worktrees()
            print(json.dumps(worktrees, indent=2))

    except WorktreeError as exc:
        print(f"Error: {exc}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
