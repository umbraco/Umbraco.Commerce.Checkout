using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Commerce.Checkout.Services;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Checkout.Web.Controllers
{
    [ApiVersion("1.0")]
    [VersionedApiBackOfficeRoute("umbraco-commerce-checkout")]
    [ApiExplorerSettings(GroupName = "Umbraco Commerce Checkout API")]
    [Authorize]
    [Authorize(AuthorizationPolicies.SectionAccessSettings)]
    public class UmbracoCommerceCheckoutApiController : ManagementApiControllerBase
    {
        private readonly IUmbracoCommerceApi _commerceApi;
        private readonly IContentService _contentService;

        public UmbracoCommerceCheckoutApiController(
            IUmbracoCommerceApi commerceApi,
            IContentService contentService)
        {
            _commerceApi = commerceApi;
            _contentService = contentService;
        }

        [HttpGet("install")]
        public async Task<object> InstallUmbracoCommerceCheckout(Guid? siteRootNodeId)
        {
            ArgumentNullException.ThrowIfNull(siteRootNodeId);

            // Validate the site root node
            IContent? siteRootNode = _contentService.GetById(siteRootNodeId.Value);
            if (siteRootNode == null)
            {
                return new { success = false, message = "Couldn't find the site root node. Please check if the root you picked stills exists." };
            }

            Guid? storeId = GetStoreId(siteRootNode);
            if (!storeId.HasValue)
            {
                return new { success = false, message = "Couldn't find a store connected to the site root node. Do you have a store picker configured?" };
            }

            StoreReadOnly store = await _commerceApi.GetStoreAsync(storeId.Value);
            if (store == null)
            {
                return new { success = false, message = "Couldn't find a store connected to the site root node. Do you have a store picker configured?" };
            }

            // Perform the install
            await new InstallService().InstallAsync(siteRootNode.Id, store);

            // Return success
            return new { success = true };
        }

        private Guid? GetStoreId(IContent content)
        {
            if (content.HasProperty(Cms.Constants.Properties.StorePropertyAlias))
            {
                return content.GetValue<Guid?>(Cms.Constants.Properties.StorePropertyAlias);
            }

            if (content.ParentId != -1)
            {
                return GetStoreId(_contentService.GetById(content.ParentId)!);
            }

            return null;
        }
    }
}
