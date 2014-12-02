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
                name: "ManageApiRoute",
                routeTemplate: "api/manage/{manageId}",
                defaults: new { controller = "Manage", manageId = RouteParameter.Optional }
            );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserVoteApiRoute",
                routeTemplate: "api/user/{userId}/vote/{voteId}",
                defaults: new { controller = "UserVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "UserPollVoteApiRoute",
                routeTemplate: "api/user/{userId}/poll/{pollId}/vote/{voteId}",
                defaults: new { controller = "UserPollVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "PollVoteApiRoute",
                routeTemplate: "api/poll/{pollId}/vote/{voteId}",
                defaults: new { controller = "PollVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "PollOptionApiRoute",
                routeTemplate: "api/poll/{pollId}/option/{optionId}",
                defaults: new { controller = "PollOption", optionId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManageVoteApiRoute",
                routeTemplate: "api/manage/{manageId}/vote/{voteId}",
                defaults: new { controller = "ManageVote", voteId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManageOptionApiRoute",
                routeTemplate: "api/manage/{manageId}/option/{optionId}",
                defaults: new { controller = "ManageOption", optionId = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}
