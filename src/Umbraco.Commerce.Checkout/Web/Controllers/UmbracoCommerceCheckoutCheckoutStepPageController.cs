using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UccCheckoutStepPageController : UmbracoCommerceCheckoutCheckoutStepPageController
    {
        public UccCheckoutStepPageController(ILogger<UmbracoCommerceCheckoutCheckoutStepPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }
    }

    public class UmbracoCommerceCheckoutCheckoutStepPageController : UmbracoCommerceCheckoutBaseController
    {
        public UmbracoCommerceCheckoutCheckoutStepPageController(ILogger<UmbracoCommerceCheckoutCheckoutStepPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }

        public override async Task<IActionResult> Index()
        {
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
