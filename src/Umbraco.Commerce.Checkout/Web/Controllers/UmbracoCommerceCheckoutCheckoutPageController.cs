using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UmbracoCommerceCheckoutCheckoutPageController : UmbracoCommerceCheckoutBaseController
    {
        public UmbracoCommerceCheckoutCheckoutPageController(ILogger<UmbracoCommerceCheckoutCheckoutPageController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        { }

        public override IActionResult Index()
        {
            // Check the cart is valid before continuing 
            if (!IsValidCart(out var redirectUrl))
                return Redirect(redirectUrl);

            // If the page has a template, use it
            if (CurrentPage.TemplateId.HasValue && CurrentPage.TemplateId.Value > 0)
                return base.Index();

            // No template so redirect to the first child if one exists
            if (CurrentPage.Children != null)
            {
                var firstChild = CurrentPage.Children.FirstOrDefault();
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
