using IDP.Repositories;
using IDP.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IDP.Infrastructure
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddIdpUserStore(this IIdentityServerBuilder identityServerBuilder)
        {
            //register user store
            identityServerBuilder.Services.AddSingleton<IIdpUserRepository, IdpUserRepository>();

            // IdpUserProfileService allows connecting custom user store to get profile info like claims
            identityServerBuilder.AddProfileService<IdpUserProfileService>();
            return identityServerBuilder;
        }
    }
}
