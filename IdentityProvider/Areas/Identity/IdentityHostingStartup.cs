[assembly: HostingStartup(typeof(IdentityProvider.Areas.Identity.IdentityHostingStartup))]
namespace IdentityProvider.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}
