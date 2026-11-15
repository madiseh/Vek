#!/usr/bin/env python3
"""
Autonomous Feature Migration Pipeline Orchestrator for Vek.
Selects the next ready feature from backlog.json, validates dependencies,
manages isolated worktrees, and coordinates the Planner -> Implementer -> Gate -> Reviewer lifecycle.
"""

import json
import os
import subprocess
import sys
from pathlib import Path

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


def get_next_ready_feature(backlog):
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


def run_cmd(cmd, cwd=ROOT_DIR):
    print(f"Executing: {cmd}")
    res = subprocess.run(cmd, shell=True, cwd=cwd, capture_output=True, text=True)
    if res.returncode != 0:
        print(f"Command failed (exit {res.returncode}):\n{res.stderr}\n{res.stdout}")
    return res.returncode == 0, res.stdout, res.stderr


def verify_quality_gates(worktree_dir):
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
    
    # 4. Brand Cleanliness Check
    clean_ok, out, err = run_cmd("git grep -i 'devexpress'", cwd=worktree_dir)
    if clean_ok and out.strip():
        return False, f"Brand anonymity violation detected:\n{out}"
    
    return True, "All Quality Gates Passed (Build, Test, Format, Brand Cleanliness)."


def main():
    backlog = load_backlog()
    feature = get_next_ready_feature(backlog)
    
    if not feature:
        print("No features currently ready for migration.")
        sys.exit(0)
        
    print(f"\n=======================================================")
    print(f" Selected Feature: [{feature['id']}] {feature['title']}")
    print(f" Module:           {feature['module']}")
    print(f" Specification:    {feature['specification']}")
    print(f"=======================================================\n")
    
    spec_path = MIGRATION_DIR / feature["specification"]
    if not spec_path.exists():
        print(f"Error: Specification file missing at {spec_path}")
        sys.exit(1)
        
    branch_name = f"{backlog['pipeline']['feature_branch_prefix']}{feature['id']}-{feature['slug']}"
    print(f"Target Branch: {branch_name}")
    print(f"Ready for Planner Agent execution.")


if __name__ == "__main__":
    main()
