using System;
using System.Collections.Generic;
using Umbraco.Commerce.Checkout.Models;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Checkout.Extensions;

public static class CustomerExtensions
{
    public static Customer GetCustomer(this OrderReadOnly order, OrderPropertyConfig orderPropertyConfig) =>
        new Customer(
            order.CustomerInfo.FirstName,
            order.CustomerInfo.LastName,
            order.CustomerInfo.Email,
            order.Properties[orderPropertyConfig.Customer.Telephone.Alias]);

    public static CustomerAddress GetBillingAddress(this OrderReadOnly order, OrderPropertyConfig orderPropertyConfig)
    {
        // This extension method should not be used in a context where order is null.
        ArgumentNullException.ThrowIfNull(order);

        return new CustomerAddress(
            order.Properties[orderPropertyConfig.Billing.AddressLine1.Alias],
            order.Properties[orderPropertyConfig.Billing.AddressLine2.Alias],
            order.Properties[orderPropertyConfig.Billing.City.Alias],
            order.Properties[orderPropertyConfig.Billing.ZipCode.Alias]);
    }

    public static CustomerAddress GetShippingAddress(this OrderReadOnly order, OrderPropertyConfig orderPropertyConfig)
    {
        // This extension method should not be used in a context where order is null.
        ArgumentNullException.ThrowIfNull(order);

        return order.Properties["shippingSameAsBilling"] == "1"
            ? order.GetBillingAddress(orderPropertyConfig)
            : new CustomerAddress(
                order.Properties[orderPropertyConfig.Shipping.AddressLine1.Alias],
                order.Properties[orderPropertyConfig.Shipping.AddressLine2.Alias],
                order.Properties[orderPropertyConfig.Shipping.City.Alias],
                order.Properties[orderPropertyConfig.Shipping.ZipCode.Alias]);
    }

    /// <summary>
    /// Returns the current string value if not null or whitespace, otherwise returns the fallback value or an empty string.
    /// </summary>
    /// <param name="value">The original string value.</param>
    /// <param name="fallbackValue">The string value for fallback.</param>
    /// <returns></returns>
    public static string ValueOrFallback(this string value, string fallbackValue) =>
        string.IsNullOrWhiteSpace(value)
            ? string.IsNullOrWhiteSpace(fallbackValue)
                ? string.Empty
                : fallbackValue
            : value;

    /// <summary>
    /// Returns the property value if exists, otherwise returns the fallback value or an empty string. 
    /// </summary>
    /// <param name="properties">The properties dictionary.</param>
    /// <param name="key">The looked up record key.</param>
    /// <param name="fallbackValue">The string value for fallback</param>
    /// <returns></returns>
    public static string ValueOrFallback(this IReadOnlyDictionary<string, PropertyValue> properties, string key, string fallbackValue)
    {
        if (properties is null || !properties.ContainsKey(key))
        {
            return string.IsNullOrWhiteSpace(fallbackValue) ? string.Empty : fallbackValue;
        }

        var propertyRecord = properties[key];
        if (propertyRecord is null)
        {
            return string.IsNullOrWhiteSpace(fallbackValue) ? string.Empty : fallbackValue;
        }

        return propertyRecord.Value.ValueOrFallback(fallbackValue);
    }
}
