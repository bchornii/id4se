using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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

            var identityServerDataDbConnectionString = Configuration["ConnectionStrings:IdentityServerDataDbConnection"];
            var migrationAssembly = typeof(Startup)
                .GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer()
                //.AddDeveloperSigningCredential() // creates temp creds for sign JWT token
                .AddSigningCredential(LoadCertificateFromStore())
                //.AddTestUsers(Config.GetUsers())
                .AddIdpUserStore()
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddInMemoryClients(Config.GetClients())
                //.AddInMemoryApiResources(Config.GetApiResources());
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlServer(identityServerDataDbConnectionString,
                            db => db.MigrationsAssembly(assemblyName: migrationAssembly));
                    };
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlServer(identityServerDataDbConnectionString,
                            db => db.MigrationsAssembly(assemblyName: migrationAssembly));
                    };
                });

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

        private X509Certificate2 LoadCertificateFromStore()
        {
            var thumbPrint = "1BDE305D05CDA1F45C69F06EABF35A092792C05F";

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates
                    .Find(X509FindType.FindByThumbprint, thumbPrint, true);
                if (certCollection.Count == 0)
                {
                    throw new Exception("The specified certificate wasn't found");
                }
                return certCollection[0];
            }
        }
    }
}
