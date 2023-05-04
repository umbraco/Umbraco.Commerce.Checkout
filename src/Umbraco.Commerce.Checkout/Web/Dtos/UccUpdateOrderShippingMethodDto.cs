using System;

namespace Umbraco.Commerce.Checkout.Web.Dtos
{
    public class UccUpdateOrderShippingMethodDto
    {
        public Guid ShippingMethod { get; set; }

        public Guid? NextStep { get; set; }
    }
}
