namespace Vek.Services
{
    // Add additional configuration objects here when adding new services

    /// <summary>
    /// Shared configuration container for Vek services registered in DI.
    /// </summary>
    /// <remarks>
    /// This is the aggregation point used by the Vek service registration helpers so callers can configure related services in one place.
    /// </remarks>
    public class VekServicesConfiguration
    {
        public SnackbarConfiguration SnackbarConfiguration { get; set; } = new SnackbarConfiguration();

        public ResizeOptions ResizeOptions { get; set; } = new ResizeOptions();

        public ResizeObserverOptions ResizeObserverOptions { get; set; } = new ResizeObserverOptions();

        public PopoverOptions PopoverOptions { get; set; } = new PopoverOptions();
    }
}
