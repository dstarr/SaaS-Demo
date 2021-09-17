using Azure.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Marketplace.Metering;
using Microsoft.Marketplace.SaaS;
using System.Threading.Tasks;

namespace LandingPage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // AAD and Graph integration
            services.AddMicrosoftIdentityWebAppAuthentication(this.Configuration, "AzureAd") // Sign on with AAD
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { "user.read" }) // Call Graph API
                    .AddMicrosoftGraph() // Use defaults with Graph V1
                    .AddInMemoryTokenCaches(); // Add token caching

            // OpenIdConnect setup
            services.Configure<OpenIdConnectOptions>(options =>
            {
                options.Events.OnSignedOutCallbackRedirect = (context) =>
                {
                    context.Response.Redirect("/");
                    context.HandleResponse();

                    return Task.CompletedTask;
                };
            });

            ConfigureMarketplaceServices(services);

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages()
                    .AddMicrosoftIdentityUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private void ConfigureMarketplaceServices(IServiceCollection services)
        {
            // wire up marketplace client
            var tenantId = Configuration["Marketplace:TenantId"];
            var clientId = Configuration["Marketplace:ClientId"];
            var clientSecret = Configuration["Marketplace:ClientSecret"];

            // get standard Azure creds
            var creds = new ClientSecretCredential(tenantId, clientId, clientSecret);

            services.TryAddScoped<IMarketplaceSaaSClient>(sp =>
            {
                return new MarketplaceSaaSClient(creds);
            });

            //services.TryAddScoped<IMarketplaceProcessor, MarketplaceProcessor>();
        }


    }
}
