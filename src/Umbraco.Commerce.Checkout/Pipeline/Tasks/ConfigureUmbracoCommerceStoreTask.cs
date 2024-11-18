using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class ConfigureUmbracoCommerceStoreTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public ConfigureUmbracoCommerceStoreTask(IUmbracoCommerceApi commerceApi) => _commerceApi = commerceApi;

        public override async Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken = default)
        {
            await _commerceApi.Uow.ExecuteAsync(
                async uow =>
                {
                    // Update order confirmation email
                    Guid? orderConfirmationEmailId = args.Model.Store.ConfirmationEmailTemplateId;
                    if (orderConfirmationEmailId.HasValue)
                    {
                        EmailTemplate orderConfirmationEmail = await _commerceApi.GetEmailTemplateAsync(orderConfirmationEmailId.Value)
                            .AsWritableAsync(uow)
                            .SetTemplateViewAsync("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderConfirmationEmail.cshtml");

                        await _commerceApi.SaveEmailTemplateAsync(orderConfirmationEmail);
                    }

                    // Update order confirmation email
                    Guid? orderErrorEmailId = args.Model.Store.ErrorEmailTemplateId;
                    if (orderErrorEmailId.HasValue)
                    {
                        EmailTemplate orderErrorEmail = await _commerceApi.GetEmailTemplateAsync(orderErrorEmailId.Value)
                            .AsWritableAsync(uow)
                            .SetTemplateViewAsync("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderErrorEmail.cshtml");

                        await _commerceApi.SaveEmailTemplateAsync(orderErrorEmail);
                    }

                    // Update gift card email
                    Guid? giftCardEmailId = args.Model.Store.DefaultGiftCardEmailTemplateId;
                    if (giftCardEmailId.HasValue)
                    {
                        EmailTemplate giftCardEmail = await _commerceApi.GetEmailTemplateAsync(giftCardEmailId.Value)
                            .AsWritableAsync(uow)
                            .SetTemplateViewAsync("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutGiftCardEmail.cshtml");

                        await _commerceApi.SaveEmailTemplateAsync(giftCardEmail);
                    }

                    uow.Complete();
                }
            , cancellationToken);

            // Continue the pipeline
            return Ok();
        }
    }
}
