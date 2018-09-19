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
        private readonly EVEStandardAPI esiClient;
        private readonly SDEService _SDEService;
        private readonly string _SDEFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadURL;

        public MarketController(ILogger<MarketController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this.esiClient = esiClient;
            _SDEFileName = _Config["SDEFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadURL = _Config["SDEDownloadURL"];

            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempFileName);
        }

        public async Task<IActionResult> Index()
        {
            List<MarketModel> result = new List<MarketModel>();
            var marketPrices = await esiClient.Market.ListMarketPricesV1Async();
            // Get the type names for display
            List<TypeNameDTO> typeNames = new List<TypeNameDTO>();
            typeNames = _SDEService.GetTypeNames();
            // Bind to the model
            foreach(var marketPrice in marketPrices.Model)
            {
                string typeName = typeNames.Where(x => x.Id == marketPrice.TypeId).Select(x => x.Name).FirstOrDefault();
                MarketModel marketRecord = new MarketModel()
                {
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
    }
}
