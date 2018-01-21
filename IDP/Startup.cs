using IDP.Entities;
using IDP.Infrastructure;
using IDP.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IDP
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var idpUserDbConnectionString = Configuration["ConnectionStrings:IdpUserDbConnection"];
            services.AddDbContext<IdpUserContext>(c => c.UseSqlServer(connectionString: idpUserDbConnectionString));

            services.AddMvc();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential() // creates temp creds for sign JWT token
                //.AddTestUsers(Config.GetUsers())
                .AddIdpUserStore()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryApiResources(Config.GetApiResources());

            // register repositories
            services.AddScoped<IIdpUserRepository, IdpUserRepository>();
        }

        public void Configure(IApplicationBuilder app, 
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
