// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.Utilities;

namespace Vek;


/// <summary>
/// Forces a line break in a flexbox layout so the items that follow wrap onto a new line.
/// </summary>
/// <seealso cref="VekGrid" />
/// <seealso cref="VekStack" />
public partial class VekFlexBreak : VekComponentBase
{
    /// <summary>
    /// Class names separated by spaces.
    /// </summary>
    protected string Classname =>
        new CssBuilder("vek-flex-break")
            .AddClass(Class)
            .Build();
}
