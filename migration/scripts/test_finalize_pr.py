#!/usr/bin/env python3
"""Unit tests for finalize_pr.py"""

import json
from pathlib import Path
import sys
import unittest
from unittest.mock import MagicMock, patch

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from finalize_pr import (
    generate_pr_title,
    build_pr_body,
    format_screenshot_markdown,
    check_existing_pr,
    finalize_feature_pr,
    PRError,
)


class TestFinalizePR(unittest.TestCase):

    def test_generate_pr_title(self):
        feat = {
            "module": "Button",
            "title": "Button Loading / Busy Spinner State with Auto-Disable",
        }
        title = generate_pr_title(feat)
        self.assertEqual(title, "feat(button): loading / Busy Spinner State with Auto-Disable")

        feat2 = {
            "module": "DataGrid",
            "title": "Master-Detail Auto-Collapse and Cascade Expand Control",
        }
        title2 = generate_pr_title(feat2)
        self.assertEqual(title2, "feat(datagrid): master-Detail Auto-Collapse and Cascade Expand Control")

    @patch("finalize_pr.find_screenshots_for_feature")
    def test_format_screenshot_markdown(self, mock_find):
        mock_find.return_value = [
            Path("/path/to/017-01-default.png"),
            Path("/path/to/017-02-expanded.png"),
        ]
        md = format_screenshot_markdown("017", "feature/migration-017", "madiseh", "Vek")
        self.assertIn("![01 Default](https://raw.githubusercontent.com/madiseh/Vek/feature/migration-017/migration/artifacts/ui-screenshots/017-01-default.png)", md)
        self.assertIn("![02 Expanded](https://raw.githubusercontent.com/madiseh/Vek/feature/migration-017/migration/artifacts/ui-screenshots/017-02-expanded.png)", md)

    def test_build_pr_body(self):
        feat = {
            "title": "My Feature",
            "module": "Dialog",
            "specification": "specifications/071-test.md",
        }
        body = build_pr_body("071", feat, "my-branch", "owner", "repo")
        self.assertIn("## Summary\nMigration 071: My Feature", body)
        self.assertIn("## Verification", body)
        self.assertIn("## UI screenshots", body)

    @patch("finalize_pr.run_cmd")
    def test_check_existing_pr(self, mock_run_cmd):
        mock_run_cmd.return_value = (
            0,
            json.dumps([{"number": 123, "url": "https://github.com/pr/123", "state": "OPEN"}]),
            "",
        )
        pr = check_existing_pr("feature/test")
        self.assertIsNotNone(pr)
        self.assertEqual(pr["number"], 123)


if __name__ == "__main__":
    unittest.main()
