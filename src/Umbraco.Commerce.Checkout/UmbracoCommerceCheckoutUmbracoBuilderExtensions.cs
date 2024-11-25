using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Commerce.Checkout.Configuration;
using Umbraco.Commerce.Checkout.Events;
using Umbraco.Commerce.Checkout.Extensions;
using Umbraco.Commerce.Checkout.Services;

namespace Umbraco.Commerce.Checkout
{
    public static class UmbracoCommerceCheckoutUmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddUmbracoCommerceCheckout(
            this IUmbracoBuilder builder,
            Action<UmbracoCommerceCheckoutSettings>? defaultOptions = default)
        {
            ArgumentNullException.ThrowIfNull(builder);

            // If the Umbraco Commerce Checkout InstallService is registered then we assume everything is already registered so we don't do it again.
            if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(InstallService)) != null)
            {
                return builder;
            }

            // Register configuration
            OptionsBuilder<UmbracoCommerceCheckoutSettings> options = builder.Services.AddOptions<UmbracoCommerceCheckoutSettings>()
                .Bind(builder.Config.GetSection("Umbraco:Commerce:Checkout"));

            if (defaultOptions != default)
            {
                options.Configure(defaultOptions);
            }

            options.ValidateDataAnnotations();

            // Register event handlers
            builder.AddUmbracoCommerceEventHandlers();

            // Register pipeline
            builder.AddUmbracoCommerceInstallPipeline();

            // Register services
            builder.Services.AddSingleton<InstallService>();

            // Register Umbraco event handlers
            builder.AddNotificationAsyncHandler<ContentCacheRefresherNotification, SyncZeroValuePaymentProviderContinueUrl>();
            builder.AddNotificationAsyncHandler<ContentCacheRefresherNotification, SetStoreCheckoutRelation>();

            return builder;
        }
    }
}
