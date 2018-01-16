using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ImageGallery.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Disables mapping btw claim types from IDP and client claim types
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookie";        // using a cookie as the primary means 
                                                             // to authenticate a user

                    options.DefaultChallengeScheme = "oidc"; // when we need the user to login, 
                                                             // we will be using the OpenID Connect scheme                                                                                 
                })
                .AddCookie("Cookie", options =>
                {                    
                    options.AccessDeniedPath = "/Authorization/AccessDenied";
                })                        
                .AddOpenIdConnect("oidc", options =>
                {                    
                    options.SignInScheme = "Cookie";       // scheme responsible for persistance 
                                                           // user identity after successful auth at IDP

                    options.Authority = "https://localhost:44332/";
                    options.RequireHttpsMetadata = true;

                    options.ClientId = "imagegalleryclient";
                    options.ClientSecret = "secret";        // secret should match to ID server value
                    options.ResponseType = "code id_token"; // means “use hybrid flow”

                    options.SaveTokens = true;              // allows middleware to save tokens

                    options.GetClaimsFromUserInfoEndpoint = true;  // middleware will call /userinfo endpoint to get additional
                                                                   // information about user after id_token is received and validated
                    
                    // add claims from json user data received from /useinfo endpoint
                    options.ClaimActions.Add(new RoleClaimAction());
                    // defines string for role claim type
                    options.TokenValidationParameters.RoleClaimType = "role";

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("address");
                    options.Scope.Add("roles");                    
                    options.Scope.Add("imagegalleryapi");                    

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = tokenValidatedContext =>
                        {
                            var identity = tokenValidatedContext
                                .Principal.Identity as ClaimsIdentity;

                            var subClaim = identity?.Claims
                                .Where(c => c.Type == "sub");

                            // Create new claims identity and add sub claim
                            var claimsIdentity = new ClaimsIdentity(
                                subClaim,
                                tokenValidatedContext.Scheme.DisplayName,
                                "given_name", "role");
                            var principal = new ClaimsPrincipal(claimsIdentity);

                            // Reinitialize request Principal with custom one
                            tokenValidatedContext.Principal = principal;

                            return Task.CompletedTask;   
                        },

                        // Trigered when claims received from userinfo endpoint
                        OnUserInformationReceived = userInfoReceivedContext =>
                        {
                            // Remove address claim
                            userInfoReceivedContext.User.Remove("address");
                            return Task.CompletedTask;
                        }
                    };
                });

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
