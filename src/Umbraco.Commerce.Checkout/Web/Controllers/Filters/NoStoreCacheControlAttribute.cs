using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Commerce.Checkout.Web.Controllers.Filters
{
    /// <summary>
    /// Add "cache-control: no-store" header to the response.
    /// </summary>
    internal sealed class NoStoreCacheControlAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context) =>
            context.HttpContext.Response.Headers.CacheControl = "no-store";
    }
}
