using Microsoft.Owin.Security.OAuth;
using System.Data.Entity.Migrations;
using System.Net.Http.Headers;
using System.Web.Http;
using VotingApplication.Data.Migrations;
using VotingApplication.Web.Api.Filters;

namespace VotingApplication.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
            config.Filters.Add(new LoggingExceptionFilterAttribute());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ManageApiRoute",
                routeTemplate: "api/manage/{manageId}",
                defaults: new { controller = "Manage", manageId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "TokenPollVoteApiRoute",
                routeTemplate: "api/token/{tokenGuid}/poll/{pollId}/vote/{voteId}",
                defaults: new { controller = "TokenPollVote", voteId = RouteParameter.Optional }
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

            config.Routes.MapHttpRoute(
                name: "ManageInvitationApiRoute",
                routeTemplate: "api/manage/{manageId}/invitation/{invitationId}",
                defaults: new { controller = "ManageInvitation", invitationId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "PollTokenApiRoute",
                routeTemplate: "api/poll/{pollId}/token/",
                defaults: new { controller = "PollToken" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            var migrator = new DbMigrator(new Configuration());
            migrator.Update();
        }
    }
}
