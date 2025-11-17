using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Checkout.Pipeline
{
    public class InstallPipelineContext
    {
        public int SiteRootNodeId { get; set; }

        public StoreReadOnly Store { get; set; }

        public string CartPageUrl { get; set; }

        public string ConfirmationPageUrl { get; set; }
    }
}