using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using IDP.Entities;
using IDP.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IDP.Quickstart.UserRegistration
{
    public class UserRegistrationController : Controller
    {
        private readonly IIdpUserRepository _idpUserRepository;
        private readonly IIdentityServerInteractionService _interactionService;

        public UserRegistrationController(IIdpUserRepository idpUserRepository,
            IIdentityServerInteractionService interactionService)
        {
            _idpUserRepository = idpUserRepository;
            _interactionService = interactionService;
        }

        [HttpGet]
        public IActionResult RegisterUser(string returnUrl)
        {
            var vm = new RegisterUserViewModel{ ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // create user + claims
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    IsActive = true
                };
                user.Claims.Add(new UserClaim("country", model.Country));
                user.Claims.Add(new UserClaim("address", model.Address));
                user.Claims.Add(new UserClaim("given_name", model.FirstName));
                user.Claims.Add(new UserClaim("family_name", model.LastName));
                user.Claims.Add(new UserClaim("email", model.Email));
                user.Claims.Add(new UserClaim("subscriptionlevel", "FreeUser"));

                _idpUserRepository.AddUser(user);

                if (!_idpUserRepository.Save())
                {
                    throw new Exception("Creating user failed.");
                }

                // log in user
                // issue authentication cookie with subject ID and username
                await HttpContext.SignInAsync(user.SubjectId, user.Username);

                // continue with flow
                if (_interactionService.IsValidReturnUrl(model.ReturnUrl) ||
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return Redirect("~");
            }

            return View(model);
        }
    }
}
