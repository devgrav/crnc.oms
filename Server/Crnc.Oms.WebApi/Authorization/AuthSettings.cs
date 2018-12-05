using System;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.WebApi.Authorization
{
    public class AuthSettings
    {
        public int JwtLifetimeSeconds { get; set; }

        public string JwtBase64SymmetricKey { get; set; }

        public const string ISSUER = "OmsCrncAuthServer";
        public const string AUDIENCE = "http://localhost:64707";
        
        public static SymmetricSecurityKey GetSymmetricSecurityKey(string base64Key)
        {
            return new SymmetricSecurityKey(Convert.FromBase64String(base64Key));
        }
    }
}