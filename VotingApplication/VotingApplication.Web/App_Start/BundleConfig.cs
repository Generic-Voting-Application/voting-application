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

            StyleBundle dateTimePickerStyle = new StyleBundle("~/Bundles/DateTimePickerStyle");
            dateTimePickerStyle.Include("~/Content/Scss/DateTimePicker.scss");
            dateTimePickerStyle.Builder = nullBuilder;
            dateTimePickerStyle.Transforms.Add(styleTransformer);
            dateTimePickerStyle.Orderer = nullOrderer;
            bundles.Add(dateTimePickerStyle);

            StyleBundle resultsChartStyle = new StyleBundle("~/Bundles/ResultsChartStyle");
            resultsChartStyle.Include("~/Content/Scss/ResultsChart.scss");
            resultsChartStyle.Builder = nullBuilder;
            resultsChartStyle.Transforms.Add(styleTransformer);
            resultsChartStyle.Orderer = nullOrderer;
            bundles.Add(resultsChartStyle);

            StyleBundle errorBarStyle = new StyleBundle("~/Bundles/ErrorBarStyle");
            errorBarStyle.Include("~/Content/Scss/ErrorBar.scss");
            errorBarStyle.Builder = nullBuilder;
            errorBarStyle.Transforms.Add(styleTransformer);
            errorBarStyle.Orderer = nullOrderer;
            bundles.Add(errorBarStyle);

            StyleBundle pollInfoBoxStyle = new StyleBundle("~/Bundles/PollInfoBox");
            pollInfoBoxStyle.Include("~/Content/Scss/PollInfoBox.scss");
            pollInfoBoxStyle.Builder = nullBuilder;
            pollInfoBoxStyle.Transforms.Add(styleTransformer);
            pollInfoBoxStyle.Orderer = nullOrderer;
            bundles.Add(pollInfoBoxStyle);

            ScriptBundle scriptLibBundle = new ScriptBundle("~/Bundles/ScriptLib");
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "*.js");
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "*.js.map");
            scriptLibBundle.Builder = nullBuilder;
            scriptLibBundle.Transforms.Add(scriptTransformer);
            scriptLibBundle.Orderer = nullOrderer;
            bundles.Add(scriptLibBundle);

            ScriptBundle scriptBundle = new ScriptBundle("~/Bundles/Script");
            scriptBundle.IncludeDirectory("~/Scripts/Modules", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Directives", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Services", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Controllers", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Filters", "*.js");
            scriptBundle.IncludeDirectory("~/Scripts/Interceptors", "*.js");
            scriptBundle.Builder = nullBuilder;
            scriptBundle.Transforms.Add(scriptTransformer);
            scriptBundle.Orderer = nullOrderer;
            bundles.Add(scriptBundle);

            BundleTable.EnableOptimizations = false;
        }
    }
}
