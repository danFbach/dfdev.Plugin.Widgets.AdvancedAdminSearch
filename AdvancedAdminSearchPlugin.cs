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
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public AdvancedAdminSearchPlugin(ISettingService settingService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IPermissionService permissionService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _permissionService = permissionService;
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
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.MaxOrderResults"] = "Number of Order Results",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerEmails"] = "Search Customer Emails",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerNames"] = "Search Customer Names",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.MaxCustomerResults"] = "Number of Customer Results",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchProductSkus"] = "Search Product Skus",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.MaxProductResults"] = "Number of Product Results",
            });

            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>();

            if (!widgetSettings.ActiveWidgetSystemNames.Contains("Widgets.AdvancedAdminSearch"))
            {
                widgetSettings.ActiveWidgetSystemNames.Add("Widgets.AdvancedAdminSearch");
                await _settingService.SaveSettingAsync(widgetSettings);
            }

            await base.InstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchOrders"] = "Search Order Numbers",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.MaxOrderResults"] = "Number of Order Results",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerEmails"] = "Search Customer Emails",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchCustomerNames"] = "Search Customer Names",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.MaxCustomerResults"] = "Number of Customer Results",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.SearchProductSkus"] = "Search Product Skus",
                ["Plugins.Widgets.AdvancedAdminSearch.Fields.MaxProductResults"] = "Number of Product Results",
            });

            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return;

            var dfdevNode = rootNode.ChildNodes.FirstOrDefault(node => node.Title.Equals("dfDev"));

            if (dfdevNode is null)
            {
                dfdevNode = new SiteMapNode()
                {
                    IconClass = "fas fa-dna",
                    Title = "dfDev",
                    SystemName = "dfdev",
                    Visible = true,
                    RouteValues = new RouteValueDictionary(),

                };
                rootNode.ChildNodes.Add(dfdevNode);
            }

            var dfdevPluginNode = dfdevNode.ChildNodes.FirstOrDefault(node => node.Title.Equals("Plugins"));

            if (dfdevPluginNode is null)
            {
                dfdevPluginNode = new SiteMapNode()
                {
                    IconClass = "fa icon-plugins",
                    Title = "Plugins",
                    SystemName = "dfdev.plugins",
                    Visible = true,
                    RouteValues = new RouteValueDictionary(),
                };
                dfdevNode.ChildNodes.Add(dfdevPluginNode);
            }

            dfdevPluginNode.ChildNodes.Insert(0, new SiteMapNode
            {
                SystemName = "Widgets.AdvancedAdminSearch.Configure",
                Title = "Advanced Admin Search",
                ControllerName = "AdvancedAdminSearch",
                ActionName = "Configure",
                IconClass = "fab fa-searchengin",
                Visible = true,
                RouteValues = new RouteValueDictionary()
            });

            return;
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
