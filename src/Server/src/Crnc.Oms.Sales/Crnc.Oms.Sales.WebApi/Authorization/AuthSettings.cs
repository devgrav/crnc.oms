using System;
using Microsoft.IdentityModel.Tokens;

namespace Crnc.Oms.Sales.WebApi.Authorization
{
    public class AuthSettings
    {
        public int JwtLifetimeSeconds { get; set; }
        public string JwtBase64SymmetricKey { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }

        public SymmetricSecurityKey SymmetricSecurityKey =>
            new SymmetricSecurityKey(Convert.FromBase64String(JwtBase64SymmetricKey));
    }
}