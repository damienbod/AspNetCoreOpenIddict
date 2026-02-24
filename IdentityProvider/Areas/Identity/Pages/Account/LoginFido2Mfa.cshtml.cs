using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityProvider.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class MfaModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool RememberMe { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public void OnPost()
    {
    }
}
