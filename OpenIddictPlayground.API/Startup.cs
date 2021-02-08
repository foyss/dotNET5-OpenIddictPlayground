using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddictPlayground.API.Models;

namespace OpenIddictPlayground.API
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
            services.AddControllersWithViews();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

                // Register the entity sets needed by OpenIddict
                // Note: use the generic overload if you need to replace the default OpenIddict entities
                options.UseOpenIddict();
            });

            services.AddOpenIddict()

                // Register the OpenIddict core components
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models
                    // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities
                    options.UseEntityFrameworkCore()
                           .UseDbContext<ApplicationDbContext>();
                })

                // Register the OpenIddict server components
                .AddServer(options =>
                {
                    // Enable the token endpoint
                    options.SetTokenEndpointUris("/connect/token");

                    // Enable the client credentials flow
                    options.AllowClientCredentialsFlow();

                    // Register the signing and encryption credentials
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options
                    options.UseAspNetCore()
                           .EnableTokenEndpointPassthrough();
                })

                // Register the OpenIddict validation components
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance
                    options.UseLocalServer();

                    // Register the ASP.NET Core host
                    options.UseAspNetCore();
                });

            // Register the worker responsible of seeding the database with the sample clients
            services.AddHostedService<InitialOpenIddictSetupWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseWelcomePage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
