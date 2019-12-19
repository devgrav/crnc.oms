using System;
using System.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace Crnc.Oms.Notification.Gateway.Integration
{
    public class JwtAuthenticator
        : IAuthenticator
    {
        private readonly string _authHeader;

        public JwtAuthenticator(string accessToken)
        {
            if (accessToken == null)
            {
                throw new ArgumentNullException("accessToken");
            }

            _authHeader = string.Format("Bearer {0}", accessToken);
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            // only add the Authorization parameter if it hasn't been added by a previous Execute
            if (!request.Parameters.Any(p => p.Type.Equals(ParameterType.HttpHeader) &&
                                             p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            {
                request.AddParameter("Authorization", this._authHeader, ParameterType.HttpHeader);
            }
        }
    }
}