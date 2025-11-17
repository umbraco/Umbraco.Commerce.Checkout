using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;
using UmbracoCommerceConstants = Umbraco.Commerce.Cms.Constants;

namespace Umbraco.Commerce.Checkout.Events
{
    public class SyncZeroValuePaymentProviderContinueUrl(
        IDocumentNavigationQueryService documentNavigationQueryService,
        IContentService contentService,
        IIdKeyMap keyMap,
        IServerRoleAccessor serverRoleAccessor,
        IUmbracoCommerceApi commerceApi,
        IPublishedUrlProvider publishedUrlProvider)
        : ContentOfTypeCacheRefresherNotificationHandlerBase(documentNavigationQueryService, contentService, keyMap, serverRoleAccessor)
    {
        private readonly IDocumentNavigationQueryService _documentNavigationQueryService = documentNavigationQueryService;
        private readonly IContentService _contentService = contentService;

        protected override string ContentTypeAlias => UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage;

        protected override async Task HandleContentOfTypeAsync(IContent content)
        {
            if (IsConfirmationPageType(content))
            {
                await DoSyncZeroValuePaymentProviderContinueUrlAsync(content);
            }
        }

        private static bool IsConfirmationPageType(IContent content)
        {
            if (!content.HasProperty("uccStepType"))
            {
                return false;
            }

            if (content.ContentType.Alias != UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage)
            {
                return false;
            }

            var stepValue = content.GetValue<string>("uccStepType");
            if (stepValue != null && stepValue.StartsWith('[') && stepValue.EndsWith(']'))
            {
                stepValue = JsonSerializer.Deserialize<string[]>(stepValue)?.FirstOrDefault();
            }

            return stepValue == "Confirmation";
        }

        private async Task DoSyncZeroValuePaymentProviderContinueUrlAsync(IContent content)
        {
            if (!content.Published)
            {
                // Don't do anything if the content is not published
                return;
            }

            // Get the store ID from the store picker in the contents ancestors
            Guid? storeId = null;

            if (content.HasProperty(UmbracoCommerceConstants.Properties.StorePropertyAlias))
            {
                storeId = content.GetValue<Guid>(UmbracoCommerceConstants.Properties.StorePropertyAlias);
            }
            else
            {
                if (_documentNavigationQueryService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorsKeys))
                {
                    foreach (Guid ancestorKey in ancestorsKeys)
                    {
                        IContent? content2 = _contentService.GetById(ancestorKey);
                        if (content2 != null && content2.HasProperty(UmbracoCommerceConstants.Properties.StorePropertyAlias))
                        {
                            storeId = content2.GetValue<Guid>(UmbracoCommerceConstants.Properties.StorePropertyAlias);
                            break;
                        }
                    }
                }
            }

            if (!storeId.HasValue)
            {
                return;
            }

            // Get the payment method and set the continue URL
            PaymentMethodReadOnly paymentMethod = await commerceApi.GetPaymentMethodAsync(storeId.Value, UmbracoCommerceCheckoutConstants.PaymentMethods.Aliases.ZeroValue);

            if (paymentMethod == null)
            {
                return;
            }

            IPaymentProvider? paymentProvider = await commerceApi.GetPaymentProviderAsync(paymentMethod.PaymentProviderAlias);
            PaymentProviderSettingDefinition? setting = paymentProvider?.SettingDefinitions.FirstOrDefault(x => x.Key.InvariantEquals("continueUrl"));

            if (setting != null)
            {
                await commerceApi.Uow.ExecuteAsync(async uow =>
                {
                    PaymentMethod writable = await paymentMethod.AsWritableAsync(uow)
                        .SetSettingAsync(setting.Key, publishedUrlProvider.GetUrl(content.Key));

                    await commerceApi.SavePaymentMethodAsync(writable);

                    uow.Complete();
                });
            }
        }
    }
}
