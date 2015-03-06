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

            StyleBundle styleLib = new StyleBundle("~/Bundles/StyleLib");
            styleLib.IncludeDirectory("~/Content/Lib/Css", "*.css");
            styleLib.Builder = nullBuilder;
            styleLib.Transforms.Add(styleTransformer);
            styleLib.Orderer = nullOrderer;
            bundles.Add(styleLib);

            StyleBundle votingStyle = new StyleBundle("~/Bundles/VotingStyle");
            votingStyle.Include("~/Content/Scss/Voting.scss");
            votingStyle.Builder = nullBuilder;
            votingStyle.Transforms.Add(styleTransformer);
            votingStyle.Orderer = nullOrderer;
            bundles.Add(votingStyle);

            StyleBundle createStyle = new StyleBundle("~/Bundles/CreateStyle");
            createStyle.Include("~/Content/Scss/Creation.scss");
            createStyle.Builder = nullBuilder;
            createStyle.Transforms.Add(styleTransformer);
            createStyle.Orderer = nullOrderer;
            bundles.Add(createStyle);

            ScriptBundle scriptLibBundle = new ScriptBundle("~/Bundles/ScriptLib");
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "*.js");
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "*.js.map");
            scriptLibBundle.Builder = nullBuilder;
            scriptLibBundle.Transforms.Add(scriptTransformer);
            scriptLibBundle.Orderer = nullOrderer;
            bundles.Add(scriptLibBundle);

            ScriptBundle scriptBundle = new ScriptBundle("~/Bundles/Script");
            scriptBundle.Include("~/Scripts/GVA-Voting.js");
            scriptBundle.Include("~/Scripts/GVA-Creation.js");
            scriptBundle.Include("~/Scripts/GVA-Common.js");
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
