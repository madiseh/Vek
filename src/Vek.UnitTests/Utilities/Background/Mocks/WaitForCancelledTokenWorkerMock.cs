// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Vek.Utilities.Background;

namespace Vek.UnitTests.Utilities.Background.Mocks;

internal class WaitForCancelledTokenWorkerMock : BackgroundWorkerBase
{
    public Task ExecutingTask { get; private set; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ExecutingTask = Task.Delay(Timeout.Infinite, stoppingToken);
        return ExecutingTask;
    }
}
