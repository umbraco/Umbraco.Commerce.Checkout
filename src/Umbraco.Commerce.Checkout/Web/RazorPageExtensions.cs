
using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Commerce.Checkout.Web
{
    public static class RazorPageExtensions
    {
        public static TService GetRequiredService<TService>(this RazorPage view)
        {
            ArgumentNullException.ThrowIfNull(view);
            return (TService)view.Context.RequestServices.GetRequiredService(typeof(TService));
        }
    }
}
