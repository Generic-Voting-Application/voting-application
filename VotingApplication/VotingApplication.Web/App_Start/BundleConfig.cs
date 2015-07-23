using BundleTransformer.Autoprefixer.PostProcessors;
using BundleTransformer.CleanCss.Minifiers;
using BundleTransformer.Core.PostProcessors;
using BundleTransformer.Core.Transformers;
using BundleTransformer.UglifyJs.Minifiers;
using System;
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

            var styleTransformer = new StyleTransformer(new CleanCssMinifier(), postProcessors);

            var scriptTransformer = new ScriptTransformer(new UglifyJsMinifier());

            // No fallback for Css style sheets if the cdn fails.
            // See http://aspnetoptimization.codeplex.com/workitem/104

            // Lib CSS
            var angularMaterialCss = new Bundle("~/Bundles/StyleLib/AngularMaterial", "https://ajax.googleapis.com/ajax/libs/angular_material/0.10.0/angular-material.min.css");
            angularMaterialCss.Include("~/Content/Lib/Css/angular-material-min.css");
            bundles.Add(angularMaterialCss);

            var fontAwesomeCss = new Bundle("~/Bundles/StyleLib/FontAwesome", "//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css");
            fontAwesomeCss.Include("~/Content/Lib/Css/font-awesome-min.css");
            bundles.Add(fontAwesomeCss);

            var angularToggleSwitchCss = new Bundle("~/Bundles/StyleLib/AngularToggleSwitch", "https://cdn.rawgit.com/cgarvis/angular-toggle-switch/v1.3.0/angular-toggle-switch.css");
            angularToggleSwitchCss.Include("~/Content/Lib/Css/angular-toggle-switch.css");
            bundles.Add(angularToggleSwitchCss);

            var ngDialogCss = new Bundle("~/Bundles/StyleLib/ngDialog", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.4.0/css/ngDialog.min.css");
            ngDialogCss.Include("~/Content/Lib/Css/ngDialog-min.css");
            bundles.Add(ngDialogCss);

            var ngDialogThemeCss = new Bundle("~/Bundles/StyleLib/ngDialogTheme", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.4.0/css/ngDialog-theme-default.min.css");
            ngDialogThemeCss.Include("~/Content/Lib/Css/ngDialog-theme-default-min.css");
            bundles.Add(ngDialogThemeCss);

            // VoteOn CSS
            var votingStyle = new Bundle("~/Bundles/VotingStyle");
            votingStyle.Include("~/Content/Scss/Voting.scss");
            votingStyle.Transforms.Add(styleTransformer);
            bundles.Add(votingStyle);

            var manageStyle = new Bundle("~/Bundles/ManageStyle");
            manageStyle.Include("~/Content/Scss/Manage.scss");
            manageStyle.Transforms.Add(styleTransformer);
            bundles.Add(manageStyle);

            var dateTimePickerStyle = new Bundle("~/Bundles/DateTimePickerStyle");
            dateTimePickerStyle.Include("~/Content/Scss/DateTimePicker.scss");
            dateTimePickerStyle.Transforms.Add(styleTransformer);
            bundles.Add(dateTimePickerStyle);

            var errorBarStyle = new Bundle("~/Bundles/ErrorBarStyle");
            errorBarStyle.Include("~/Content/Scss/ErrorBar.scss");
            errorBarStyle.Transforms.Add(styleTransformer);
            bundles.Add(errorBarStyle);

            var voteOnStyle = new Bundle("~/Bundles/VoteOnStyle");
            voteOnStyle.Include("~/Content/Scss/VoteOn.scss");
            voteOnStyle.Transforms.Add(styleTransformer);
            bundles.Add(voteOnStyle);

            var angularMaterialExtensions = new Bundle("~/Bundles/AngularMaterialExtensions");
            angularMaterialExtensions.Include("~/Content/Scss/AngularMaterialExtensions.scss");
            angularMaterialExtensions.Transforms.Add(styleTransformer);
            bundles.Add(angularMaterialExtensions);

            var dateTimePicker = new Bundle("~/Bundles/Components/DateTimePicker");
            dateTimePicker.Include("~/Content/Scss/Components/DateTimePicker.scss");
            dateTimePicker.Transforms.Add(styleTransformer);
            bundles.Add(dateTimePicker);


            // Lib Javascript
            const string angularCdnBase = "https://ajax.googleapis.com/ajax/libs/angularjs/1.3.15";

            // Angular
            var angular = new Bundle("~/Bundles/ScriptLib/Angular", String.Format("{0}/angular.min.js", angularCdnBase));
            angular.CdnFallbackExpression = "window.angular";
            angular.Include("~/Scripts/Lib/angular.min.js");
            bundles.Add(angular);

            var angularRoute = new Bundle("~/Bundles/ScriptLib/AngularRoute", String.Format("{0}/angular-route.min.js", angularCdnBase));
            angularRoute.CdnFallbackExpression = AngularModuleFallbackCheck("ngRoute");
            angularRoute.Include("~/Scripts/Lib/angular-route.min.js");
            bundles.Add(angularRoute);

            var angularMessages = new Bundle("~/Bundles/ScriptLib/AngularMessages", string.Format("{0}/angular-messages.min.js", angularCdnBase));
            angularMessages.CdnFallbackExpression = AngularModuleFallbackCheck("ngMessages");
            angularMessages.Include("~/Scripts/Lib/angular-messages.min.js");
            bundles.Add(angularMessages);

            // Angular Material and dependencies
            var angularAnimate = new Bundle("~/Bundles/ScriptLib/AngularAnimate", String.Format("{0}/angular-animate.min.js", angularCdnBase));
            angularAnimate.CdnFallbackExpression = AngularModuleFallbackCheck("ngAnimate");
            angularAnimate.Include("~/Scripts/Lib/angular-animate.min.js");
            bundles.Add(angularAnimate);

            var angularAria = new Bundle("~/Bundles/ScriptLib/AngularAria", String.Format("{0}/angular-aria.min.js", angularCdnBase));
            angularAria.CdnFallbackExpression = AngularModuleFallbackCheck("ngAria");
            angularAria.Include("~/Scripts/Lib/angular-aria.min.js");
            bundles.Add(angularAria);

            var angularMaterial = new Bundle("~/Bundles/ScriptLib/AngularMaterial", "https://ajax.googleapis.com/ajax/libs/angular_material/0.10.0/angular-material.min.js");
            angularMaterial.CdnFallbackExpression = AngularModuleFallbackCheck("ngMaterial");
            angularMaterial.Include("~/Scripts/Lib/angular-material.min.js");
            bundles.Add(angularMaterial);

            var angularCharts = new Bundle("~/Bundles/ScriptLib/AngularCharts", "https://cdn.rawgit.com/bouil/angular-google-chart/0.0.11/ng-google-chart.js");
            angularCharts.CdnFallbackExpression = AngularModuleFallbackCheck("googlechart");
            angularCharts.Include("~/Scripts/Lib/ng-google-chart.js");
            angularCharts.Transforms.Add(scriptTransformer);
            bundles.Add(angularCharts);

            var angularQr = new Bundle("~/Bundles/ScriptLib/AngularQr", "https://cdn.rawgit.com/monospaced/angular-qrcode/5.1.0/qrcode.js");
            angularQr.CdnFallbackExpression = AngularModuleFallbackCheck("monospaced.qrcode");
            angularQr.Include("~/Scripts/Lib/angular-qrcode.js");
            angularQr.Transforms.Add(scriptTransformer);
            bundles.Add(angularQr);

            var angularSignalR = new Bundle("~/Bundles/ScriptLib/AngularSignalR", "https://cdn.rawgit.com/JustMaier/angular-signalr-hub/v1.5.0/signalr-hub.min.js");
            angularSignalR.CdnFallbackExpression = AngularModuleFallbackCheck("SignalR");
            angularSignalR.Include("~/Scripts/Lib/signalr-hub.min.js");
            bundles.Add(angularSignalR);

            var qrcode = new Bundle("~/Bundles/ScriptLib/qrcode", "https://cdn.rawgit.com/kazuhikoarase/qrcode-generator/v20140808/js/qrcode.js");
            qrcode.CdnFallbackExpression = "window.qrcode";
            qrcode.Include("~/Scripts/Lib/qrcode.js");
            qrcode.Transforms.Add(scriptTransformer);
            bundles.Add(qrcode);

            var angularToggleSwitch = new Bundle("~/Bundles/ScriptLib/AngularToggleSwitch", "https://cdn.rawgit.com/cgarvis/angular-toggle-switch/v1.3.0/angular-toggle-switch.min.js");
            angularToggleSwitch.CdnFallbackExpression = AngularModuleFallbackCheck("toggle-switch");
            angularToggleSwitch.Include("~/Scripts/Lib/angular-toggle-switch.min.js");
            bundles.Add(angularToggleSwitch);

            var angularZeroClipboard = new Bundle("~/Bundles/ScriptLib/AngularZeroClipboard", "https://cdn.rawgit.com/lisposter/angular-zeroclipboard/v0.4.3/src/angular-zeroclipboard.js");
            angularZeroClipboard.CdnFallbackExpression = AngularModuleFallbackCheck("zeroclipboard");
            angularZeroClipboard.Include("~/Scripts/Lib/angular-zeroclipboard.js");
            angularZeroClipboard.Transforms.Add(scriptTransformer);
            bundles.Add(angularZeroClipboard);

            var ngDialog = new Bundle("~/Bundles/ScriptLib/ngDialog", "https://cdnjs.cloudflare.com/ajax/libs/ng-dialog/0.4.0/js/ngDialog.min.js");
            ngDialog.CdnFallbackExpression = AngularModuleFallbackCheck("ngDialog");
            ngDialog.Include("~/Scripts/Lib/ngDialog.min.js");
            bundles.Add(ngDialog);

            var ngStorage = new Bundle("~/Bundles/ScriptLib/ngStorage", "https://cdnjs.cloudflare.com/ajax/libs/ngStorage/0.3.6/ngStorage.min.js");
            ngStorage.CdnFallbackExpression = AngularModuleFallbackCheck("ngStorage");
            ngStorage.Include("~/Scripts/Lib/ngStorage.min.js");
            bundles.Add(ngStorage);

            // JQuery and SignalR
            var jQuery = new Bundle("~/Bundles/ScriptLib/JQuery", "//code.jquery.com/jquery-2.1.4.min.js");
            jQuery.CdnFallbackExpression = "window.jQuery";
            jQuery.Include("~/Scripts/Lib/jquery-2.1.4.min.js");
            bundles.Add(jQuery);

            var jQuerySignalR = new Bundle("~/Bundles/ScriptLib/JQuerySignalR", "https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-2.2.0.min.js");
            jQuerySignalR.CdnFallbackExpression = "window.jQuery.signalR";
            jQuerySignalR.Include("~/Scripts/Lib/jquery.signalr-2.2.0.min.js");
            bundles.Add(jQuerySignalR);

            // moment
            var moment = new Bundle("~/Bundles/ScriptLib/moment", "https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.10.3/moment.min.js");
            moment.CdnFallbackExpression = "window.moment";
            moment.Include("~/Scripts/Lib/moment.min.js");
            bundles.Add(moment);

            // ZeroClipboard
            var scriptLibBundle = new Bundle("~/Bundles/ScriptLib");
            scriptLibBundle.IncludeDirectory("~/Scripts/Lib", "ZeroClipboard.min.js");
            bundles.Add(scriptLibBundle);

            // VoteOn Javascript
            var scriptBundle = new Bundle("~/Bundles/Script");
            scriptBundle.IncludeDirectory("~/Scripts/Modules", "*.js", true);
            scriptBundle.IncludeDirectory("~/Scripts/Dialogs", "*.js", true);
            scriptBundle.IncludeDirectory("~/Scripts/Directives", "*.js", true);
            scriptBundle.IncludeDirectory("~/Scripts/Services", "*.js", true);
            scriptBundle.IncludeDirectory("~/Scripts/Controllers", "*.js", true);
            scriptBundle.IncludeDirectory("~/Scripts/Filters", "*.js", true);
            scriptBundle.Transforms.Add(scriptTransformer);
            bundles.Add(scriptBundle);
        }

        private static string AngularModuleFallbackCheck(string module)
        {
            return @"
                    function() { 
                        try { 
                            window.angular.module('" + module + @"');
                        } catch(e) {
                            return false;
                        } 
                        return true;
                    })(";
        }
    }
}
