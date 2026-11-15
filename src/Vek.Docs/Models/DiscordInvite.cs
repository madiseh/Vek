// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Vek.Docs.Models;

public class DiscordInvite
{
    [JsonPropertyName("approximate_member_count")]
    public int ApproximateMemberCount { get; set; }

    [JsonPropertyName("approximate_presence_count")]
    public int ApproximatePresenceCount { get; set; }
}
