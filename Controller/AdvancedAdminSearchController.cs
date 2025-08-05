using System.Linq.Dynamic.Core;
using dfdev.Plugin.Widgets.AdvancedAdminSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace dfdev.Plugin.Widgets.AdvancedAdminSearch.Controller;

public class AdvancedAdminSearchController : BaseAdminController
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    private readonly AdvancedAdminSearchSettings _advancedAdminSearchSettings;

    private readonly IRepository<Order> _orders;
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<Product> _products;

    #endregion

    #region Ctor

    public AdvancedAdminSearchController(ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        AdvancedAdminSearchSettings advancedAdminSearchSettings,
        IRepository<Order> orders,
        IRepository<Customer> customers,
        IRepository<Product> products)
    {
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;

        _advancedAdminSearchSettings = advancedAdminSearchSettings;

        _orders = orders;
        _customers = customers;
        _products = products;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel
        {
            SearchCustomerEmails = _advancedAdminSearchSettings.SearchCustomerEmails,
            SearchCustomerNames = _advancedAdminSearchSettings.SearchCustomerNames,
            MaxCustomerResults = _advancedAdminSearchSettings.MaxCustomerResults,
            SearchOrders = _advancedAdminSearchSettings.SearchOrders,
            MaxOrderResults = _advancedAdminSearchSettings.MaxOrderResults,
            SearchProductSkus = _advancedAdminSearchSettings.SearchProductSkus,
            MaxProductResults = _advancedAdminSearchSettings.MaxProductResults,
        };

        return View("~/Plugins/Widgets.AdvancedAdminSearch/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        //save settings
        _advancedAdminSearchSettings.SearchCustomerEmails = model.SearchCustomerEmails;
        _advancedAdminSearchSettings.SearchCustomerNames = model.SearchCustomerNames;
        _advancedAdminSearchSettings.MaxCustomerResults = model.MaxCustomerResults;
        _advancedAdminSearchSettings.SearchOrders = model.SearchOrders;
        _advancedAdminSearchSettings.MaxOrderResults = model.MaxOrderResults;
        _advancedAdminSearchSettings.SearchProductSkus = model.SearchProductSkus;
        _advancedAdminSearchSettings.MaxProductResults = model.MaxProductResults;

        await _settingService.SaveSettingAsync(_advancedAdminSearchSettings);
        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost, ActionName("GetResults")]
    public async Task<IActionResult> GetResultsAsync(AdminQueryModel model)
    {
        var htmlLinebreak = "<br/>";

        model.SearchQuery = model.SearchQuery.Trim();

        if (string.IsNullOrEmpty(model.SearchQuery))
            return Json(new { status = true, model.SearchQuery });

        var (products, customers, orders) = (getProductsAsync(model.SearchQuery), getCustomersAsync(model.SearchQuery), getOrdersAsync(model.SearchQuery));

        return Json(new
        {
            status = true,
            model.SearchQuery,
            results = new SearchResultsModel(await orders, await customers, await products)
        });

        #region Data retrieval

        async Task<IEnumerable<SearchResultModel>> getOrdersAsync(string query)
        {
            if (_advancedAdminSearchSettings.SearchOrders)
            {
                var orderGuid = Guid.TryParse(query, out var og) ? og : Guid.Empty;
                var orderIdSearch = int.TryParse(query, out var id) ? id : 0;

                var order = await (from o in _orders.Table
                                   join c in _customers.Table on o.CustomerId equals c.Id
                                   where o.Id == orderIdSearch || o.OrderGuid == orderGuid
                                   select new { o.Id, CustomerName = c.FirstName + " " + c.LastName }).FirstOrDefaultAsync();

                if (order != null)
                    return new List<SearchResultModel>() { new(order.Id, $"Order #{order.Id}{htmlLinebreak}{order.CustomerName}", $"/Admin/Order/Edit/{order.Id}") };
            }

            return new List<SearchResultModel>();
        }

        async Task<IEnumerable<SearchResultModel>> getProductsAsync(string query)
        {
            if (_advancedAdminSearchSettings.SearchProductSkus)
            {
                var products = (await (from p in _products.Table
                                       where p.Sku.Contains(query, StringComparison.InvariantCultureIgnoreCase) && !p.Deleted
                                       select new
                                       {
                                           p.Id,
                                           p.Sku,
                                           p.Name
                                       }).ToListAsync())
                                      .Select(p => new
                                      {
                                          p.Id,
                                          p.Sku,
                                          p.Name,
                                          Score =
                                              (p.Sku.Equals(query, StringComparison.InvariantCultureIgnoreCase) ? 50 : 0) +
                                              (p.Sku.StartsWith(query, StringComparison.InvariantCultureIgnoreCase) ? 5 : 0) +
                                              (p.Sku.StartsWith(query, StringComparison.InvariantCultureIgnoreCase) ? 5 : 0) +
                                              (p.Sku.Contains(query, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0)
                                      })
                                      .OrderByDescending(x => x.Score).Take(_advancedAdminSearchSettings.MaxProductResults).ToList();

                if (products.Count != 0)
                    return products.Select(x => new SearchResultModel(x.Id, $"{x.Sku} - {x.Name}", $"/Admin/Product/Edit/{x.Id}"));
            }

            return new List<SearchResultModel>();
        }

        async Task<IEnumerable<SearchResultModel>> getCustomersAsync(string query)
        {
            var customerResults = new List<SearchResultModel>();

            if (_advancedAdminSearchSettings.SearchCustomerEmails)
            {
                var customers = (await (from c in _customers.Table
                                        where !c.Deleted && c.Email.Contains(query, StringComparison.InvariantCultureIgnoreCase)
                                        select new { c.Id, c.FirstName, c.LastName, c.Email }).ToListAsync())
                                       .Select(x => new
                                       {
                                           x.Id,
                                           Name = $"{x.FirstName} {x.LastName}",
                                           x.Email,
                                           Score = (x.Email.Equals(query, StringComparison.InvariantCultureIgnoreCase) ? 100 : 0) +
                                           (x.Email.StartsWith(query, StringComparison.InvariantCultureIgnoreCase) ? 10 : 0) +
                                            (x.Email.Contains(query, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0)
                                       });

                if (customers.Any())
                    customerResults.AddRange(customers.Select(x => new SearchResultModel(x.Id, $"{x.Name}{htmlLinebreak}{x.Email}", $"/Admin/Customer/Edit/{x.Id}", x.Score)));
            }

            if (_advancedAdminSearchSettings.SearchCustomerNames)
            {
                var customers = (await (from c in _customers.Table
                                        where !c.Deleted &&
                                             (c.FirstName + " " + c.LastName).Contains(query, StringComparison.InvariantCultureIgnoreCase)
                                        select new { c.Id, c.FirstName, c.LastName, c.Email }).ToListAsync())
                                       .Select(x => new
                                       {
                                           x.Id,
                                           Name = $"{x.FirstName} {x.LastName}",
                                           x.Email,
                                           Score = ($"{x.FirstName} {x.LastName}".Equals(query, StringComparison.InvariantCultureIgnoreCase) ? 100 : 0) +
                                           (x.FirstName.StartsWith(query, StringComparison.InvariantCultureIgnoreCase) ? 15 : 0) +
                                            (x.LastName.StartsWith(query, StringComparison.InvariantCultureIgnoreCase) ? 10 : 0) +
                                            ($"{x.FirstName} {x.LastName}".Contains(query, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0)
                                       });

                foreach (var customer in customers)
                {
                    if (customerResults.FirstOrDefault(x => x.EntityId == customer.Id) is SearchResultModel existingResult)
                        existingResult.RelevanceScore += customer.Score;
                    else
                        customerResults.Add(new SearchResultModel(customer.Id, $"{customer.Name}{htmlLinebreak}{customer.Email}", $"/Admin/Customer/Edit/{customer.Id}", customer.Score));
                }

            }

            customerResults = customerResults.OrderByDescending(x => x.RelevanceScore).ToList();

            if (customerResults.Count > _advancedAdminSearchSettings.MaxCustomerResults)
                customerResults = customerResults.Take(_advancedAdminSearchSettings.MaxCustomerResults).ToList();

            return customerResults;
        }

        #endregion
    }

    #endregion
}
