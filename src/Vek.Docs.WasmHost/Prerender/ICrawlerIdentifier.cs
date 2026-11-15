// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Vek.Docs.WasmHost.Prerender;

public interface ICrawlerIdentifier
{
    Task Initialize();

    Task<bool> IsRequestByCrawler(HttpContext context);
}
