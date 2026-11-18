#!/usr/bin/env python3
"""
Populates the full catalog of 100 Feature Specifications for the Vek Autonomous Migration Pipeline.
Covers DataGrid, Button, TextBox/Input, Tabs, DatePicker/TimePicker, TreeView, Dialog,
FileUpload, Slider/Rating, SplitPanel/Card/Drawer, Notifications, and Stepper/Pagination/ColorPicker.
Strict Clean-Room standard: 0 vendor brand mentions across all generated files.
"""

import json
from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parents[2]
MIGRATION_DIR = ROOT_DIR / "migration"
SPECS_DIR = MIGRATION_DIR / "specifications"
BACKLOG_PATH = MIGRATION_DIR / "backlog.json"

SPECS_DIR.mkdir(parents=True, exist_ok=True)

# Features 053 to 100
FEATURES_053_TO_100 = [
    # --- DATE & TIME SUITE (053 - 058) ---
    {
        "id": "053",
        "slug": "datepicker-disabled-dates-and-custom-rendering",
        "title": "DatePicker Disabled Date Predicates and Custom Day Cell Templates",
        "module": "DatePicker",
        "priority": 53,
        "status": "ready",
        "dependencies": [],
        "summary": "Custom predicate function to disable specific calendar dates (holidays, weekends) and RenderFragment template for day cell rendering.",
        "target_files": [
            "src/Vek/Components/DatePicker/VekDatePicker.razor",
            "src/Vek/Components/DatePicker/VekDatePicker.razor.cs",
            "src/Vek/Styles/components/_datepicker.scss"
        ],
        "requirements": [
            "IsDateDisabledFunc predicate parameter Func<DateTime, bool>.",
            "DayCellTemplate RenderFragment slot providing DatePickerDayContext.",
            "Disabled dates visually grayed out with aria-disabled='true' and click interception.",
            "Keyboard navigation skips disabled dates smoothly."
        ]
    },
    {
        "id": "054",
        "slug": "datepicker-range-picker-mode",
        "title": "DatePicker Dual Calendar Range Selection with Preset Ranges",
        "module": "DatePicker",
        "priority": 54,
        "status": "ready",
        "dependencies": [],
        "summary": "Dual-month calendar range picker with live hover highlight between start and end dates and quick-preset ranges (Today, This Week, Last 30 Days).",
        "target_files": [
            "src/Vek/Components/DatePicker/VekDateRangePicker.razor",
            "src/Vek/Components/DatePicker/VekDateRangePicker.razor.cs",
            "src/Vek/Components/DatePicker/DateRangePresets.cs"
        ],
        "requirements": [
            "DateRange two-way bindable property (Start, End).",
            "Visual continuous highlight range spanning between start and end day cells.",
            "PresetRanges collection (e.g. 'Last 7 Days', 'This Month', 'Year to Date').",
            "Auto-swap if end date is clicked before start date."
        ]
    },
    {
        "id": "055",
        "slug": "datepicker-inline-calendar-mode",
        "title": "DatePicker Embedded Inline Calendar Display (Non-Popup Mode)",
        "module": "DatePicker",
        "priority": 55,
        "status": "ready",
        "dependencies": [],
        "summary": "Inline permanent calendar widget that renders directly in page layout without dropdown popovers.",
        "target_files": [
            "src/Vek/Components/DatePicker/VekCalendar.razor",
            "src/Vek/Components/DatePicker/VekCalendar.razor.cs"
        ],
        "requirements": [
            "PickerVariant enum: Popup, Inline.",
            "Direct calendar view rendering inside layout without popover overlay.",
            "Full month/year header navigation and keyboard accessibility.",
            "Multiple selection and week number column display options."
        ]
    },
    {
        "id": "056",
        "slug": "timepicker-time-intervals-and-12-24-format",
        "title": "TimePicker Configurable Minute Intervals, Seconds, and 12/24-Hour Formats",
        "module": "TimePicker",
        "priority": 56,
        "status": "ready",
        "dependencies": [],
        "summary": "TimePicker with configurable step intervals (15, 30 min), optional seconds column, and AM/PM 12-hour or 24-hour display format.",
        "target_files": [
            "src/Vek/Components/TimePicker/VekTimePicker.razor",
            "src/Vek/Components/TimePicker/VekTimePicker.razor.cs",
            "src/Vek/Components/TimePicker/TimeEditFormat.cs"
        ],
        "requirements": [
            "MinuteInterval parameter (1, 5, 10, 15, 30).",
            "ShowSeconds boolean parameter adding a third seconds column.",
            "TimeFormat enum: TwelveHour (AM/PM) or TwentyFourHour.",
            "Scroll list column snapping with smooth touch scrolling."
        ]
    },
    {
        "id": "057",
        "slug": "timepicker-analog-clock-face-selector",
        "title": "TimePicker Interactive Radial Clock Face and Touch Hand Selector",
        "module": "TimePicker",
        "priority": 57,
        "status": "ready",
        "dependencies": [],
        "summary": "Analog circular clock face selector with rotating hand animation for interactive hour and minute picking on touch and desktop devices.",
        "target_files": [
            "src/Vek/Components/TimePicker/VekClockFace.razor",
            "src/Vek/Components/TimePicker/VekClockFace.razor.cs",
            "src/Vek/Styles/components/_timepicker.scss"
        ],
        "requirements": [
            "Radial SVG clock dial with touch/mouse dragging hand.",
            "Auto-transition from hours selection to minutes selection upon pick.",
            "Inner ring for 24-hour formats (13-24) and outer ring for 1-12.",
            "Accessible keyboard arrow rotation."
        ]
    },
    {
        "id": "058",
        "slug": "datepicker-multi-view-decade-century-navigation",
        "title": "DatePicker Multi-Level Decade, Century, and Year Fast-Jump Navigation",
        "module": "DatePicker",
        "priority": 58,
        "status": "ready",
        "dependencies": [],
        "summary": "Hierarchical calendar navigation enabling users to zoom out from month view into year grid, decade view, and century fast-jump.",
        "target_files": [
            "src/Vek/Components/DatePicker/VekDatePicker.razor.cs",
            "src/Vek/Components/DatePicker/DatePickerViewMode.cs"
        ],
        "requirements": [
            "ViewMode enum: Days, Months, Years, Decades.",
            "Clicking month header switches to 12-month year picker.",
            "Clicking year header switches to 12-year decade picker.",
            "Smooth zoom animation between calendar zoom levels."
        ]
    },

    # --- TREEVIEW & HIERARCHICAL SUITE (059 - 065) ---
    {
        "id": "059",
        "slug": "treeview-checkbox-tri-state-selection",
        "title": "TreeView Recursive Tri-State Hierarchical Checkbox Selection",
        "module": "TreeView",
        "priority": 59,
        "status": "ready",
        "dependencies": [],
        "summary": "Tri-state recursive checkbox selection where checking a parent checks all descendants, and partially checked branches show indeterminate state.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeView.razor",
            "src/Vek/Components/TreeView/VekTreeView.razor.cs",
            "src/Vek/Components/TreeView/VekTreeItem.razor"
        ],
        "requirements": [
            "Checkboxes boolean parameter.",
            "TriStateSelection boolean enabling recursive parent/child synchronization.",
            "CheckedValues two-way binding with generic keys.",
            "Indeterminate visual state on partially selected branch nodes."
        ]
    },
    {
        "id": "060",
        "slug": "treeview-node-drag-and-drop-reorder",
        "title": "TreeView Interactive Node Drag-and-Drop Reordering and Reparenting",
        "module": "TreeView",
        "priority": 60,
        "status": "ready",
        "dependencies": [],
        "summary": "Drag-and-drop tree node management enabling users to reorder sibling nodes or move a node into another branch as a child.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeView.razor",
            "src/Vek/Components/TreeView/TreeNodeDragManager.cs",
            "src/Vek/Components/TreeView/TreeNodeDropEventArgs.cs"
        ],
        "requirements": [
            "AllowDragDrop boolean property.",
            "Visual drop position cue: Before, After, or Into (as child).",
            "OnNodeDrop EventCallback with source and target node payloads.",
            "Drop validation predicate CanDropFunc."
        ]
    },
    {
        "id": "061",
        "slug": "treeview-lazy-load-child-nodes-async",
        "title": "TreeView On-Demand Asynchronous Child Node Fetching on Expand",
        "module": "TreeView",
        "priority": 61,
        "status": "ready",
        "dependencies": [],
        "summary": "On-demand async data loading for deep hierarchy trees, fetching child nodes from API or database only when the expand arrow is clicked.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeView.razor.cs",
            "src/Vek/Components/TreeView/ITreeNodeProvider.cs"
        ],
        "requirements": [
            "LoadChildrenAsync delegate Func<TNode, Task<IEnumerable<TNode>>>.",
            "Inline loading spinner on node while children are being fetched.",
            "HasChildren predicate determining if expand chevron should be rendered.",
            "Caching loaded nodes to avoid refetching on collapse/re-expand."
        ]
    },
    {
        "id": "062",
        "slug": "treeview-search-filter-and-auto-expand",
        "title": "TreeView Live Search Filtering with Auto-Expansion of Matching Ancestors",
        "module": "TreeView",
        "priority": 62,
        "status": "ready",
        "dependencies": [],
        "summary": "Tree search filter input filtering node labels in real-time, highlighting matching text, and automatically expanding ancestor branches.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeView.razor",
            "src/Vek/Components/TreeView/VekTreeView.razor.cs"
        ],
        "requirements": [
            "FilterText string parameter with two-way binding.",
            "Auto-expands parent branches containing matching leaf nodes.",
            "Hides nodes that do not match filter criteria.",
            "Text match highlighting with <mark class='vek-tree-highlight'>."
        ]
    },
    {
        "id": "063",
        "slug": "treeview-node-inline-editing-and-renaming",
        "title": "TreeView In-Place Node Text Editing and Keyboard Renaming (F2/Enter)",
        "module": "TreeView",
        "priority": 63,
        "status": "ready",
        "dependencies": [],
        "summary": "In-place inline renaming for tree nodes triggered by F2 shortcut or double-click, with validation and save/cancel callbacks.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeItem.razor",
            "src/Vek/Components/TreeView/TreeNodeRenameEventArgs.cs"
        ],
        "requirements": [
            "AllowNodeRenaming boolean parameter.",
            "F2 key enters inline text editing on focused node.",
            "Enter commits rename; Escape cancels editing.",
            "OnNodeRenamed EventCallback with validation support."
        ]
    },
    {
        "id": "064",
        "slug": "treeview-custom-node-templates-and-badges",
        "title": "TreeView Custom Node Templates, Action Buttons, and Status Badges",
        "module": "TreeView",
        "priority": 64,
        "status": "ready",
        "dependencies": [],
        "summary": "Rich RenderFragment templates for tree node content, enabling custom action buttons (add, delete, edit) and count badges on hover.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeView.razor",
            "src/Vek/Components/TreeView/TreeNodeTemplateContext.cs"
        ],
        "requirements": [
            "NodeTemplate RenderFragment slot providing TreeNodeContext.",
            "Hover action buttons revealed on mouse-over without breaking selection.",
            "Custom leading and trailing icon slots.",
            "Zero extra component allocations."
        ]
    },
    {
        "id": "065",
        "slug": "treeview-multi-column-tree-list-layout",
        "title": "TreeView Multi-Column TreeList Layout with Resizable Columns",
        "module": "TreeView",
        "priority": 65,
        "status": "ready",
        "dependencies": [],
        "summary": "Multi-column TreeList combining hierarchical tree expand/collapse in the first column with tabular data columns and resizable headers.",
        "target_files": [
            "src/Vek/Components/TreeView/VekTreeList.razor",
            "src/Vek/Components/TreeView/VekTreeListColumn.razor",
            "src/Vek/Styles/components/_treeview.scss"
        ],
        "requirements": [
            "VekTreeList component with Column definitions.",
            "Hierarchical indent and chevron in primary tree column.",
            "Tabular cell values aligned across columns.",
            "Header sorting and column resizing."
        ]
    },

    # --- DIALOG & MODAL WINDOW SUITE (066 - 071) ---
    {
        "id": "066",
        "slug": "dialog-draggable-and-resizable-modals",
        "title": "Dialog Draggable Header, Viewport Clamping, and Resizable Edges",
        "module": "Dialog",
        "priority": 66,
        "status": "ready",
        "dependencies": [],
        "summary": "Draggable modal dialog header enabling free repositioning, boundary clamping within viewport, and 8-direction edge/corner resizing.",
        "target_files": [
            "src/Vek/Components/Dialog/VekDialog.razor",
            "src/Vek/Components/Dialog/VekDialog.razor.cs",
            "src/Vek/Styles/components/_dialog.scss"
        ],
        "requirements": [
            "Draggable boolean parameter allowing dragging by header bar.",
            "Resizable boolean parameter adding resizable grab handles.",
            "Viewport boundary clamp preventing dialog from getting dragged offscreen.",
            "Preserves position on state re-render."
        ]
    },
    {
        "id": "067",
        "slug": "dialog-modal-stacking-and-z-index-manager",
        "title": "Dialog Multi-Modal Cascading Stacking and Focus Isolation",
        "module": "Dialog",
        "priority": 67,
        "status": "ready",
        "dependencies": [],
        "summary": "Multi-dialog stacking manager ensuring nested modals increment z-index correctly, manage backdrops, and preserve focus order.",
        "target_files": [
            "src/Vek/Components/Dialog/DialogStackManager.cs",
            "src/Vek/Components/Dialog/IDialogService.cs"
        ],
        "requirements": [
            "Automatic z-index stacking order for multiple concurrent dialogs.",
            "Active dialog receives top z-index upon click or focus.",
            "Backdrop management preventing backdrop duplication.",
            "Escape key closes only the topmost active dialog."
        ]
    },
    {
        "id": "068",
        "slug": "dialog-fullscreen-toggle-and-maximize",
        "title": "Dialog Header Maximize / Restore Toggle and Fullscreen Mode",
        "module": "Dialog",
        "priority": 68,
        "status": "ready",
        "dependencies": [],
        "summary": "Window management buttons in dialog header for Maximize / Restore to full viewport dimensions, and Fullscreen toggle.",
        "target_files": [
            "src/Vek/Components/Dialog/VekDialog.razor",
            "src/Vek/Components/Dialog/VekDialog.razor.cs"
        ],
        "requirements": [
            "ShowMaximizeButton boolean parameter in header.",
            "Maximize / Restore button toggling 100vw/100vh fullscreen layout.",
            "Double-clicking header bar toggles maximize.",
            "Restores previous width, height, and coordinates on restore."
        ]
    },
    {
        "id": "069",
        "slug": "messagebox-fluent-confirm-prompt-api",
        "title": "MessageBox Fluent Async ShowConfirmAsync, ShowPromptAsync, and Custom Buttons",
        "module": "MessageBox",
        "priority": 69,
        "status": "ready",
        "dependencies": [],
        "summary": "Fluent C# API for displaying alert, confirm, and text prompt message boxes asynchronously awaiting user response without markup boilerplate.",
        "target_files": [
            "src/Vek/Components/MessageBox/VekMessageBox.razor",
            "src/Vek/Components/MessageBox/MessageBoxService.cs",
            "src/Vek/Components/MessageBox/MessageBoxOptions.cs"
        ],
        "requirements": [
            "ShowConfirmAsync(title, message) returning Task<bool>.",
            "ShowPromptAsync(title, message, defaultValue) returning Task<string?>.",
            "Custom button definitions with destructive styling and icon support.",
            "Return focus to trigger element upon closing."
        ]
    },
    {
        "id": "070",
        "slug": "dialog-animation-transitions-and-drawer-sheet",
        "title": "Dialog Configurable Entrance Animations and Slide-In Sheet Position",
        "module": "Dialog",
        "priority": 70,
        "status": "ready",
        "dependencies": [],
        "summary": "Configurable dialog entrance animations (Fade, Zoom, SlideUp, Bounce) and bottom-sheet mode for responsive mobile devices.",
        "target_files": [
            "src/Vek/Components/Dialog/DialogAnimation.cs",
            "src/Vek/Styles/components/_dialog.scss"
        ],
        "requirements": [
            "Animation enum: Fade, Zoom, SlideUp, SlideDown, None.",
            "BottomSheetOnMobile boolean parameter converting dialog to bottom sheet on small screens.",
            "CSS transitions utilizing Vek design tokens.",
            "Smooth backdrop fade-in and fade-out."
        ]
    },
    {
        "id": "071",
        "slug": "dialog-focus-trap-and-escape-key-handling",
        "title": "Dialog W3C Modal Focus Trap and Escape Key Interception",
        "module": "Dialog",
        "priority": 71,
        "status": "ready",
        "dependencies": [],
        "summary": "Strict W3C modal accessibility with roving focus trap keeping Tab navigation inside the dialog and customizable Escape key behavior.",
        "target_files": [
            "src/Vek/Components/Dialog/VekDialog.razor.cs",
            "src/Vek/Components/FocusTrap/VekFocusTrap.razor"
        ],
        "requirements": [
            "Automatic focus trap constraining Tab / Shift+Tab within active modal.",
            "CloseOnEscapeKey boolean parameter.",
            "CloseOnBackdropClick boolean parameter.",
            "Restores focus to trigger button upon close."
        ]
    },

    # --- FILE UPLOAD & STORAGE SUITE (072 - 076) ---
    {
        "id": "072",
        "slug": "fileupload-drag-and-drop-zone",
        "title": "FileUpload Full-Surface Drag-and-Drop Area with Hover Visual Cue",
        "module": "FileUpload",
        "priority": 72,
        "status": "ready",
        "dependencies": [],
        "summary": "Full-surface drag-and-drop zone with animated dashed borders, active hover visual state, and multi-file drag detection.",
        "target_files": [
            "src/Vek/Components/FileUpload/VekFileUpload.razor",
            "src/Vek/Components/FileUpload/VekFileUpload.razor.cs",
            "src/Vek/Styles/components/_fileupload.scss"
        ],
        "requirements": [
            "DragAndDrop boolean parameter enabling full-zone dropping.",
            "Visual drag-over highlighting with pulse animation.",
            "Automatic extraction of dropped files via HTML5 Drag and Drop API.",
            "Graceful fallback to standard file browser button."
        ]
    },
    {
        "id": "073",
        "slug": "fileupload-chunked-and-resumable-upload",
        "title": "FileUpload Client-Side Chunking for Large Files and Progress Tracking",
        "module": "FileUpload",
        "priority": 73,
        "status": "ready",
        "dependencies": [],
        "summary": "Client-side slicing of large files into chunks for reliable streaming upload, network retry, and accurate percent progress tracking.",
        "target_files": [
            "src/Vek/Components/FileUpload/FileUploadChunkManager.cs",
            "src/Vek/Components/FileUpload/FileUploadProgressEventArgs.cs"
        ],
        "requirements": [
            "ChunkSizeInBytes parameter (default: 2MB).",
            "Slices files via JavaScript Blob.slice interop for memory efficiency.",
            "Real-time upload progress reporting (bytes uploaded, percent, speed).",
            "Automatic retry on transient connection drops."
        ]
    },
    {
        "id": "074",
        "slug": "fileupload-image-preview-and-client-validation",
        "title": "FileUpload Instant Image Thumbnail Preview and File Type/Size Client Validation",
        "module": "FileUpload",
        "priority": 74,
        "status": "ready",
        "dependencies": [],
        "summary": "Instant client-side image thumbnail preview generated via object URL before upload, with client-side extension, MIME, and size validation.",
        "target_files": [
            "src/Vek/Components/FileUpload/VekFileUpload.razor",
            "src/Vek/Components/FileUpload/FileUploadValidator.cs"
        ],
        "requirements": [
            "Instant image thumbnail preview using URL.createObjectURL.",
            "AllowedExtensions and AllowedMimeTypes validation arrays.",
            "MaxFileSize parameter with localized error messages.",
            "Validation errors displayed per file item."
        ]
    },
    {
        "id": "075",
        "slug": "fileupload-multi-file-queue-and-cancel",
        "title": "FileUpload Multi-File Queue Management with Individual Retry and Cancel",
        "module": "FileUpload",
        "priority": 75,
        "status": "ready",
        "dependencies": [],
        "summary": "Interactive file queue table showing file names, sizes, individual progress bars, cancel buttons, and retry actions.",
        "target_files": [
            "src/Vek/Components/FileUpload/VekFileUpload.razor",
            "src/Vek/Components/FileUpload/FileUploadItem.cs"
        ],
        "requirements": [
            "Interactive file queue showing upload status (Queued, Uploading, Success, Error).",
            "CancelUpload(fileId) method aborting active request.",
            "RetryUpload(fileId) retrying failed item.",
            "ClearCompleted() method cleaning up queue."
        ]
    },
    {
        "id": "076",
        "slug": "dropzone-custom-slot-templates",
        "title": "DropZone Customizable Empty State, Active Dragging, and Uploading Templates",
        "module": "DropZone",
        "priority": 76,
        "status": "ready",
        "dependencies": [],
        "summary": "RenderFragment slot templates for empty state prompt, active dragging overlay, and custom uploading list.",
        "target_files": [
            "src/Vek/Components/DropZone/VekDropZone.razor",
            "src/Vek/Components/DropZone/VekDropZone.razor.cs"
        ],
        "requirements": [
            "EmptyTemplate RenderFragment for idle state.",
            "DragOverTemplate RenderFragment for active drag-hover state.",
            "ItemTemplate RenderFragment for custom item representation.",
            "Keyboard focus and accessible enter-key trigger."
        ]
    },

    # --- SLIDERS, RANGE & RATING SUITE (077 - 082) ---
    {
        "id": "077",
        "slug": "slider-dual-thumb-range-selection",
        "title": "Slider Dual-Thumb Range Selection with Lower and Upper Value Binding",
        "module": "Slider",
        "priority": 77,
        "status": "ready",
        "dependencies": [],
        "summary": "Range slider with two draggable thumbs selecting an interval (min and max values) with collision prevention and dual value binding.",
        "target_files": [
            "src/Vek/Components/Slider/VekRangeSlider.razor",
            "src/Vek/Components/Slider/VekRangeSlider.razor.cs",
            "src/Vek/Components/Slider/SliderRange.cs"
        ],
        "requirements": [
            "ValueStart and ValueEnd two-way binding.",
            "Selected range track highlighted between the two thumbs.",
            "MinDistance parameter preventing thumbs from crossing each other.",
            "Independent keyboard arrow navigation per active thumb."
        ]
    },
    {
        "id": "078",
        "slug": "slider-tick-marks-and-labeled-steps",
        "title": "Slider Discrete Step Snapping, Custom Ticks, and Scale Value Labels",
        "module": "Slider",
        "priority": 78,
        "status": "ready",
        "dependencies": [],
        "summary": "Slider with discrete step snapping, visible tick marks, and custom text or numeric labels along the track.",
        "target_files": [
            "src/Vek/Components/Slider/VekSlider.razor",
            "src/Vek/Components/Slider/VekSlider.razor.cs",
            "src/Vek/Styles/components/_slider.scss"
        ],
        "requirements": [
            "ShowTicks boolean parameter.",
            "TickMarks collection or TickInterval parameter.",
            "Custom label formatter Func<double, string> for scale marks.",
            "Magnetic step snapping to discrete tick values."
        ]
    },
    {
        "id": "079",
        "slug": "slider-vertical-orientation-and-touch-track",
        "title": "Slider Vertical Layout Orientation with Touch-Accurate Scrubbing",
        "module": "Slider",
        "priority": 79,
        "status": "ready",
        "dependencies": [],
        "summary": "Vertical slider orientation for audio mixer, volume, and vertical parameter controls with touch dragging support.",
        "target_files": [
            "src/Vek/Components/Slider/VekSlider.razor",
            "src/Vek/Styles/components/_slider.scss"
        ],
        "requirements": [
            "Vertical boolean parameter.",
            "Height styling property controlling vertical track length.",
            "Inverted boolean parameter (top-to-bottom or bottom-to-top).",
            "Keyboard ArrowUp/ArrowDown controls."
        ]
    },
    {
        "id": "080",
        "slug": "slider-tooltip-value-formatting",
        "title": "Slider Floating Value Tooltip with Custom String and Currency Formatter",
        "module": "Slider",
        "priority": 80,
        "status": "ready",
        "dependencies": [],
        "summary": "Floating tooltip above slider thumb displaying live formatted value during dragging, with custom string formatting and currency options.",
        "target_files": [
            "src/Vek/Components/Slider/VekSlider.razor",
            "src/Vek/Components/Slider/SliderTooltipMode.cs"
        ],
        "requirements": [
            "ValueLabelDisplay enum: Never, OnHover, Always.",
            "ValueLabelFormat string parameter (e.g. '{0}%', '{0:C}').",
            "Tooltip moves synchronously with thumb position.",
            "High-contrast tooltip styling with theme tokens."
        ]
    },
    {
        "id": "081",
        "slug": "rating-half-star-precision-and-hover-preview",
        "title": "Rating Fractional/Half-Star Precision and Live Hover Score Preview",
        "module": "Rating",
        "priority": 81,
        "status": "ready",
        "dependencies": [],
        "summary": "Rating component supporting half-star (0.5) increments, live hover preview of fractional score, and read-only mode.",
        "target_files": [
            "src/Vek/Components/Rating/VekRating.razor",
            "src/Vek/Components/Rating/VekRating.razor.cs",
            "src/Vek/Components/Rating/VekRatingItem.razor"
        ],
        "requirements": [
            "Precision enum: Full, Half.",
            "Value double parameter supporting 0.5 increments (e.g. 4.5 stars).",
            "Live hover score preview without committing value until clicked.",
            "ReadOnly and Disabled parameters."
        ]
    },
    {
        "id": "082",
        "slug": "rating-custom-icons-and-sentiment-emojis",
        "title": "Rating Custom SVG Icon Sets, Hearts, and Multi-Sentiment Emojis",
        "module": "Rating",
        "priority": 82,
        "status": "ready",
        "dependencies": [],
        "summary": "Configurable icon sets for rating control, including hearts, thumbs up/down, and sentiment emojis (angry to happy).",
        "target_files": [
            "src/Vek/Components/Rating/VekRating.razor",
            "src/Vek/Components/Rating/RatingIconType.cs"
        ],
        "requirements": [
            "FullIcon and EmptyIcon SVG string parameters.",
            "ItemHoverColor and ItemActiveColor parameters.",
            "ItemTemplate RenderFragment for custom emoji or animated icons.",
            "Clearable boolean parameter allowing resetting rating to 0."
        ]
    },

    # --- CARD, LAYOUT & SPLITTER SUITE (083 - 088) ---
    {
        "id": "083",
        "slug": "splitpanel-resizable-panes-and-collapse",
        "title": "SplitPanel Resizable Splitter Bar with Double-Click Pane Collapse",
        "module": "SplitPanel",
        "priority": 83,
        "status": "ready",
        "dependencies": [],
        "summary": "SplitPanel with draggable divider bar, minimum/maximum pane constraints, and double-click to collapse one pane.",
        "target_files": [
            "src/Vek/Components/SplitPanel/VekSplitPanel.razor",
            "src/Vek/Components/SplitPanel/VekSplitPanel.razor.cs",
            "src/Vek/Styles/components/_splitpanel.scss"
        ],
        "requirements": [
            "PaneSize two-way double parameter (percentage or pixels).",
            "MinSize and MaxSize constraints for Left/Top pane.",
            "Collapsible boolean parameter: double-clicking splitter collapses pane.",
            "Keyboard arrow navigation on focused splitter bar."
        ]
    },
    {
        "id": "084",
        "slug": "splitpanel-nested-horizontal-vertical-splits",
        "title": "SplitPanel Nested Multi-Directional Layout Panes (Horizontal in Vertical)",
        "module": "SplitPanel",
        "priority": 84,
        "status": "ready",
        "dependencies": ["083"],
        "summary": "Multi-pane layout docking system supporting horizontal split inside vertical split for complex dashboard workbenches.",
        "target_files": [
            "src/Vek/Components/SplitPanel/VekSplitPanel.razor",
            "src/Vek/Components/SplitPanel/SplitPanelOrientation.cs"
        ],
        "requirements": [
            "Orientation enum: Horizontal, Vertical.",
            "Seamless nesting of child SplitPanels without layout jitter.",
            "State persistence support for saving all pane sizes.",
            "Touch dragging support on mobile and tablet devices."
        ]
    },
    {
        "id": "085",
        "slug": "card-expandable-collapsible-content",
        "title": "Card Collapsible Body with Animated Toggle Action in Header",
        "module": "Card",
        "priority": 85,
        "status": "ready",
        "dependencies": [],
        "summary": "VekCard with built-in collapse/expand toggle in header, smooth height animation, and collapsed state binding.",
        "target_files": [
            "src/Vek/Components/Card/VekCard.razor",
            "src/Vek/Components/Card/VekCard.razor.cs",
            "src/Vek/Styles/components/_card.scss"
        ],
        "requirements": [
            "Collapsible boolean parameter.",
            "Expanded two-way boolean parameter with OnExpandedChanged event.",
            "Header toggle chevron button with aria-expanded attribute.",
            "Smooth CSS height transition without content overflow glitches."
        ]
    },
    {
        "id": "086",
        "slug": "card-header-actions-and-media-ratio",
        "title": "Card Aspect-Ratio Media Box, Header Context Menu, and Action Button Toolbar",
        "module": "Card",
        "priority": 86,
        "status": "ready",
        "dependencies": [],
        "summary": "Rich card header with trailing action buttons, contextual dropdown trigger, and 16:9 / 4:3 aspect-ratio media box.",
        "target_files": [
            "src/Vek/Components/Card/VekCardHeader.razor",
            "src/Vek/Components/Card/VekCardMedia.razor"
        ],
        "requirements": [
            "CardHeader Actions RenderFragment slot.",
            "AspectRatio parameter on VekCardMedia (16:9, 4:3, 1:1, Auto).",
            "Title, Subtitle, and Avatar slots in header.",
            "Zero extra component allocations."
        ]
    },
    {
        "id": "087",
        "slug": "drawer-docked-persistent-and-mini-variants",
        "title": "Drawer Mini / Rail Variant with Icon-Only Mode and Smooth Width Expansion",
        "module": "Drawer",
        "priority": 87,
        "status": "ready",
        "dependencies": [],
        "summary": "Navigation drawer with Mini Rail variant that collapses to icon-only strip and smoothly expands on hover or toggle.",
        "target_files": [
            "src/Vek/Components/Drawer/VekDrawer.razor",
            "src/Vek/Components/Drawer/VekDrawer.razor.cs",
            "src/Vek/Components/Drawer/DrawerVariant.cs",
            "src/Vek/Styles/components/_drawer.scss"
        ],
        "requirements": [
            "Variant enum: Temporary, Persistent, Mini.",
            "MiniWidth parameter (e.g. 56px) showing only icons.",
            "ExpandOnHover boolean parameter for mini drawer.",
            "Content area automatically offsets width when drawer is docked."
        ]
    },
    {
        "id": "088",
        "slug": "drawer-swipe-gesture-and-backdrop-dismiss",
        "title": "Drawer Mobile Edge-Swipe Touch Gestures and Backdrop Tap Dismiss",
        "module": "Drawer",
        "priority": 88,
        "status": "ready",
        "dependencies": [],
        "summary": "Mobile touch edge-swipe gestures to open and dismiss temporary navigation drawers, with backdrop tap dismiss.",
        "target_files": [
            "src/Vek/Components/Drawer/VekDrawer.razor",
            "src/Vek/Components/Drawer/DrawerSwipeHandler.cs"
        ],
        "requirements": [
            "Touch swipe gesture detection for left/right screen edges.",
            "Interactive dragging following finger before release.",
            "Backdrop tap dismisses temporary drawer.",
            "Escape key closes active drawer."
        ]
    },

    # --- NOTIFICATIONS, FEEDBACK & INDICATORS (089 - 094) ---
    {
        "id": "089",
        "slug": "snackbar-action-button-and-undo-callback",
        "title": "Snackbar Interactive Action Buttons with Async Undo Callback",
        "module": "Snackbar",
        "priority": 89,
        "status": "ready",
        "dependencies": [],
        "summary": "Toast notification with interactive action button (e.g. 'Undo', 'Retry') executing asynchronous callback before toast dismisses.",
        "target_files": [
            "src/Vek/Components/Snackbar/VekSnackbar.razor",
            "src/Vek/Components/Snackbar/SnackbarMessage.cs",
            "src/Vek/Components/Snackbar/ISnackbarService.cs"
        ],
        "requirements": [
            "ActionText string and ActionCallback Func<Task> parameters.",
            "Clicking action button pauses auto-dismiss and executes callback.",
            "CloseButton boolean parameter for manual dismissal.",
            "Severity color tokens (Success, Info, Warning, Error)."
        ]
    },
    {
        "id": "090",
        "slug": "snackbar-queue-stacking-and-auto-dismiss-timer",
        "title": "Snackbar Multi-Toast Queue Stacking, Visual Progress Countdown, and Pause on Hover",
        "module": "Snackbar",
        "priority": 90,
        "status": "ready",
        "dependencies": ["089"],
        "summary": "Multi-notification queue stacking (bottom-right, top-center), animated progress bar countdown, and timer pause on mouse hover.",
        "target_files": [
            "src/Vek/Components/Snackbar/VekSnackbarProvider.razor",
            "src/Vek/Components/Snackbar/SnackbarOptions.cs",
            "src/Vek/Styles/components/_snackbar.scss"
        ],
        "requirements": [
            "MaxDisplayedSnackbars parameter managing toast queue.",
            "Animated progress bar displaying remaining duration.",
            "Pauses dismiss timer while user hovers mouse over toast.",
            "Screen reader live region (aria-live='polite') announcements."
        ]
    },
    {
        "id": "091",
        "slug": "skeleton-bone-shapes-and-shimmer-animation",
        "title": "Skeleton Pre-Configured Shape Cards, Avatars, and Synchronized Shimmer",
        "module": "Skeleton",
        "priority": 91,
        "status": "ready",
        "dependencies": [],
        "summary": "Loading skeleton placeholders with pre-configured bone shapes (Circle, Text, Rectangle, Card) and synchronized wave shimmer.",
        "target_files": [
            "src/Vek/Components/Skeleton/VekSkeleton.razor",
            "src/Vek/Components/Skeleton/SkeletonShape.cs",
            "src/Vek/Styles/components/_skeleton.scss"
        ],
        "requirements": [
            "Shape enum: Text, Circle, Rectangle, Card.",
            "Animation enum: Shimmer, Pulse, None.",
            "Configurable width, height, and border-radius.",
            "Accessible aria-busy='true' and aria-label='Loading'."
        ]
    },
    {
        "id": "092",
        "slug": "progress-circular-and-linear-indeterminate-modes",
        "title": "Progress Circular and Linear Percentage Indicators with Animated Buffer Bar",
        "module": "Progress",
        "priority": 92,
        "status": "ready",
        "dependencies": [],
        "summary": "Circular and linear progress bars supporting determinate percentages, streaming indeterminate animations, and buffer progress bars.",
        "target_files": [
            "src/Vek/Components/Progress/VekProgressLinear.razor",
            "src/Vek/Components/Progress/VekProgressCircular.razor",
            "src/Vek/Styles/components/_progress.scss"
        ],
        "requirements": [
            "Indeterminate boolean parameter.",
            "Value double parameter (0 - 100) and BufferValue parameter.",
            "Custom size, stroke width, and color tokens.",
            "aria-valuenow, aria-valuemin, aria-valuemax attributes."
        ]
    },
    {
        "id": "093",
        "slug": "badge-status-dots-and-overlap-positions",
        "title": "Badge Status Indicator Dots, Max Value Truncation (99+), and Placement Anchors",
        "module": "Badge",
        "priority": 93,
        "status": "ready",
        "dependencies": [],
        "summary": "Standalone and wrapper badge component supporting status dots, numeric counters with automatic '99+' overflow capping, and custom placements.",
        "target_files": [
            "src/Vek/Components/Badge/VekBadge.razor",
            "src/Vek/Components/Badge/VekBadge.razor.cs",
            "src/Vek/Styles/components/_badge.scss"
        ],
        "requirements": [
            "Dot boolean parameter rendering compact status circle.",
            "MaxNumber parameter truncating counters (e.g. 150 -> '99+').",
            "Position enum: TopEnd, TopStart, BottomEnd, BottomStart.",
            "Visible boolean parameter allowing toggling without removing badge."
        ]
    },
    {
        "id": "094",
        "slug": "avatar-group-stack-and-overflow-counter",
        "title": "AvatarGroup Overlapping Facepile Stack with +N Excess Member Counter",
        "module": "Avatar",
        "priority": 94,
        "status": "ready",
        "dependencies": [],
        "summary": "AvatarGroup stacking multiple user avatars into an overlapping facepile, showing a '+N' counter badge for remaining users.",
        "target_files": [
            "src/Vek/Components/Avatar/VekAvatarGroup.razor",
            "src/Vek/Components/Avatar/VekAvatarGroup.razor.cs",
            "src/Vek/Styles/components/_avatar.scss"
        ],
        "requirements": [
            "Max parameter specifying maximum visible avatars.",
            "Automatic '+N' overflow avatar generated for extra users.",
            "Spacing parameter controlling overlap offset.",
            "Hover elevation raising active avatar above peers."
        ]
    },

    # --- NAVIGATION, STEPPER, PAGINATION, COLOR & HOTKEYS (095 - 100) ---
    {
        "id": "095",
        "slug": "stepper-vertical-and-horizontal-orientations",
        "title": "Stepper Horizontal and Vertical Orientation with Responsive Step Collapse",
        "module": "Stepper",
        "priority": 95,
        "status": "ready",
        "dependencies": [],
        "summary": "Multi-step wizard stepper supporting horizontal and vertical orientations with responsive collapse of step titles on narrow screens.",
        "target_files": [
            "src/Vek/Components/Stepper/VekStepper.razor",
            "src/Vek/Components/Stepper/VekStep.razor",
            "src/Vek/Styles/components/_stepper.scss"
        ],
        "requirements": [
            "Orientation enum: Horizontal, Vertical.",
            "ActiveIndex two-way binding with NextStepAsync() and PreviousStepAsync() API.",
            "Step icons, titles, and optional subtitles.",
            "Responsive icon-only mode when viewport width is restricted."
        ]
    },
    {
        "id": "096",
        "slug": "stepper-step-validation-and-cancellable-navigation",
        "title": "Stepper Per-Step Form Validation and Async Cancellable Navigation Guards",
        "module": "Stepper",
        "priority": 96,
        "status": "ready",
        "dependencies": ["095"],
        "summary": "Asynchronous validation guards on wizard steps preventing progression until the current step's form fields are valid.",
        "target_files": [
            "src/Vek/Components/Stepper/VekStep.razor.cs",
            "src/Vek/Components/Stepper/StepNavigationContext.cs"
        ],
        "requirements": [
            "BeforeStepChange cancellable EventCallback with validation logic.",
            "HasError boolean parameter displaying red error icon on step.",
            "Prevents jumping forward to incomplete steps in linear mode.",
            "Automatic validation trigger on Next button click."
        ]
    },
    {
        "id": "097",
        "slug": "stepper-linear-vs-non-linear-step-modes",
        "title": "Stepper Non-Linear Step Completion, Editable Steps, and Error State Badges",
        "module": "Stepper",
        "priority": 97,
        "status": "ready",
        "dependencies": ["095"],
        "summary": "Non-linear stepper mode allowing users to jump directly to any step, mark steps as completed out of order, or return to edit previous steps.",
        "target_files": [
            "src/Vek/Components/Stepper/VekStepper.razor.cs",
            "src/Vek/Components/Stepper/StepState.cs"
        ],
        "requirements": [
            "Linear boolean parameter (when false, any step can be clicked).",
            "CompletedSteps collection tracking finished steps.",
            "Editable boolean parameter on steps allowing revisit after completion.",
            "Checkmark icon rendered automatically on completed steps."
        ]
    },
    {
        "id": "098",
        "slug": "pagination-page-size-selector-and-quick-jump",
        "title": "Pagination Integrated Page Size Selector Dropdown and Direct Page Jump Input",
        "module": "Pagination",
        "priority": 98,
        "status": "ready",
        "dependencies": [],
        "summary": "Pagination control with integrated page size dropdown ('10, 25, 50, 100'), direct page number jump input, and total item summary text.",
        "target_files": [
            "src/Vek/Components/Pagination/VekPagination.razor",
            "src/Vek/Components/Pagination/VekPagination.razor.cs",
            "src/Vek/Styles/components/_pagination.scss"
        ],
        "requirements": [
            "ShowPageSizeSelector boolean parameter with PageSizeOptions array.",
            "ShowDirectPageJump boolean parameter adding numeric input box.",
            "TotalItems and ItemRangeText display (e.g. '1-25 of 1,200 items').",
            "First, Previous, Next, Last navigation buttons."
        ]
    },
    {
        "id": "099",
        "slug": "colorpicker-rgba-hex-hsv-and-palette-presets",
        "title": "ColorPicker Visual Spectrum Canvas, Alpha Slider, and Swatch Palette Presets",
        "module": "ColorPicker",
        "priority": 99,
        "status": "ready",
        "dependencies": [],
        "summary": "ColorPicker with visual 2D saturation/brightness spectrum canvas, hue and alpha opacity sliders, format switcher (HEX, RGBA, HSV), and preset color swatches.",
        "target_files": [
            "src/Vek/Components/ColorPicker/VekColorPicker.razor",
            "src/Vek/Components/ColorPicker/VekColorPicker.razor.cs",
            "src/Vek/Styles/components/_colorpicker.scss"
        ],
        "requirements": [
            "Value string two-way binding supporting #RRGGBB and rgba(...).",
            "2D Saturation/Value canvas with smooth dragging cursor.",
            "Alpha slider controlling opacity channel.",
            "ColorPalette presets array with customizable swatches."
        ]
    },
    {
        "id": "100",
        "slug": "hotkey-global-and-scoped-keyboard-shortcuts",
        "title": "Hotkey Global and Scoped Shortcut Binding Manager with Chord Key Sequences",
        "module": "Hotkey",
        "priority": 100,
        "status": "ready",
        "dependencies": [],
        "summary": "Declarative keyboard shortcut manager supporting global hotkeys (Ctrl+S, Cmd+K), scoped container shortcuts, chord key sequences (g then d), and input focus suppression.",
        "target_files": [
            "src/Vek/Components/Hotkey/VekHotkey.razor",
            "src/Vek/Components/Hotkey/VekHotkey.razor.cs",
            "src/Vek/Components/Hotkey/HotkeyManager.cs"
        ],
        "requirements": [
            "KeyCombination string parameter (e.g. 'Ctrl+S', 'Alt+Shift+F', 'Escape').",
            "OnHotkeyDown EventCallback emitted when sequence is matched.",
            "PreventDefault and StopPropagation parameters.",
            "IgnoreInInputs boolean (default true) suppressing hotkey while typing in text boxes."
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
    print(f"Current features in backlog: {len(existing_ids)}")

    print(f"\nGenerating specifications 053 through 100 in {SPECS_DIR}...")
    for feat in FEATURES_053_TO_100:
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
            for idx, item in enumerate(backlog_data["features"]):
                if item["id"] == feat["id"]:
                    backlog_data["features"][idx] = b_entry
                    break

    # Sort backlog features by ID
    backlog_data["features"].sort(key=lambda x: int(x.get("id", 999)))

    print(f"\nWriting updated backlog with {len(backlog_data['features'])} total features to {BACKLOG_PATH}...")
    BACKLOG_PATH.write_text(json.dumps(backlog_data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"Grand catalog generation complete! Total features: {len(backlog_data['features'])}")


if __name__ == "__main__":
    main()
