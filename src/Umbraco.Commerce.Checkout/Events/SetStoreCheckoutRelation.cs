using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using Umbraco.Commerce.Checkout.Helpers;
using UmbracoCommerceConstants = Umbraco.Commerce.Cms.Constants;

namespace Umbraco.Commerce.Checkout.Events;

public class SetStoreCheckoutRelation(
    IDocumentNavigationQueryService documentNavigationQueryService,
    IContentService contentService,
    IIdKeyMap keyMap,
    IServerRoleAccessor serverRoleAccessor,
    StoreCheckoutRelationHelper relationHelper)
    : ContentOfTypeCacheRefresherNotificationHandlerBase(documentNavigationQueryService, contentService, keyMap, serverRoleAccessor)
{
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService = documentNavigationQueryService;
    private readonly IContentService _contentService = contentService;

    protected override string ContentTypeAlias => UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage;

    protected override Task HandleContentOfTypeAsync(IContent content)
    {
        if (content.HasProperty(UmbracoCommerceConstants.Properties.StorePropertyAlias))
        {
            relationHelper.EnsureStoreCheckoutRelation(content.Id, content.Id);
        }
        else
        {
            if (_documentNavigationQueryService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorsKeys))
            {
                foreach (Guid ancestorKey in ancestorsKeys)
                {
                    IContent? content2 = _contentService.GetById(ancestorKey);
                    if (content2 != null)
                    {
                        if (content2.HasProperty(UmbracoCommerceConstants.Properties.StorePropertyAlias))
                        {
                            relationHelper.EnsureStoreCheckoutRelation(content2.Id, content.Id);
                            break;
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}
