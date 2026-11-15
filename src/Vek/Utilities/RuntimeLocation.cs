using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Vek.Utilities
{
    /// <summary>
    /// Reports whether the app is running client-side in WebAssembly or server-side.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RuntimeLocation
    {
        public static bool IsClientSide => RuntimeInformation.OSDescription == "Browser"; // WASM
        public static bool IsServerSide => RuntimeInformation.OSDescription != "Browser";
    }

}
