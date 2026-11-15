using Vek.Docs.Models;

namespace Vek.Docs.Services;

public interface IDocsNavigationService
{
    NavigationFooterLink Next { get; }

    NavigationFooterLink Previous { get; }

    NavigationSection Section { get; }
}
