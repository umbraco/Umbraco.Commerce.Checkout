using Umbraco.Commerce.Checkout.Events;
using Umbraco.Commerce.Checkout.Pipeline;
using Umbraco.Commerce.Checkout.Pipeline.Tasks;
using Umbraco.Commerce.Core;
using Umbraco.Commerce.Core.Events.Notification;
using Umbraco.Commerce.Extensions;

using IBuilder = Umbraco.Cms.Core.DependencyInjection.IUmbracoBuilder;

namespace Umbraco.Commerce.Checkout.Extensions
{
    internal static class CompositionExtensions
    {
        public static IBuilder AddUmbracoCommerceEventHandlers(this IBuilder builder)
        {
            IUmbracoCommerceBuilder commerceBuilder = builder.WithUmbracoCommerceBuilder();

            // Reset shipping / payment methods when certain elements of
            // an order change
            // commerceBuilder.WithNotificationEvent<OrderProductAddingNotification>()
            //     .RegisterHandler<OrderProductAddingHandler>();
            //
            // commerceBuilder.WithNotificationEvent<OrderLineChangingNotification>()
            //     .RegisterHandler<OrderLineChangingHandler>();
            //
            // commerceBuilder.WithNotificationEvent<OrderLineRemovingNotification>()
            //     .RegisterHandler<OrderLineRemovingHandler>();
            //
            // commerceBuilder.WithNotificationEvent<OrderPaymentCountryRegionChangingNotification>()
            //     .RegisterHandler<OrderPaymentCountryRegionChangingHandler>();
            //
            // commerceBuilder.WithNotificationEvent<OrderShippingCountryRegionChangingNotification>()
            //     .RegisterHandler<OrderShippingCountryRegionChangingHandler>();
            //
            // commerceBuilder.WithNotificationEvent<OrderShippingMethodChangingNotification>()
            //     .RegisterHandler<OrderShippingMethodChangingHandler>();

            return builder;
        }

        public static IBuilder AddUmbracoCommerceInstallPipeline(this IBuilder builder)
        {
            IUmbracoCommerceBuilder commerceBuilder = builder.WithUmbracoCommerceBuilder();
            commerceBuilder.WithPipeline<InstallAsyncPipelineTask, InstallPipelineContext>()
                .Add<CreateUmbracoCommerceCheckoutDataTypesTask>()
                .Add<CreateUmbracoCommerceCheckoutDocumentTypesTask>()
                .Add<CreateUmbracoCommerceCheckoutNodesTask>()
                .Add<ConfigureUmbracoCommerceStoreTask>()
                .Add<CreateUmbracoCommerceCheckoutZeroValuePaymentMethodTask>();

            return builder;
        }
    }
}
