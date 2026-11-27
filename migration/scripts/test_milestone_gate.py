#!/usr/bin/env python3
"""Unit tests for milestone_gate.py"""

import unittest
from unittest.mock import MagicMock, patch
import json
from pathlib import Path
import sys

# Ensure script dir is in python path
SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from milestone_gate import (
    extract_feature_ids_from_text,
    verify_feature_milestone,
    propose_batch,
    MilestoneInfo,
    MilestoneError,
)

SAMPLE_MILESTONE_DESC = """
### Summary
- Total Tasks: 4 Feature Migrations

### Tasks Status
| ID | Component | Feature Title | Status |
|:---|:---|:---|:---:|
| **014** | DataGrid | Popup Filter Menu | Pending |
| **017** | DataGrid | Master-Detail Collapse | Pending |
| **070** | Dialog | Animations | Pending |
- **099**: ColorPicker Palette Presets
"""


class TestMilestoneGate(unittest.TestCase):

    def test_extract_feature_ids_from_text(self):
        ids = extract_feature_ids_from_text(SAMPLE_MILESTONE_DESC)
        self.assertEqual(ids, ["014", "017", "070", "099"])

    @patch("milestone_gate.get_primary_active_milestone")
    def test_verify_feature_milestone(self, mock_get_milestone):
        mock_get_milestone.return_value = MilestoneInfo(
            id=123,
            number=1,
            title="v1.5.0",
            description=SAMPLE_MILESTONE_DESC,
            html_url="http://github.com/test",
            feature_ids=["014", "017", "070", "099"],
        )

        res_true = verify_feature_milestone("017")
        self.assertTrue(res_true["in_milestone"])
        self.assertEqual(res_true["active_milestone"], "v1.5.0")

        res_false = verify_feature_milestone("001")
        self.assertFalse(res_false["in_milestone"])

    @patch("milestone_gate.get_primary_active_milestone")
    @patch("milestone_gate.inspect_candidate_eligibility")
    def test_propose_batch(self, mock_inspect, mock_get_milestone):
        mock_get_milestone.return_value = MilestoneInfo(
            id=123,
            number=1,
            title="v1.5.0",
            description=SAMPLE_MILESTONE_DESC,
            html_url="http://github.com/test",
            feature_ids=["017", "070", "099"],
        )

        # Let 017 and 070 be eligible, 099 have active claim
        def mock_eligibility(fid):
            if fid == "099":
                return False, "Active claim exists", {}
            return True, None, {"dependencies_complete": True, "claim_active": False, "eligible": True}

        mock_inspect.side_effect = mock_eligibility

        result = propose_batch(max_batch_size=2)
        self.assertEqual(result["milestone"]["title"], "v1.5.0")
        selected_ids = [c["id"] for c in result["selected_batch"]]
        self.assertTrue(len(selected_ids) <= 2)
        self.assertNotIn("099", selected_ids)


if __name__ == "__main__":
    unittest.main()
