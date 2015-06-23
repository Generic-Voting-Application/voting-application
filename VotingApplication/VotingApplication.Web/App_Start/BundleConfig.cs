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

            // Lib CSS
            StyleBundle angularMaterialCss = new StyleBundle("~/Bundles/StyleLib/AngularMaterial", "https://ajax.googleapis.com/ajax/libs/angular_material/0.10.0/angular-material.min.css");
            angularMaterialCss.Include("~/Content/Lib/Css/angular-material-min.css");
            bundles.Add(angularMaterialCss);

            StyleBundle fontAwesomeCss = new StyleBundle("~/Bundles/StyleLib/FontAwesome", "//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css");
            fontAwesomeCss.Include("~/Content/Lib/Css/font-awesome-min.css");
            bundles.Add(fontAwesomeCss);

            StyleBundle angularToggleSwitchCss = new StyleBundle("~/Bundles/StyleLib/AngularToggleSwitch", "https://cdn.rawgit.com/cgarvis/angular-toggle-switch/v1.3.0/angular-toggle-switch.css");
            angularToggleSwitchCss.Include("~/Content/Lib/Css/angular-toggle-switch.css");
            bundles.Add(angularToggleSwitchCss);

            StyleBundle ngDialogCss = new StyleBundle("~/Bundles/StyleLib/ngDialog", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.3.11/css/ngDialog.min.css");
            ngDialogCss.Include("~/Content/Lib/Css/ngDialog-min.css");
            bundles.Add(ngDialogCss);

            StyleBundle ngDialogThemeCss = new StyleBundle("~/Bundles/StyleLib/ngDialogTheme", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.3.11/css/ngDialog-theme-default.min.css");
            ngDialogThemeCss.Include("~/Content/Lib/Css/ngDialog-theme-default-min.css");
            bundles.Add(ngDialogThemeCss);


            // VoteOn CSS
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
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "ZeroClipboard-min.js");
            scriptLibBundle.Builder = nullBuilder;
            scriptLibBundle.Transforms.Add(scriptTransformer);
            scriptLibBundle.Orderer = nullOrderer;
            bundles.Add(scriptLibBundle);


            // Lib Javascript
            const string angularCdnBase = "https://ajax.googleapis.com/ajax/libs/angularjs/1.3.13";

            // Angular
            var angular = new Bundle("~/Bundles/ScriptLib/Angular", string.Format("{0}/angular.min.js", angularCdnBase));
            angular.Include("~/Scripts/Lib/angular-min.js");
            bundles.Add(angular);

            var angularRoute = new Bundle("~/Bundles/ScriptLib/AngularRoute", string.Format("{0}/angular-route.min.js", angularCdnBase));
            angularRoute.Include("~/Scripts/Lib/angular-route-min.js");
            bundles.Add(angularRoute);


            // Angular Material and dependencies
            var angularAnimate = new Bundle("~/Bundles/ScriptLib/AngularAnimate", string.Format("{0}/angular-animate.min.js", angularCdnBase));
            angularAnimate.Include("~/Scripts/Lib/angular-animate-min.js");
            bundles.Add(angularAnimate);

            var angularAria = new Bundle("~/Bundles/ScriptLib/AngularAnimate", string.Format("{0}/angular-aria.min.js", angularCdnBase));
            angularAria.Include("~/Scripts/Lib/angular-aria-min.js");
            bundles.Add(angularAria);

            var angularMaterial = new Bundle("~/Bundles/ScriptLib/AngularMaterial", "https://ajax.googleapis.com/ajax/libs/angular_material/0.10.0/angular-material.min.js");
            angularMaterial.Include("~/Scripts/Lib/angular-material-min.js");
            bundles.Add(angularMaterial);



            var angularQr = new ScriptBundle("~/Bundles/ScriptLib/AngularQr", "https://cdn.rawgit.com/monospaced/angular-qrcode/5.1.0/qrcode.js");
            angularQr.Include("~/Scripts/Lib/angular-qrcode.js");
            bundles.Add(angularQr);

            var qrcode = new ScriptBundle("~/Bundles/ScriptLib/qrcode", "https://cdn.rawgit.com/kazuhikoarase/qrcode-generator/v20140808/js/qrcode.js");
            qrcode.Include("~/Scripts/Lib/qrcode.js");
            bundles.Add(qrcode);

            var angularToggleSwitch = new Bundle("~/Bundles/ScriptLib/AngularToggleSwitch", "https://cdn.rawgit.com/cgarvis/angular-toggle-switch/v1.3.0/angular-toggle-switch.min.js");
            angularToggleSwitch.Include("~/Scripts/Lib/angular-toggle-switch-min.js");
            bundles.Add(angularToggleSwitch);

            var angularZeroClipboard = new ScriptBundle("~/Bundles/ScriptLib/AngularZeroClipboard", "https://cdn.rawgit.com/lisposter/angular-zeroclipboard/v0.4.3/src/angular-zeroclipboard.js");
            angularZeroClipboard.Include("~/Scripts/Lib/angular-zeroclipboard.js");
            bundles.Add(angularZeroClipboard);

            var ngDialog = new Bundle("~/Bundles/ScriptLib/ngDialog", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.3.11/js/ngDialog.min.js");
            ngDialog.Include("~/Scripts/Lib/ngDialog-min.js");
            bundles.Add(ngDialog);

            var ngStorage = new Bundle("~/Bundles/ScriptLib/ngStorage", "https://cdnjs.cloudflare.com/ajax/libs/ngStorage/0.3.0/ngStorage.min.js");
            ngStorage.Include("~/Scripts/Lib/ngStorage-min.js");
            bundles.Add(ngStorage);

            // d3
            var d3 = new Bundle("~/Bundles/ScriptLib/d3", "https://cdnjs.cloudflare.com/ajax/libs/d3/3.5.5/d3.min.js");
            d3.Include("~/Scripts/Lib/d3-min.js");
            bundles.Add(d3);

            var d3tip = new ScriptBundle("~/Bundles/ScriptLib/d3tip", "https://cdn.rawgit.com/Caged/d3-tip/v0.6.7/index.js");
            d3tip.Include("~/Scripts/Lib/d3-tip.js");
            bundles.Add(d3tip);

            // moment
            var moment = new Bundle("~/Bundles/ScriptLib/moment", "https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.9.0/moment.min.js");
            moment.Include("~/Scripts/Lib/moment-min.js");
            bundles.Add(moment);



            // VoteOn Javascript
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
