// Must stay the first import so the license banner leads the bundle.
import "./vekAAAlicense";

import VekDragAndDrop from "./vekDragAndDrop";
import VekElementReference from "./vekElementReference";
import VekExitPrompt from "./vekExitPrompt";
import VekFileUpload from "./vekFileUpload";
import { getTabbableElements, hasDefinedParentHeight, vekGetSvgBBox, vekObserveElementSize, serializeParameter } from "./vekHelpers";
import VekHotkeyListener from "./vekHotkeyListener";
import VekInput from "./vekInput";
import VekInputSizing from "./vekInputSizing";
import VekJsEventFactory from "./vekJsEvent";
import VekKeyInterceptorFactory from "./vekKeyInterceptor";
import VekPointerCapture from "./vekPointerCapture";
import { registerRippleHandlers } from "./vekRipple";
import VekPopover, { vekpopoverHelper } from "./vekPopover";
import { VekResizeListener, VekResizeListenerFactory } from "./vekResizeListener";
import VekPointerEventsNone from "./vekPointerEventsNone";
import VekResizeObserverFactory from "./vekResizeObserver";
import VekScrollListener from "./vekScrollListener";
import VekScrollManager from "./vekScrollManager";
import VekScrollSpy from "./vekScrollSpy";
import VekSplitPanel, {
    vekSplitPanel_destroy,
    vekSplitPanel_getDividerPosition,
    vekSplitPanel_resetDividerPosition,
    vekSplitPanel_setDividerPosition,
    vekSplitPanel_update,
} from "./vekSplitPanel";
import VekTableCell from "./vekTableCell";
import VekThemeProvider from "./vekThemeProvider";
import VekTimePicker from "./vekTimePicker";
import VekWindow from "./VekWindow";
import { downloadFileFromStream } from "./vekDataGridExport";

declare global {
    interface Window {
        getTabbableElements: typeof getTabbableElements;
        hasDefinedParentHeight: typeof hasDefinedParentHeight;
        vekDragAndDrop: VekDragAndDrop;
        vekDataGridExport: { downloadFileFromStream: typeof downloadFileFromStream };
        vekElementRef: VekElementReference;
        vekExitPrompt: VekExitPrompt;
        vekFileUpload: VekFileUpload;
        vekGetSvgBBox: typeof vekGetSvgBBox;
        vekHotkeyListener: VekHotkeyListener;
        vekInput: VekInput;
        vekInputSizing: VekInputSizing;
        vekObserveElementSize: typeof vekObserveElementSize;
        vekJsEvent: VekJsEventFactory;
        vekKeyInterceptor: VekKeyInterceptorFactory;
        vekPointerCapture: VekPointerCapture;
        vekPointerEventsNone: VekPointerEventsNone;
        vekPopover: VekPopover;
        vekpopoverHelper: typeof vekpopoverHelper;
        vekResizeListener: VekResizeListener;
        vekResizeListenerFactory: VekResizeListenerFactory;
        vekResizeObserver: VekResizeObserverFactory;
        vekScrollListener: VekScrollListener;
        vekScrollManager: VekScrollManager;
        vekScrollSpy: VekScrollSpy;
        vekSplitPanel: typeof VekSplitPanel;
        vekSplitPanel_destroy: typeof vekSplitPanel_destroy;
        vekSplitPanel_getDividerPosition: typeof vekSplitPanel_getDividerPosition;
        vekSplitPanel_resetDividerPosition: typeof vekSplitPanel_resetDividerPosition;
        vekSplitPanel_setDividerPosition: typeof vekSplitPanel_setDividerPosition;
        vekSplitPanel_update: typeof vekSplitPanel_update;
        vekTableCell: VekTableCell;
        vekThemeProvider: VekThemeProvider;
        vekTimePicker: VekTimePicker;
        vekWindow: VekWindow;
        serializeParameter: typeof serializeParameter;
        splitPanels: Record<string, VekSplitPanel>;
    }
}

window.getTabbableElements = getTabbableElements;
window.hasDefinedParentHeight = hasDefinedParentHeight;
window.vekDragAndDrop = new VekDragAndDrop();
window.vekDataGridExport = { downloadFileFromStream };
window.vekElementRef = new VekElementReference();
// Guard against a second copy of the script replacing the instance that owns the registered prompts.
if (!window.vekExitPrompt) {
    window.vekExitPrompt = new VekExitPrompt();
}
window.vekFileUpload = new VekFileUpload();
window.vekGetSvgBBox = vekGetSvgBBox;
if (!window.vekHotkeyListener) {
    window.vekHotkeyListener = new VekHotkeyListener();
}
window.vekInput = new VekInput();
window.vekInputSizing = new VekInputSizing();
window.vekJsEvent = new VekJsEventFactory();
window.vekKeyInterceptor = new VekKeyInterceptorFactory();
window.vekObserveElementSize = vekObserveElementSize;
window.vekPointerCapture = new VekPointerCapture();
window.vekPointerEventsNone = new VekPointerEventsNone();
window.vekPopover = new VekPopover();
window.vekpopoverHelper = vekpopoverHelper;
window.vekResizeListener = new VekResizeListener();
window.vekResizeListenerFactory = new VekResizeListenerFactory();
window.vekResizeObserver = new VekResizeObserverFactory();
if (!window.vekScrollListener) {
    window.vekScrollListener = new VekScrollListener();
}
window.vekScrollManager = new VekScrollManager();
window.vekScrollSpy = new VekScrollSpy();
// The registry is shared state, so a second copy of the script keeps using the existing one.
if (!window.vekSplitPanel) {
    window.vekSplitPanel = VekSplitPanel;
    window.splitPanels = {};
}
window.vekSplitPanel_destroy = vekSplitPanel_destroy;
window.vekSplitPanel_getDividerPosition = vekSplitPanel_getDividerPosition;
window.vekSplitPanel_resetDividerPosition = vekSplitPanel_resetDividerPosition;
window.vekSplitPanel_setDividerPosition = vekSplitPanel_setDividerPosition;
window.vekSplitPanel_update = vekSplitPanel_update;
window.vekTableCell = new VekTableCell();
window.vekThemeProvider = new VekThemeProvider();
window.vekTimePicker = new VekTimePicker();
window.vekWindow = new VekWindow();
window.serializeParameter = serializeParameter;

registerRippleHandlers();
