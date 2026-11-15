// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// NOTE: This file is a copy of src/Vek/State/ParameterUsageOptions.cs
// Keep this file in sync with the original.
// Analyzers cannot reference Vek assembly, so we duplicate the enum here
// following Microsoft's pattern (see ILLink analyzer DynamicallyAccessedMemberTypes.cs).

namespace Vek.State;

[Flags]
internal enum ParameterUsageOptions
{
    None = 0,
    Read = 1 << 1,
    Write = 1 << 2,
    All = Read | Write
}
