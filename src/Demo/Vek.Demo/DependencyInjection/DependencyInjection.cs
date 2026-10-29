using Vek.Demo.DependencyInjection.Modules;

namespace Vek.Demo.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDemoServices(this IServiceCollection services)
    {
        services.AddSimulatedDataModule();

        return services;
    }
}
