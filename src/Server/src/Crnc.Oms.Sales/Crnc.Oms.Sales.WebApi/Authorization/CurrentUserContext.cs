﻿using System;
using System.Linq;
 using System.Security.Claims;
 using Crnc.Oms.Sales.Domain.SeedWork;
 using Microsoft.AspNetCore.Http;

namespace Crnc.Oms.Sales.WebApi.Authorization
{
    public class CurrentUserContext
        : ICurrentUserContext
    {
        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            var authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var authHeaderValue = authHeader.ToString();
            
            if (string.IsNullOrWhiteSpace(authHeaderValue))
                IsAnonymous = true;
            else
            {
                AuthToken = authHeaderValue.Split(" ", StringSplitOptions.RemoveEmptyEntries).Last();

                var claims = httpContextAccessor.HttpContext.User.Claims;

                if (claims != null && claims.Any())
                {
                    Login = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                    FirstName = claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                    LastName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                }
            }
        }

        public string AuthToken { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string FullName => $"{FirstName} {LastName}";
        public string Login { get; }
        public bool IsAnonymous { get; }
    }
}