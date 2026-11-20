using System;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace Vek.Docs.Extensions
{
    internal static class NavigationManagerExtensions
    {
        /// <summary>
        /// Gets the section part of the documentation page
        /// Ex: /components/button;  "components" is the section
        /// </summary>
        public static string GetSection(this NavigationManager navMan)
        {
            // get the absolute path with out the base path
            var currentUri = navMan.Uri.Remove(0, navMan.BaseUri.Length - 1);
            var firstElement = currentUri
                .Split("/", StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            return firstElement;
        }

        /// <summary>
        /// Gets the link of the component on the documentation page
        /// Ex: api/button; "button" is the component link, and "api" is the section
        /// </summary>
        public static string GetComponentLink(this NavigationManager navMan)
        {
            // get the absolute path with out the base path
            var currentUri = navMan.Uri.Remove(0, navMan.BaseUri.Length - 1);
            var path = currentUri.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 1)
            {
                return string.Join('/', segments.Skip(1));
            }

            return null;
        }

        /// <summary>
        /// Determines if the current page is the base page
        /// </summary>
        public static bool IsHomePage(this NavigationManager navMan)
        {
            return navMan.Uri == navMan.BaseUri;
        }
    }
}
