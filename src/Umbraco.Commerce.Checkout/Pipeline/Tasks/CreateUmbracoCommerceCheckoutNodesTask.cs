using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutNodesTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IContentTypeService _contentTypeService;
        private readonly IContentService _contentService;

        public CreateUmbracoCommerceCheckoutNodesTask(IScopeProvider scopeProvider,
            IContentTypeService contentTypeService,
            IContentService contentService)
        {
            _scopeProvider = scopeProvider;
            _contentTypeService = contentTypeService;
            _contentService = contentService;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var uccCheckoutPageContenType = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage);
                var uccCheckoutStepPageContenType = _contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);

                // Check to see if the checkout node already exists
                var filter = scope.SqlContext.Query<IContent>().Where(x => x.ContentTypeId == uccCheckoutPageContenType.Id);
                var childNodes = _contentService.GetPagedChildren(args.Model.SiteRootNodeId, 1, 1, out long totalRecords, filter);

                if (totalRecords == 0)
                {
                    // Create the checkout page
                    var checkoutNode = _contentService.CreateAndSave("Checkout", args.Model.SiteRootNodeId,
                        UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage);

                    // Create the checkout steps pages
                    CreateCheckoutStepPage(checkoutNode, "Customer Information", "Information", "Information");
                    CreateCheckoutStepPage(checkoutNode, "Shipping Method", "Shipping Method", "ShippingMethod");
                    CreateCheckoutStepPage(checkoutNode, "Payment Method", "Payment Method", "PaymentMethod");
                    CreateCheckoutStepPage(checkoutNode, "Review Order", "Review", "Review");
                    CreateCheckoutStepPage(checkoutNode, "Process Payment", "Payment", "Payment");
                    CreateCheckoutStepPage(checkoutNode, "Order Confirmation", "Confirmation", "Confirmation");
                }

                scope.Complete();
            }

            // Continue the pipeline
            return Ok();
        }

        private void CreateCheckoutStepPage(IContent parent, string name, string shortName, string stepType)
        {
            var checkoutStepNode = _contentService.Create(name, parent.Id,
                UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);

            checkoutStepNode.SetValue("uccShortStepName", shortName);
            checkoutStepNode.SetValue("uccStepType", $"[\"{stepType}\"]");

            _contentService.Save(checkoutStepNode);
        }
    }
}
