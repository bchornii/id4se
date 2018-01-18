using System;
using System.Linq;
using System.Threading.Tasks;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ImageGallery.API.Infrastructure.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository _galleryRepository;

        public MustOwnImageHandler(IGalleryRepository galleryRepository)
        {
            _galleryRepository = galleryRepository;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
            MustOwnImageRequirement requirement)
        {
            // If can't get resources - fail
            if (!(context.Resource is AuthorizationFilterContext filterContext))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // If can't get image id - fail
            var imageId = filterContext.RouteData.Values["id"].ToString();
            if (!Guid.TryParse(imageId, out var imageAsGuidId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // If user is not owner - fail
            var ownerId = context.User.Claims
                .FirstOrDefault(c => c.Type == "sub")?.Value;
            if (!_galleryRepository.IsOwner(imageAsGuidId, ownerId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
