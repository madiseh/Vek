// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Vek.Utilities.Background;

namespace Vek.UnitTests.Utilities.Background.Mocks;

internal class MyBackgroundWorkerMock : BackgroundWorkerBase
{
    private readonly Task _task;

    public MyBackgroundWorkerMock(Task task)
    {
        _task = task;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteCore(stoppingToken);
    }

    private async Task ExecuteCore(CancellationToken stoppingToken)
    {
        var task = await Task.WhenAny(_task, Task.Delay(Timeout.Infinite, stoppingToken));

        await task;
    }
}
