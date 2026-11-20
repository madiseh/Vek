// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Vek.UnitTests.Dummy;

[JsonSerializable(typeof(PaletteLight))]
internal sealed partial class VekColorSerializerContext : JsonSerializerContext;
