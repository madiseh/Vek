#!/usr/bin/env python3
"""
Expands the migration backlog beyond DataGrid into Button, TextBox/Input, and Tabs/Navigation components.
Generates individual specification contracts in migration/specifications/ and updates migration/backlog.json.
Maintains clean-room anonymity (0 brand mentions) and strict architectural alignment with Vek.
"""

import json
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT_DIR / "migration"
SPECS_DIR = MIGRATION_DIR / "specifications"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"

NEW_FEATURES = [
    # --- BUTTON & ACTION CONTROLS (031 - 035) ---
    {
        "id": "031",
        "slug": "button-loading-state-and-auto-disable",
        "title": "Button Loading / Busy Spinner State with Auto-Disable",
        "module": "Button",
        "priority": 31,
        "status": "ready",
        "dependencies": [],
        "summary": "Inline spinner loading state that automatically disables user interaction, preserves dimensions to prevent layout shifts, and supports custom loading content.",
        "target_files": [
            "src/Vek/Components/Button/VekButton.razor",
            "src/Vek/Components/Button/VekButton.razor.cs",
            "src/Vek/Styles/components/_button.scss"
        ],
        "requirements": [
            "Loading boolean parameter toggling busy state.",
            "LoadingTemplate RenderFragment for custom spinners.",
            "Automatic disabled attribute binding while Loading is true to prevent double clicks.",
            "CSS layout preservation maintaining button width and height during state switch."
        ]
    },
    {
        "id": "032",
        "slug": "button-group-selection-modes",
        "title": "ButtonGroup Single and Multiple Selection Toggle Modes",
        "module": "ButtonGroup",
        "priority": 32,
        "status": "ready",
        "dependencies": [],
        "summary": "Selection management for ButtonGroup supporting Single, Multiple, and Radio-style toggle behaviors with two-way data binding.",
        "target_files": [
            "src/Vek/Components/ButtonGroup/VekButtonGroup.razor",
            "src/Vek/Components/ButtonGroup/VekButtonGroup.razor.cs",
            "src/Vek/Components/ButtonGroup/ButtonGroupSelectionMode.cs"
        ],
        "requirements": [
            "SelectionMode enum: None, Single, Multiple.",
            "SelectedValues two-way binding for multi-selection, and SelectedValue for single-selection.",
            "OnSelectionChanged event callback with selected keys.",
            "Proper ARIA attributes: aria-pressed and role='group'."
        ]
    },
    {
        "id": "033",
        "slug": "split-button-and-dropdown-actions",
        "title": "SplitButton with Dual Actions and DropDown Menu",
        "module": "Button",
        "priority": 33,
        "status": "ready",
        "dependencies": [],
        "summary": "SplitButton combining a primary action button and an attached dropdown arrow triggering a contextual flyout action menu.",
        "target_files": [
            "src/Vek/Components/Button/VekSplitButton.razor",
            "src/Vek/Components/Button/VekSplitButton.razor.cs",
            "src/Vek/Components/Button/VekDropDownButton.razor",
            "src/Vek/Styles/components/_button.scss"
        ],
        "requirements": [
            "Primary action button invoking OnClick.",
            "Secondary dropdown trigger button opening flyout popover menu.",
            "MenuItems template slot or child item collection.",
            "Keyboard navigation (Alt+Down, Enter, Escape, Arrow keys)."
        ]
    },
    {
        "id": "034",
        "slug": "button-double-click-and-debounce-guard",
        "title": "Button Click Debounce and Double-Click Protection",
        "module": "Button",
        "priority": 34,
        "status": "ready",
        "dependencies": [],
        "summary": "Built-in click rate limiting to prevent duplicate submissions on critical actions, with separate on-double-click event support.",
        "target_files": [
            "src/Vek/Components/Button/VekButton.razor.cs"
        ],
        "requirements": [
            "ClickDebounceInterval parameter in milliseconds (default: 300ms).",
            "Prevents duplicate rapid clicks from firing multiple event handler invocations.",
            "Dedicated OnDoubleClick EventCallback with configurable double-click threshold."
        ]
    },
    {
        "id": "035",
        "slug": "button-badge-and-counter-adornment",
        "title": "Button Floating Badge, Status Dot, and Counter Adornment",
        "module": "Button",
        "priority": 35,
        "status": "ready",
        "dependencies": [],
        "summary": "Direct badge integration on buttons displaying notification numbers, status indicator dots, or badges without outer wrapper components.",
        "target_files": [
            "src/Vek/Components/Button/VekButton.razor",
            "src/Vek/Components/Button/VekButton.razor.cs",
            "src/Vek/Styles/components/_button.scss"
        ],
        "requirements": [
            "BadgeContent string/object parameter.",
            "BadgeColor and BadgeDot boolean parameters.",
            "BadgePosition enum (TopEnd, TopStart, BottomEnd, BottomStart).",
            "Zero extra component allocation per button."
        ]
    },

    # --- TEXTBOX & INPUT CONTROLS (036 - 043) ---
    {
        "id": "036",
        "slug": "textbox-clear-button-modes",
        "title": "TextBox Clear Button Display Modes and Action Hooks",
        "module": "TextField",
        "priority": 36,
        "status": "ready",
        "dependencies": [],
        "summary": "Configurable clear button display modes inside input frames, automatically clearing bound text and restoring focus.",
        "target_files": [
            "src/Vek/Components/TextField/VekTextField.razor",
            "src/Vek/Components/TextField/VekTextField.razor.cs",
            "src/Vek/Components/TextField/ClearButtonDisplayMode.cs",
            "src/Vek/Styles/components/_textfield.scss"
        ],
        "requirements": [
            "ClearButtonDisplayMode enum: Never, WhileEditing, Always.",
            "Clears bound Text / Value and raises OnClearButtonClick callback.",
            "Maintains input focus after clearing.",
            "Accessible keyboard focus and aria-label='Clear text'."
        ]
    },
    {
        "id": "037",
        "slug": "textbox-password-visibility-toggle",
        "title": "TextBox Password Masking with Interactive Visibility Toggle",
        "module": "TextField",
        "priority": 37,
        "status": "ready",
        "dependencies": [],
        "summary": "Interactive password visibility toggle button switching input between masked password and plain text with accessible announcements.",
        "target_files": [
            "src/Vek/Components/TextField/VekTextField.razor",
            "src/Vek/Components/TextField/VekTextField.razor.cs"
        ],
        "requirements": [
            "ShowPasswordToggle boolean property on password inputs.",
            "Interactive eye / eye-slash icon button toggling input type.",
            "Preserves cursor position and focus during toggle.",
            "Dynamic aria-label announcing 'Show password' / 'Hide password'."
        ]
    },
    {
        "id": "038",
        "slug": "textbox-debounced-bind-value-modes",
        "title": "TextBox Configurable Value Binding Modes and Debounce Delay",
        "module": "TextField",
        "priority": 38,
        "status": "ready",
        "dependencies": [],
        "summary": "Fine-grained value binding modes (OnInput, OnChange, OnDelayedInput) with configurable debounce delay for high-performance forms.",
        "target_files": [
            "src/Vek/Components/TextField/VekTextField.razor.cs",
            "src/Vek/Components/Input/VekInputControl.cs"
        ],
        "requirements": [
            "BindValueMode enum: OnInput, OnChange, OnDelayedInput.",
            "InputDelay debounce interval parameter (default: 300ms).",
            "Prevents expensive re-renders and model updates on every single keystroke.",
            "Immediate commit on Enter key or blur."
        ]
    },
    {
        "id": "039",
        "slug": "textbox-custom-buttons-and-adornments",
        "title": "TextBox Embedded Custom Action Buttons and Adornment Slots",
        "module": "TextField",
        "priority": 39,
        "status": "ready",
        "dependencies": [],
        "summary": "Embedded custom action buttons and adornment templates rendered directly inside the input border frame.",
        "target_files": [
            "src/Vek/Components/TextField/VekEditorButton.cs",
            "src/Vek/Components/TextField/VekTextField.razor",
            "src/Vek/Components/TextField/VekTextField.razor.cs"
        ],
        "requirements": [
            "Buttons RenderFragment slot accepting multiple VekEditorButton instances.",
            "Leading and trailing position configuration per button.",
            "Seamless click handling without losing focus on the parent text box.",
            "Consistent height, padding, and theme token styling."
        ]
    },
    {
        "id": "040",
        "slug": "masked-input-regex-and-literals",
        "title": "MaskedInput Pattern Rules, Prompt Characters, and Literals Preservation",
        "module": "Mask",
        "priority": 40,
        "status": "ready",
        "dependencies": [],
        "summary": "Advanced masked text input supporting phone, date, numeric, RegEx patterns, customizable prompt characters, and literal preservation options.",
        "target_files": [
            "src/Vek/Components/Mask/VekMask.cs",
            "src/Vek/Components/TextField/VekMaskedInput.razor",
            "src/Vek/Components/TextField/VekMaskedInput.razor.cs"
        ],
        "requirements": [
            "Standard mask tokens (numeric, alphabetic, alphanumeric, optional).",
            "PromptChar parameter (default: '_').",
            "ShowLiterals enum (Always, WhileEditing, Never).",
            "SaveLiteralRepresentation boolean controlling whether value contains formatting."
        ]
    },
    {
        "id": "041",
        "slug": "spin-edit-mousewheel-and-acceleration",
        "title": "SpinEdit Stepper Controls, Mouse Wheel Scrolling, and Acceleration",
        "module": "NumericField",
        "priority": 41,
        "status": "ready",
        "dependencies": [],
        "summary": "Numeric stepper with touch-friendly up/down buttons, mouse wheel increment on focus, step acceleration, and min/max clamp.",
        "target_files": [
            "src/Vek/Components/NumericField/VekNumericField.razor",
            "src/Vek/Components/NumericField/VekNumericField.razor.cs"
        ],
        "requirements": [
            "ShowSpinButtons boolean parameter.",
            "AllowMouseWheel boolean parameter enabling wheel scrolling when focused.",
            "Step acceleration when holding down spin buttons.",
            "Strict Min and Max clamping with visual boundary indicators."
        ]
    },
    {
        "id": "042",
        "slug": "combobox-incremental-filtering-and-virtual-scroll",
        "title": "ComboBox Incremental Live Filtering and Virtualized Dropdown",
        "module": "Autocomplete",
        "priority": 42,
        "status": "ready",
        "dependencies": [],
        "summary": "Combo box / Autocomplete dropdown with real-time incremental character filtering, match highlighting, and virtualized scrolling for large item lists.",
        "target_files": [
            "src/Vek/Components/Autocomplete/VekAutocomplete.razor",
            "src/Vek/Components/Autocomplete/VekAutocomplete.razor.cs"
        ],
        "requirements": [
            "Incremental search matching substrings or word prefixes.",
            "Visual text highlighting of matched tokens in dropdown items.",
            "Virtualize scroll support rendering only visible dropdown rows.",
            "AllowCustomValues boolean property allowing free text input."
        ]
    },
    {
        "id": "043",
        "slug": "combobox-multi-selection-and-chip-tags",
        "title": "ComboBox Multi-Selection with Removable Chip Tags",
        "module": "Select",
        "priority": 43,
        "status": "backlog",
        "dependencies": ["042"],
        "summary": "Multi-selection dropdown rendering chosen values as removable chip tags inside the input box, with 'Select All' and max-tag truncation.",
        "target_files": [
            "src/Vek/Components/Select/VekSelect.razor",
            "src/Vek/Components/Select/VekSelect.razor.cs"
        ],
        "requirements": [
            "MultiSelection boolean parameter.",
            "Selected items rendered as compact removable VekChip elements.",
            "MaxDisplayedTags parameter with '+N more' overflow badge.",
            "Select All checkbox in dropdown header."
        ]
    },

    # --- TABS & NAVIGATION SUITE (044 - 052) ---
    {
        "id": "044",
        "slug": "tabs-scroll-and-navigation-buttons",
        "title": "Tabs Overflow Scrolling with Interactive Navigation Buttons",
        "module": "Tabs",
        "priority": 44,
        "status": "ready",
        "dependencies": [],
        "summary": "Smooth horizontal scrolling for tab headers with left/right navigation arrow buttons when tabs exceed viewport width.",
        "target_files": [
            "src/Vek/Components/Tabs/VekTabs.razor",
            "src/Vek/Components/Tabs/VekTabs.razor.cs",
            "src/Vek/Components/Tabs/TabsScrollMode.cs",
            "src/Vek/Styles/components/_tabs.scss"
        ],
        "requirements": [
            "ScrollMode enum: Auto, NavButtons, None.",
            "Left and right scroll buttons automatically shown when headers overflow.",
            "Mouse wheel horizontal scrolling over tab header strip.",
            "Smooth animated scroll into view when switching active tab."
        ]
    },
    {
        "id": "045",
        "slug": "tabs-closable-tabs-and-lifecycle-events",
        "title": "Tabs Dynamic Close Button and Tab Removal Lifecycle",
        "module": "Tabs",
        "priority": 45,
        "status": "ready",
        "dependencies": [],
        "summary": "Dynamic tab close buttons on tab headers with cancellable closing events and automatic adjacent tab activation.",
        "target_files": [
            "src/Vek/Components/Tabs/VekTabPanel.razor",
            "src/Vek/Components/Tabs/VekTabPanel.razor.cs",
            "src/Vek/Components/Tabs/VekTabs.razor.cs",
            "src/Vek/Components/Tabs/TabClosingEventArgs.cs"
        ],
        "requirements": [
            "CloseButtonDisplayMode enum: Never, Always, OnHover, ActiveTabOnly.",
            "OnTabClosing cancellable EventCallback (allows prompting to save unsaved changes).",
            "OnTabClosed EventCallback emitted when tab is removed.",
            "Auto-selection of next logical tab when current active tab is closed."
        ]
    },
    {
        "id": "046",
        "slug": "tabs-lazy-render-and-caching-modes",
        "title": "Tabs Render Modes: Active Tab Lazily with DOM Caching",
        "module": "Tabs",
        "priority": 46,
        "status": "ready",
        "dependencies": [],
        "summary": "Configurable tab rendering modes: ActiveTabLazily creates tab panels on demand and preserves their state in DOM across tab switches.",
        "target_files": [
            "src/Vek/Components/Tabs/TabsRenderMode.cs",
            "src/Vek/Components/Tabs/VekTabs.razor.cs",
            "src/Vek/Components/Tabs/VekTabPanel.razor"
        ],
        "requirements": [
            "RenderMode enum: AllTabs, ActiveTab, ActiveTabLazily.",
            "ActiveTabLazily renders tabs only upon first activation, avoiding heavy initial DOM load.",
            "Inactive activated tabs are hidden via CSS to preserve scroll position, inputs, and form states."
        ]
    },
    {
        "id": "047",
        "slug": "tabs-multi-orientation-and-positions",
        "title": "Tabs Vertical Orientation and Position (Top, Bottom, Left, Right)",
        "module": "Tabs",
        "priority": 47,
        "status": "ready",
        "dependencies": [],
        "summary": "Multi-orientation tab strip positioning with vertical layout support for Left and Right alignments and keyboard navigation.",
        "target_files": [
            "src/Vek/Components/Tabs/VekTabs.razor",
            "src/Vek/Components/Tabs/VekTabs.razor.cs",
            "src/Vek/Components/Tabs/TabPosition.cs",
            "src/Vek/Styles/components/_tabs.scss"
        ],
        "requirements": [
            "TabPosition enum: Top, Bottom, Left, Right.",
            "Vertical tab strip layout for Left and Right alignments.",
            "Active indicator alignment matching position.",
            "Keyboard ArrowUp / ArrowDown navigation when in vertical orientation."
        ]
    },
    {
        "id": "048",
        "slug": "tabs-drag-and-drop-reorder",
        "title": "Tabs Interactive Drag-and-Drop Tab Reordering",
        "module": "Tabs",
        "priority": 48,
        "status": "backlog",
        "dependencies": ["044"],
        "summary": "Drag-and-drop tab header reordering with smooth animations, drop indicators, and reorder callback events.",
        "target_files": [
            "src/Vek/Components/Tabs/VekTabs.razor",
            "src/Vek/Components/Tabs/VekTabs.razor.cs",
            "src/Vek/Components/Tabs/TabReorderedEventArgs.cs"
        ],
        "requirements": [
            "AllowTabReorder boolean parameter.",
            "Visual drop position indicator line between tab headers.",
            "OnTabReordered EventCallback with source index, target index, and panel reference.",
            "Touch device drag support."
        ]
    },
    {
        "id": "049",
        "slug": "tabs-badge-and-custom-header-template",
        "title": "Tabs Custom Header Template, Status Badges, and Icon Slots",
        "module": "Tabs",
        "priority": 49,
        "status": "ready",
        "dependencies": [],
        "summary": "Rich tab headers with custom template slots, status notification badges, and leading/trailing icon properties.",
        "target_files": [
            "src/Vek/Components/Tabs/VekTabPanel.razor",
            "src/Vek/Components/Tabs/VekTabPanel.razor.cs",
            "src/Vek/Styles/components/_tabs.scss"
        ],
        "requirements": [
            "HeaderTemplate RenderFragment slot per VekTabPanel.",
            "BadgeContent and BadgeColor parameters directly on VekTabPanel.",
            "Icon and IconPosition (Start, End) parameters on tab panels."
        ]
    },
    {
        "id": "050",
        "slug": "accordion-expand-modes-and-animation",
        "title": "Accordion Expand Modes (Single, Multiple, SingleOrNone) and Animation",
        "module": "ExpansionPanel",
        "priority": 50,
        "status": "ready",
        "dependencies": [],
        "summary": "ExpansionPanels / Accordion expand mode management (Single, Multiple, SingleOrNone) with smooth CSS height animations.",
        "target_files": [
            "src/Vek/Components/ExpansionPanel/VekExpansionPanels.razor",
            "src/Vek/Components/ExpansionPanel/VekExpansionPanels.razor.cs",
            "src/Vek/Components/ExpansionPanel/AccordionExpandMode.cs"
        ],
        "requirements": [
            "ExpandMode enum: Single, Multiple, SingleOrNone.",
            "Smooth height animation transitions during expand and collapse.",
            "ExpandAllAsync() and CollapseAllAsync() public methods.",
            "Custom expand/collapse icon position (Start or End)."
        ]
    },
    {
        "id": "051",
        "slug": "context-menu-target-selector-and-triggers",
        "title": "ContextMenu Target Selector Binding, Long-Press Touch, and Submenus",
        "module": "Menu",
        "priority": 51,
        "status": "ready",
        "dependencies": [],
        "summary": "Context menu component that binds to any DOM target selector or component reference, supporting right-click, touch long-press, and nested submenus.",
        "target_files": [
            "src/Vek/Components/Menu/VekContextMenu.razor",
            "src/Vek/Components/Menu/VekContextMenu.razor.cs"
        ],
        "requirements": [
            "Target selector string or ElementReference binding.",
            "Triggers on right-click contextmenu event and mobile long-press.",
            "Multi-level hierarchical submenu flyouts.",
            "Full keyboard accessibility (Arrow keys, Escape to close, Enter to invoke)."
        ]
    },
    {
        "id": "052",
        "slug": "toolbar-adaptive-collapse-and-overflow-menu",
        "title": "Toolbar Adaptive Layout with Overflow Dropdown Menu",
        "module": "ToolBar",
        "priority": 52,
        "status": "ready",
        "dependencies": [],
        "summary": "Adaptive responsive toolbar that automatically moves items overflowing the available container width into an overflow dropdown menu.",
        "target_files": [
            "src/Vek/Components/ToolBar/VekToolBar.razor",
            "src/Vek/Components/ToolBar/VekToolBar.razor.cs",
            "src/Vek/Styles/components/_toolbar.scss"
        ],
        "requirements": [
            "AdaptiveOverflow boolean parameter.",
            "Automatic calculation of container width vs total items width.",
            "Overflow button (...) opening popover containing hidden actions.",
            "Item CollapsePriority parameter controlling which items collapse first."
        ]
    }
]


