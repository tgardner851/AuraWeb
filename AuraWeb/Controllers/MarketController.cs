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
        private readonly SDEService _SDEService;
        private readonly string _SDEFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadUrl;
        private readonly MarketService _MarketService;
        private readonly string _MarketDbPath;

        public MarketController(ILogger<MarketController> logger, IConfiguration configuration, EVEStandardAPI _ESIClient)
        {
            _Log = logger;
            _Config = configuration;
            this._ESIClient = _ESIClient;
            _SDEFileName = _Config["SDEFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _MarketDbPath = _Config["MarketFileName"];

            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempFileName, _SDEDownloadUrl);
            _MarketService = new MarketService(_Log, _MarketDbPath);
        }

        public async Task<IActionResult> Index()
        {
            List<MarketModel> result = new List<MarketModel>();
            var marketPrices = await _ESIClient.Market.ListMarketPricesV1Async();
            // GetById the type names for display
            List<TypeNameDTO> typeNames = new List<TypeNameDTO>();
            typeNames = _SDEService.GetTypeNames();
            // Bind to the model
            foreach(var marketPrice in marketPrices.Model)
            {
                string typeName = typeNames.Where(x => x.Id == marketPrice.TypeId).Select(x => x.Name).FirstOrDefault();
                MarketModel marketRecord = new MarketModel()
                {
                    TypeId = marketPrice.TypeId,
                    TypeName = typeName,
                    AdjustedPrice = marketPrice.AdjustedPrice,
                    AveragePrice = marketPrice.AveragePrice
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

            List<RegionMarketOrdersRow> orders = _MarketService.GetBestSellPrices();
            List<TypeNameDTO> typeNames = _SDEService.GetTypeNames();
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

            List<RegionMarketOrdersRow> orders = _MarketService.GetBestBuyPrices();
            List<TypeNameDTO> typeNames = _SDEService.GetTypeNames();
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
    }
}
