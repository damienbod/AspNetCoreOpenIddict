using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IdentityProvider.Data;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;

namespace IdentityProvider.Areas.Identity.Pages.Account;

public class PasskeysModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public PasskeysModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IList<UserPasskeyInfo> CurrentPasskeys { get; set; }

    [BindProperty]
    public InputModel? Input { get; set; }

    public class InputModel
    {
        public string? CredentialId { get; set; }

        public string? Action { get; set; }

        public PasskeyInputModel? Passkey { get; set; }
    }

    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        CurrentPasskeys = await _userManager.GetPasskeysAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdatePasskeyAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (string.IsNullOrEmpty(Input?.CredentialId))
        {
            StatusMessage = "Could not find the passkey.";
            return RedirectToPage();
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(Input.CredentialId);
        }
        catch (FormatException)
        {
            StatusMessage = "The specified passkey ID had an invalid format.";
            return RedirectToPage();
        }

        switch (Input?.Action)
        {
            case "rename":
                return RedirectToPage("./RenamePasskey", new { id = Input.CredentialId });
            case "delete":
                return await DeletePasskey(user, credentialId);
            default:
                StatusMessage = "Unknown action.";
                return RedirectToPage();
        }
    }

    private async Task<IActionResult> DeletePasskey([NotNull] ApplicationUser user, byte[] credentialId)
    {
        var result = await _userManager.RemovePasskeyAsync(user, credentialId);
        if (!result.Succeeded)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            throw new InvalidOperationException($"Unexpected error occurred removing passkey for user with ID '{userId}'.");
        }

        StatusMessage = "The passkey was removed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddPasskeyAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!string.IsNullOrEmpty(Input?.Passkey?.Error))
        {
            StatusMessage = $"Could not add a passkey: {Input.Passkey.Error}";
            return RedirectToPage();
        }

        if (string.IsNullOrEmpty(Input?.Passkey?.CredentialJson))
        {
            StatusMessage = "The browser did not provide a passkey.";
            return RedirectToPage();
        }

        var attestationResult = await _signInManager.PerformPasskeyAttestationAsync(Input.Passkey.CredentialJson);
        if (!attestationResult.Succeeded)
        {
            StatusMessage = $"Could not add the passkey: {attestationResult.Failure.Message}.";
            return RedirectToPage();
        }

        var setPasskeyResult = await _userManager.AddOrUpdatePasskeyAsync(user, attestationResult.Passkey);
        if (!setPasskeyResult.Succeeded)
        {
            StatusMessage = "The passkey could not be added to your account.";
            return RedirectToPage();
        }

        // Immediately prompt the user to enter a name for the credential
        StatusMessage = "The passkey was added to your account. You can now use it to sign in. Give it an easy to remember name.";
        return RedirectToPage("./RenamePasskey", new { id = Base64Url.EncodeToString(attestationResult.Passkey.CredentialId) });
    }
}
