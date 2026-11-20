// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek.Docs.Enums;

[EnumExtensions]
public enum CommunityCategories
{
    [Description("Parent")]
    Parent = 0,

    [Description("Custom Components")]
    Components = 1,

    [Description("Utility Extensions")]
    Utility = 2,

    [Description("Styling & Theming")]
    Style = 3,

    [Description("Other Extensions")]
    Other = 4
}
