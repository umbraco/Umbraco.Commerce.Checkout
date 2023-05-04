using System;
using Umbraco.Commerce.Checkout.Services;
using Umbraco.Commerce.Core.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    [PluginController("UmbracoCommerceCheckout")]
    public class UmbracoCommerceCheckoutApiController : UmbracoAuthorizedApiController
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly IContentService _contentService;

        public UmbracoCommerceCheckoutApiController(IUmbracoCommerceApi commerceApi,
            IContentService contentService)
        {
            _commerceApi = commerceApi;
            _contentService = contentService;
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
        public object InstallUmbracoCommerceCheckout(GuidUdi siteRootNodeId)
        {
            // Validate the site root node
            var siteRootNode = _contentService.GetById(siteRootNodeId.Guid);

            var storeId = GetStoreId(siteRootNode);
            if (!storeId.HasValue)
                return new { success = false, message = "Couldn't find a store connected to the site root node. Do you have a store picker configured?" };

            var store = _commerceApi.GetStore(storeId.Value);
            if (store == null)
                return new { success = false, message = "Couldn't find a store connected to the site root node. Do you have a store picker configured?" };

            //  Perform the install
            new InstallService()
                .Install(siteRootNode.Id, store);

            // Return success
            return new { success = true };
        }

        private Guid? GetStoreId(IContent content)
        {
            if (content.HasProperty(Umbraco.Commerce.Cms.Constants.Properties.StorePropertyAlias))
                return content.GetValue<Guid?>(Umbraco.Commerce.Cms.Constants.Properties.StorePropertyAlias);

            if (content.ParentId != -1)
                return GetStoreId(_contentService.GetById(content.ParentId));

            return null;
        }
    }
}
