using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Commerce.Checkout.Web;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Events
{
    public class SyncZeroValuePaymentProviderContinueUrl : INotificationAsyncHandler<ContentCacheRefresherNotification>
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly IPublishedContentTypeCache _publishedContentTypeCacheService;
        private readonly IDocumentNavigationQueryService _documentNavigationQueryService;

        public SyncZeroValuePaymentProviderContinueUrl(
            IUmbracoContextFactory umbracoContextFactory,
            IUmbracoCommerceApi commerceApi,
            IPublishedContentTypeCache publishedContentTypeCacheService,
            IDocumentNavigationQueryService documentNavigationQueryService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _commerceApi = commerceApi;
            _publishedContentTypeCacheService = publishedContentTypeCacheService;
            _documentNavigationQueryService = documentNavigationQueryService;
        }

        public Task HandleAsync(ContentCacheRefresherNotification notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification, nameof(notification));
            return HandleAsync(notification.MessageObject, notification.MessageType);
        }

        public async Task HandleAsync(object messageObject, MessageType messageType)
        {
            if (messageType != MessageType.RefreshByPayload)
            {
                throw new NotSupportedException();
            }

            if (messageObject is not ContentCacheRefresher.JsonPayload[] payloads)
            {
                return;
            }

            using (UmbracoContextReference ctx = _umbracoContextFactory.EnsureUmbracoContext())
            {
                foreach (ContentCacheRefresher.JsonPayload payload in payloads)
                {
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshNode))
                    {
                        // Single node refresh
                        IPublishedContent? node = ctx.UmbracoContext.Content.GetById(payload.Id);
                        if (node != null && IsConfirmationPageType(node))
                        {
                            await DoSyncZeroValuePaymentProviderContinueUrlAsync(node);
                        }
                    }
                    else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        // Branch refresh
                        IPublishedContent? rootNode = ctx.UmbracoContext.Content.GetById(payload.Id);
                        if (rootNode != null)
                        {
                            IPublishedContentType checkoutStepPageDocType = _publishedContentTypeCacheService.Get(PublishedItemType.Content, UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);
                            // TODO - Dinh: remove this line
                            //IPublishedContentType? nodeType = ctx.UmbracoContext.Content.GetContentType(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);
                            if (checkoutStepPageDocType == null)
                            {
                                continue;
                            }

                            // TODO: fix the obsolete warning. Try using INavigationQueryService
                            IEnumerable<IPublishedContent> nodes = ctx.UmbracoContext.Content.GetByContentType(checkoutStepPageDocType);
                            IEnumerable<IPublishedContent> confimationPages = nodes?.Where(x => IsConfirmationPageType(x) && x.Path.StartsWith(rootNode.Path, StringComparison.Ordinal)) ?? [];
                            foreach (IPublishedContent? node in confimationPages)
                            {
                                await DoSyncZeroValuePaymentProviderContinueUrlAsync(node);
                            }
                        }
                    }
                    else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                    {
                        // All refresh
                        // TODO - Dinh: remove this line
                        //IPublishedContentType? nodeType = ctx.UmbracoContext.Content.GetContentType(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);
                        IPublishedContentType checkoutStepPageDocType = _publishedContentTypeCacheService.Get(PublishedItemType.Content, UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage);
                        if (checkoutStepPageDocType == null)
                        {
                            continue;
                        }

                        // TODO: fix the obsolete warning. Try using INavigationQueryService
                        IEnumerable<IPublishedContent> nodes = ctx.UmbracoContext.Content.GetByContentType(checkoutStepPageDocType);
                        IEnumerable<IPublishedContent> confimationPages = nodes?.Where(IsConfirmationPageType) ?? [];
                        foreach (IPublishedContent? node in confimationPages)
                        {
                            await DoSyncZeroValuePaymentProviderContinueUrlAsync(node);
                        }
                    }
                }
            }
        }

        private static bool IsConfirmationPageType(IPublishedContent node)
        {
            if (node == null || node.ContentType == null || !node.HasProperty("uccStepType"))
            {
                return false;
            }

            return node.ContentType.Alias == UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutStepPage && node.Value<string>("uccStepType") == "Confirmation";
        }

        private async Task DoSyncZeroValuePaymentProviderContinueUrlAsync(IPublishedContent confirmationNode)
        {
            if (confirmationNode == null)
            {
                return;
            }

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
