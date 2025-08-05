using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch.Components;

public class AdminSearchComponent : NopViewComponent
{
    public AdminSearchComponent() { }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return await Task.FromResult(View("~/Plugins/Widgets.AdvancedAdminSearch/Views/Components/AdminSearch.cshtml"));
    }
}
