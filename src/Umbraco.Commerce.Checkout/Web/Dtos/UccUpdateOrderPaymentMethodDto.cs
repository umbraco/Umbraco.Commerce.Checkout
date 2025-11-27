using System;

namespace Umbraco.Commerce.Checkout.Web.Dtos
{
    public class UccUpdateOrderPaymentMethodDto
    {
        public Guid PaymentMethod { get; set; }

        public Guid? NextStep { get; set; }
    }
}
