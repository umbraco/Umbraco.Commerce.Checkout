using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
        public static IUmbracoBuilder AddUmbracoCommerceCheckout(this IUmbracoBuilder builder, Action<UmbracoCommerceCheckoutSettings> defaultOptions = default)
        {
            // If the Umbraco Commerce Checkout InstallService is registred then we assume everything is already registered so we don't do it again. 
            if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(InstallService)) != null)
                return builder;

            // Register configuration
            var options = builder.Services.AddOptions<UmbracoCommerceCheckoutSettings>()
                .Bind(builder.Config.GetSection("Umbraco:Commerce:Checkout"));

            if (defaultOptions != default)
                options.Configure(defaultOptions);

            if (!builder.ManifestFilters().Has<UmbracoCommerceCheckoutManifestFilter>())
                builder.ManifestFilters().Append<UmbracoCommerceCheckoutManifestFilter>();

            options.ValidateDataAnnotations();

            // Register event handlers
            builder.AddUmbracoCommerceEventHandlers();

            // Register pipeline
            builder.AddUmbracoCommerceInstallPipeline();

            // Register services
            builder.Services.AddSingleton<InstallService>();

            // Register Umbraco event handlers
            builder.AddNotificationHandler<ContentCacheRefresherNotification, SyncZeroValuePaymentProviderContinueUrl>();

            return builder;
        }
    }
}
