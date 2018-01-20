using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<HttpClient> GetClient()
        {
            var idToken = await _httpContextAccessor.HttpContext
                .GetTokenAsync(OpenIdConnectParameterNames.IdToken);

            var accessToken = string.Empty;            

            // check for token renew
            var context = _httpContextAccessor.HttpContext;
            var expiresAt = await context.GetTokenAsync("expires_at");

            if (string.IsNullOrWhiteSpace(expiresAt) ||
                DateTime.Parse(expiresAt).AddSeconds(-60).ToUniversalTime() < DateTime.UtcNow)
            {
                accessToken = await RenewTokens();
            }
            else
            {
                accessToken = await _httpContextAccessor.HttpContext
                    .GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _httpClient.SetBearerToken(accessToken);
            }

            _httpClient.BaseAddress = new Uri("https://localhost:44330/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            var context = _httpContextAccessor.HttpContext;

            // get IDP metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44332/");
            var metaData = await discoveryClient.GetAsync();

            // create new token client to get new tokens
            var tokenClient = new TokenClient(metaData.TokenEndpoint, "imagegalleryclient", "secret");

            // get saved refresh token 
            var currRefreshToken = await context.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            // refresh the tokens
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currRefreshToken);

            if (!tokenResult.IsError)
            {
                // Save a tokens

                // get auth info
                var authenticateInfo = await context.AuthenticateAsync("Cookie");                

                // create new value expires_at and save it
                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);

                authenticateInfo.Properties.UpdateTokenValue("expires_at",
                    expiresAt.ToString("o", CultureInfo.InvariantCulture));

                authenticateInfo.Properties.UpdateTokenValue(
                    OpenIdConnectParameterNames.AccessToken,
                    tokenResult.AccessToken);

                authenticateInfo.Properties.UpdateTokenValue(
                    OpenIdConnectParameterNames.RefreshToken,
                    tokenResult.RefreshToken);

                // we're sing in again
                await context.SignInAsync("Cookie", authenticateInfo.Principal, 
                    authenticateInfo.Properties);

                // return new token
                return tokenResult.AccessToken;
            }
            
            throw new Exception("Problem occured during refreshing tokens.", 
                tokenResult.Exception);
        }
    }
}

