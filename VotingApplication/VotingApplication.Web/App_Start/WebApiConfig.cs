using Microsoft.Owin.Security.OAuth;
using System.Data.Entity.Migrations;
using System.Net.Http.Headers;
using System.Web.Http;
using VotingApplication.Data.Migrations;

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

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ManageApiRoute",
                routeTemplate: "api/manage/{manageId}",
                defaults: new { controller = "Manage", manageId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "PollVoteApiRoute",
                routeTemplate: "api/poll/{pollId}/token/{tokenGuid}/vote",
                defaults: new { controller = "PollVote" }
            );

            config.Routes.MapHttpRoute(
                name: "PollResultsApiRoute",
                routeTemplate: "api/poll/{pollId}/results",
                defaults: new { controller = "PollResults" }
            );

            config.Routes.MapHttpRoute(
                name: "PollChoiceApiRoute",
                routeTemplate: "api/poll/{pollId}/choice",
                defaults: new { controller = "PollChoice" }
            );

            config.Routes.MapHttpRoute(
                name: "ManageVotersApiRoute",
                routeTemplate: "api/manage/{manageId}/voters/{ballotManageId}",
                defaults: new { controller = "ManageVoter", ballotManageId = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ManageChoiceApiRoute",
                routeTemplate: "api/manage/{manageId}/choice",
                defaults: new { controller = "ManageChoice" }
            );

            config.Routes.MapHttpRoute(
                name: "ManageInvitationApiRoute",
                routeTemplate: "api/manage/{manageId}/invitation/",
                defaults: new { controller = "ManageInvitation" }
            );

            config.Routes.MapHttpRoute(
                name: "ManageExpiryApiRoute",
                routeTemplate: "api/manage/{manageId}/expiry/",
                defaults: new { controller = "ManageExpiry" }
            );

            config.Routes.MapHttpRoute(
                name: "ManagePollTypeApiRoute",
                routeTemplate: "api/manage/{manageId}/pollType/",
                defaults: new { controller = "ManagePollType" }
            );

            config.Routes.MapHttpRoute(
                name: "ManageQuestionApiRoute",
                routeTemplate: "api/manage/{manageId}/question/",
                defaults: new { controller = "ManageQuestion" }
            );

            config.Routes.MapHttpRoute(
                name: "ManageMiscApiRoute",
                routeTemplate: "api/manage/{manageId}/misc/",
                defaults: new { controller = "ManageMisc" }
            );

            config.Routes.MapHttpRoute(
                name: "PollTokenApiRoute",
                routeTemplate: "api/poll/{pollId}/token/",
                defaults: new { controller = "PollToken" }
            );

            config.Routes.MapHttpRoute(
                name: "DashboardRoute",
                routeTemplate: "api/dashboard/{action}/{id}",
                defaults: new { controller = "Dashboard", id = RouteParameter.Optional }
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
