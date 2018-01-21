using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IDP.Repositories;

namespace IDP.Services
{
    public class IdpUserProviderService : IProfileService
    {
        private readonly IIdpUserRepository _idpUserRepository;

        public IdpUserProviderService(IIdpUserRepository idpUserRepository)
        {
            _idpUserRepository = idpUserRepository;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subId = context.Subject.GetSubjectId();
            var userClaims = _idpUserRepository.GetUserClaimsBySubjectId(subId);
            context.IssuedClaims = userClaims
                .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                .ToList();
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var subId = context.Subject.GetSubjectId();
            context.IsActive = _idpUserRepository.IsUserActive(subId);
            return Task.CompletedTask;
        }
    }
}
