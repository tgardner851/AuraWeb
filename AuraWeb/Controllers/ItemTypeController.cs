using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class ItemTypeController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<ItemTypeController> _Log;
        private readonly EVEStandardAPI _ESIClient;
        private readonly string _SDEFileName;
        private readonly string _SDEDownloadUrl;
        private readonly string _SDEBackupFileName;
        private readonly string _SDETempCompressedFileName;
        private readonly string _SDETempFileName;
        private readonly SDEService _SDEService;
        private readonly MarketService _MarketService;
        private readonly string _MarketDbPath;

        public ItemTypeController(ILogger<ItemTypeController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;

            _SDEFileName = _Config["SDEFileName"];
            _SDEBackupFileName = _Config["SDEBackupFileName"];
            _SDETempCompressedFileName = _Config["SDETempCompressedFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempCompressedFileName, _SDETempFileName, _SDEBackupFileName, _SDEDownloadUrl);

            _MarketDbPath = _Config["MarketFileName"];
            _MarketService = new MarketService(_Log, _MarketDbPath);

            this._ESIClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ItemTypePageViewModel
            {

            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ItemTypeInfoOpenInfoWindowForItemType(ItemTypeInfoPageViewModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, model.OpenInfoModel.ItemTypeId);
            return RedirectToAction("ItemTypeInfo", new { id = model.OpenInfoModel.ItemTypeId });
        }

        [HttpPost]
        public async Task<ActionResult> ItemTypeInfoOpenMarketWindowForItemType(ItemTypeInfoPageViewModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenMarketDetailsV1Async(auth, model.OpenMarketModel.ItemTypeId);
            return RedirectToAction("ItemTypeInfo", new { id = model.OpenMarketModel.ItemTypeId });
        }

        public async Task<IActionResult> ItemTypes(string query)
        {
            List<ItemType_V_Row> itemTypes = new List<ItemType_V_Row>();

            if (String.IsNullOrWhiteSpace(query))
            {
                itemTypes = _SDEService.GetAllItemTypes();
            }
            else
            {
                itemTypes = _SDEService.SearchItemTypes(query);
            }

            var model = new ItemTypesPageViewModel
            {
                ItemTypes = itemTypes,
                Query = query
            };

            return View(model);
        }

        public async Task<IActionResult> ItemTypeInfo(int id)
        {
            ItemType_V_Row itemType = _SDEService.GetItemType(id);

            // TODO: SDE views don't handle this right now
            var itemTypeApi = await _ESIClient.Universe.GetTypeInfoV3Async(id);
            EVEStandard.Models.Type itemTypeApiModel = itemTypeApi.Model;

            MarketAveragePrices_Row averagePrice = _MarketService.GetAveragePriceForTypeId(id);

            #region Best Sell/Buy Prices
            List<RegionMarketOrdersModel> bestSellPrices = new List<RegionMarketOrdersModel>();
            List<RegionMarketOrdersRow> bestSellPricesResult = _MarketService.GetBestSellPricesForTypeId(id);
            List<RegionMarketOrdersModel> bestBuyPrices = new List<RegionMarketOrdersModel>();
            List<RegionMarketOrdersRow> bestBuyPricesResult = _MarketService.GetBestBuyPricesForTypeId(id);
            List<int> systemIds = bestSellPricesResult.Select(x => x.SystemId).ToList();
            systemIds.AddRange(bestBuyPrices.Select(x => x.SystemId));
            List<SolarSystem_V_Row> systems = _SDEService.GetSolarSystems(systemIds);

            // TODO: Fucking fix this, system name is not always getting assigned to the object

            foreach (RegionMarketOrdersRow r in bestSellPricesResult)
            {
                string systemName = systems.Where(x => x.Id == r.SystemId).Select(x => x.Name).FirstOrDefault();
                bestSellPrices.Add(new RegionMarketOrdersModel()
                {
                    SystemId = r.SystemId,
                    SystemName = systemName,
                    Range = r.Range,
                    Price = r.Price
                });
            }

            // TODO: Fucking fix this, system name is not always getting assigned to the object

            foreach (RegionMarketOrdersRow r in bestBuyPricesResult)
            {
                string systemName = systems.Where(x => x.Id == r.SystemId).Select(x => x.Name).FirstOrDefault();
                bestBuyPrices.Add(new RegionMarketOrdersModel()
                {
                    SystemId = r.SystemId,
                    SystemName = systemName,
                    Range = r.Range,
                    Price = r.Price
                });
            }
            #endregion

            var model = new ItemTypeInfoPageViewModel
            {
                ItemTypeId = id,
                ItemType = itemType,
                ItemType_API = itemTypeApiModel,
                AveragePrice = averagePrice,
                BestSellPrices = bestSellPrices,
                BestBuyPrices = bestBuyPrices,
                OpenMarketModel = new ItemTypeInfoOpenMarketModel(),
                OpenInfoModel = new ItemTypeInfoOpenInfoModel()
            };

            return View(model);
        }

        public async Task<IActionResult> Ships(string query)
        {
            List<ItemType_V_Row> ships = new List<ItemType_V_Row>();
            if (!String.IsNullOrWhiteSpace(query)) // Search for ships
            {
                ships = _SDEService.SearchShips(query);
            }
            else // Return all ships
            {
                ships = _SDEService.GetAllShips();
            }

            var model = new ShipsPageViewModel
            {
                Query = query,
                Ships = ships
            };

            return View(model);
        }

        public async Task<IActionResult> Modules(string query)
        {
            List<ItemType_V_Row> modules = new List<ItemType_V_Row>();
            if (!String.IsNullOrWhiteSpace(query)) // Search for modules
            {
                modules = _SDEService.SearchModules(query);
            }
            else // Return all modules
            {
                modules = _SDEService.GetAllModules();
            }

            var model = new ModulesPageViewModel
            {
                Query = query,
                Modules = modules
            };

            return View(model);
        }
    }
}
