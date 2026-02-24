using OpenIddict.Abstractions;
using IdentityProvider.Data;
using System.Globalization;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityProvider;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        await RegisterApplicationsAsync(scope.ServiceProvider);
        await RegisterScopesAsync(scope.ServiceProvider);

        static async Task RegisterApplicationsAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

            //var dd = await manager.FindByClientIdAsync("oidc-pkce-confidential");

            //await manager.DeleteAsync(dd);

            // OIDC Code flow confidential client
            if (await manager.FindByClientIdAsync("oidc-pkce-confidential") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "oidc-pkce-confidential",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "OIDC confidential Code Flow PKCE",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente MVC"
                    },
                    PostLogoutRedirectUris =
                    {
                        new Uri("https://localhost:5001/signout-callback-oidc"),
                        new Uri("https://localhost:64265/signout-callback-oidc")
                    },
                    RedirectUris =
                    {
                        new Uri("https://localhost:5001/signin-oidc"),
                        new Uri("https://localhost:64265/signin-oidc"),
                    },
                    ClientSecret = "oidc-pkce-confidential_secret",
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.EndSession,
                        Permissions.Endpoints.Token,
                        Permissions.Endpoints.Revocation,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        Permissions.Prefixes.Scope + "dataEventRecords"
                    },
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    }
                });
            }
        }

        static async Task RegisterScopesAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByNameAsync("dataEventRecords") is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    DisplayName = "dataEventRecords API access",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("fr-FR")] = "Accès à l'API de démo"
                    },
                    Name = "dataEventRecords",
                    Resources =
                    {
                        "rs_dataEventRecordsApi"
                    }
                });
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
