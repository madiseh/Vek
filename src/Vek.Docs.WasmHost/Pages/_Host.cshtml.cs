using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Vek.Docs.WasmHost.Pages;

public class HostModel : PageModel
{
    public bool PreRender { get; set; }

    public IActionResult OnGet()
    {
        if (Request.Headers.ContainsKey("UsePrerender"))
        {
            PreRender = true;
        }

        return Page();
    }
}
