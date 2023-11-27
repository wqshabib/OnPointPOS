using System.Web;
using System.Web.Optimization;

namespace POSSUM.OrderingSystem
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                       "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));


            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
           "~/Scripts/bootstrap.js",
           "~/Scripts/bootstrap-typeahead.js",
           "~/Scripts/kendo.all.min.js",
           "~/Scripts/kendo.aspnetmvc.min.js"
           ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/icons.css",
                "~/Content/bootstrap.css",
                "~/Content/font-awesome.css",
                "~/Content/themes/base/kendo.common.min.css",
                "~/Content/themes/base/kendo.bootstrap.min.css",
                "~/Content/themes/base/kendo.dataviz.min.css",
                "~/Content/themes/base/kendo.dataviz.default.min.css",
                "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/bootstrapcss").Include("~/Content/bootstrap.css", "~/Content/bootstrap-theme.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                       "~/Content/themes/base/jquery.ui.core.css",
                       "~/Content/themes/base/jquery.ui.resizable.css",
                       "~/Content/themes/base/jquery.ui.selectable.css",
                       "~/Content/themes/base/jquery.ui.accordion.css",
                       "~/Content/themes/base/jquery.ui.autocomplete.css",
                       "~/Content/themes/base/jquery.ui.button.css",
                       "~/Content/themes/base/jquery.ui.dialog.css",
                       "~/Content/themes/base/jquery.ui.slider.css",
                       "~/Content/themes/base/jquery.ui.tabs.css",
                       "~/Content/themes/base/jquery.ui.datepicker.css",
                       "~/Content/themes/base/jquery.ui.progressbar.css",
                       "~/Content/themes/base/jquery.ui.theme.css"));

        }
    }
}
