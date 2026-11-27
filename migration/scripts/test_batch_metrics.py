#!/usr/bin/env python3
"""Unit tests for batch_metrics.py"""

import json
from pathlib import Path
import shutil
import sys
import tempfile
import unittest

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from batch_metrics import (
    BatchMetricsManager,
    format_duration,
    STANDARD_PHASES,
)


class TestBatchMetrics(unittest.TestCase):

    def setUp(self):
        self.test_dir = Path(tempfile.mkdtemp())
        self.mgr = BatchMetricsManager(batch_id="test", reports_dir=self.test_dir)

    def tearDown(self):
        shutil.rmtree(self.test_dir, ignore_errors=True)

    def test_format_duration(self):
        self.assertEqual(format_duration(45), "45s")
        self.assertEqual(format_duration(125), "2m 5s")
        self.assertEqual(format_duration(3665), "1h 1m 5s")

    def test_record_phases_and_summary(self):
        self.mgr.init_feature("017", title="DataGrid Master-Detail", pr_url="https://github.com/pr/123")
        self.mgr.record_phase("017", "claim_worktree_setup", 60.0, notes="Worktree initialized")
        self.mgr.record_phase("017", "implementation", 300.0)
        self.mgr.record_phase("017", "ci_wait", 120.0)

        summary = self.mgr.compute_summary()
        self.assertEqual(summary["features_count"], 1)
        self.assertEqual(summary["agent_active_seconds"], 360.0)  # non-CI phases
        self.assertEqual(summary["sum_feature_elapsed_seconds"], 480.0)

        f017 = summary["features"]["017"]
        self.assertEqual(f017["active_seconds"], 360.0)
        self.assertEqual(f017["total_seconds"], 480.0)

    def test_markdown_report_generation(self):
        self.mgr.init_feature("071", title="Dialog Focus Trap")
        self.mgr.record_phase("071", "tests_and_builds", 45.0)
        self.mgr.set_feature_status("071", "completed", pr_url="https://github.com/pr/155")

        md = self.mgr.generate_markdown_report()
        self.assertIn("# Batch Migration Timing Report (test)", md)
        self.assertIn("## Executive Timing Summary", md)
        self.assertIn("## Feature Phase Matrix", md)
        self.assertIn("## Detailed Phase Breakdown Per Feature", md)
        self.assertIn("| Tests & Builds |", md)
        self.assertIn("https://github.com/pr/155", md)


if __name__ == "__main__":
    unittest.main()
