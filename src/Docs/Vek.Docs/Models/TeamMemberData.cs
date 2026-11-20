// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Docs.Models
{
    /// <summary>
    /// The content for the team page.
    /// </summary>
    /// <remarks>
    /// Data is copied from GitHub manually. In the future this could be automated as a build step.
    /// </remarks>
    public class TeamMemberData
    {
        public static TeamMember[] GetTeam() =>
        [
            new TeamMember
            {
                Name = "Amin Sabaghi",
                GitHub = "madiseh",
                GitHubSponsor = false,
                LinkedIn = null,
                Bio = "Creator and Lead Developer of Vek.",
                Team = Team.Core
            },
            new TeamMember
            {
                Name = "Max Behzadi",
                GitHub = "maxbehzadi",
                GitHubSponsor = false,
                LinkedIn = null,
                Bio = "Contributor to Vek.",
                Team = Team.Contribution
            }
        ];

        public static TeamMember[] GetHallOfFameTeam() => [];
    }
}
