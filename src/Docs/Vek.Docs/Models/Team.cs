// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace Vek.Docs.Models
{
    [EnumExtensions]
    public enum Team
    {
        [Description("Core Maintainer")]
        CoreMaintainer,

        [Description("Core Team")]
        Core,

        [Description("Contribution Team")]
        Contribution
    }
}
