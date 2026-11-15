// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.Docs.Models;

#nullable enable
public class VekIcons
{
    public string Name { get; }

    public string Code { get; }

    public string Category { get; }

    public VekIcons(string name, string code, string category)
    {
        Name = name;
        Code = code;
        Category = category;
    }

    public static readonly VekIcons Empty = new(string.Empty, string.Empty, string.Empty);
}
