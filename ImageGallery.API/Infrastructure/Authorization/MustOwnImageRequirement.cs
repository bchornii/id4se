using Microsoft.AspNetCore.Authorization;

namespace ImageGallery.API.Infrastructure.Authorization
{
    public class MustOwnImageRequirement : IAuthorizationRequirement { }
}
