// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Vek;

/// <summary>
/// Record encapsulating data regarding a completed transaction
/// </summary>
/// <typeparam name="T">Type of dragged item</typeparam>
/// <param name="Item">The dragged item during the transaction</param>
/// <param name="DropzoneIdentifier">Identifier of the zone where the transaction ended</param>
/// <param name="IndexInZone">The index of the item within in the drop zone</param>
public record VekItemDropInfo<T>(T? Item, string DropzoneIdentifier, int IndexInZone)
{
}
