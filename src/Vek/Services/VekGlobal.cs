// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

/// <summary>
/// <para>
/// Static properties that let you control the default behavior of some parts of Vek.
/// </para>
/// <para>
/// <b>Warning:</b> This feature is under development and breaking changes to the API <b>will occur</b> between releases.
/// See <see href="https://github.com/madiseh/vekcustomization/globals#usage">our website</see> for more info including our support policy.
/// </para>
/// </summary>
public static class VekGlobal
{
    /// <summary>
    /// Default settings for <see cref="VekMenu"/>.
    /// <br/>
    /// <b>Warning:</b> This feature is under development and breaking changes to the API <b>will occur</b> between releases.
    /// </summary>
    public static class MenuDefaults
    {
        /// <summary>
        /// The delay in milliseconds before a <see cref="VekMenu"/> is shown when hovered, or hidden after the cursor moves away.
        /// </summary>
        public static int HoverDelay { get; set; } = 300;
    }

    /// <summary>
    /// Default settings for <see cref="VekTooltip"/>.
    /// <br/>
    /// <b>Warning:</b> This feature is under development and breaking changes to the API <b>will occur</b> between releases.
    /// </summary>
    public static class TooltipDefaults
    {
        /// <summary>
        /// The amount of time in milliseconds to wait from opening the <see cref="VekTooltip"/> before beginning to perform the transition.
        /// </summary>
        public static TimeSpan Delay { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// The length of time that the opening transition for <see cref="VekTooltip"/> takes to complete.
        /// </summary>
        public static TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(251);
    }

    /// <summary>
    /// The handler for unhandled Vek component exceptions.
    /// </summary>
    /// <remarks>
    /// Exceptions which use this handler are typically rare, such as errors which occur during a "fire-and-forget" <see cref="Task"/> which cannot be awaited.<br />
    /// By default, exceptions are logged to the console via <see cref="Console.Write(object?)"/>.<br />
    /// To handle all .NET exceptions, see: <see href="https://learn.microsoft.com/aspnet/core/fundamentals/error-handling">Handle errors in ASP.NET Core</see>.
    /// </remarks>
    public static Action<Exception> UnhandledExceptionHandler { get; set; } = (exception) => Console.Write(exception);
}
