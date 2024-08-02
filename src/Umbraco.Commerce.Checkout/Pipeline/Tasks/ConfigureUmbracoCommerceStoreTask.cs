using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class ConfigureUmbracoCommerceStoreTask : AsyncPipelineTaskBase<InstallPipelineContext>
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public ConfigureUmbracoCommerceStoreTask(IUmbracoCommerceApi commerceApi)
        {
            _commerceApi = commerceApi;
        }

        public override Task<PipelineResult<InstallPipelineContext>> ExecuteAsync(PipelineArgs<InstallPipelineContext> args, CancellationToken cancellationToken = default)
        {
            _commerceApi.Uow.Execute(uow =>
            {
                // Update order confirmation email
                Guid? orderConfirmationEmailId = args.Model.Store.ConfirmationEmailTemplateId;
                if (orderConfirmationEmailId.HasValue)
                {
                    EmailTemplate orderConfirmationEmail = _commerceApi.GetEmailTemplate(orderConfirmationEmailId.Value)
                        .AsWritable(uow)
                        .SetTemplateView("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderConfirmationEmail.cshtml");

                    _commerceApi.SaveEmailTemplate(orderConfirmationEmail);
                }

                // Update order confirmation email
                Guid? orderErrorEmailId = args.Model.Store.ErrorEmailTemplateId;
                if (orderErrorEmailId.HasValue)
                {
                    EmailTemplate orderErrorEmail = _commerceApi.GetEmailTemplate(orderErrorEmailId.Value)
                        .AsWritable(uow)
                        .SetTemplateView("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderErrorEmail.cshtml");

                    _commerceApi.SaveEmailTemplate(orderErrorEmail);
                }

                // Update gift card email
                Guid? giftCardEmailId = args.Model.Store.DefaultGiftCardEmailTemplateId;
                if (giftCardEmailId.HasValue)
                {
                    EmailTemplate giftCardEmail = _commerceApi.GetEmailTemplate(giftCardEmailId.Value)
                        .AsWritable(uow)
                        .SetTemplateView("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutGiftCardEmail.cshtml");

                    _commerceApi.SaveEmailTemplate(giftCardEmail);
                }

                uow.Complete();
            });

            // Continue the pipeline
            return Task.FromResult(Ok());
        }
    }
}
