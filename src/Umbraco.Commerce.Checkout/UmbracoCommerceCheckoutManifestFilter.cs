//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Reflection;
//using Umbraco.Cms.Core.Manifest;

//namespace Umbraco.Commerce.Checkout
//{
//    public class UmbracoCommerceCheckoutManifestFilter : IManifestFilter
//    {
//        public void Filter(List<PackageManifest> manifests)
//        {
//            var manifest = new PackageManifest()
//            {
//                PackageId = "Umbraco.Commerce.Checkout",
//                PackageName = "Umbraco Commerce Checkout",
//                Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion?.Split('+')[0],
//                BundleOptions = BundleOptions.None,
//                AllowPackageTelemetry = true,
//                Dashboards = new ManifestDashboard[]
//                {
//                    new ManifestDashboard
//                    {
//                        Alias = "ucc",
//                        View = "/App_Plugins/UmbracoCommerceCheckout/backoffice/views/dashboards/installer.html",
//                        Sections = new string[] { "settings" }
//                    }
//                },
//                Scripts = new string[]
//                {
//                    "/App_Plugins/UmbracoCommerceCheckout/backoffice/js/uccheckout.js",
//                }
//            };

//            manifests.Add(manifest);
//        }
//    }
//}
