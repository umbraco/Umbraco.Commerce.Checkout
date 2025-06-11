using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UccCheckoutPageController : UmbracoCommerceCheckoutCheckoutPageController
    {
        public UccCheckoutPageController(ILogger<UmbracoCommerceCheckoutCheckoutPageController> logger, ICompositeViewEngine compositeViewEngine)
            : base(logger, compositeViewEngine)
        { }
    }

    public class UmbracoCommerceCheckoutCheckoutPageController : UmbracoCommerceCheckoutBaseController
    {
        public UmbracoCommerceCheckoutCheckoutPageController(ILogger<UmbracoCommerceCheckoutCheckoutPageController> logger, ICompositeViewEngine compositeViewEngine)
            : base(logger, compositeViewEngine)
        { }

        public override async Task<IActionResult> Index()
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
            var children = CurrentPage.Children().ToList();
            if (children.Count > 0)
            {
                Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent? firstChild = children.FirstOrDefault();
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
