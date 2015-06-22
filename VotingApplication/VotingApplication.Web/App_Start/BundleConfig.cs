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
            bundles.UseCdn = true;

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

            StyleBundle manageStyle = new StyleBundle("~/Bundles/ManageStyle");
            manageStyle.Include("~/Content/Scss/Manage.scss");
            manageStyle.Builder = nullBuilder;
            manageStyle.Transforms.Add(styleTransformer);
            manageStyle.Orderer = nullOrderer;
            bundles.Add(manageStyle);

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

            ScriptBundle scriptLibBundle = new ScriptBundle("~/Bundles/ScriptLib");
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "*.js");
            scriptLibBundle.Builder = nullBuilder;
            scriptLibBundle.Transforms.Add(scriptTransformer);
            scriptLibBundle.Orderer = nullOrderer;
            bundles.Add(scriptLibBundle);

            const string angularCdnBase = "https://ajax.googleapis.com/ajax/libs/angularjs/1.3.13";

            var angular = new Bundle("~/Bundles/ScriptLib/Angular", string.Format("{0}/angular.min.js", angularCdnBase));
            angular.Include("~/Scripts/Lib/angular-min.js");
            bundles.Add(angular);

            var angularRoute = new Bundle("~/Bundles/ScriptLib/AngularRoute", string.Format("{0}/angular-route.min.js", angularCdnBase));
            angularRoute.Include("~/Scripts/Lib/angular-route-min.js");
            bundles.Add(angularRoute);

            var angularQr = new ScriptBundle("~/Bundles/ScriptLib/AngularQr", "https://cdn.rawgit.com/monospaced/angular-qrcode/5.1.0/qrcode.js");
            angularQr.Include("~/Scripts/Lib/angular-qrcode.js");
            bundles.Add(angularQr);

            var d3 = new Bundle("~/Bundles/ScriptLib/d3", "https://cdnjs.cloudflare.com/ajax/libs/d3/3.5.5/d3.min.js");
            d3.Include("~/Scripts/Lib/d3-min.js");
            bundles.Add(d3);

            var d3tip = new ScriptBundle("~/Bundles/ScriptLib/d3tip", "https://cdn.rawgit.com/Caged/d3-tip/v0.6.7/index.js");
            d3tip.Include("~/Scripts/Lib/d3-tip.js");
            bundles.Add(d3tip);

            var moment = new Bundle("~/Bundles/ScriptLib/moment", "https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.9.0/moment.min.js");
            moment.Include("~/Scripts/Lib/moment-min.js");
            bundles.Add(moment);

            var ngDialog = new Bundle("~/Bundles/ScriptLib/ngDialog", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.3.11/js/ngDialog.min.js");
            ngDialog.Include("~/Scripts/Lib/ngDialog-min.js");
            bundles.Add(ngDialog);

            var ngStorage = new Bundle("~/Bundles/ScriptLib/ngStorage", "https://cdnjs.cloudflare.com/ajax/libs/ngStorage/0.3.0/ngStorage.min.js");
            ngStorage.Include("~/Scripts/Lib/ngStorage-min.js");
            bundles.Add(ngStorage);

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
        }
    }
}
