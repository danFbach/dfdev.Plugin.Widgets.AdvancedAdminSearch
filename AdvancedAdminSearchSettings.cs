using Nop.Core.Configuration;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch
{
    public class AdvancedAdminSearchSettings : ISettings
    {
        public bool SearchOrders { get; set; }

        public bool SearchCustomerEmails { get; set; }

        public bool SearchCustomerNames { get; set; }

        public bool SearchProductSkus { get; set; }

    }
}
