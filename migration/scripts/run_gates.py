#!/usr/bin/env python3
"""
Deterministic Verification, Test, and Format Quality Gates for Vek Migration.

Automates the non-reasoning steps of quality gates:
1. Changed File Detection:
   - Uses `git diff --name-only <target_branch>...HEAD` (or uncommitted diff) to detect
     all modified and added files in `src/` and `tests/`.
2. Format Gate (`dotnet format`):
   - Runs `dotnet format Vek.slnx --no-restore --include <changed files>` to automatically repair.
   - Runs `dotnet format Vek.slnx --no-restore --verify-no-changes --include <changed files>` to verify.
   - Runs `dotnet format whitespace --folder --include <changed files>` to ensure UTF-8 BOM, LF line endings, and whitespace compliance.
3. Test Gate (`dotnet test`):
   - Auto-detects relevant test filter from changed component or specification metadata.
   - Executes filtered `dotnet test` using Microsoft.Testing.Platform runner syntax:
     `dotnet test --project tests/Components/Vek.UnitTests/Vek.UnitTests.csproj --no-restore -p:SkipBunCompile=true -- --filter "..." --output Normal --no-ansi --hangdump --hangdump-timeout 30s`
4. Lint Gate (oxlint via bun if TS/JS files changed):
   - Detects if TS/JS files under `src/Vek/TScripts/` changed, and runs `bun run src/Vek/build.mts lint`.
5. Clean JSON Reporting:
   - Provides structured JSON result to the agent with stage status, diagnostics, and failure details.
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
SOLUTION_PATH = "Vek.slnx"
UNIT_TESTS_PROJECT = "tests/Components/Vek.UnitTests/Vek.UnitTests.csproj"


class GateError(Exception):
    """Exception raised when gate execution encounters a fatal setup error."""


@dataclass
class GateResult:
    name: str
    passed: bool
    command: str
    duration_seconds: float
    output: str
    error: str | None = None


def run_command(
    args: list[str],
    cwd: Path | str = ROOT,
    env: dict[str, str] | None = None,
) -> tuple[int, str, str]:
    """Execute command safely and capture output."""
    merged_env = os.environ.copy()
    if env:
        merged_env.update(env)

    try:
        res = subprocess.run(
            args,
            cwd=str(cwd),
            capture_output=True,
            text=True,
            check=False,
            env=merged_env,
        )
        return res.returncode, res.stdout, res.stderr
    except FileNotFoundError as exc:
        return 127, "", f"Executable not found: {args[0]} ({exc})"


def get_changed_files(
    target_branch: str = "master",
    worktree_dir: Path | str = ROOT,
    include_uncommitted: bool = True,
) -> list[str]:
    """
    Get all changed files relative to target branch and working directory.
    Uses forward slashes for cross-platform dotnet format compatibility.
    """
    wt_path = Path(worktree_dir).resolve()

    # 1. Check committed changes against target_branch
    code, stdout, _ = run_command(
        ["git", "diff", f"{target_branch}...HEAD", "--name-only"],
        cwd=wt_path,
    )
    files = set(stdout.splitlines()) if code == 0 else set()

    # 2. Check uncommitted changes (staged + unstaged) if requested
    if include_uncommitted:
        code_diff, stdout_diff, _ = run_command(["git", "diff", "HEAD", "--name-only"], cwd=wt_path)
        if code_diff == 0:
            files.update(stdout_diff.splitlines())

        code_untracked, stdout_untracked, _ = run_command(
            ["git", "status", "--porcelain"],
            cwd=wt_path,
        )
        if code_untracked == 0:
            for line in stdout_untracked.splitlines():
                if len(line) > 3:
                    files.add(line[3:].strip())

    # Filter out empty strings, normalize slashes
    normalized = []
    for f in sorted(files):
        f_clean = f.strip().replace("\\", "/")
        if f_clean and not f_clean.startswith(".git/"):
            normalized.append(f_clean)

    return normalized


def filter_format_files(files: list[str]) -> list[str]:
    """Filters changed files to those applicable for dotnet format (src/** and tests/** .cs and .razor)."""
    applicable = []
    for f in files:
        if (f.startswith("src/") or f.startswith("tests/")) and f.endswith((".cs", ".razor")):
            applicable.append(f)
    return applicable


def run_format_gate(
    changed_files: list[str],
    worktree_dir: Path | str = ROOT,
    repair: bool = True,
) -> GateResult:
    """Executes dotnet format repair and verify passes."""
    import time

    start_time = time.time()
    applicable_files = filter_format_files(changed_files)

    if not applicable_files:
        return GateResult(
            name="format",
            passed=True,
            command="dotnet format (skipped: no applicable C#/Razor files)",
            duration_seconds=0.0,
            output="No changed C#/Razor files in src/ or tests/.",
        )

    wt_path = Path(worktree_dir).resolve()
    include_args = []
    for f in applicable_files:
        include_args.extend(["--include", f])

    # 1. Optional repair pass
    repair_output = ""
    if repair:
        code, stdout, stderr = run_command(
            ["dotnet", "format", SOLUTION_PATH, "--no-restore", *include_args],
            cwd=wt_path,
        )
        repair_output = (stdout + "\n" + stderr).strip()

    # 2. Whitespace / BOM / LF repair pass
    ws_code, ws_stdout, ws_stderr = run_command(
        ["dotnet", "format", "whitespace", "--folder", *include_args],
        cwd=wt_path,
    )

    # 3. Verification pass (--verify-no-changes)
    verify_cmd = ["dotnet", "format", SOLUTION_PATH, "--no-restore", "--verify-no-changes", *include_args]
    code, stdout, stderr = run_command(verify_cmd, cwd=wt_path)
    elapsed = time.time() - start_time

    combined_out = (repair_output + "\n" + stdout + "\n" + stderr).strip()
    passed = (code == 0)

    return GateResult(
        name="format",
        passed=passed,
        command=" ".join(verify_cmd),
        duration_seconds=round(elapsed, 2),
        output=combined_out,
        error=stderr.strip() if not passed else None,
    )


def derive_test_filter(
    feature_id: str | None = None,
    module: str | None = None,
    changed_files: list[str] | None = None,
) -> str | None:
    """Derive appropriate bUnit test filter from feature metadata or changed paths."""
    if module:
        return f"FullyQualifiedName~{module}"

    if changed_files:
        for f in changed_files:
            m = re.search(r"Components/([A-Za-z0-9]+)/", f)
            if m:
                return f"FullyQualifiedName~{m.group(1)}"

    return None


def run_test_gate(
    test_filter: str,
    worktree_dir: Path | str = ROOT,
    timeout: str = "30s",
) -> GateResult:
    """Executes filtered dotnet test using Microsoft.Testing.Platform runner syntax."""
    import time

    start_time = time.time()
    wt_path = Path(worktree_dir).resolve()

    cmd = [
        "dotnet", "test",
        "--project", UNIT_TESTS_PROJECT,
        "--no-restore",
        "-p:SkipBunCompile=true",
        "--",
        "--filter", test_filter,
        "--output", "Normal",
        "--no-ansi",
        "--hangdump",
        "--hangdump-timeout", timeout,
    ]

    code, stdout, stderr = run_command(cmd, cwd=wt_path)
    elapsed = time.time() - start_time

    # Microsoft.Testing.Platform check: exit code 0 and non-zero tests ran
    output_text = stdout + "\n" + stderr
    passed = (code == 0)

    # Safety: verify test didn't pass with 0 tests executed
    if "Zero tests ran" in output_text or "No test matches the given testcase filter" in output_text:
        passed = False
        error_msg = "Gate failed: Zero tests ran for the given filter."
    else:
        error_msg = stderr.strip() if not passed else None

    return GateResult(
        name="test",
        passed=passed,
        command=" ".join(cmd),
        duration_seconds=round(elapsed, 2),
        output=output_text.strip(),
        error=error_msg,
    )


def run_lint_gate(
    changed_files: list[str],
    worktree_dir: Path | str = ROOT,
) -> GateResult | None:
    """Runs oxlint / bun lint check if TypeScript or JavaScript files changed."""
    import time

    ts_files = [f for f in changed_files if f.endswith((".ts", ".mts", ".js", ".mjs"))]
    if not ts_files:
        return None

    start_time = time.time()
    wt_path = Path(worktree_dir).resolve()
    cmd = ["bun", "run", "src/Vek/build.mts", "lint"]
    code, stdout, stderr = run_command(cmd, cwd=wt_path)
    elapsed = time.time() - start_time

    return GateResult(
        name="lint",
        passed=(code == 0),
        command=" ".join(cmd),
        duration_seconds=round(elapsed, 2),
        output=(stdout + "\n" + stderr).strip(),
        error=stderr.strip() if code != 0 else None,
    )


def execute_gates(
    worktree_dir: Path | str = ROOT,
    target_branch: str = "master",
    test_filter: str | None = None,
    module: str | None = None,
    feature_id: str | None = None,
    skip_tests: bool = False,
    repair_format: bool = True,
) -> dict[str, Any]:
    """Runs all relevant quality gates deterministically and returns a comprehensive report."""
    changed_files = get_changed_files(target_branch=target_branch, worktree_dir=worktree_dir)
    results: list[GateResult] = []

    # 1. Format Gate
    fmt_res = run_format_gate(changed_files, worktree_dir=worktree_dir, repair=repair_format)
    results.append(fmt_res)

    # 2. Lint Gate (if JS/TS changed)
    lint_res = run_lint_gate(changed_files, worktree_dir=worktree_dir)
    if lint_res:
        results.append(lint_res)

    # 3. Test Gate
    if not skip_tests:
        filter_str = test_filter or derive_test_filter(feature_id, module, changed_files)
        if filter_str:
            test_res = run_test_gate(filter_str, worktree_dir=worktree_dir)
            results.append(test_res)
        else:
            results.append(
                GateResult(
                    name="test",
                    passed=True,
                    command="dotnet test (skipped: no filter derived)",
                    duration_seconds=0.0,
                    output="No test filter provided or derived from changed files.",
                )
            )

    all_passed = all(r.passed for r in results)

    return {
        "all_passed": all_passed,
        "changed_files_count": len(changed_files),
        "changed_files": changed_files,
        "gates": [asdict(r) for r in results],
    }


def main():
    parser = argparse.ArgumentParser(description="Deterministic Quality Gates (Format, Test, Lint) for Vek")
    parser.add_argument("--dir", type=Path, default=ROOT, help="Working directory / worktree path")
    parser.add_argument("--target-branch", default="master", help="Target branch to diff against (default: master)")
    parser.add_argument("--test-filter", help="Explicit bUnit test filter (e.g. FullyQualifiedName~DialogTests)")
    parser.add_argument("--module", help="Component module name to derive test filter (e.g. Dialog, DataGrid)")
    parser.add_argument("--feature-id", help="3-digit feature ID")
    parser.add_argument("--skip-tests", action="store_true", help="Skip test execution gate")
    parser.add_argument("--no-repair", action="store_true", help="Do not repair formatting, only verify")

    args = parser.parse_args()

    report = execute_gates(
        worktree_dir=args.dir,
        target_branch=args.target_branch,
        test_filter=args.test_filter,
        module=args.module,
        feature_id=args.feature_id,
        skip_tests=args.skip_tests,
        repair_format=not args.no_repair,
    )

    print(json.dumps(report, indent=2))
    if not report["all_passed"]:
        sys.exit(1)


if __name__ == "__main__":
    main()
