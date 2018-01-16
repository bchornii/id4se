using System.Linq;
using System.Security.Claims;

namespace ImageGallery.API.Infrastructure
{
    public static class IdentityExtensions
    {
        public static string GetOwnerId(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        }
    }
}
