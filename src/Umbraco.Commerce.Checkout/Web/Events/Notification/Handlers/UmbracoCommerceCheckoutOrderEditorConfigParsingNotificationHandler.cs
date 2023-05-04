using System.Linq;
using Umbraco.Commerce.Common.Events;
using Umbraco.Commerce.Cms.Web.Events.Notification;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Commerce.Checkout.Web.Events.Notification.Handlers
{
    public class UmbracoCommerceCheckoutOrderEditorConfigParsingNotificationHandler : NotificationEventHandlerBase<OrderEditorConfigParsingNotification>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public UmbracoCommerceCheckoutOrderEditorConfigParsingNotificationHandler(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        private IUmbracoContext UmbracoContext => _umbracoContextAccessor.GetRequiredUmbracoContext();

        public override void Handle(OrderEditorConfigParsingNotification evt)
        {
            if (evt.Config == null)
                return;

            var checkoutContentType = UmbracoContext.Content.GetContentType("uccCheckoutPage");
            var checkoutPages = UmbracoContext.Content.GetByContentType(checkoutContentType);
            var checkoutPage = checkoutPages.FirstOrDefault(x => x.GetStore()?.Id == evt.StoreId);

            if (checkoutPage != null)
            {
                // Extract config settings
                var uccCollectShippingInfo = checkoutPage.Value<bool>("uccCollectShippingInfo");

                // Update parsed config
                evt.Config["shipping"]["enabled"] = uccCollectShippingInfo;
            }
        }
    }
}
