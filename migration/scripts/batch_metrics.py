#!/usr/bin/env python3
"""
Deterministic Batch Timing, Metrics Logging, and Report Generator for Vek Migration.

Automates the non-reasoning steps of Section 5 in vek-migration-batch-agent:
1. Phase Logging:
   - Records UTC timestamps for the 8 standard pipeline phases per feature:
     * claim_worktree_setup
     * implementation
     * tests_and_builds
     * documentation_and_screenshot
     * formatting_and_review
     * commit_and_push
     * pr_opening_and_metadata
     * ci_wait
2. Metrics Storage:
   - Maintains a structured JSON log under `migration/reports/batch_<batch_id>_metrics.json`.
3. Metrics Calculations:
   - Calendar batch duration (earliest start to latest end).
   - Sum of feature elapsed durations.
   - Agent active time (sum of 7 non-CI phases).
   - CI wait duration per feature.
4. Report Formatting:
   - Formats clean, copy-paste ready Markdown report with timing tables and summary.
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from dataclasses import asdict, dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[2]
REPORTS_DIR = ROOT / "migration" / "reports"

STANDARD_PHASES = [
    "claim_worktree_setup",
    "implementation",
    "tests_and_builds",
    "documentation_and_screenshot",
    "formatting_and_review",
    "commit_and_push",
    "pr_opening_and_metadata",
    "ci_wait",
]

PHASE_DISPLAY_NAMES = {
    "claim_worktree_setup": "Claim & Worktree Setup",
    "implementation": "Implementation",
    "tests_and_builds": "Tests & Builds",
    "documentation_and_screenshot": "Docs & Screenshots",
    "formatting_and_review": "Formatting & Review",
    "commit_and_push": "Commit & Push",
    "pr_opening_and_metadata": "PR & Completion Metadata",
    "ci_wait": "CI Wait",
}


def utc_now() -> datetime:
    """Returns current UTC datetime without microseconds."""
    return datetime.now(timezone.utc).replace(microsecond=0)


def iso_utc(dt: datetime) -> str:
    """Returns ISO 8601 UTC string."""
    return dt.isoformat(timespec="seconds").replace("+00:00", "Z")


def parse_iso(ts_str: str) -> datetime:
    """Parses ISO 8601 UTC string."""
    return datetime.fromisoformat(ts_str.replace("Z", "+00:00")).astimezone(timezone.utc)


def format_duration(seconds: float) -> str:
    """Formats seconds into human readable duration (e.g. '12m 34s' or '45s')."""
    total_secs = int(round(seconds))
    if total_secs < 60:
        return f"{total_secs}s"
    minutes = total_secs // 60
    secs = total_secs % 60
    if minutes < 60:
        return f"{minutes}m {secs}s"
    hours = minutes // 60
    rem_min = minutes % 60
    return f"{hours}h {rem_min}m {secs}s"


@dataclass
class PhaseRecord:
    start_time: str
    end_time: str | None = None
    duration_seconds: float | None = None
    notes: str | None = None


@dataclass
class FeatureMetrics:
    feature_id: str
    title: str = ""
    pr_url: str | None = None
    status: str = "in_progress"  # in_progress, ready, blocked, completed
    phases: dict[str, dict[str, Any]] = field(default_factory=dict)
    total_elapsed_seconds: float | None = None
    agent_active_seconds: float | None = None


@dataclass
class BatchMetrics:
    batch_id: str
    milestone: str = ""
    created_at: str = field(default_factory=lambda: iso_utc(utc_now()))
    updated_at: str = field(default_factory=lambda: iso_utc(utc_now()))
    features: dict[str, dict[str, Any]] = field(default_factory=dict)


class BatchMetricsManager:
    """Manages recording and reporting of batch timing and phase metrics."""

    def __init__(self, batch_id: str = "current", reports_dir: Path | str = REPORTS_DIR):
        self.reports_dir = Path(reports_dir)
        self.reports_dir.mkdir(parents=True, exist_ok=True)
        self.batch_id = batch_id
        self.file_path = self.reports_dir / f"batch_{batch_id}_metrics.json"
        self.data: dict[str, Any] = self._load()

    def _load(self) -> dict[str, Any]:
        if self.file_path.exists():
            try:
                return json.loads(self.file_path.read_text(encoding="utf-8"))
            except Exception:
                pass
        return {
            "batch_id": self.batch_id,
            "milestone": "",
            "created_at": iso_utc(utc_now()),
            "updated_at": iso_utc(utc_now()),
            "features": {},
        }

    def save(self):
        self.data["updated_at"] = iso_utc(utc_now())
        self.file_path.write_text(json.dumps(self.data, indent=2), encoding="utf-8")

    def init_feature(self, feature_id: str, title: str = "", pr_url: str | None = None):
        if feature_id not in self.data["features"]:
            self.data["features"][feature_id] = {
                "feature_id": feature_id,
                "title": title,
                "pr_url": pr_url,
                "status": "in_progress",
                "phases": {},
            }
        else:
            if title:
                self.data["features"][feature_id]["title"] = title
            if pr_url:
                self.data["features"][feature_id]["pr_url"] = pr_url
        self.save()

    def record_phase_start(self, feature_id: str, phase_name: str, start_time: datetime | None = None):
        self.init_feature(feature_id)
        st = start_time or utc_now()
        feat = self.data["features"][feature_id]
        if phase_name not in feat["phases"]:
            feat["phases"][phase_name] = {}
        feat["phases"][phase_name]["start_time"] = iso_utc(st)
        self.save()

    def record_phase_end(
        self,
        feature_id: str,
        phase_name: str,
        end_time: datetime | None = None,
        notes: str | None = None,
    ):
        self.init_feature(feature_id)
        et = end_time or utc_now()
        feat = self.data["features"][feature_id]
        if phase_name not in feat["phases"]:
            feat["phases"][phase_name] = {"start_time": iso_utc(et)}

        phase_data = feat["phases"][phase_name]
        phase_data["end_time"] = iso_utc(et)
        st = parse_iso(phase_data["start_time"])
        duration = max(0.0, (et - st).total_seconds())
        phase_data["duration_seconds"] = round(duration, 1)
        if notes:
            phase_data["notes"] = notes
        self.save()

    def record_phase(
        self,
        feature_id: str,
        phase_name: str,
        duration_seconds: float,
        notes: str | None = None,
    ):
        """Record a completed phase directly with duration."""
        self.init_feature(feature_id)
        now = utc_now()
        start = datetime.fromtimestamp(now.timestamp() - duration_seconds, tz=timezone.utc)
        feat = self.data["features"][feature_id]
        feat["phases"][phase_name] = {
            "start_time": iso_utc(start),
            "end_time": iso_utc(now),
            "duration_seconds": round(duration_seconds, 1),
            "notes": notes,
        }
        self.save()

    def set_feature_status(self, feature_id: str, status: str, pr_url: str | None = None):
        self.init_feature(feature_id)
        self.data["features"][feature_id]["status"] = status
        if pr_url:
            self.data["features"][feature_id]["pr_url"] = pr_url
        self.save()

    def compute_summary(self) -> dict[str, Any]:
        """Calculates batch metrics: calendar duration, feature sum, agent active time."""
        earliest_start: datetime | None = None
        latest_end: datetime | None = None
        total_feature_elapsed = 0.0
        total_agent_active = 0.0
        feature_summaries = {}

        for fid, feat in self.data["features"].items():
            f_start: datetime | None = None
            f_end: datetime | None = None
            f_active = 0.0
            f_total = 0.0

            for pname, pinfo in feat.get("phases", {}).items():
                st_str = pinfo.get("start_time")
                et_str = pinfo.get("end_time")
                dur = pinfo.get("duration_seconds")

                if st_str:
                    st = parse_iso(st_str)
                    if f_start is None or st < f_start:
                        f_start = st
                    if earliest_start is None or st < earliest_start:
                        earliest_start = st

                if et_str:
                    et = parse_iso(et_str)
                    if f_end is None or et > f_end:
                        f_end = et
                    if latest_end is None or et > latest_end:
                        latest_end = et

                if dur is not None:
                    if pname != "ci_wait":
                        f_active += dur
                    f_total += dur

            total_agent_active += f_active
            total_feature_elapsed += f_total

            feature_summaries[fid] = {
                "feature_id": fid,
                "title": feat.get("title", ""),
                "status": feat.get("status", "unknown"),
                "pr_url": feat.get("pr_url"),
                "active_seconds": round(f_active, 1),
                "total_seconds": round(f_total, 1),
                "phases": feat.get("phases", {}),
            }

        now = utc_now()
        calc_end = latest_end or now
        calendar_duration = (calc_end - earliest_start).total_seconds() if earliest_start else 0.0

        return {
            "batch_id": self.batch_id,
            "milestone": self.data.get("milestone", ""),
            "calendar_batch_duration_seconds": round(calendar_duration, 1),
            "calendar_batch_duration_formatted": format_duration(calendar_duration),
            "sum_feature_elapsed_seconds": round(total_feature_elapsed, 1),
            "sum_feature_elapsed_formatted": format_duration(total_feature_elapsed),
            "agent_active_seconds": round(total_agent_active, 1),
            "agent_active_formatted": format_duration(total_agent_active),
            "features_count": len(self.data["features"]),
            "features": feature_summaries,
        }

    def generate_markdown_report(self) -> str:
        """Generates a complete markdown report following Section 5 format."""
        summary = self.compute_summary()

        md = []
        md.append(f"# Batch Migration Timing Report ({self.batch_id})")
        if summary.get("milestone"):
            md.append(f"**Milestone**: {summary['milestone']}")
        md.append("")
        md.append("## Executive Timing Summary")
        md.append(f"- **Calendar Batch Duration**: {summary['calendar_batch_duration_formatted']} ({summary['calendar_batch_duration_seconds']}s)")
        md.append(f"- **Sum of Feature Elapsed Durations**: {summary['sum_feature_elapsed_formatted']} ({summary['sum_feature_elapsed_seconds']}s)")
        md.append(f"- **Agent Active Time (Non-CI)**: {summary['agent_active_formatted']} ({summary['agent_active_seconds']}s)")
        md.append("")

        md.append("## Feature Phase Matrix")
        headers = ["Feature ID", "Status", "Active Time", "Total Time", "PR"]
        md.append(f"| {' | '.join(headers)} |")
        md.append(f"|{':--- | ' * (len(headers) - 1)}:---|")

        for fid, fsummary in summary["features"].items():
            pr_link = f"[PR]({fsummary['pr_url']})" if fsummary.get("pr_url") else "-"
            md.append(
                f"| **{fid}** | {fsummary['status'].title()} | {format_duration(fsummary['active_seconds'])} | "
                f"{format_duration(fsummary['total_seconds'])} | {pr_link} |"
            )
        md.append("")

        md.append("## Detailed Phase Breakdown Per Feature")
        for fid, fsummary in summary["features"].items():
            md.append(f"### Feature {fid}: {fsummary['title']}")
            md.append(f"- **Status**: `{fsummary['status']}`")
            if fsummary.get("pr_url"):
                md.append(f"- **Pull Request**: {fsummary['pr_url']}")

            md.append("| Phase | Start (UTC) | End (UTC) | Duration | Notes |")
            md.append("|:---|:---|:---|:---|:---|")

            phases = fsummary.get("phases", {})
            for pkey in STANDARD_PHASES:
                pinfo = phases.get(pkey, {})
                display_name = PHASE_DISPLAY_NAMES.get(pkey, pkey)
                st = pinfo.get("start_time", "-")
                et = pinfo.get("end_time", "-")
                dur_str = format_duration(pinfo["duration_seconds"]) if "duration_seconds" in pinfo else "-"
                notes = pinfo.get("notes") or ""
                md.append(f"| {display_name} | {st} | {et} | {dur_str} | {notes} |")
            md.append("")

        return "\n".join(md)


def main():
    parser = argparse.ArgumentParser(description="Deterministic Batch Metrics & Timing Reporter for Vek")
    parser.add_argument("--batch-id", default="current", help="Batch identifier (default: current)")
    subparsers = parser.add_subparsers(dest="command", required=True)

    # start-phase
    p_start = subparsers.add_parser("start-phase", help="Record the start of a phase")
    p_start.add_argument("feature_id", help="Feature ID (e.g. 017)")
    p_start.add_argument("phase", choices=STANDARD_PHASES, help="Standard phase name")

    # end-phase
    p_end = subparsers.add_parser("end-phase", help="Record the completion of a phase")
    p_end.add_argument("feature_id", help="Feature ID (e.g. 017)")
    p_end.add_argument("phase", choices=STANDARD_PHASES, help="Standard phase name")
    p_end.add_argument("--notes", help="Optional notes")

    # record-phase
    p_rec = subparsers.add_parser("record-phase", help="Directly record phase duration")
    p_rec.add_argument("feature_id", help="Feature ID (e.g. 017)")
    p_rec.add_argument("phase", choices=STANDARD_PHASES, help="Standard phase name")
    p_rec.add_argument("duration", type=float, help="Duration in seconds")
    p_rec.add_argument("--notes", help="Optional notes")

    # set-status
    p_stat = subparsers.add_parser("set-status", help="Update feature status and PR URL")
    p_stat.add_argument("feature_id", help="Feature ID")
    p_stat.add_argument("status", choices=["in_progress", "ready", "blocked", "completed"])
    p_stat.add_argument("--title", help="Feature title")
    p_stat.add_argument("--pr", help="Pull request URL")

    # report
    p_rep = subparsers.add_parser("report", help="Generate timing report")
    p_rep.add_argument("--json", action="store_true", help="Output summary in JSON format")

    args = parser.parse_args()
    mgr = BatchMetricsManager(batch_id=args.batch_id)

    if args.command == "start-phase":
        mgr.record_phase_start(args.feature_id, args.phase)
        print(f"Recorded start of phase '{args.phase}' for {args.feature_id}")

    elif args.command == "end-phase":
        mgr.record_phase_end(args.feature_id, args.phase, notes=args.notes)
        print(f"Recorded end of phase '{args.phase}' for {args.feature_id}")

    elif args.command == "record-phase":
        mgr.record_phase(args.feature_id, args.phase, args.duration, notes=args.notes)
        print(f"Recorded {args.duration}s for phase '{args.phase}' on {args.feature_id}")

    elif args.command == "set-status":
        if args.title or args.pr:
            mgr.init_feature(args.feature_id, title=args.title or "", pr_url=args.pr)
        mgr.set_feature_status(args.feature_id, args.status, pr_url=args.pr)
        print(f"Updated status of {args.feature_id} to '{args.status}'")

    elif args.command == "report":
        if args.json:
            print(json.dumps(mgr.compute_summary(), indent=2))
        else:
            print(mgr.generate_markdown_report())


if __name__ == "__main__":
    main()
