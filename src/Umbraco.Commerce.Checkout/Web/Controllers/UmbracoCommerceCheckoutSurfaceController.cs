using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Commerce.Checkout.Web.Dtos;
using Umbraco.Commerce.Common.Models;
using Umbraco.Commerce.Common.Validation;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;
using UmbracoCommerceConstants = Umbraco.Commerce.Core.Constants;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    public class UmbracoCommerceCheckoutSurfaceController : SurfaceController
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public UmbracoCommerceCheckoutSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IUmbracoCommerceApi commerceApi)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _commerceApi = commerceApi;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> ApplyDiscountOrGiftCardCode(UccDiscountOrGiftCardCodeDto model)
        {
            try
            {
                await _commerceApi.Uow.ExecuteAsync(async uow =>
                {
                    Core.Models.StoreReadOnly store = CurrentPage!.GetStore();
                    Core.Models.Order order = await _commerceApi.GetCurrentOrderAsync(store.Id)
                        .AsWritableAsync(uow)
                        .RedeemAsync(model.Code);

                    await _commerceApi.SaveOrderAsync(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("code", "Failed to redeem discount code: " + ex.Message);

                return IsAjaxRequest()
                    ? Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? Json(new { success = true })
                : RedirectToCurrentUmbracoPage();
        }

        [HttpGet]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> RemoveDiscountOrGiftCardCode(UccDiscountOrGiftCardCodeDto model)
        {
            try
            {
                await _commerceApi.Uow.ExecuteAsync(async uow =>
                {
                    Core.Models.StoreReadOnly store = CurrentPage!.GetStore();
                    Core.Models.Order order = await _commerceApi.GetCurrentOrderAsync(store.Id)
                        .AsWritableAsync(uow)
                        .UnredeemAsync(model.Code);

                    await _commerceApi.SaveOrderAsync(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to unredeem discount code: " + ex.Message);

                return IsAjaxRequest()
                    ? JsonGet(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? JsonGet(new { success = true })
                : RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> UpdateOrderInformation(UccUpdateOrderInformationDto model)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent checkoutPage = CurrentPage!.GetCheckoutPage();

                await _commerceApi.Uow.ExecuteAsync(async uow =>
                {
                    Core.Models.StoreReadOnly? store = CurrentPage!.GetStore();
                    Core.Models.Order order = await _commerceApi.GetCurrentOrderAsync(store.Id)
                        .AsWritableAsync(uow)
                        .SetPropertiesAsync(new Dictionary<string, string>
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
                        .SetPaymentCountryRegionAsync(model.BillingAddress.Country, model.BillingAddress.Region);

                    if (checkoutPage.Value<bool>("uccCollectShippingInfo"))
                    {
                        await order.SetPropertiesAsync(new Dictionary<string, string>
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
                         .SetShippingCountryRegionAsync(
                             model.ShippingSameAsBilling ? model.BillingAddress.Country : model.ShippingAddress.Country,
                             model.ShippingSameAsBilling ? model.BillingAddress.Region : model.ShippingAddress.Region);
                    }
                    else
                    {
                        await order.SetShippingCountryRegionAsync(model.BillingAddress.Country, null)
                            .ClearShippingMethodAsync();
                    }

                    await _commerceApi.SaveOrderAsync(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to update information: " + ex.Message);

                return IsAjaxRequest()
                    ? Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? Json(new { success = true })
                : model.NextStep.HasValue
                    ? RedirectToUmbracoPage(model.NextStep.Value)
                    : RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> UpdateOrderShippingMethod(UccUpdateOrderShippingMethodDto model)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                await _commerceApi.Uow.ExecuteAsync(async uow =>
                {
                    Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent checkoutPage = CurrentPage!.GetCheckoutPage();
                    Core.Models.StoreReadOnly? store = CurrentPage!.GetStore();
                    Core.Models.Order order = await _commerceApi.GetCurrentOrderAsync(store.Id)
                        .AsWritableAsync(uow);

                    if (!model.ShippingOptionId.IsNullOrWhiteSpace())
                    {
                        Core.Models.ShippingMethodReadOnly shippingMethod = await _commerceApi.GetShippingMethodAsync(model.ShippingMethod);
                        Attempt<Core.Models.ShippingRate> shippingRateAttempt = await shippingMethod.TryCalculateRateAsync(model.ShippingOptionId, order);

                        if (shippingRateAttempt.Success)
                        {
                            await order.SetShippingMethodAsync(model.ShippingMethod, shippingRateAttempt.Result!.Option);
                        }
                        else
                        {
                            throw new ValidationException([new ValidationError("Unable to locate the selected shipping option")]);
                        }
                    }
                    else
                    {
                        await order.SetShippingMethodAsync(model.ShippingMethod);
                    }

                    await _commerceApi.SaveOrderAsync(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to update shipping method: " + ex.Message);

                return IsAjaxRequest()
                    ? Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? Json(new { success = true })
                : model.NextStep.HasValue
                    ? RedirectToUmbracoPage(model.NextStep.Value)
                    : RedirectToCurrentUmbracoPage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public async Task<IActionResult> UpdateOrderPaymentMethod(UccUpdateOrderPaymentMethodDto model)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                await _commerceApi.Uow.ExecuteAsync(async uow =>
                {
                    Umbraco.Cms.Core.Models.PublishedContent.IPublishedContent checkoutPage = CurrentPage!.GetCheckoutPage();
                    Core.Models.StoreReadOnly store = CurrentPage!.GetStore();
                    Core.Models.Order order = await _commerceApi.GetCurrentOrderAsync(store.Id)
                        .AsWritableAsync(uow)
                        .SetPaymentMethodAsync(model.PaymentMethod);

                    await _commerceApi.SaveOrderAsync(order);

                    uow.Complete();
                });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, "Failed to update payment method: " + ex.Message);

                return IsAjaxRequest()
                    ? Json(new { success = false, errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage) })
                    : CurrentUmbracoPage();
            }

            return IsAjaxRequest()
                ? Json(new { success = true })
                : model.NextStep.HasValue
                    ? RedirectToUmbracoPage(model.NextStep.Value)
                    : RedirectToCurrentUmbracoPage();
        }

        private string GetIPAddress()
        {
            string? ipAddress = HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR");
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return HttpContext.GetServerVariable("REMOTE_ADDR") ?? string.Empty;
        }

        private bool IsAjaxRequest()
        {
            string headerName = "X-Requested-With";
            string headerValue = "xmlhttprequest";

            return (Request.Query.ContainsKey(headerName) && Request.Query[headerName].ToString().InvariantEquals(headerValue))
                || (Request.HasFormContentType && Request.Form.ContainsKey(headerName) && Request.Form[headerName].ToString().InvariantEquals(headerValue))
                || (Request.Headers != null && Request.Headers.ContainsKey(headerName) && Request.Headers[headerName].ToString().InvariantEquals(headerValue));
        }

        private JsonResult JsonGet(object model)
            => Json(model);
    }
}
