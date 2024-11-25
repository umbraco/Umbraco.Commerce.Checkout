using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;
using UmbracoCommerceConstants = Umbraco.Commerce.Cms.Constants;

namespace Umbraco.Commerce.Checkout.Events;

public class SetStoreCheckoutRelation(
    IDocumentNavigationQueryService documentNavigationQueryService,
    IContentService contentService,
    IIdKeyMap keyMap,
    IServerRoleAccessor serverRoleAccessor,
    IRelationService relationService)
    : ContentOfTypeCacheRefresherNotificationHandlerBase(documentNavigationQueryService, contentService, keyMap, serverRoleAccessor)
{
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService = documentNavigationQueryService;
    private readonly IContentService _contentService = contentService;

    protected override string ContentTypeAlias => UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage;

    protected override Task HandleContentOfTypeAsync(IContent content)
    {
        if (content.HasProperty(UmbracoCommerceConstants.Properties.StorePropertyAlias))
        {
            EnsureStoreCheckoutStoreRelation(content, content);
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
                            EnsureStoreCheckoutStoreRelation(content2, content);
                            break;
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private void EnsureStoreCheckoutStoreRelation(IContent storeRootPage, IContent checkoutPage)
    {
        if (!relationService.AreRelated(checkoutPage.Id, storeRootPage.Id, UmbracoCommerceCheckoutConstants.RelationTypes.Aliases.StoreCheckout))
        {
            IRelationType? relationType = relationService.GetRelationTypeByAlias(UmbracoCommerceCheckoutConstants.RelationTypes.Aliases.StoreCheckout);

            if (relationType == null)
            {
                relationType = new RelationType(
                    "[Umbraco Commerce Checkout] Store Checkout",
                    UmbracoCommerceCheckoutConstants.RelationTypes.Aliases.StoreCheckout,
                    true,
                    Umbraco.Cms.Core.Constants.ObjectTypes.Document,
                    Umbraco.Cms.Core.Constants.ObjectTypes.Document,
                    false);
                relationService.Save(relationType);
            }

            relationService.Save(new Relation(storeRootPage.Id, checkoutPage.Id, relationType));
        }
    }
}
