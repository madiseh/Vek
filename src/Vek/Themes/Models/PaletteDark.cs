// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Dark-mode color palette that overrides <see cref="Palette"/> defaults for Vek's dark theme.
    /// </summary>
    public class PaletteDark : Palette
    {
        /// <inheritdoc />
        public override VekColor Black { get; set; } = "#27272f";

        /// <inheritdoc />
        public override VekColor Primary { get; set; } = "#776be7";

        /// <inheritdoc />
        public override VekColor Info { get; set; } = "#3299ff";

        /// <inheritdoc />
        public override VekColor Success { get; set; } = "#0bba83";

        /// <inheritdoc />
        public override VekColor Warning { get; set; } = "#ffa800";

        /// <inheritdoc />
        public override VekColor Error { get; set; } = "#f64e62";

        /// <inheritdoc />
        public override VekColor Dark { get; set; } = "#27272f";

        /// <inheritdoc />
        public override VekColor TextPrimary { get; set; } = "rgba(255,255,255, 0.70)";

        /// <inheritdoc />
        public override VekColor TextSecondary { get; set; } = "rgba(255,255,255, 0.50)";

        /// <inheritdoc />
        public override VekColor TextDisabled { get; set; } = "rgba(255,255,255, 0.2)";

        /// <inheritdoc />
        public override VekColor ActionDefault { get; set; } = "#adadb1";

        /// <inheritdoc />
        public override VekColor ActionDisabled { get; set; } = "rgba(255,255,255, 0.26)";

        /// <inheritdoc />
        public override VekColor ActionDisabledBackground { get; set; } = "rgba(255,255,255, 0.12)";

        /// <inheritdoc />
        public override VekColor Background { get; set; } = "#32333d";

        /// <inheritdoc />
        public override VekColor BackgroundGray { get; set; } = "#27272f";

        /// <inheritdoc />
        public override VekColor Surface { get; set; } = "#373740";

        /// <inheritdoc />
        public override VekColor DrawerBackground { get; set; } = "#27272f";

        /// <inheritdoc />
        public override VekColor DrawerText { get; set; } = "rgba(255,255,255, 0.50)";

        /// <inheritdoc />
        public override VekColor DrawerIcon { get; set; } = "rgba(255,255,255, 0.50)";

        /// <inheritdoc />
        public override VekColor AppbarBackground { get; set; } = "#27272f";

        /// <inheritdoc />
        public override VekColor AppbarText { get; set; } = "rgba(255,255,255, 0.70)";

        /// <inheritdoc />
        public override VekColor LinesDefault { get; set; } = "rgba(255,255,255, 0.12)";

        /// <inheritdoc />
        public override VekColor LinesInputs { get; set; } = "rgba(255,255,255, 0.3)";

        /// <inheritdoc />
        public override VekColor TableLines { get; set; } = "rgba(255,255,255, 0.12)";

        /// <inheritdoc />
        public override VekColor TableStriped { get; set; } = "rgba(255,255,255, 0.2)";

        /// <inheritdoc />
        public override VekColor Divider { get; set; } = "rgba(255,255,255, 0.12)";

        /// <inheritdoc />
        public override VekColor DividerLight { get; set; } = "rgba(255,255,255, 0.06)";

        /// <inheritdoc />
        public override VekColor Skeleton { get; set; } = "rgba(255,255,255, 0.11)";
    }
}
