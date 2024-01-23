using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch
{
    /// <summary>
    /// Rename this file and change to the correct type
    /// </summary>
    public class AdvancedAdminSearchPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public AdvancedAdminSearchPlugin(ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        public bool HideInWidgetList => false;

        #endregion

        #region Methods
        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AdvancedAdminSearch/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == AdminWidgetZones.HeaderNavbarBefore)
                return typeof(Components.AdminSearchComponent);

            throw new NotImplementedException();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.HeaderNavbarBefore });
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new AdvancedAdminSearchSettings());

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchOrders"] = "Search Order Numbers",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerEmails"] = "Search Customer Emails",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerNames"] = "Search Customer Names",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchProductSkus"] = "Search Product Skus",
            });

            await base.InstallAsync();
        }

        public Task ManageSiteMapAsync(SiteMapNode rootNode)
        {

            var configNode = rootNode.ChildNodes.FirstOrDefault(node => node.Title.Equals("Configuration"));
            if (configNode == null)
                return Task.CompletedTask;

            var settingsNode = configNode.ChildNodes.FirstOrDefault(x => x.Title.Equals("Settings"));
            if (settingsNode == null)
                return Task.CompletedTask;

            var index = 0;

            var allSettingsNode = settingsNode.ChildNodes.FirstOrDefault(x => x.Title.Equals("All settings (advanced)"));

            if (allSettingsNode != null)
                index = settingsNode.ChildNodes.IndexOf(allSettingsNode);

            configNode.ChildNodes.Insert(index - 1, new SiteMapNode
            {
                SystemName = "Widgets.AdvancedAdminSearch.Configure",
                Title = "Advanced Admin Search",
                ControllerName = "AdvancedAdminSearch",
                ActionName = "Configure",
                IconClass = "far fa-dot-circle",
                Visible = true,
                RouteValues = new RouteValueDictionary() { }
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            await base.UninstallAsync();
        }
        #endregion
    }
}
