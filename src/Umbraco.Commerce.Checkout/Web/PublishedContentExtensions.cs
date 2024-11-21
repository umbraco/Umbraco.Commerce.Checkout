using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Commerce.Checkout.Exceptions;
using Umbraco.Commerce.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web
{
    public static class PublishedContentExtensions
    {
        // Temporary fix for AncestorOrSelf until https://github.com/umbraco/Umbraco-CMS/pull/17581 is merged in
        private static IPublishedContent? AncestorOrSelf2(this IPublishedContent content, string contentTypeAlias) =>
            content.ContentType.Alias == contentTypeAlias
                ? content
                : content.Ancestor(contentTypeAlias);

        public static StoreReadOnly GetStore(this IPublishedContent content)
        {
            return content.Value<StoreReadOnly>(Cms.Constants.Properties.StorePropertyAlias, fallback: Fallback.ToAncestors) ?? throw new StoreDataNotFoundException();
        }

        public static IPublishedContent GetCheckoutPage(this IPublishedContent content)
        {
            return content.AncestorOrSelf2(UmbracoCommerceCheckoutConstants.ContentTypes.Aliases.CheckoutPage)!;
        }

        public static IPublishedContent GetCheckoutBackPage(this IPublishedContent content)
        {
            return GetCheckoutPage(content).Value<IPublishedContent>("uccBackPage")!;
        }

        public static string GetThemeColor(this IPublishedContent content)
        {
            string themeColor = GetCheckoutPage(content).Value("uccThemeColor", defaultValue: "#000000")!.TrimStart('#');

            if (UmbracoCommerceCheckoutConstants.ColorMap.TryGetValue(themeColor, out string? value))
            {
                return value;
            }

            return UmbracoCommerceCheckoutConstants.ColorMap.First().Value;
        }

        public static IPublishedContent GetPreviousPage(this IPublishedContent content)
        {
            return content.Parent.Children.TakeWhile(x => !x.Id.Equals(content.Id)).LastOrDefault();
        }

        public static IPublishedContent? GetPreviousStepPage(this IPublishedContent content)
        {
            IPublishedContent prevPage = GetPreviousPage(content);
            if (prevPage == null)
            {
                return null;
            }

            string? stepType = prevPage.Value<string>("uccStepType");
            if (stepType is null or not "ShippingMethod")
            {
                return prevPage;
            }

            IPublishedContent checkoutPage = GetCheckoutPage(content);
            if (checkoutPage.Value<bool>("uccCollectShippingInfo"))
            {
                return prevPage;
            }

            return GetPreviousStepPage(prevPage);
        }

        public static IPublishedContent GetNextPage(this IPublishedContent content)
        {
            return content.Parent.Children.SkipWhile(x => !x.Id.Equals(content.Id)).Skip(1).FirstOrDefault();
        }

        public static IPublishedContent? GetNextStepPage(this IPublishedContent content)
        {
            IPublishedContent nextPage = GetNextPage(content);
            if (nextPage == null)
            {
                return null;
            }

            string? stepType = nextPage.Value<string>("uccStepType");
            if (stepType is null or not "ShippingMethod")
            {
                return nextPage;
            }

            IPublishedContent checkoutPage = GetCheckoutPage(content);
            if (checkoutPage.Value<bool>("uccCollectShippingInfo"))
            {
                return nextPage;
            }

            return GetNextStepPage(nextPage);
        }
    }
}
