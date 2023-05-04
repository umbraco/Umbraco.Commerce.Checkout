using System.Linq;
using System.Collections.Generic;
using Umbraco.Commerce.Checkout.Web.Dtos;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

using UmbracoCommerceConstants = Umbraco.Commerce.Core.Constants;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UmbracoCommerceCheckoutSurfaceController : SurfaceController
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public UmbracoCommerceCheckoutSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, 
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IUmbracoCommerceApi commerceApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult ApplyDiscountOrGiftCardCode(UccDiscountOrGiftCardCodeDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .Redeem(model.Code);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("code", "Failed to redeem discount code: "+ ex.Message);

                return IsAjaxRequest()
                    ? (IActionResult)Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? (IActionResult)Json(new { success = true })
                : RedirectToCurrentUmbracoPage();
        }

        [HttpGet]
        [ValidateUmbracoFormRouteString]
        public IActionResult RemoveDiscountOrGiftCardCode(UccDiscountOrGiftCardCodeDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .Unredeem(model.Code);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to unredeem discount code: " + ex.Message);

                return IsAjaxRequest()
                    ? (IActionResult)JsonGet(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? (IActionResult)JsonGet(new { success = true })
                : RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult UpdateOrderInformation(UccUpdateOrderInformationDto model)
        {
            try
            {
                var checkoutPage = CurrentPage.GetCheckoutPage();

                _commerceApi.Uow.Execute(uow =>
                {
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetProperties(new Dictionary<string, string>
                        {
                            { UmbracoCommerceConstants.Properties.Customer.EmailPropertyAlias, model.Email },
                            { "marketingOptIn", model.MarketingOptIn ? "1" : "0" },

                            { UmbracoCommerceConstants.Properties.Customer.FirstNamePropertyAlias, model.BillingAddress.FirstName },
                            { UmbracoCommerceConstants.Properties.Customer.LastNamePropertyAlias, model.BillingAddress.LastName },
                            { "billingAddressLine1", model.BillingAddress.Line1 },
                            { "billingAddressLine2", model.BillingAddress.Line2 },
                            { "billingCity", model.BillingAddress.City },
                            { "billingZipCode", model.BillingAddress.ZipCode },
                            { "billingTelephone", model.BillingAddress.Telephone },
                            { "comments", model.Comments },
                            { "ipAddress", GetIPAddress() }
                        })
                        .SetPaymentCountryRegion(model.BillingAddress.Country, model.BillingAddress.Region);

                    if (checkoutPage.Value<bool>("uccCollectShippingInfo"))
                    {
                        order.SetProperties(new Dictionary<string, string>
                        {
                            { "shippingSameAsBilling", model.ShippingSameAsBilling ? "1" : "0" },
                            { "shippingFirstName", model.ShippingSameAsBilling? model.BillingAddress.FirstName : model.ShippingAddress.FirstName },
                            { "shippingLastName", model.ShippingSameAsBilling? model.BillingAddress.LastName : model.ShippingAddress.LastName },
                            { "shippingAddressLine1", model.ShippingSameAsBilling? model.BillingAddress.Line1 : model.ShippingAddress.Line1 },
                            { "shippingAddressLine2", model.ShippingSameAsBilling? model.BillingAddress.Line2 : model.ShippingAddress.Line2 },
                            { "shippingCity", model.ShippingSameAsBilling? model.BillingAddress.City : model.ShippingAddress.City },
                            { "shippingZipCode", model.ShippingSameAsBilling? model.BillingAddress.ZipCode : model.ShippingAddress.ZipCode },
                            { "shippingTelephone", model.ShippingSameAsBilling? model.BillingAddress.Telephone : model.ShippingAddress.Telephone }
                        })
                        .SetShippingCountryRegion(model.ShippingSameAsBilling ? model.BillingAddress.Country : model.ShippingAddress.Country,
                            model.ShippingSameAsBilling ? model.BillingAddress.Region : model.ShippingAddress.Region);
                    }
                    else
                    {
                        order.SetShippingCountryRegion(model.BillingAddress.Country, null)
                            .ClearShippingMethod();
                    }

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to update information: " + ex.Message);

                return IsAjaxRequest()
                    ? (IActionResult)Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? (IActionResult)Json(new { success = true })
                : model.NextStep.HasValue
                    ? RedirectToUmbracoPage(model.NextStep.Value)
                    : RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult UpdateOrderShippingMethod(UccUpdateOrderShippingMethodDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var checkoutPage = CurrentPage.GetCheckoutPage();
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetShippingMethod(model.ShippingMethod);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to update shipping method: " + ex.Message);

                return IsAjaxRequest()
                    ? (IActionResult)Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? (IActionResult)Json(new { success = true })
                : model.NextStep.HasValue
                    ? RedirectToUmbracoPage(model.NextStep.Value)
                    : RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public IActionResult UpdateOrderPaymentMethod(UccUpdateOrderPaymentMethodDto model)
        {
            try
            {
                _commerceApi.Uow.Execute(uow =>
                {
                    var checkoutPage = CurrentPage.GetCheckoutPage();
                    var store = CurrentPage.GetStore();
                    var order = _commerceApi.GetCurrentOrder(store.Id)
                        .AsWritable(uow)
                        .SetPaymentMethod(model.PaymentMethod);

                    _commerceApi.SaveOrder(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", "Failed to update payment method: " + ex.Message);

                return IsAjaxRequest()
                    ? (IActionResult)Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? (IActionResult)Json(new { success = true })
                : model.NextStep.HasValue
                    ? RedirectToUmbracoPage(model.NextStep.Value)
                    : RedirectToCurrentUmbracoPage();
        }

        private string GetIPAddress()
        {
            var ipAddress = HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR");
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                    return addresses[0];
            }

            return HttpContext.GetServerVariable("REMOTE_ADDR");
        }

        private bool IsAjaxRequest()
        {
            var headerName = "X-Requested-With";
            var headerValue = "xmlhttprequest";

            return (Request.Query.ContainsKey(headerName) && Request.Query[headerName].ToString().InvariantEquals(headerValue))
                || (Request.HasFormContentType && Request.Form.ContainsKey(headerName) && Request.Form[headerName].ToString().InvariantEquals(headerValue))
                || (Request.Headers != null && Request.Headers.ContainsKey(headerName) && Request.Headers[headerName].ToString().InvariantEquals(headerValue));
        }

        private JsonResult JsonGet(object model)
            => Json(model);
    }
}
