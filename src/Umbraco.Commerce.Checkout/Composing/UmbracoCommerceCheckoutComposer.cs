using Umbraco.Cms.Core.Composing;
using Umbraco.Commerce.Cms;
using IBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Commerce.Checkout.Composing
{
    [ComposeAfter(typeof(UmbracoCommerceComposer))]
    public class UmbracoCommerceCheckoutComposer : IComposer
    {
        public void Compose(IBuilder builder)
            // If Umbraco Commerce Checkout hasn't been added manually by now,
            // add it automatically with the default configuration.
            // If Umbraco Commerce Checkout has already been added manually, then
            // the AddUmbracoCommerceCheckout() call will just exit early.
            => builder.AddUmbracoCommerceCheckout();
    }
}
