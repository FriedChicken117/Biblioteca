using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Biblioteca.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    public string UserName { get; set; } = string.Empty;

    public void OnGet()
    {
        UserName = User.Identity?.Name ?? string.Empty;
    }
}
