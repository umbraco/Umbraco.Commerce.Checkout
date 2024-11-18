using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UccCheckoutPageController : UmbracoCommerceCheckoutCheckoutPageController
    {
        public UccCheckoutPageController(ILogger<UmbracoCommerceCheckoutCheckoutPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }
    }

    public class UmbracoCommerceCheckoutCheckoutPageController : UmbracoCommerceCheckoutBaseController
    {
        public UmbracoCommerceCheckoutCheckoutPageController(ILogger<UmbracoCommerceCheckoutCheckoutPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }

        public new async Task<IActionResult> Index()
        {
            // Check the cart is valid before continuing
            if (!await IsValidCartAsync())
            {
                return Redirect(InvalidCartRedirectUrl);
            }

            ArgumentNullException.ThrowIfNull(CurrentPage);
            // If the page has a template, use it
            if (CurrentPage.TemplateId.HasValue && CurrentPage.TemplateId.Value > 0)
            {
                return await base.Index();
            }

            // No template so redirect to the first child if one exists
            // TODO - Dinh: solve obsolete warning
            if (CurrentPage.Children != null)
            {
                Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent? firstChild = CurrentPage.Children.FirstOrDefault();
                if (firstChild != null)
                {
                    return RedirectPermanent(firstChild.Url());
                }
            }

            // Still nothing so 404
            return NotFound();
        }
    }
}
