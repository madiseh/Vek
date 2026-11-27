#!/usr/bin/env python3
"""Unit tests for setup_worktree.py"""

import json
from pathlib import Path
import sys
import unittest
from unittest.mock import MagicMock, patch

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from setup_worktree import (
    get_feature_info,
    default_worktree_path,
    list_existing_worktrees,
    setup_worker_environment,
    WorktreeError,
)


class TestSetupWorktree(unittest.TestCase):

    def test_get_feature_info(self):
        info = get_feature_info("017")
        self.assertEqual(info["feature"]["id"], "017")
        self.assertIn("target_branch", info)
        self.assertTrue(info["target_branch"].startswith("feature/migration-017"))

    def test_default_worktree_path(self):
        p = default_worktree_path("017", "my-slug", base_dir=Path("/custom/dir"))
        self.assertEqual(p, Path("/custom/dir/wt-017-my-slug").resolve())

    @patch("setup_worktree.run_cmd")
    def test_list_existing_worktrees(self, mock_run_cmd):
        mock_run_cmd.return_value = MagicMock(
            stdout=(
                "worktree /repo/main\n"
                "HEAD abc1234\n"
                "branch refs/heads/master\n\n"
                "worktree /repo/wt-1\n"
                "HEAD def5678\n"
                "branch refs/heads/feature/migration-017\n"
            )
        )
        wts = list_existing_worktrees()
        self.assertEqual(len(wts), 2)

    @patch("setup_worktree.sync_upstream_master")
    @patch("setup_worktree.run_cmd")
    @patch("setup_worktree.list_existing_worktrees")
    def test_setup_worker_environment(self, mock_list_wt, mock_run_cmd, mock_sync):
        mock_sync.return_value = "commithash123"
        mock_list_wt.return_value = {}

        # Mock branch check
        mock_run_cmd.return_value = MagicMock(stdout="", returncode=0)

        with patch("pathlib.Path.write_text") as mock_write, patch("pathlib.Path.exists") as mock_exists:
            mock_exists.return_value = True
            res = setup_worker_environment(
                feature_id="017",
                target_dir=Path("/tmp/wt-017"),
                run_id="run-test",
                owner="agent-test",
                skip_fetch=True,
            )

            self.assertEqual(res["feature_id"], "017")
            self.assertEqual(res["run_id"], "run-test")
            self.assertEqual(res["owner"], "agent-test")


if __name__ == "__main__":
    unittest.main()
