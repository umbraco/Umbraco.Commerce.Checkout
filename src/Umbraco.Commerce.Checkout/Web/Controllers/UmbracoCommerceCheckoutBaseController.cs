using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public abstract class UmbracoCommerceCheckoutBaseController : RenderController
    {
        public UmbracoCommerceCheckoutBaseController(ILogger<UmbracoCommerceCheckoutBaseController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }

        protected bool IsValidCart(out string redirectUrl)
        { 
            var store = CurrentPage.Value<StoreReadOnly>(Cms.Constants.Properties.StorePropertyAlias, fallback: Fallback.ToAncestors);
            var order = !IsConfirmationPageType(CurrentPage)
                ? UmbracoCommerceApi.Instance.GetCurrentOrder(store.Id)
                : UmbracoCommerceApi.Instance.GetCurrentFinalizedOrder(store.Id);

            if (order == null || order.OrderLines.Count == 0)
            {
                var backPage = CurrentPage.Value<IPublishedContent>("uccBackPage", fallback: Fallback.ToAncestors);
                redirectUrl = backPage != null ? backPage.Url() : "/";
                return false;
            }

            redirectUrl = null;
            return true;
        }

        private bool IsConfirmationPageType(IPublishedContent node)
        {
            if (node == null || node.ContentType == null || !node.HasProperty("uccStepType"))
                return false;

            return node.ContentType.Alias == UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage && node.Value<string>("uccStepType") == "Confirmation";
        }
    }
}