def generate_spec_content(feat):
    status_title = feat["status"].title()
    pr_line = f"- **Pull Request**: {feat['pr_url']}\n" if feat.get("pr_url") else ""
    completed_line = f"- **Completed At**: {feat['completed_at']}\n" if feat.get("completed_at") else ""
    deps_line = f"- **Dependencies**: {json.dumps(feat.get('dependencies', []))}\n"

    target_files_str = "\n".join(f"  - `{f}`" for f in feat.get("target_files", []))
    reqs_str = "\n".join(f"  - {r}" for r in feat.get("requirements", []))

    content = f"""# Migration Specification: {feat['id']} - {feat['title']}

## 1. Metadata
- **Feature ID**: {feat['id']}
- **Feature Slug**: {feat['slug']}
- **Module**: {feat['module']}
- **Status**: {status_title}
- **Priority**: {feat['priority']}
{deps_line}{pr_line}{completed_line}
---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: {feat['summary']}
- **Functional Requirements**:
{reqs_str}
- **Observable Behavior**:
  - Seamlessly integrates with native Vek component lifecycle and ParameterState<T> framework.
  - Zero redundant component allocations per render cycle.
  - Full W3C ARIA accessibility and keyboard navigation compliance.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Files**:
{target_files_str}
- **State Management**:
  - Parameters declared as auto-properties annotated with `[Parameter, ParameterState]`.
  - Registered in constructor scope using `CreateRegisterScope()` and managed via `ParameterState<T>`.
- **Subfolder Discipline**:
  - Components and support models placed strictly into designated functional directories under `src/Vek/Components/{feat['module']}/`.
- **Documentation Example**:
  - Every feature must include a working interactive example in `src/Vek.Docs/Pages/Components/{feat['module']}/Examples/` referenced on the component docs page via `Code="@nameof(...)"`.
- **Styling**:
  - Zero hardcoded colors; use Vek design tokens and CSS variables composed via `CssBuilder` and `StyleBuilder`.

---

## 4. Verification & Acceptance Criteria
- **Build**: Compiles cleanly under multi-targeting (`net8.0`, `net9.0`, `net10.0`, `net11.0`) with 0 warnings (`TreatWarningsAsErrors=true`).
- **Testing**: Dedicated bUnit & unit tests verifying happy path, boundary edge cases, and disabled states.
- **Documentation Build**: `dotnet build src/Vek.Docs/Vek.Docs.csproj` succeeds with 0 errors.
- **Formatting**: Passes `dotnet format whitespace` validation.
- **Clean-Room Anonymity**: Absolute zero mention of proprietary vendor names or commercial brand identifiers in code, commits, or documentation.
- **Pull Request**: Fully documented pull request with verification summary and example usage.
"""
    return content


