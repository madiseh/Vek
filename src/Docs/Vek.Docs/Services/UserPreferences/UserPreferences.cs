// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.Docs.Enums;

namespace Vek.Docs.Services.UserPreferences
{
    public class UserPreferences
    {
        /// <summary>
        /// The direction of the layout.
        /// </summary>
        public bool RightToLeft { get; set; }

        /// <summary>
        /// The preferred dark mode configuration.
        /// </summary>
        public DarkLightMode DarkLightTheme { get; set; }
    }
}
