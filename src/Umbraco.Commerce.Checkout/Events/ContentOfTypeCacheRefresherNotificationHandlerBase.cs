using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Events;

public abstract class ContentOfTypeCacheRefresherNotificationHandlerBase(
    IDocumentNavigationQueryService documentNavigationQueryService,
    IContentService contentService,
    IIdKeyMap keyMap,
    IServerRoleAccessor serverRoleAccessor) : INotificationAsyncHandler<ContentCacheRefresherNotification>, IDistributedCacheNotificationHandler
{
    protected abstract string ContentTypeAlias { get; }
    protected abstract Task HandleContentOfTypeAsync(IContent content);

    public async Task HandleAsync(ContentCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        if (serverRoleAccessor.CurrentServerRole is ServerRole.Subscriber or ServerRole.Unknown)
        {
            return;
        }

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
                IContent? content = contentService.GetById(payload.Id);
                if (content != null && content.ContentType.Alias == ContentTypeAlias)
                {
                    await HandleContentOfTypeAsync(content);
                }
            }
            else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
            {
                // Branch refresh
                Guid rootNodeKey = payload.Key ?? keyMap.GetKeyForId(payload.Id, UmbracoObjectTypes.Document).ResultOr(Guid.Empty);

                // Handle the branch root
                if (rootNodeKey != Guid.Empty)
                {
                    IContent? content = contentService.GetById(rootNodeKey);
                    if (content != null && content.ContentType.Alias == ContentTypeAlias)
                    {
                        await HandleContentOfTypeAsync(content);
                    }
                }

                // Handle the descendants
                if (rootNodeKey != Guid.Empty && documentNavigationQueryService.TryGetDescendantsKeysOfType(rootNodeKey, ContentTypeAlias, out IEnumerable<Guid> keys))
                {
                    foreach (Guid key in keys)
                    {
                        IContent? content = contentService.GetById(key);
                        if (content != null)
                        {
                            await HandleContentOfTypeAsync(content);
                        }
                    }
                }
            }
            else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
            {
                // Handle root nodes
                if (documentNavigationQueryService.TryGetRootKeysOfType(ContentTypeAlias, out IEnumerable<Guid> rootKeysOfType))
                {
                    foreach (Guid rootKey in rootKeysOfType)
                    {
                        IContent? content = contentService.GetById(rootKey);
                        if (content != null)
                        {
                            await HandleContentOfTypeAsync(content);
                        }
                    }
                }

                // Handle descendants
                if (documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys))
                {
                    foreach (Guid rootKey in rootKeys)
                    {
                        if (documentNavigationQueryService.TryGetDescendantsKeysOfType(rootKey, ContentTypeAlias, out IEnumerable<Guid> keys))
                        {
                            foreach (Guid key in keys)
                            {
                                IContent? content = contentService.GetById(key);
                                if (content != null)
                                {
                                    await HandleContentOfTypeAsync(content);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
