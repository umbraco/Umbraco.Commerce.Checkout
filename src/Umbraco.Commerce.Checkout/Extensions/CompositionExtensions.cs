using Umbraco.Commerce.Checkout.Events;
using Umbraco.Commerce.Checkout.Pipeline;
using Umbraco.Commerce.Checkout.Pipeline.Tasks;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Extensions;

using IBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Commerce.Checkout.Extensions
{
    internal static class CompositionExtensions
    {
        public static IBuilder AddUmbracoCommerceEventHandlers(this IBuilder builder)
        {
            // Reset shipping / payment methods when certain elements of
            // an order change
#pragma warning disable CS0618 // Type or member is obsolete
            builder.WithNotificationEvent<OrderProductAddingNotification>()
                .RegisterHandler<OrderProductAddingHandler>();

            builder.WithNotificationEvent<OrderLineChangingNotification>()
                .RegisterHandler<OrderLineChangingHandler>();

            builder.WithNotificationEvent<OrderLineRemovingNotification>()
                .RegisterHandler<OrderLineRemovingHandler>();

            builder.WithNotificationEvent<OrderPaymentCountryRegionChangingNotification>()
                .RegisterHandler<OrderPaymentCountryRegionChangingHandler>();

            builder.WithNotificationEvent<OrderShippingCountryRegionChangingNotification>()
                .RegisterHandler<OrderShippingCountryRegionChangingHandler>();

            builder.WithNotificationEvent<OrderShippingMethodChangingNotification>()
                .RegisterHandler<OrderShippingMethodChangingHandler>();

            // Toggle order editor shipping address enabled flag based on
            // whether there umbraco commerce checkout is configured to collect a shipping address
#pragma warning restore CS0618 // Type or member is obsolete

            return builder;
        }

        public static IBuilder AddUmbracoCommerceInstallPipeline(this IBuilder builder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            builder.WithPipeline<InstallPipeline, InstallPipelineContext>()
                .Append<CreateUmbracoCommerceCheckoutDataTypesTask>()
                .Append<CreateUmbracoCommerceCheckoutDocumentTypesTask>()
                .Append<CreateUmbracoCommerceCheckoutNodesTask>()
                .Append<ConfigureUmbracoCommerceStoreTask>()
                .Append<CreateUmbracoCommerceCheckoutZeroValuePaymentMethodTask>();
#pragma warning restore CS0618 // Type or member is obsolete

            return builder;
        }
    }
}
