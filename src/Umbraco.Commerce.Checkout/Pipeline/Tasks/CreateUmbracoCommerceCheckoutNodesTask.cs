using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutNodesTask(
        IScopeProvider scopeProvider,
        IContentTypeService contentTypeService,
        IContentService contentService)
        : PipelineTaskWithTypedArgsBase<InstallPipelineArgs, InstallPipelineData>
    {
        public override Task<PipelineResult<InstallPipelineData>> ExecuteAsync(InstallPipelineArgs args, CancellationToken cancellationToken)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                IContentType uccCheckoutPageContentType = contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage)
                    ?? throw new InvalidOperationException("Checkout Page Document Type is not found");
                IContentType uccCheckoutStepPageContentType = contentTypeService.Get(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage)
                    ?? throw new InvalidOperationException("Checkout Step Page Document Type is not found");

                // Check to see if the checkout node already exists
                IQuery<IContent> filter = scope.SqlContext.Query<IContent>().Where(x => x.ContentTypeId == uccCheckoutPageContentType.Id);
                IEnumerable<IContent> childNodes = contentService.GetPagedChildren(args.Model.SiteRootNodeId, 1, 1, out long totalRecords, filter);

                if (totalRecords == 0)
                {
                    // Create the checkout page
                    IContent checkoutNode = contentService.CreateAndSave(
                        "Checkout",
                        args.Model.SiteRootNodeId,
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
            return Task.FromResult(Ok());
        }

        private void CreateCheckoutStepPage(IContent parent, string name, string shortName, string stepType)
        {
            IContent checkoutStepNode = contentService.Create(
                name,
                parent.Id,
                UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);

            checkoutStepNode.SetValue("uccShortStepName", shortName);
            checkoutStepNode.SetValue("uccStepType", $"[\"{stepType}\"]");

            contentService.Save(checkoutStepNode);
        }
    }
}
