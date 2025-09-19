using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Commerce.Checkout.Web.Controllers.Filters;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    [NoStoreCacheControl]
    public abstract class UmbracoCommerceCheckoutBaseController : UmbracoPageController, IRenderController
    {
        protected UmbracoCommerceCheckoutBaseController(
            ILogger<UmbracoCommerceCheckoutBaseController> logger,
            ICompositeViewEngine compositeViewEngine)
            : base(logger, compositeViewEngine)
        { }

        /// <summary>
        /// Return the url where user should be redirected to when the cart is invalid.
        /// </summary>
        protected string InvalidCartRedirectUrl
        {
            get
            {
                IPublishedContent? backPage = CurrentPage!.Value<IPublishedContent>("uccBackPage", fallback: Fallback.ToAncestors);
                string redirectUrl = backPage != null ? backPage.Url() : "/";
                return redirectUrl;
            }
        }

        public virtual Task<IActionResult> Index()
            => Task.FromResult(CurrentTemplate(new ContentModel(CurrentPage)));

        protected async Task<bool> IsValidCartAsync()
        {
            StoreReadOnly store = CurrentPage.GetStore();
            OrderReadOnly order = !IsConfirmationPageType(CurrentPage)
                ? await UmbracoCommerceApi.Instance.GetCurrentOrderAsync(store.Id)
                : await UmbracoCommerceApi.Instance.GetCurrentFinalizedOrderAsync(store.Id);

            if (order == null || order.OrderLines.Count == 0)
            {
                return false;
            }

            return true;
        }

        protected bool IsLoginRequired(out string loginPageUrl)
        {
            IPublishedContent checkoutPage = CurrentPage.GetCheckoutPage();
            IPublishedContent loginPage = CurrentPage.GetLoginPage();
            loginPageUrl = loginPage?.Url(Thread.CurrentThread.CurrentCulture.Name) ?? string.Empty;
            return checkoutPage?.Value<bool>("uccRequireLogin") ?? false;
        }

        private static bool IsConfirmationPageType(IPublishedContent node)
        {
            if (!node.HasProperty("uccStepType"))
            {
                return false;
            }

            return node.ContentType.Alias == UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage && node.Value<string>("uccStepType") == "Confirmation";
        }
    }
}
