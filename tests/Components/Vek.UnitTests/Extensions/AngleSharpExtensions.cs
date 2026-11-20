using AngleSharp.Dom;

namespace Vek.UnitTests
{
    public static class AngleSharpExtensions
    {
        public static string TrimmedText(this IElement self)
        {
            return self.TextContent?.Trim();
        }
    }
}
