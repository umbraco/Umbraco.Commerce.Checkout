using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UccCheckoutStepPageController : UmbracoCommerceCheckoutCheckoutStepPageController
    {
        public UccCheckoutStepPageController(ILogger<UmbracoCommerceCheckoutCheckoutStepPageController> logger, ICompositeViewEngine compositeViewEngine)
            : base(logger, compositeViewEngine)
        { }
    }

    public class UmbracoCommerceCheckoutCheckoutStepPageController : UmbracoCommerceCheckoutBaseController
    {
        public UmbracoCommerceCheckoutCheckoutStepPageController(ILogger<UmbracoCommerceCheckoutCheckoutStepPageController> logger, ICompositeViewEngine compositeViewEngine)
            : base(logger, compositeViewEngine)
        { }

        public override async Task<IActionResult> Index()
        {
            // Check if login is required
            if (IsLoginRequired(out string loginPageUrl) && (!User.Identity?.IsAuthenticated ?? false))
            {
                return string.IsNullOrEmpty(loginPageUrl)
                    ? Unauthorized()
                    : Redirect(loginPageUrl);
            }

            // Check the cart is valid before continuing
            if (!await IsValidCartAsync())
            {
                return Redirect(InvalidCartRedirectUrl);
            }

            // If the page has a template, use it
            if (CurrentPage!.TemplateId.HasValue && CurrentPage.TemplateId.Value > 0)
            {
                return await base.Index();
            }

            // Get the current step from the page and render the appropriate view
            return View($"~/Views/UmbracoCommerceCheckout/UmbracoCommerceCheckout{CurrentPage.Value<string>("uccStepType")}Page.cshtml", CurrentPage);
        }
    }
}
