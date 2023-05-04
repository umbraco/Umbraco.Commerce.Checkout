using Umbraco.Commerce.Common.Pipelines;
using Umbraco.Commerce.Common.Pipelines.Tasks;
using Umbraco.Commerce.Core.Api;

namespace Umbraco.Commerce.Checkout.Pipeline.Tasks
{
    public class ConfigureUmbracoCommerceStoreTask : PipelineTaskBase<InstallPipelineContext>
    {
        private readonly IUmbracoCommerceApi _commerceApi;

        public ConfigureUmbracoCommerceStoreTask(IUmbracoCommerceApi commerceApi)
        {
            _commerceApi = commerceApi;
        }

        public override PipelineResult<InstallPipelineContext> Execute(PipelineArgs<InstallPipelineContext> args)
        {
            _commerceApi.Uow.Execute(uow =>
            {
                // Update order confirmation email
                var orderConfirmationEmailId = args.Model.Store.ConfirmationEmailTemplateId;
                if (orderConfirmationEmailId.HasValue)
                {
                    var orderConfirmationEmail = _commerceApi.GetEmailTemplate(orderConfirmationEmailId.Value)
                        .AsWritable(uow)
                        .SetTemplateView("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderConfirmationEmail.cshtml");

                    _commerceApi.SaveEmailTemplate(orderConfirmationEmail);
                }

                // Update order confirmation email
                var orderErrorEmailId = args.Model.Store.ErrorEmailTemplateId;
                if (orderErrorEmailId.HasValue)
                {
                    var orderErrorEmail = _commerceApi.GetEmailTemplate(orderErrorEmailId.Value)
                        .AsWritable(uow)
                        .SetTemplateView("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutOrderErrorEmail.cshtml");

                    _commerceApi.SaveEmailTemplate(orderErrorEmail);
                }

                // Update gift card email
                var giftCardEmailId = args.Model.Store.DefaultGiftCardEmailTemplateId;
                if (giftCardEmailId.HasValue)
                {
                    var giftCardEmail = _commerceApi.GetEmailTemplate(giftCardEmailId.Value)
                        .AsWritable(uow)
                        .SetTemplateView("~/Views/UmbracoCommerceCheckout/Templates/Email/UmbracoCommerceCheckoutGiftCardEmail.cshtml");

                    _commerceApi.SaveEmailTemplate(giftCardEmail);
                }

                uow.Complete();
            });

            // Continue the pipeline
            return Ok();
        }
    }
}
