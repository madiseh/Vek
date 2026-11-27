#!/usr/bin/env python3
"""Unit tests for run_gates.py"""

import json
from pathlib import Path
import sys
import unittest
from unittest.mock import MagicMock, patch

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from run_gates import (
    filter_format_files,
    derive_test_filter,
    run_format_gate,
    run_test_gate,
    run_lint_gate,
    execute_gates,
    GateResult,
)


class TestRunGates(unittest.TestCase):

    def test_filter_format_files(self):
        files = [
            "src/Vek/Components/Button/VekButton.razor",
            "src/Vek/Components/Button/VekButton.razor.cs",
            "tests/Components/Vek.UnitTests/Components/ButtonTests.cs",
            "docs/README.md",
            "migration/scripts/run_gates.py",
            "src/Vek/Styles/button.scss",
        ]
        filtered = filter_format_files(files)
        self.assertEqual(len(filtered), 3)
        self.assertIn("src/Vek/Components/Button/VekButton.razor", filtered)
        self.assertIn("tests/Components/Vek.UnitTests/Components/ButtonTests.cs", filtered)
        self.assertNotIn("docs/README.md", filtered)

    def test_derive_test_filter(self):
        f1 = derive_test_filter(module="Dialog")
        self.assertEqual(f1, "FullyQualifiedName~Dialog")

        f2 = derive_test_filter(changed_files=["src/Vek/Components/Menu/VekMenu.razor.cs"])
        self.assertEqual(f2, "FullyQualifiedName~Menu")

    @patch("run_gates.run_command")
    def test_run_format_gate_success(self, mock_run_cmd):
        mock_run_cmd.return_value = (0, "All files formatted.", "")
        res = run_format_gate(["src/Vek/VekComponent.cs"])
        self.assertTrue(res.passed)
        self.assertEqual(res.name, "format")

    @patch("run_gates.run_command")
    def test_run_format_gate_failure(self, mock_run_cmd):
        # Repair passes, but verify fails with exit code 1
        mock_run_cmd.side_effect = [
            (0, "repair ok", ""),
            (0, "whitespace ok", ""),
            (1, "", "Formatted 1 file(s) that were not supposed to be changed"),
        ]
        res = run_format_gate(["src/Vek/VekComponent.cs"])
        self.assertFalse(res.passed)
        self.assertIsNotNone(res.error)

    @patch("run_gates.run_command")
    def test_run_test_gate_success(self, mock_run_cmd):
        mock_run_cmd.return_value = (0, "Passed! - Failed: 0, Passed: 5, Total: 5", "")
        res = run_test_gate("FullyQualifiedName~ButtonTests")
        self.assertTrue(res.passed)
        self.assertEqual(res.name, "test")

    @patch("run_gates.run_command")
    def test_run_test_gate_zero_tests(self, mock_run_cmd):
        mock_run_cmd.return_value = (0, "Zero tests ran", "")
        res = run_test_gate("FullyQualifiedName~NonExistent")
        self.assertFalse(res.passed)
        self.assertIn("Zero tests ran", res.error)

    @patch("run_gates.get_changed_files")
    @patch("run_gates.run_format_gate")
    @patch("run_gates.run_test_gate")
    def test_execute_gates(self, mock_test, mock_format, mock_changed):
        mock_changed.return_value = ["src/Vek/Components/Dialog/VekDialog.cs"]
        mock_format.return_value = GateResult("format", True, "cmd", 1.0, "ok")
        mock_test.return_value = GateResult("test", True, "cmd", 2.0, "ok")

        report = execute_gates(module="Dialog")
        self.assertTrue(report["all_passed"])
        self.assertEqual(len(report["gates"]), 2)


if __name__ == "__main__":
    unittest.main()
