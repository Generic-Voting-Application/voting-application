using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using VotingApplication.Web.Common;
using Microsoft.AspNet.SignalR;
using VotingApplication.Web.Api.App_Start;

[assembly: OwinStartup(typeof(VotingApplication.Web.Api.Startup))]

namespace VotingApplication.Web.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Setup the SignalR connections using a Ninject resolver
            var config = new HubConfiguration();
            config.Resolver = NinjectConfigurator.SignalRResolver;
            app.MapSignalR(config);
        }
    }
}
