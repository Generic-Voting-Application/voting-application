using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using VotingApplication.Data.Context;
using VotingApplication.Web.Api.Migrations;

namespace VotingApplication.Web.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Fix the infinite recursion of Session.OptionSet.Options[0].OptionSets[0].Options[0].[...]
            // by not populating the Option.OptionSets after already encountering Session.OptionSet
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            //Enable automatic migrations
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<VotingContext, Configuration>());
            new VotingContext().Database.Initialize(false);
        }
    }
}
