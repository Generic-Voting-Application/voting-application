using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace VotingApplication.Web.Api.Filters
{
    public class BasicAuthenticator : Attribute, IAuthenticationFilter
    {
        private readonly string realm;
        public bool AllowMultiple { get { return false; } }

        public BasicAuthenticator(string realm)
        {
            this.realm = "realm=" + realm;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var req = context.Request;
            if (req.Headers.Authorization != null && req.Headers.Authorization.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase))
            {
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string credentials = encoding.GetString(Convert.FromBase64String(req.Headers.Authorization.Parameter));
                string[] credentialParts = credentials.Split(':');
                string user = credentialParts[0].Trim();
                string password = credentialParts[1].Trim();

                if (user == "admin" && password == "correcthorsebatterystaple")
                {
                    var claims = new List<Claim>() { new Claim(ClaimTypes.Name, user)};
                    var identity = new ClaimsIdentity(claims, "Basic");
                    var principal = new ClaimsPrincipal(new[] { identity });
                    context.Principal = principal;
                }
                else
                {
                    context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
                }
            }
            else
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            context.Result = new ResultWithChallenge(context.Result, realm);
            return Task.FromResult(0);
        }

        public class ResultWithChallenge : IHttpActionResult
        {
            private readonly IHttpActionResult next;
            private readonly string realm;

            public ResultWithChallenge(IHttpActionResult next, string realm)
            {
                this.next = next;
                this.realm = realm;
            }

            public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = await next.ExecuteAsync(cancellationToken);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", this.realm));
                }

                return response;
            }
        }
    }
}