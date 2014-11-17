using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;

namespace VotingApplication.Web.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name:"UserVoteApiRoute",
                routeTemplate: "api/user/{userId}/vote/{voteId}",
                defaults: new { controller = "UserVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserSessionVoteApiRoute",
                routeTemplate: "api/user/{userId}/session/{sessionId}/vote/{voteId}",
                defaults: new { controller = "UserSessionVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SessionVoteApiRoute",
                routeTemplate: "api/session/{sessionId}/vote/{voteId}",
                defaults: new { controller = "SessionVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SessionOptionApiRoute",
                routeTemplate: "api/session/{sessionId}/option/{optionId}",
                defaults: new { controller = "SessionOption", optionId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManageVoteApiRoute",
                routeTemplate: "api/manage/{manageId}/vote/{voteId}",
                defaults: new { controller = "ManageVote", voteId = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}
