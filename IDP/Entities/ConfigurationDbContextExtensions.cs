using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;

namespace IDP.Entities
{
    public static class ConfigurationDbContextExtensions
    {
        public static void EnsureSeedDataForContext(this ConfigurationDbContext configurationDbContext)
        {
            if (!configurationDbContext.Clients.Any())
            {
                foreach (var client in Config.GetClients())
                {
                    configurationDbContext.Clients.Add(client.ToEntity());
                }
                configurationDbContext.SaveChanges();
            }

            if (!configurationDbContext.IdentityResources.Any())
            {
                foreach (var identityResource in Config.GetIdentityResources())
                {
                    configurationDbContext.IdentityResources.Add(identityResource.ToEntity());
                }
                configurationDbContext.SaveChanges();
            }

            if (!configurationDbContext.ApiResources.Any())
            {
                foreach (var apiResource in Config.GetApiResources())
                {
                    configurationDbContext.ApiResources.Add(apiResource.ToEntity());
                }
                configurationDbContext.SaveChanges();
            }
        }
    }
}
