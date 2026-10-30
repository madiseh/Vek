namespace Vek.WPF.Demo.Navigation;

public enum DemoPageImplementationStatus
{
    InDevelopment,
    Implemented
}

public sealed record DemoPageDefinition(
    string Id,
    string Title,
    string Section,
    string Description,
    DemoPageImplementationStatus ImplementationStatus);
