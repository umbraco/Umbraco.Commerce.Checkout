using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Commerce.Checkout.Web;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Events
{
    public class SyncZeroValuePaymentProviderContinueUrl : INotificationAsyncHandler<ContentCacheRefresherNotification>
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly INavigationQueryService _navigationQueryService;
        private readonly IPublishedContentCache _publishedContentCacheService;
        private readonly IPublishedContentTypeCache _publishedContentTypeCacheService;

        public SyncZeroValuePaymentProviderContinueUrl(
            IUmbracoCommerceApi commerceApi,
            INavigationQueryService navigationQueryService,
            IPublishedContentCache publishedContentCacheService,
            IPublishedContentTypeCache publishedContentTypeCacheService)
        {
            _commerceApi = commerceApi;
            _navigationQueryService = navigationQueryService;
            _publishedContentCacheService = publishedContentCacheService;
            _publishedContentTypeCacheService = publishedContentTypeCacheService;
        }

        public async Task HandleAsync(ContentCacheRefresherNotification notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification, nameof(notification));

            if (notification.MessageType != MessageType.RefreshByPayload)
            {
                throw new NotSupportedException();
            }

            if (notification.MessageObject is not ContentCacheRefresher.JsonPayload[] payloads)
            {
                return;
            }

            foreach (ContentCacheRefresher.JsonPayload payload in payloads)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
                {
                    // Single node refresh
                    IPublishedContent? node = _publishedContentCacheService.GetById(payload.Id);
                    if (node != null && IsConfirmationPageType(node))
                    {
                        await DoSyncZeroValuePaymentProviderContinueUrlAsync(node);
                    }
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                {
                    // Branch refresh
                    IPublishedContent? rootNode = _publishedContentCacheService.GetById(payload.Id);
                    if (rootNode != null && _navigationQueryService.TryGetDescendantsKeysOfType(rootNode.Key, UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage, out IEnumerable<Guid> keys))
                    {
                        foreach (Guid key in keys)
                        {
                            IPublishedContent? node = _publishedContentCacheService.GetById(key);
                            if (node != null && IsConfirmationPageType(node))
                            {
                                await DoSyncZeroValuePaymentProviderContinueUrlAsync(node);
                            }
                        }
                    }
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    if (_navigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys))
                    {
                        foreach (Guid rootKey in rootKeys)
                        {
                            if (_navigationQueryService.TryGetDescendantsKeysOfType(rootKey, UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage, out IEnumerable<Guid> keys))
                            {
                                foreach (Guid key in keys)
                                {
                                    IPublishedContent? node = _publishedContentCacheService.GetById(key);
                                    if (node != null && IsConfirmationPageType(node))
                                    {
                                        await DoSyncZeroValuePaymentProviderContinueUrlAsync(node);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool IsConfirmationPageType(IPublishedContent node)
        {
            if (!node.HasProperty("uccStepType"))
            {
                return false;
            }

            return node.ContentType.Alias == UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage && node.Value<string>("uccStepType") == "Confirmation";
        }

        private async Task DoSyncZeroValuePaymentProviderContinueUrlAsync(IPublishedContent confirmationNode)
        {
            StoreReadOnly store = confirmationNode.GetStore();
            PaymentMethodReadOnly paymentMethod = await _commerceApi.GetPaymentMethodAsync(store.Id, UmbracoCommerceCheckoutConstants.PaymentMethods.Aliases.ZeroValue);

            if (paymentMethod == null)
            {
                return;
            }

            await _commerceApi.Uow.ExecuteAsync(async uow =>
            {
                PaymentMethod writable = await paymentMethod.AsWritableAsync(uow)
                    .SetSettingAsync("ContinueUrl", confirmationNode.Url());

                await _commerceApi.SavePaymentMethodAsync(writable);

                uow.Complete();
            });
        }
    }
}
