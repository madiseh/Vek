using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Vek.Docs.Server;

/// <summary>
/// Redirects API documentation URLs to their lowercase canonical path.
/// </summary>
internal sealed class ApiCanonicalizationMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Redirects mixed-case API paths before they reach the documentation router.
    /// </summary>
    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            && path is not null
            && !string.Equals(path, path.ToLowerInvariant(), StringComparison.Ordinal))
        {
            var canonicalPath = new PathString(path.ToLowerInvariant()).ToUriComponent();
            var query = context.Request.QueryString.ToUriComponent();
            context.Response.Redirect(canonicalPath + query, permanent: true);
            return Task.CompletedTask;
        }

        return next(context);
    }
}
