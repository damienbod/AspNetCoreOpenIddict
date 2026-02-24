using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using IdentityProvider.Data;
using System.Buffers.Text;

namespace IdentityProvider.Areas.Identity.Pages.Account;

public class RenamePasskeyModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public RenamePasskeyModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [BindProperty] public InputModel? Input { get; set; }

    public class InputModel
    {
        public string? CredentialId { get; set; }

        public string? Name { get; set; }
    }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(id);
        }
        catch (FormatException)
        {
            StatusMessage = "The specified passkey ID had an invalid format.";
            return RedirectToPage("./Passkeys");
        }

        var passkey = await _userManager.GetPasskeyAsync(user, credentialId);
        if (passkey == null)
        {
            return NotFound($"Unable to load passkey ID '{_userManager.GetUserId(User)}'.");
        }

        Input = new InputModel
        {
            CredentialId = id,
            Name = passkey.Name
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(Input?.CredentialId);
        }
        catch (FormatException)
        {
            StatusMessage = "The specified passkey ID had an invalid format.";
            return RedirectToPage("./Passkeys");
        }

        var passkey = await _userManager.GetPasskeyAsync(user, credentialId);
        if (passkey == null)
        {
            return NotFound($"Unable to load passkey ID '{_userManager.GetUserId(User)}'.");
        }

        // Rename
        passkey.Name = Input?.Name;

        var result = await _userManager.AddOrUpdatePasskeyAsync(user, passkey);
        if (!result.Succeeded)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            throw new InvalidOperationException($"Unexpected error occurred removing passkey for user with ID '{userId}'.");
        }

        // REVIEW: Only one of the .NET 10 user stores update the Name property of the passkey. Doing direct database access here to ensure the name is stored.
        var passkeyEntity = await _dbContext.UserPasskeys.SingleOrDefaultAsync(userPasskey => userPasskey.CredentialId.SequenceEqual(credentialId));
        if (passkeyEntity != null)
        {
            passkeyEntity.Data.Name = Input?.Name;
            await _dbContext.SaveChangesAsync();
        }

        StatusMessage = "The passkey was updated.";
        return RedirectToPage("./Passkeys");
    }
}
