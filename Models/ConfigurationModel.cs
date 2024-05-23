using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.SearchOrders")]
        public bool SearchOrders { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.MaxOrderResults")]
        public int MaxOrderResults { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerEmails")]
        public bool SearchCustomerEmails { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerNames")]
        public bool SearchCustomerNames { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.MaxCustomerResults")]
        public int MaxCustomerResults { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.SearchProductSkus")]
        public bool SearchProductSkus { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.AdvancedAdminSearch.Fields.MaxProductResults")]
        public int MaxProductResults { get; set; }
    }
}