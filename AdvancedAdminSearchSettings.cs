using Nop.Core.Configuration;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch;

public class AdvancedAdminSearchSettings : ISettings
{
    public bool SearchOrders { get; set; }

    public int MaxOrderResults { get; set; } = 5;

    public bool SearchCustomerEmails { get; set; }

    public bool SearchCustomerNames { get; set; }

    public int MaxCustomerResults { get; set; } = 5;

    public bool SearchProductSkus { get; set; }

    public int MaxProductResults { get; set; } = 5;

}
