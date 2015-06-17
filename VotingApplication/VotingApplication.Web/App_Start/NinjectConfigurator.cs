using System;
using System.Net;
using System.Web.Configuration;
using System.Web.Http;
using Ninject;
using VotingApplication.Data.Context;
using VotingApplication.Web.Api.Metrics;
using VotingApplication.Web.Api.Services;
using VotingApplication.Web.Api.Validators;

namespace VotingApplication.Web
{
    public class NinjectConfigurator
    {
        public void Configure(IKernel container)
        {
            // Add all bindings/dependencies
            AddBindings(container);

            // Use the container and our NinjectDependencyResolver as
            // application's resolver
            var resolver = new NinjectDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
        }

        private static void ConfigureMailSender(IKernel container)
        {
            IMailSender mailSender;

            // If SendMail is configured
            if (!String.IsNullOrEmpty(WebConfigurationManager.AppSettings["HostLogin"]))
            {
                var emailCredentials = new NetworkCredential(
                    WebConfigurationManager.AppSettings["HostLogin"],
                    WebConfigurationManager.AppSettings["HostPassword"]);

                mailSender = new SendMailEmailSender(emailCredentials, WebConfigurationManager.AppSettings["HostEmail"]);
            }
            else
            {
                mailSender = new Smtp4DevEmailSender("test@voting-app.com");
            }

            container.Bind<IMailSender>().ToMethod(_ => mailSender).InSingletonScope();

        }

        private static void AddBindings(IKernel container)
        {
            //Do Bindings here
            container.Bind<IContextFactory>().To<ContextFactory>();
            container.Bind<IVotingContext>().To<VotingContext>();
            container.Bind<IVoteValidatorFactory>().To<VoteValidatorFactory>();
            container.Bind<ICorrespondenceService>().To<CorrespondenceService>();
            container.Bind<IMetricHandler>().To<MetricHandler>();

            ConfigureMailSender(container);
        }

    }
}