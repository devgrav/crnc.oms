using System;
using System.Linq;
using Crnc.Oms.Notification.Gateway.Integration;
using Microsoft.AspNetCore.Http;

namespace Crnc.Oms.Notification.Gateway.WebApi.Authorization
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
            }
        }

        public string AuthToken { get; }

        public bool IsAnonymous { get; }
    }
}