using Crnc.Oms.Notification.Gateway.Integration;
using Microsoft.AspNetCore.Http;

namespace Crnc.Oms.Notification.Gateway.WebApi.Authorization
{
    public class CurrentUserContext
        : ICurrentUserContext
    {
        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            var authToken = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authToken))
                IsAnonymous = true;
            else
                AuthToken = authToken;
        }

        public string AuthToken { get; }

        public bool IsAnonymous { get; }
    }
}