﻿using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace IDP
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",        // must be unique at the level of IDP
                    Username = "Frank",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "1, Main Road"),
                        new Claim("role", "FreeUser"),
                        new Claim("subscriptionlevel", "FreeUser"),
                        new Claim("country", "nl")
                    }
                },

                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "2, Big Street"),
                        new Claim("role", "PayingUser"),
                        new Claim("role", "Admin"),
                        new Claim("subscriptionlevel", "PayingUser"),
                        new Claim("country", "be")
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),  // this is required - it maps to subject id
                new IdentityResources.Profile(), // maps to profile related claims like given_name
                new IdentityResources.Address(),

                // Add new resource for role as far as this is not standard
                new IdentityResource("roles", "Your role(s)", new List<string>{"role"}),

                // Add county identity resourse - when requested country claim will be included
                new IdentityResource("country", "The country you're living in.",
                    new List<string>{"country"}),

                // Add subscriptionlevel identity resourse - when requested subscriptionlevel
                // will be included
                new IdentityResource("subscriptionlevel", "Your subscription level.",
                    new List<string>{"subscriptionlevel"})
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {                
                new ApiResource("imagegalleryapi", "Image Gallery API",
                    new List<string>{"role"})   // list of accociated user claims that should be included
                                                // when this resource is requested
                                                // claims will be included in access_token
                    {
                        ApiSecrets = { new Secret("apisecret".Sha256()) }
                    }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                // Mvc client
                new Client
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RequireConsent = true,

                    AccessTokenType = AccessTokenType.Reference,

                    // IdentityTokenLifetime = 300,
                    // AuthorizationCodeLifetime = 300,
                    AccessTokenLifetime = 120,  // api validation contains 5min because of delay for sync
                    // AbsoluteRefreshTokenLifetime = 2592000, // default is 30 days

                    // RefreshTokenExpiration = TokenExpiration.Sliding,
                    // SlidingRefreshTokenLifetime = 1296000, // default 15 days

                    // to refresh access token claims on refresh - this is useful
                    // for cases when user claims changes - without this setup changes will be
                    // reflected after refresh token expires - 30 days by default
                    UpdateAccessTokenClaimsOnRefresh = true,

                    AllowOfflineAccess = true,

                    AllowedGrantTypes = GrantTypes.Hybrid,

                    RedirectUris = { "https://localhost:44318/signin-oidc" },

                    PostLogoutRedirectUris = { "https://localhost:44318/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionlevel"
                    }
                },

                // Spa client
                new Client
                {
                    ClientId = "imagegalleryspa",
                    ClientName = "Image Gallery SPA",

                    RequireConsent = false,

                    // If the token leaves the company infrastructure (e.g. to a browser or a mobile device), 
                    // use reference tokens to be in complete control over lifetime. 
                    // If the token is used internally only, self contained tokens are fine.
                    AccessTokenType = AccessTokenType.Reference,

                    AllowedGrantTypes = GrantTypes.Implicit,

                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { "https://localhost:44347/callback.html" },
                    PostLogoutRedirectUris = { "https://localhost:44347/index.html" },
                    AllowedCorsOrigins = { "https://localhost:44347" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "imagegalleryapi"
                    }
                },

                // app client
                new Client
                {
                    ClientId = "imagegalleryappclient",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("imagegalleryappclientsecret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes =
                    {
                        "imagegalleryapi"
                    }
                }
            };
        }
    }
}
