using Vek.Demo.Abstractions;
using Vek.SimulatedData;

namespace Vek.Demo.DependencyInjection.Modules;

internal static class SimulatedDataModule
{
    internal static IServiceCollection AddSimulatedDataModule(this IServiceCollection services)
    {
        services.AddSingleton<IDemoProductProvider, JsonDemoProductProvider>();

        return services;
    }
}
