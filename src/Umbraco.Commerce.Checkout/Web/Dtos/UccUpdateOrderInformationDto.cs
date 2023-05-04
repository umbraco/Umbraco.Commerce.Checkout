using System;

namespace Umbraco.Commerce.Checkout.Web.Dtos
{
    public class UccUpdateOrderInformationDto
    {
        public string Email { get; set; }

        public bool MarketingOptIn { get; set; }

        public UccOrderAddressDto BillingAddress { get; set; }

        public UccOrderAddressDto ShippingAddress { get; set; }

        public bool ShippingSameAsBilling { get; set; }

        public string Comments { get; set; }

        public Guid? NextStep { get; set; }
    }
}
