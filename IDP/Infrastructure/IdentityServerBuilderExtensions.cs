using IDP.Repositories;
using IDP.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IDP.Infrastructure
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddIdpUserStore(this IIdentityServerBuilder identityServerBuilder)
        {
            identityServerBuilder.Services.AddSingleton<IIdpUserRepository, IdpUserRepository>();      //register user store
            identityServerBuilder.AddProfileService<IdpUserProfileService>();
            return identityServerBuilder;
        }
    }
}
