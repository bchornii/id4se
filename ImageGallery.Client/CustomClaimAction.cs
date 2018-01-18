using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Newtonsoft.Json.Linq;

namespace ImageGallery.Client
{
    public class CustomClaimAction : ClaimAction
    {
        private readonly string _claimType;

        public CustomClaimAction(string claimType, string valueType)
            : base(claimType, valueType)
        {
            _claimType = claimType;
        }

        public override void Run(JObject userData, ClaimsIdentity identity, string issuer)
        {
            var tokens = userData.SelectTokens(_claimType);
            var claimValues = new List<string>();

            foreach (var token in tokens)
            {
                if (token is JArray)
                {
                    var jarr = token as JArray;
                    claimValues.AddRange(jarr.Values<string>());
                }
                else
                {
                    claimValues.AddRange(new[]{token.Value<string>()});
                }

                foreach (var role in claimValues)
                {
                    var claim = new Claim(_claimType, role, ValueType, issuer);
                    if (!identity.HasClaim(c => c.Subject == claim.Subject && c.Value == claim.Value))
                    {
                        identity.AddClaim(claim);
                    }
                }
            }
        }
    }
}