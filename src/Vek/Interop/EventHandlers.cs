using System;
using Microsoft.AspNetCore.Components;

namespace Vek;

// used in VekCollapse
/// <summary>
/// Registers the <c>ontransitionend</c> DOM event with Blazor's event system.
/// </summary>
[EventHandler("ontransitionend", typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: false)]
public static class EventHandlers;
