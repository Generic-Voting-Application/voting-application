using BundleTransformer.Autoprefixer.PostProcessors;
using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.PostProcessors;
using BundleTransformer.Core.Transformers;
using System.Collections.Generic;
using System.Web.Optimization;

namespace VotingApplication.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            List<IPostProcessor> postProcessors = new List<IPostProcessor>();
            postProcessors.Add(new AutoprefixCssPostProcessor());
            var styleTransformer = new StyleTransformer(postProcessors);

            var nullBuilder = new NullBuilder();
            var scriptTransformer = new ScriptTransformer();
            var nullOrderer = new NullOrderer();

            StyleBundle styleBundle = new StyleBundle("~/Bundles/Style");
            styleBundle.IncludeDirectory("~/Content/Lib/Css", "*.css");
            styleBundle.Include("~/Content/Site.scss");
            styleBundle.Builder = nullBuilder;
            styleBundle.Transforms.Add(styleTransformer);
            styleBundle.Orderer = nullOrderer;
            bundles.Add(styleBundle);

            StyleBundle votingBundle = new StyleBundle("~/Bundles/VotingStyle");
            votingBundle.Include("~/Content/Site.scss");
            votingBundle.Include("~/Content/Scss/Voting.scss");
            votingBundle.Builder = nullBuilder;
            votingBundle.Transforms.Add(styleTransformer);
            votingBundle.Orderer = nullOrderer;
            bundles.Add(votingBundle);

            StyleBundle createBundle = new StyleBundle("~/Bundles/CreateStyle");
            createBundle.Include("~/Content/Site.scss");
            createBundle.Include("~/Content/Scss/Creation.scss");
            createBundle.Builder = nullBuilder;
            createBundle.Transforms.Add(styleTransformer);
            createBundle.Orderer = nullOrderer;
            bundles.Add(createBundle);

            ScriptBundle scriptBundle = new ScriptBundle("~/Bundles/Script");
            scriptBundle.Include("~/Scripts/VotingApp.js");
            scriptBundle.IncludeDirectory("~/Scripts/Lib", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Directives", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Services", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Controllers", "*.js");
            scriptBundle.Builder = nullBuilder;
            scriptBundle.Transforms.Add(scriptTransformer);
            scriptBundle.Orderer = nullOrderer;
            bundles.Add(scriptBundle);

            BundleTable.EnableOptimizations = false;
        }
    }
}
