using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace IdentityProvider.Passkeys;

[HtmlTargetElement("passkey-submit")]
public class PasskeySubmitTagHelper : TagHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    [HtmlAttributeName("operation")]
    public PasskeyOperation Operation { get; set; }

    [HtmlAttributeName("name")]
    public string Name { get; set; } = null!;

    [HtmlAttributeName("email-name")]
    public string? EmailName { get; set; }

    public PasskeySubmitTagHelper(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Get tokens
        var tokens = _httpContextAccessor.HttpContext?.RequestServices
            .GetService<IAntiforgery>()?.GetTokens(_httpContextAccessor.HttpContext);

        // Button is the main element we want to create, capture all attributes etc.
        var buttonAttributes = output.Attributes.Where(it => it.Name != "operation" && it.Name != "name" && it.Name != "email-name").ToList();
        var buttonContent = (await output.GetChildContentAsync(NullHtmlEncoder.Default))
            .GetContent(NullHtmlEncoder.Default);

        // Create the button
        using var htmlWriter = new StringWriter();
        htmlWriter.Write("<button type=\"submit\" name=\"__passkeySubmit\" ");
        foreach (var buttonAttribute in buttonAttributes)
        {
            buttonAttribute.WriteTo(htmlWriter, NullHtmlEncoder.Default);
            htmlWriter.Write(" ");
        }
        htmlWriter.Write(">");
        if (!string.IsNullOrEmpty(buttonContent))
        {
            htmlWriter.Write(buttonContent);
        }
        htmlWriter.Write("</button>");
        htmlWriter.WriteLine();

        // Create the element
        htmlWriter.Write("<passkey-submit ");
        htmlWriter.Write($"operation=\"{Operation}\" ");
        htmlWriter.Write($"name=\"{Name}\" ");
        htmlWriter.Write($"email-name=\"{EmailName ?? ""}\" ");
        htmlWriter.Write($"request-token-name=\"{tokens?.HeaderName ?? ""}\" ");
        htmlWriter.Write($"request-token-value=\"{tokens?.RequestToken ?? ""}\" ");
        htmlWriter.Write(">");
        htmlWriter.Write("</passkey-submit>");

        // Emit the element
        output.TagName = null;
        output.Attributes.Clear();
        output.Content.Clear();
        output.Content.SetHtmlContent(htmlWriter.ToString());

        await base.ProcessAsync(context, output);
    }
}
