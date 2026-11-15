using System.Threading.Tasks;

namespace Vek.Interfaces
{
    /// <summary>
    /// Receives a notification when navigation occurs so a component can respond.
    /// </summary>
    public interface INavigationEventReceiver
    {
        Task OnNavigation();
    }
}
