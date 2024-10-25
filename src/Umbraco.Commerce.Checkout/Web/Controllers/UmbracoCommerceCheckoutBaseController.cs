using System;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Commerce.Checkout.Exceptions;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Extensions;
using Umbraco.Commerce.Checkout.Web.Controllers.Filters;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    [NoStoreCacheControl]
    public abstract class UmbracoCommerceCheckoutBaseController : RenderController
    {
        protected UmbracoCommerceCheckoutBaseController(
            ILogger<UmbracoCommerceCheckoutBaseController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }

        protected bool IsValidCart(out string? redirectUrl)
        {
            ArgumentNullException.ThrowIfNull(CurrentPage);

            StoreReadOnly store = CurrentPage.GetStore() ?? throw new StoreDataNotFoundException();
            OrderReadOnly order = !IsConfirmationPageType(CurrentPage)
                ? UmbracoCommerceApi.Instance.GetCurrentOrder(store.Id)
                : UmbracoCommerceApi.Instance.GetCurrentFinalizedOrder(store.Id);

            if (order == null || order.OrderLines.Count == 0)
            {
                IPublishedContent? backPage = CurrentPage.Value<IPublishedContent>("uccBackPage", fallback: Fallback.ToAncestors);
                redirectUrl = backPage != null ? backPage.Url() : "/";
                return false;
            }

            redirectUrl = null;
            return true;
        }

        private static bool IsConfirmationPageType(IPublishedContent node)
        {
            if (node == null || node.ContentType == null || !node.HasProperty("uccStepType"))
            {
                return false;
            }

            return node.ContentType.Alias == UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage && node.Value<string>("uccStepType") == "Confirmation";
        }
    }
}
