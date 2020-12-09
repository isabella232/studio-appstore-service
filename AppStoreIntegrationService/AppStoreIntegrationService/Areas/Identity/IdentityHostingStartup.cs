using System;
using AppStoreIntegrationService.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(AppStoreIntegrationService.Areas.Identity.IdentityHostingStartup))]
namespace AppStoreIntegrationService.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppStoreIntegrationServiceContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("AppStoreIntegrationServiceContextConnection")));

                services.AddDefaultIdentity<IdentityUser>(
                    options =>
                    {
                        options.Password.RequireDigit = false;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.SignIn.RequireConfirmedAccount = false;
                    })
                    .AddEntityFrameworkStores
                    <AppStoreIntegrationServiceContext>();
            });
        }
    }
}