using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => int.MaxValue;
    }
}
