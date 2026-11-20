// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.State;

namespace Vek.UnitTests.State.Mocks;

#nullable enable
internal class ParameterChangedHandlerMock<TArgs> : IParameterChangedHandler<TArgs>
{
    private readonly List<ParameterChangedEventArgs<TArgs>> _changes = new();

    public IReadOnlyList<ParameterChangedEventArgs<TArgs>> Changes => _changes;

    public Task HandleAsync(ParameterChangedEventArgs<TArgs> parameterChangedEventArgs, ParameterChangedContext context)
    {
        _changes.Add(parameterChangedEventArgs);

        return Task.CompletedTask;
    }
}
