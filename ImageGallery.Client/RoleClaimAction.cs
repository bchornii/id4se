using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Newtonsoft.Json.Linq;

namespace ImageGallery.Client
{
    public class RoleClaimAction : ClaimAction
    {
        public RoleClaimAction() 
            : base("role", ClaimValueTypes.String) {}

        public override void Run(JObject userData, ClaimsIdentity identity, string issuer)
        {
            var tokens = userData.SelectTokens("role");
            var roles = new List<string>();

            foreach (var token in tokens)
            {
                if (token is JArray)
                {
                    var jarr = token as JArray;
                    roles.AddRange(jarr.Values<string>());
                }
                else
                {
                    roles.AddRange(new[]{token.Value<string>()});
                }

                foreach (var role in roles)
                {
                    var claim = new Claim("role", role, ValueType, issuer);
                    if (!identity.HasClaim(c => c.Subject == claim.Subject && c.Value == claim.Value))
                    {
                        identity.AddClaim(claim);
                    }
                }
            }
        }
    }
}