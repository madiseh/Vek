#!/usr/bin/env python3
"""
Deterministic Milestone Gate and Batch Candidate Selection for Vek Migration.

This module automates the non-reasoning aspects of section 0 and section 1
of vek-migration-batch-agent:
1. Milestone Alignment & Verification:
   - Queries GitHub via `gh api` for open milestones.
   - Extracts allowed feature IDs from the active milestone description/scope.
   - Verifies whether a specific feature ID or set of features belongs to the milestone.
2. Candidate Selection & Filtering:
   - Reads backlog.json and syncs statuses.
   - Filters features by READY status, completed dependencies, and active milestone inclusion.
   - Checks claims via claim_ledger.
   - Checks open PRs via GitHub CLI.
   - Proposes batches with at most one feature per module/domain.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from dataclasses import asdict, dataclass
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT / "migration"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"
SPECS_DIR = MIGRATION_DIR / "specifications"

# Matches markdown table rows like: | **014** | or | 014 | or lines starting with - 014:
TABLE_ID_PATTERN = re.compile(r"^\s*\|\s*\*{0,2}(\d{3})\*{0,2}\s*\|", re.MULTILINE)
BULLET_ID_PATTERN = re.compile(r"^\s*[-*]\s+\*{0,2}(\d{3})\*{0,2}[:\s]", re.MULTILINE)


class MilestoneError(Exception):
    """Exception raised when milestone operations or verifications fail."""


@dataclass
class MilestoneInfo:
    id: int
    number: int
    title: str
    description: str
    html_url: str
    feature_ids: list[str]


@dataclass
class CandidateFeature:
    id: str
    title: str
    slug: str
    module: str
    specification: str
    target_branch: str
    milestone: str
    dependencies: list[str]
    eligible: bool
    skip_reason: str | None = None


def run_gh(args: list[str]) -> str:
    """Execute gh CLI command and return stdout."""
    try:
        res = subprocess.run(
            ["gh", *args],
            cwd=ROOT,
            capture_output=True,
            text=True,
            check=False,
        )
    except FileNotFoundError as exc:
        raise MilestoneError("GitHub CLI ('gh') is required but not installed or not in PATH.") from exc

    if res.returncode != 0:
        err = res.stderr.strip() or res.stdout.strip()
        raise MilestoneError(f"gh command failed: {err}")
    return res.stdout


def extract_feature_ids_from_text(text: str) -> list[str]:
    """
    Extract 3-digit feature IDs from milestone description.
    Specifically targets:
    1. Table rows: `| **014** | DataGrid | ...` or `| 014 | ...`
    2. Bullet lists: `- **014**: ...` or `* 014 ...`
    Ignores arbitrary issue numbers like #139 or other 3-digit numbers.
    """
    if not text:
        return []
    ids: list[str] = []
    seen = set()

    for m in TABLE_ID_PATTERN.finditer(text):
        fid = m.group(1)
        if fid not in seen:
            seen.add(fid)
            ids.append(fid)

    for m in BULLET_ID_PATTERN.finditer(text):
        fid = m.group(1)
        if fid not in seen:
            seen.add(fid)
            ids.append(fid)

    # Fallback only if no table or bullets found: search for bold 3-digits `**\d{3}**`
    if not ids:
        for fid in re.findall(r"\*\*(\d{3})\*\*", text):
            if fid not in seen:
                seen.add(fid)
                ids.append(fid)

    return sorted(ids)


def get_active_milestones() -> list[MilestoneInfo]:
    """Retrieve open milestones from GitHub and parse their scoped feature IDs."""
    raw = run_gh(["api", "repos/:owner/:repo/milestones", "--jq", "."])
    try:
        data = json.loads(raw)
    except json.JSONDecodeError as exc:
        raise MilestoneError("Failed to parse milestone JSON from GitHub API") from exc

    milestones: list[MilestoneInfo] = []
    for item in data:
        if item.get("state", "open") == "open":
            desc = item.get("description", "") or ""
            feature_ids = extract_feature_ids_from_text(desc)
            milestones.append(
                MilestoneInfo(
                    id=item.get("id"),
                    number=item.get("number"),
                    title=item.get("title", ""),
                    description=desc,
                    html_url=item.get("html_url", ""),
                    feature_ids=feature_ids,
                )
            )
    return milestones


def get_primary_active_milestone(target_milestone: str | None = None) -> MilestoneInfo:
    """
    Finds the active milestone.
    If target_milestone is specified, matches by title or number.
    Otherwise returns the first open milestone.
    Raises MilestoneError if no open milestone exists.
    """
    milestones = get_active_milestones()
    if not milestones:
        raise MilestoneError("No open milestones found in repository! (Step 0 Gate Failed)")

    if target_milestone:
        for m in milestones:
            if m.title.lower() == target_milestone.lower() or str(m.number) == target_milestone:
                return m
        raise MilestoneError(f"Milestone '{target_milestone}' not found among open milestones.")

    return milestones[0]


def verify_feature_milestone(feature_id: str, milestone_title: str | None = None) -> dict[str, Any]:
    """
    Deterministic check: Verify if feature_id is part of the active milestone.
    Returns details dict with pass/fail and milestone info.
    """
    milestone = get_primary_active_milestone(milestone_title)
    is_in_milestone = feature_id in milestone.feature_ids
    return {
        "feature_id": feature_id,
        "active_milestone": milestone.title,
        "milestone_number": milestone.number,
        "in_milestone": is_in_milestone,
        "milestone_total_features": len(milestone.feature_ids),
    }


def inspect_candidate_eligibility(feature_id: str) -> tuple[bool, str | None, dict[str, Any]]:
    """Runs claim_ledger inspect deterministically to check claims and open PRs."""
    script_path = MIGRATION_DIR / "scripts" / "claim_ledger.py"
    res = subprocess.run(
        [sys.executable, str(script_path), "inspect", feature_id],
        cwd=ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    if res.returncode != 0:
        err = res.stderr.strip() or res.stdout.strip()
        return False, f"claim_ledger inspect failed: {err}", {}

    try:
        data = json.loads(res.stdout)
    except json.JSONDecodeError:
        return False, "claim_ledger returned non-JSON output", {}

    if not data.get("dependencies_complete", False):
        return False, "Dependencies incomplete", data
    if data.get("claim_active", False):
        return False, f"Active claim exists (owner: {data.get('claim', {}).get('owner')})", data
    if data.get("open_prs"):
        return False, f"Open PR exists ({data['open_prs'][0].get('url')})", data
    if not data.get("eligible", False):
        return False, "Not marked eligible in ledger", data

    return True, None, data


def propose_batch(
    max_batch_size: int = 3,
    milestone_name: str | None = None,
) -> dict[str, Any]:
    """
    Deterministic implementation of Section 0 & Section 1:
    Builds a read-only batch proposal adhering strictly to active milestone,
    claim status, open PRs, and at most one feature per module/domain.
    """
    milestone = get_primary_active_milestone(milestone_name)
    if not BACKLOG_PATH.exists():
        raise MilestoneError(f"Backlog not found at {BACKLOG_PATH}")

    with open(BACKLOG_PATH, "r", encoding="utf-8") as f:
        backlog = json.load(f)

    prefix = backlog.get("pipeline", {}).get("feature_branch_prefix", "feature/migration-")
    features = backlog.get("features", [])

    candidates: list[CandidateFeature] = []
    skipped: list[dict[str, Any]] = []

    # Map of completed features to verify dependencies quickly
    completed_ids = {f["id"] for f in features if f.get("status") == "completed"}

    # Pass 1: Quick local screening before doing any git/ledger inspection
    prefiltered = []
    for feat in features:
        fid = feat["id"]
        title = feat.get("title", "")
        slug = feat.get("slug", "")
        module = feat.get("module", "")
        spec = feat.get("specification", "")
        target_branch = f"{prefix}{fid}-{slug}"
        status = feat.get("status", "").lower()
        deps = feat.get("dependencies", [])

        # 1. Milestone scope check
        if fid not in milestone.feature_ids:
            skipped.append({
                "id": fid,
                "title": title,
                "reason": f"Not in active milestone '{milestone.title}'",
            })
            continue

        # 2. Backlog status check
        if status != "ready":
            skipped.append({
                "id": fid,
                "title": title,
                "reason": f"Backlog status is '{status}' (must be 'ready')",
            })
            continue

        # 3. Dependencies check
        missing_deps = [d for d in deps if d not in completed_ids]
        if missing_deps:
            skipped.append({
                "id": fid,
                "title": title,
                "reason": f"Dependencies not completed: {missing_deps}",
            })
            continue

        prefiltered.append((feat, target_branch, deps))

    # Pass 2: Inspect eligible candidates, taking at most one per module/domain.
    selected: list[CandidateFeature] = []
    seen_modules = set()

    for feat, target_branch, deps in prefiltered:
        if len(selected) >= max_batch_size:
            break

        fid = feat["id"]
        title = feat.get("title", "")
        slug = feat.get("slug", "")
        module = feat.get("module", "")
        spec = feat.get("specification", "")

        module_key = module.strip().casefold()
        if not module_key:
            skipped.append({
                "id": fid,
                "title": title,
                "reason": "Missing module/domain in backlog",
            })
            continue

        # Enforce domain diversity before the remote eligibility check.
        if module_key in seen_modules:
            skipped.append({
                "id": fid,
                "title": title,
                "reason": f"Skipped because module/domain '{module}' is already represented in the batch",
            })
            continue

        # Now do the remote ledger inspect only for viable candidates
        eligible, reason, ledger_info = inspect_candidate_eligibility(fid)

        candidate = CandidateFeature(
            id=fid,
            title=title,
            slug=slug,
            module=module,
            specification=spec,
            target_branch=target_branch,
            milestone=milestone.title,
            dependencies=deps,
            eligible=eligible,
            skip_reason=reason,
        )

        if not eligible:
            skipped.append({
                "id": fid,
                "title": title,
                "reason": reason,
            })
            continue

        seen_modules.add(module_key)

        selected.append(candidate)

    return {
        "milestone": {
            "title": milestone.title,
            "number": milestone.number,
            "url": milestone.html_url,
            "total_scoped_features": len(milestone.feature_ids),
        },
        "selected_batch": [asdict(c) for c in selected],
        "batch_size": len(selected),
        "skipped_count": len(skipped),
        "skipped_summary": skipped[:20],
    }


def main():
    parser = argparse.ArgumentParser(description="Deterministic Milestone Gate and Batch Proposal")
    subparsers = parser.add_subparsers(dest="command", required=True)

    # verify
    verify_parser = subparsers.add_parser("verify", help="Verify if a feature belongs to the active milestone")
    verify_parser.add_argument("feature_id", help="3-digit feature ID (e.g. 017)")
    verify_parser.add_argument("--milestone", help="Target milestone title or number")

    # list-milestones
    subparsers.add_parser("list", help="List active milestones and scoped features")

    # propose
    propose_parser = subparsers.add_parser("propose", help="Propose a batch of eligible features")
    propose_parser.add_argument("--size", type=int, default=3, help="Max batch size (default: 3)")
    propose_parser.add_argument("--milestone", help="Target milestone title or number")

    args = parser.parse_args()

    try:
        if args.command == "verify":
            res = verify_feature_milestone(args.feature_id, args.milestone)
            print(json.dumps(res, indent=2))
            if not res["in_milestone"]:
                sys.exit(2)

        elif args.command == "list":
            milestones = get_active_milestones()
            output = [
                {
                    "title": m.title,
                    "number": m.number,
                    "url": m.html_url,
                    "features_count": len(m.feature_ids),
                    "feature_ids": m.feature_ids,
                }
                for m in milestones
            ]
            print(json.dumps(output, indent=2))

        elif args.command == "propose":
            res = propose_batch(
                max_batch_size=args.size,
                milestone_name=args.milestone,
            )
            print(json.dumps(res, indent=2))

    except MilestoneError as exc:
        print(f"Error: {exc}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
