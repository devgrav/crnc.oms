﻿using System;
using System.Linq;
 using System.Security.Claims;
 using Crnc.Oms.Production.Domain.Aggregates.JobAggregate;
 using Crnc.Oms.Production.Domain.SeedWork;
 using Microsoft.AspNetCore.Http;

namespace Crnc.Oms.Production.WebApi.Authorization
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
                    var firstName = claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                    var lastName = claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                    FullName = new FullName(firstName, lastName);
                    Id = Guid.Parse(claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
                    Email = new Email(claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value);
                }
            }
        }

        public Guid Id { get; set; }

        public string AuthToken { get; }
        
        public Email Email { get; }

        public FullName FullName { get; }

        public string Login { get; }
        public bool IsAnonymous { get; }
    }
}