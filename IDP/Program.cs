using System;
using IdentityServer4.EntityFramework.DbContexts;
using IDP.Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IDP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = BuildWebHost(args);

            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    // migrate and seed IDP database (users, claims, logins)
                    var idpDb = services.GetRequiredService<IdpUserContext>();
                    idpDb.Database.Migrate();
                    idpDb.EnsureSeedDataForContext();

                    // migrate and seed configuration database (identity resources,api resources, clients)
                    var configDb = services.GetRequiredService<ConfigurationDbContext>();
                    configDb.Database.Migrate();
                    configDb.EnsureSeedDataForContext();

                    // migrate persisted grant database (tokens)
                    var persistedDb = services.GetRequiredService<PersistedGrantDbContext>();
                    persistedDb.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
