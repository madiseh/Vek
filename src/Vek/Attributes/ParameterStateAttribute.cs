// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.State;

/// <summary>
/// Marks a component parameter that is managed by Vek's ParameterState framework so the analyzer can verify it is registered correctly.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ParameterStateAttribute : Attribute
{
    public ParameterUsageOptions ParameterUsage { get; set; } = ParameterUsageOptions.All;
}