def main():
    if not BACKLOG_PATH.exists():
        print("Backlog path does not exist!")
        return

    with open(BACKLOG_PATH, "r", encoding="utf-8") as f:
        backlog_data = json.load(f)

    existing_ids = {f["id"] for f in backlog_data["features"]}
    print(f"Existing features in backlog: {len(existing_ids)}")

    print(f"\nWriting new feature specifications in {SPECS_DIR}...")
    for feat in NEW_FEATURES:
        filename = f"{feat['id']}-{feat['slug']}.md"
        spec_path = SPECS_DIR / filename
        spec_rel = f"specifications/{filename}"

        # Write specification markdown
        content = generate_spec_content(feat)
        spec_path.write_text(content, encoding="utf-8")
        print(f"  [+] {spec_path.name}")

        # Add or update in backlog
        b_entry = {
            "id": feat["id"],
            "slug": feat["slug"],
            "title": feat["title"],
            "module": feat["module"],
            "priority": feat["priority"],
            "status": feat["status"],
            "specification": spec_rel,
            "dependencies": feat.get("dependencies", [])
        }

        if feat["id"] not in existing_ids:
            backlog_data["features"].append(b_entry)
            existing_ids.add(feat["id"])
        else:
            # Update existing
            for idx, item in enumerate(backlog_data["features"]):
                if item["id"] == feat["id"]:
                    backlog_data["features"][idx] = b_entry
                    break

    # Sort backlog features by ID
    backlog_data["features"].sort(key=lambda x: int(x.get("id", 999)))

    print(f"\nWriting updated backlog with {len(backlog_data['features'])} total features to {BACKLOG_PATH}...")
    BACKLOG_PATH.write_text(json.dumps(backlog_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print("Multi-component feature expansion successfully completed!")


if __name__ == "__main__":
    main()
