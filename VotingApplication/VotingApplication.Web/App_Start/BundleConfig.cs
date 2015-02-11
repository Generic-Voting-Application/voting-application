using System.Web.Optimization;

namespace VotingApplication.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new LessBundle("~/Style")
                .IncludeDirectory("~/Content/Less", "*.less"));

            bundles.Add(new ScriptBundle("~/Scripts/VotingApp")
                .IncludeDirectory("~/Scripts/Controllers", "*.js")
                .Include("~/Scripts/VotingApp.js"));

            BundleTable.EnableOptimizations = true;
        }
    }
}