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
    public class ConfigureUmbracoCommerceStoreTask(IUmbracoCommerceApi commerceApi)
        : PipelineTaskWithTypedArgsBase<InstallPipelineArgs, InstallPipelineData>
    {
        public override async Task<PipelineResult<InstallPipelineData>> ExecuteAsync(InstallPipelineArgs args, CancellationToken cancellationToken)
        {
            await commerceApi.Uow.ExecuteAsync(
                async uow =>
                {
                    // Update order confirmation email
                    Guid? orderConfirmationEmailId = args.Model.Store.ConfirmationEmailTemplateId;
                    if (orderConfirmationEmailId.HasValue)
                    {
                        EmailTemplate orderConfirmationEmail = await commerceApi.GetEmailTemplateAsync(orderConfirmationEmailId.Value)
                            .AsWritableAsync(uow)
                            .SetTemplateViewAsync("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderConfirmationEmail.cshtml");

                        await commerceApi.SaveEmailTemplateAsync(orderConfirmationEmail, cancellationToken);
                    }

                    // Update order confirmation email
                    Guid? orderErrorEmailId = args.Model.Store.ErrorEmailTemplateId;
                    if (orderErrorEmailId.HasValue)
                    {
                        EmailTemplate orderErrorEmail = await commerceApi.GetEmailTemplateAsync(orderErrorEmailId.Value)
                            .AsWritableAsync(uow)
                            .SetTemplateViewAsync("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderErrorEmail.cshtml");

                        await commerceApi.SaveEmailTemplateAsync(orderErrorEmail, cancellationToken);
                    }

                    // Update gift card email
                    Guid? giftCardEmailId = args.Model.Store.DefaultGiftCardEmailTemplateId;
                    if (giftCardEmailId.HasValue)
                    {
                        EmailTemplate giftCardEmail = await commerceApi.GetEmailTemplateAsync(giftCardEmailId.Value)
                            .AsWritableAsync(uow)
                            .SetTemplateViewAsync("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutGiftCardEmail.cshtml");

                        await commerceApi.SaveEmailTemplateAsync(giftCardEmail, cancellationToken);
                    }

                    uow.Complete();
                }
            , cancellationToken);

            // Continue the pipeline
            return Ok();
        }
    }
}
