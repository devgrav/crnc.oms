using System;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.Security.WebApi.Authorization
{
    public class AuthSettings
    {
        public int JwtLifetimeSeconds { get; set; }
        public string JwtBase64SymmetricKey { get; set; }
        public string JwtIssuer = "OmsCrncAuthServer";
        public string JwtAudience = "OmsCrncApis";

        public SymmetricSecurityKey SymmetricSecurityKey =>
            new SymmetricSecurityKey(Convert.FromBase64String(JwtBase64SymmetricKey));
    }
}