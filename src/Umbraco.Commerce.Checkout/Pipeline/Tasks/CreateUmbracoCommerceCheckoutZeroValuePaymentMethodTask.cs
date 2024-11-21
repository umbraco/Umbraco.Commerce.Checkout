using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class CreateUmbracoCommerceCheckoutZeroValuePaymentMethodTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public CreateUmbracoCommerceCheckoutZeroValuePaymentMethodTask(IUmbracoCommerceApi commerceApi) => _commerceApi = commerceApi;

        public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken)
        {
            await _commerceApi.Uow.ExecuteAsync(
                async uow =>
                {
                    if (!await _commerceApi.PaymentMethodExistsAsync(args.Model.Store.Id, UmbracoCommerceCheckoutConstants.PaymentMethods.Aliases.ZeroValue))
                    {
                        PaymentMethod paymentMethod = await PaymentMethod.CreateAsync(uow, args.Model.Store.Id, UmbracoCommerceCheckoutConstants.PaymentMethods.Aliases.ZeroValue, "[Umbraco Commerce Checkout] Zero Value", "zeroValue");

                        await paymentMethod.SetSkuAsync("UCCZV01")
                            .SetTaxClassAsync(args.Model.Store.DefaultTaxClassId!.Value)
                            .AllowInCountryAsync(args.Model.Store.DefaultCountryId!.Value);

                        // We need to set the Continue URL to the checkout confirmation page
                        // but we create nodes as unpublished, thus without a URL so we'll
                        // have to listen for the confirmation page being published and
                        // sync it's URL accordingly
                        await _commerceApi.SavePaymentMethodAsync(paymentMethod, cancellationToken);
                    }

                    uow.Complete();
                },
                cancellationToken);

            return Ok();
        }
    }
}
