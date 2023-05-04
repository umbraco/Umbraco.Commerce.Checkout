using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutZeroValuePaymentMethodTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public CreateUmbracoCommerceCheckoutZeroValuePaymentMethodTask(IUmbracoCommerceApi commerceApi)
        {
            _commerceApi = commerceApi;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            _commerceApi.Uow.Execute(uow => 
            {
                if (!_commerceApi.PaymentMethodExists(args.Model.Store.Id, UmbracoCommerceCheckoutConstants.PaymentMethods.Aliases.ZeroValue))
                {
                    var paymentMethod = PaymentMethod.Create(uow, args.Model.Store.Id, UmbracoCommerceCheckoutConstants.PaymentMethods.Aliases.ZeroValue, "[Umbraco Commerce Checkout] Zero Value", "zeroValue");

                    paymentMethod.SetSku("UCCZV01")
                        .SetTaxClass(args.Model.Store.DefaultTaxClassId.Value)
                        .AllowInCountry(args.Model.Store.DefaultCountryId.Value);

                    // We need to set the Continue URL to the checkout confirmation page
                    // but we create nodes as unpublished, thus without a URL so we'll
                    // have to listen for the confirmation page being published and
                    // sync it's URL accordingly

                    _commerceApi.SavePaymentMethod(paymentMethod);
                }

                uow.Complete();
            });

            return Ok();
        }
    }
}
