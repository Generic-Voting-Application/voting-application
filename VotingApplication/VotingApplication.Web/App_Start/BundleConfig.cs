using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using System.Web.Optimization;

namespace VotingApplication.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var nullBuilder = new NullBuilder();
            var styleTransformer = new StyleTransformer();
            var scriptTransformer = new ScriptTransformer();
            var nullOrderer = new NullOrderer();

            StyleBundle styleBundle = new StyleBundle("~/Bundles/Style");
            styleBundle.IncludeDirectory("~/Content/Scss", "*.scss");
            styleBundle.Builder = nullBuilder;
            styleBundle.Transforms.Add(styleTransformer);
            styleBundle.Orderer = nullOrderer;
            bundles.Add(styleBundle);

            ScriptBundle scriptBundle = new ScriptBundle("~/Bundles/Script");
            scriptBundle.IncludeDirectory("~/Scripts/Controllers", "*.js");
            scriptBundle.Include("~/Scripts/VotingApp.js");
            scriptBundle.Builder = nullBuilder;
            scriptBundle.Transforms.Add(scriptTransformer);
            scriptBundle.Orderer = nullOrderer;
            bundles.Add(scriptBundle);

            BundleTable.EnableOptimizations = true;
        }
    }
}