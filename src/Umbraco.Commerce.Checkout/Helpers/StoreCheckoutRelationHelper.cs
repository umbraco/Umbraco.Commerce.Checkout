using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Commerce.Checkout.Web;
using Umbraco.Commerce.Core.Cache;

namespace Umbraco.Commerce.Checkout.Helpers;

public class StoreCheckoutRelationHelper(IRelationService relationService, IPublishedContentCache contentCache, ICacheAccessor cache)
{
    private static string CACHE_KEY = "uc:store-checkout-relations";

    public void EnsureStoreCheckoutRelation(int storeRootPageId, int checkoutPageId)
    {
        if (!relationService.AreRelated(storeRootPageId, checkoutPageId, UmbracoCommerceCheckoutConstants.RelationTypes.Aliases.StoreCheckout))
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

            relationService.Save(new Relation(storeRootPageId, checkoutPageId, relationType));

            ClearCache();
        }
    }

    public IPublishedContent? ResolveCheckoutPageByStoreId(Guid storeId)
    {
        IEnumerable<IRelation>? relations = cache.RuntimeCache.GetTyped(CACHE_KEY,
            () => relationService.GetByRelationTypeAlias(UmbracoCommerceCheckoutConstants.RelationTypes.Aliases
                .StoreCheckout));

        IRelation? storeCheckoutRelation = cache.RuntimeCache.GetTyped($"{CACHE_KEY}:{storeId}",
            () => relations.FirstOrDefault(x => contentCache.GetById(x.ParentId)?.GetStore().Id == storeId));

        return storeCheckoutRelation != null ? contentCache.GetById(storeCheckoutRelation.ChildId) : null;
    }

    public void ClearCache()
    {
        cache.RuntimeCache.Clear(CACHE_KEY);
        cache.RuntimeCache.ClearByKey(CACHE_KEY);
    }
}
