using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class MarketController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<MarketController> _Log;
        private readonly EVEStandardAPI _ESIClient;
        private readonly DBService _DBService;

        public MarketController(ILogger<MarketController> logger, IConfiguration configuration, EVEStandardAPI _ESIClient)
        {
            _Log = logger;
            _Config = configuration;
            this._ESIClient = _ESIClient;

            string dbFileName = _Config["DBFileName"];
            string sdeFileName = _Config["SDEFileName"];
            string sdeTempCompressedFileName = _Config["SDETempCompressedFileName"];
            string sdeTempFileName = _Config["SDETempFileName"];
            string sdeDownloadUrl = _Config["SDEDownloadURL"];
            _DBService = new DBService(_Log, dbFileName, sdeFileName, sdeTempCompressedFileName, sdeTempFileName, sdeDownloadUrl);
        }

        public async Task<IActionResult> Index()
        {
            List<MarketModel> result = new List<MarketModel>();
            List<MarketAveragePrices_Row> marketPrices = _DBService.GetAveragePrices();
            // GetById the type names for display
            List<TypeNameDTO> typeNames = new List<TypeNameDTO>();
            typeNames = _DBService.GetTypeNames();
            // Bind to the model
            foreach(MarketAveragePrices_Row marketPrice in marketPrices)
            {
                string typeName = typeNames.Where(x => x.Id == marketPrice.TypeId).Select(x => x.Name).FirstOrDefault();
                MarketModel marketRecord = new MarketModel()
                {
                    TypeId = marketPrice.TypeId,
                    TypeName = typeName,
                    AdjustedPrice = marketPrice.AdjustedPrice,
                    AveragePrice = marketPrice.AveragePrice,
                    Timestamp = marketPrice.Timestamp
                };
                result.Add(marketRecord);
            }

            var model = new MarketPageViewModel
            {
                Prices = result
            };

            return View(model);
        }

        public async Task<IActionResult> BestSellPrices()
        {
            List<RegionMarketOrder> result = new List<RegionMarketOrder>();

            List<RegionMarketOrdersRow> orders = _DBService.GetBestSellPrices();
            List<TypeNameDTO> typeNames = _DBService.GetTypeNames();
            // Bind to a model with type id name
            for (int x = 0; x < orders.Count; x++)
            {
                TypeNameDTO typeName = typeNames.Where(y => y.Id == orders[x].TypeId).FirstOrDefault();
                if (typeName != null && !String.IsNullOrWhiteSpace(typeName.Name))
                {
                    RegionMarketOrder order = new RegionMarketOrder(orders[x], typeName.Name);
                    result.Add(order);
                }
            }

            var model = new MarketBestPricesPageViewModel
            {
                Orders = result
            };

            return View(model);
        }

        public async Task<IActionResult> BestBuyPrices()
        {
            List<RegionMarketOrder> result = new List<RegionMarketOrder>();

            List<RegionMarketOrdersRow> orders = _DBService.GetBestBuyPrices();
            List<TypeNameDTO> typeNames = _DBService.GetTypeNames();
            // Bind to a model with type id name
            for (int x = 0; x < orders.Count; x++)
            {
                TypeNameDTO typeName = typeNames.Where(y => y.Id == orders[x].TypeId).FirstOrDefault();
                if (typeName != null && !String.IsNullOrWhiteSpace(typeName.Name))
                {
                    RegionMarketOrder order = new RegionMarketOrder(orders[x], typeName.Name);
                    result.Add(order);
                }
            }

            var model = new MarketBestPricesPageViewModel
            {
                Orders = result
            };

            return View(model);
        }

        public async Task<IActionResult> Opportunities(int threshold, string marketGroup, string group, string groupCategory)
        {
            List<string> marketGroups = _DBService.GetMarketOpportunityMarketGroups();
            List<string> groups = _DBService.GetMarketOpportunityGroups();
            List<string> groupCategories = _DBService.GetMarketOpportunityGroupCategories();

            if (threshold == null || (threshold != 1000000 && threshold != 10000000 && threshold != 100000000))
            {
                threshold = 1000000;
            }
            string queryMarketGroupName = marketGroup;
            string queryGroupName = group;
            string queryGroupCategoryName = groupCategory;
            if (marketGroup == "All") queryMarketGroupName = null;
            if (group == "All") queryGroupName = null;
            if (groupCategory == "All") queryGroupCategoryName = null;

            List<MarketOpportunitiesDetail_Row> opportunities = new List<MarketOpportunitiesDetail_Row>();
            opportunities = _DBService.GetMarketOpportunities(threshold, queryMarketGroupName, queryGroupName, queryGroupCategoryName);

            var model = new MarketOpportunitiesPageViewModel()
            {
                MarketGroups = marketGroups,
                Groups = groups,
                GroupCategories = groupCategories,
                QueryThreshold = threshold,
                QueryMarketGroupName = marketGroup,
                QueryGroupName = group,
                QueryGroupCategoryName = groupCategory,
                Opportunities = opportunities
            };

            return View(model);
        }
    }
}
