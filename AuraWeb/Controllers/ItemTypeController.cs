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
            systemIds.AddRange(bestBuyPricesResult.Select(x => x.SystemId));
            List<SolarSystem_V_Row> systems = _SDEService.GetSolarSystems(systemIds);

            for (int x = 0; x < bestSellPricesResult.Count; x++)
            {
                RegionMarketOrdersRow r = bestSellPricesResult[x];
                int systemId = r.SystemId;
                string systemName = systems.Where(a => a.Id == systemId).Select(a => a.Name).FirstOrDefault();
                string range = r.Range; // Format range string
                int rangeInt = -1;
                Int32.TryParse(range, out rangeInt);
                if (rangeInt > 0) range = String.Format("{0} Jumps", rangeInt);
                else range = range.FirstCharToUpper();
                RegionMarketOrdersModel orderModel = new RegionMarketOrdersModel()
                {
                    SystemId = r.SystemId,
                    SystemName = systemName,
                    Range = range,
                    Price = r.Price
                };
                bestSellPrices.Add(orderModel);
            }
            for (int x = 0; x < bestBuyPricesResult.Count; x++)
            {
                RegionMarketOrdersRow r = bestBuyPricesResult[x];
                int systemId = r.SystemId;
                string systemName = systems.Where(a => a.Id == systemId).Select(a => a.Name).FirstOrDefault();
                string range = r.Range; // Format range string
                int rangeInt = -1;
                Int32.TryParse(range, out rangeInt);
                if (rangeInt > 0) range = String.Format("{0} Jumps", rangeInt);
                else range = range.FirstCharToUpper();
                RegionMarketOrdersModel orderModel = new RegionMarketOrdersModel()
                {
                    SystemId = r.SystemId,
                    SystemName = systemName,
                    Range = range,
                    Price = r.Price
                };
                bestBuyPrices.Add(orderModel);
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

        public async Task<IActionResult> Ships(string view, string name, string race, string group)
        {
            List<string> shipRaces = _SDEService.GetAllShipRaces();
            List<string> shipGroups = _SDEService.GetAllShipGroups();

            string queryRace = race;
            string queryGroup = group;
            string queryName = name;
            if (race == "All") queryRace = null;
            if (group == "All") queryGroup = null;
            if (String.IsNullOrEmpty(name)) queryName = null;

            List<ItemType_V_Row> ships = _SDEService.GetAllShipsForGroupRaceAndName(queryName, queryRace, queryGroup);

            if (view == null) view = "Table";

            var model = new ShipsPageViewModel
            {
                View = view,

                QueryName = name,
                QueryRace = race,
                QueryGroup = group,
                ShipRaces = shipRaces,
                ShipGroups = shipGroups,
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

        public async Task<IActionResult> Ores(string view, string name)
        {
            List<OreDataModel> ores = new List<OreDataModel>();
            List<ItemType_V_Row> oresData = new List<ItemType_V_Row>();
            if (!String.IsNullOrWhiteSpace(name)) // Return all modules
            {
                oresData = _SDEService.SearchOre(name);
            }
            else
            {
                oresData = _SDEService.GetAllOre();
            }

            // Get the first (1) result for each item
            foreach (ItemType_V_Row ore in oresData)
            {
                string systemName = String.Empty;

                RegionMarketOrdersRow bestBuyData = _MarketService.GetBestBuyPricesForTypeId(ore.Id, 1).FirstOrDefault();
                if (bestBuyData == null || bestBuyData.SystemId <= 0) systemName = "--";
                else systemName = _SDEService.GetSolarSystem(bestBuyData.SystemId).Name;
                RegionMarketOrdersModel bestBuy = new RegionMarketOrdersModel()
                {
                    SystemId = (bestBuyData == null) ? -1 : bestBuyData.SystemId,
                    SystemName = systemName,
                    Range = (bestBuyData == null) ? "--" : SDEHelpers.FormatString_Range(bestBuyData.Range),
                    Price = (bestBuyData == null) ? -1 : bestBuyData.Price
                };

                RegionMarketOrdersRow bestSellData = _MarketService.GetBestSellPricesForTypeId(ore.Id, 1).FirstOrDefault();
                if (bestSellData == null || bestSellData.SystemId <= 0) systemName = "--";
                else systemName = _SDEService.GetSolarSystem(bestSellData.SystemId).Name;
                RegionMarketOrdersModel bestSell = new RegionMarketOrdersModel()
                {
                    SystemId = (bestSellData == null) ? -1 : bestSellData.SystemId,
                    SystemName = systemName,
                    Range = (bestSellData == null) ? "--" : SDEHelpers.FormatString_Range(bestSellData.Range),
                    Price = (bestSellData == null) ? -1 : bestSellData.Price
                };

                ores.Add(new OreDataModel()
                {
                    Ore = ore,
                    BestBuyPrice = bestBuy,
                    BestSellPrice = bestSell
                });
            }

            ores = ores.OrderBy(x => x.Ore.Name).ToList();

            var model = new OresPageViewModel
            {
                View = view,
                QueryName = name,
                Ores = ores
            };

            return View(model);
        }
    }
}
