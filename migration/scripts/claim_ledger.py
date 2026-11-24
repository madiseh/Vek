#!/usr/bin/env python3
"""Manage feature claims in the shared migration-claims Git branch."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import tempfile
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
BACKLOG_PATH = ROOT / "migration" / "backlog.json"
LEDGER_BRANCH = "migration-claims"
LEDGER_REF = f"refs/heads/{LEDGER_BRANCH}"
TRACKING_REF = f"refs/remotes/origin/{LEDGER_BRANCH}"
LEASE_HOURS = 48
MAX_PUSH_ATTEMPTS = 5
FEATURE_ID_PATTERN = re.compile(r"^\d{3}$")


class LedgerError(Exception):
    """An expected claim-ledger operation failure."""


def run_git(
    args: list[str],
    *,
    env: dict[str, str] | None = None,
    input_text: str | None = None,
    check: bool = True,
) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(
        ["git", *args],
        cwd=ROOT,
        env=env,
        input=input_text,
        capture_output=True,
        text=True,
        check=False,
    )
    if check and result.returncode:
        detail = result.stderr.strip() or result.stdout.strip()
        raise LedgerError(f"git {' '.join(args)} failed: {detail}")
    return result


def utc_now() -> datetime:
    return datetime.now(timezone.utc).replace(microsecond=0)


def iso_utc(value: datetime) -> str:
    return value.isoformat(timespec="seconds").replace("+00:00", "Z")


def parse_timestamp(value: str) -> datetime:
    try:
        parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError as exc:
        raise LedgerError(f"Invalid UTC timestamp in claim record: {value!r}") from exc
    if parsed.tzinfo is None:
        raise LedgerError(f"Claim timestamp has no timezone: {value!r}")
    return parsed.astimezone(timezone.utc)


def read_backlog_feature(feature_id: str) -> tuple[dict[str, Any], dict[str, Any]]:
    try:
        backlog = json.loads(BACKLOG_PATH.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise LedgerError(f"Could not read migration backlog: {exc}") from exc

    feature = next(
        (item for item in backlog.get("features", []) if item.get("id") == feature_id),
        None,
    )
    if feature is None:
        raise LedgerError(f"Feature {feature_id} is not in migration/backlog.json")

    return backlog, feature


def get_feature_branch(feature_id: str) -> tuple[str, str, bool]:
    backlog, feature = read_backlog_feature(feature_id)
    completed = {
        item.get("id")
        for item in backlog.get("features", [])
        if item.get("status") == "completed"
    }
    dependencies_complete = all(
        dep in completed for dep in feature.get("dependencies", [])
    )

    prefix = backlog.get("pipeline", {}).get("feature_branch_prefix", "feature/migration-")
    return (
        f"{prefix}{feature_id}-{feature['slug']}",
        feature.get("status", "unknown"),
        dependencies_complete,
    )


def get_open_prs(feature_branch: str) -> list[dict[str, Any]]:
    try:
        result = subprocess.run(
            ["gh", "pr", "list", "--state", "open", "--head", feature_branch, "--json", "number,url"],
            cwd=ROOT,
            capture_output=True,
            text=True,
            check=False,
        )
    except FileNotFoundError as exc:
        raise LedgerError("GitHub CLI 'gh' is required to check for an open feature PR") from exc
    if result.returncode:
        detail = result.stderr.strip() or result.stdout.strip()
        raise LedgerError(f"Could not check open PRs with gh: {detail}")

    try:
        open_prs = json.loads(result.stdout)
    except json.JSONDecodeError as exc:
        raise LedgerError("GitHub CLI returned invalid JSON while checking open PRs") from exc
    return open_prs


def verify_claim_candidate(feature_id: str, record: dict[str, Any] | None) -> str:
    feature_branch, status, dependencies_complete = get_feature_branch(feature_id)
    if status not in {"ready", "in_progress"}:
        raise LedgerError(
            f"Feature {feature_id} is {status!r}; only READY features or an expired "
            "in-progress claim can be claimed"
        )
    if not dependencies_complete:
        raise LedgerError(f"Feature {feature_id} has incomplete dependencies")
    if status == "in_progress" and (
        record is None or record.get("state") not in {"claimed", "blocked"}
    ):
        raise LedgerError(
            f"Feature {feature_id} is already in progress but has no reclaimable ledger claim"
        )
    open_prs = get_open_prs(feature_branch)
    if open_prs:
        urls = ", ".join(pr.get("url", "unknown URL") for pr in open_prs)
        raise LedgerError(f"Feature branch {feature_branch} already has an open PR: {urls}")
    return feature_branch


def inspect_feature(feature_id: str) -> dict[str, Any]:
    feature_branch, status, dependencies_complete = get_feature_branch(feature_id)
    base = fetch_ledger_tip()
    record = read_record(base, feature_id)
    open_prs = get_open_prs(feature_branch)
    now = utc_now()
    active = record_is_active(record, now)
    status_claimable = status == "ready" or (
        status == "in_progress"
        and record is not None
        and record.get("state") in {"claimed", "blocked"}
        and not active
    )
    return {
        "feature_id": feature_id,
        "feature_branch": feature_branch,
        "backlog_status": status,
        "dependencies_complete": dependencies_complete,
        "open_prs": open_prs,
        "claim": record,
        "claim_active": active,
        "eligible": status_claimable and dependencies_complete and not open_prs and not active,
    }


def fetch_ledger_tip() -> str:
    remote = run_git(["ls-remote", "--exit-code", "--heads", "origin", LEDGER_REF], check=False)
    if remote.returncode == 0:
        run_git(["fetch", "--quiet", "origin", f"+{LEDGER_REF}:{TRACKING_REF}"])
        return run_git(["rev-parse", TRACKING_REF]).stdout.strip()
    if remote.returncode != 2:
        detail = remote.stderr.strip() or remote.stdout.strip()
        raise LedgerError(f"Could not inspect origin/{LEDGER_BRANCH}: {detail}")

    run_git(["fetch", "--quiet", "origin", "master:refs/remotes/origin/master"])
    return run_git(["rev-parse", "refs/remotes/origin/master"]).stdout.strip()


def read_record(base: str, feature_id: str) -> dict[str, Any] | None:
    path = f"claims/{feature_id}.json"
    result = run_git(["show", f"{base}:{path}"], check=False)
    if result.returncode:
        return None
    try:
        record = json.loads(result.stdout)
    except json.JSONDecodeError as exc:
        raise LedgerError(f"Claim record {path} on origin/{LEDGER_BRANCH} is invalid JSON") from exc
    if not isinstance(record, dict):
        raise LedgerError(f"Claim record {path} on origin/{LEDGER_BRANCH} must be a JSON object")
    if record.get("feature_id") != feature_id:
        raise LedgerError(f"Claim record {path} contains a mismatched feature_id")
    return record


def record_is_active(record: dict[str, Any] | None, now: datetime) -> bool:
    if not record or record.get("state") not in {"claimed", "blocked"}:
        return False
    expiry = record.get("lease_expires_at")
    if not isinstance(expiry, str):
        raise LedgerError("Active claim record is missing lease_expires_at")
    return parse_timestamp(expiry) > now


def make_commit(base: str, feature_id: str, record: dict[str, Any], action: str) -> str:
    path = f"claims/{feature_id}.json"
    content = json.dumps(record, indent=2, sort_keys=True) + "\n"
    with tempfile.TemporaryDirectory(prefix="vek-claim-ledger-") as temp_dir:
        env = os.environ.copy()
        env["GIT_INDEX_FILE"] = str(Path(temp_dir) / "index")
        run_git(["read-tree", base], env=env)
        blob = run_git(["hash-object", "-w", "--stdin"], env=env, input_text=content).stdout.strip()
        run_git(["update-index", "--add", "--cacheinfo", "100644", blob, path], env=env)
        tree = run_git(["write-tree"], env=env).stdout.strip()
        message = f"chore(migration): {action} claim {feature_id}"
        return run_git(["commit-tree", tree, "-p", base, "-m", message], env=env).stdout.strip()


def push_record(base: str, commit: str) -> bool:
    result = run_git(["push", "--quiet", "origin", f"{commit}:{LEDGER_REF}"], check=False)
    if result.returncode == 0:
        return True

    remote = run_git(["ls-remote", "--heads", "origin", LEDGER_REF], check=False)
    remote_tip = remote.stdout.split()[0] if remote.returncode == 0 and remote.stdout.strip() else None
    if remote_tip is None or remote_tip == base:
        detail = result.stderr.strip() or result.stdout.strip()
        raise LedgerError(f"Could not push claim ledger commit: {detail}")
    return False


def write_record(
    feature_id: str,
    action: str,
    run_id: str | None,
    owner: str | None,
) -> dict[str, Any]:
    now = utc_now()

    for _attempt in range(MAX_PUSH_ATTEMPTS):
        base = fetch_ledger_tip()
        record = read_record(base, feature_id)

        if action == "claim":
            assert run_id is not None and owner is not None
            feature_branch = verify_claim_candidate(feature_id, record)
            if record_is_active(record, now):
                if record and record.get("run_id") == run_id and record.get("owner") == owner:
                    return record
                raise LedgerError(
                    f"Feature {feature_id} already has an active claim by "
                    f"{record.get('owner')} (lease expires {record.get('lease_expires_at')})"
                )
            record = {
                "feature_id": feature_id,
                "run_id": run_id,
                "owner": owner,
                "state": "claimed",
                "claimed_at": iso_utc(now),
                "last_heartbeat": iso_utc(now),
                "lease_expires_at": iso_utc(now + timedelta(hours=LEASE_HOURS)),
            }
        else:
            if record is None:
                raise LedgerError(f"Feature {feature_id} has no record in origin/{LEDGER_BRANCH}")
            if record.get("run_id") != run_id:
                raise LedgerError(
                    f"Feature {feature_id} is owned by run {record.get('run_id')!r}, "
                    f"not {run_id!r}"
                )
            if record.get("state") == "released":
                if action == "release":
                    return record
                raise LedgerError(f"Feature {feature_id} has already been released")
            if action in {"renew", "blocked"}:
                record["state"] = "blocked" if action == "blocked" else record.get("state", "claimed")
                record["last_heartbeat"] = iso_utc(now)
                record["lease_expires_at"] = iso_utc(now + timedelta(hours=LEASE_HOURS))
            elif action == "release":
                record["state"] = "released"
                record["released_at"] = iso_utc(now)
                record["last_heartbeat"] = iso_utc(now)

        commit = make_commit(base, feature_id, record, action)
        if push_record(base, commit):
            return record

    raise LedgerError(
        f"Claim ledger kept changing while updating feature {feature_id}; "
        "fetch and recheck its latest record before retrying"
    )


def print_record(record: dict[str, Any]) -> None:
    print(json.dumps(record, indent=2, sort_keys=True))


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Manage migration feature claims in the shared migration-claims branch."
    )
    subparsers = parser.add_subparsers(dest="action", required=True)

    claim_parser = subparsers.add_parser("claim", help="claim an approved feature")
    claim_parser.add_argument("feature_id")
    claim_parser.add_argument("--run-id", required=True)
    claim_parser.add_argument("--owner", required=True)

    for action in ("renew", "blocked", "release"):
        action_parser = subparsers.add_parser(action, help=f"{action} a feature claim")
        action_parser.add_argument("feature_id")
        action_parser.add_argument("--run-id", required=True)

    inspect_parser = subparsers.add_parser("inspect", help="check PR and claim eligibility")
    inspect_parser.add_argument("feature_id")

    return parser


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()
    if not FEATURE_ID_PATTERN.fullmatch(args.feature_id):
        parser.error("feature_id must be a three-digit migration ID")

    try:
        if args.action == "inspect":
            print_record(inspect_feature(args.feature_id))
            return 0
        record = write_record(
            args.feature_id,
            args.action,
            getattr(args, "run_id", None),
            getattr(args, "owner", None),
        )
    except LedgerError as exc:
        print(f"Claim ledger error: {exc}", file=sys.stderr)
        return 1

    print_record(record)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
