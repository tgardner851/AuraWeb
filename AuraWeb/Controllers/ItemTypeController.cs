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
        private readonly DBService _DBService;

        public ItemTypeController(ILogger<ItemTypeController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this._ESIClient = esiClient;

            string dbFileName = _Config["DBFileName"];
            string sdeFileName = _Config["SDEFileName"];
            string sdeTempCompressedFileName = _Config["SDETempCompressedFileName"];
            string sdeTempFileName = _Config["SDETempFileName"];
            string sdeDownloadUrl = _Config["SDEDownloadURL"];
            _DBService = new DBService(_Log, dbFileName, sdeFileName, sdeTempCompressedFileName, sdeTempFileName, sdeDownloadUrl);
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
                itemTypes = _DBService.GetAllItemTypes();
            }
            else
            {
                itemTypes = _DBService.SearchItemTypes(query);
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
            ItemType_V_Row itemType = _DBService.GetItemType(id);

            // TODO: SDE views don't handle this right now
            var itemTypeApi = await _ESIClient.Universe.GetTypeInfoV3Async(id);
            EVEStandard.Models.Type itemTypeApiModel = itemTypeApi.Model;

            MarketAveragePrices_Row averagePrice = _DBService.GetAveragePriceForTypeId(id);

            #region Best Sell/Buy Prices
            List<RegionMarketOrdersModel> bestSellPrices = new List<RegionMarketOrdersModel>();
            List<RegionMarketOrdersRow> bestSellPricesResult = _DBService.GetBestSellPricesForTypeId(id);
            List<RegionMarketOrdersModel> bestBuyPrices = new List<RegionMarketOrdersModel>();
            List<RegionMarketOrdersRow> bestBuyPricesResult = _DBService.GetBestBuyPricesForTypeId(id);
            List<int> systemIds = bestSellPricesResult.Select(x => x.SystemId).ToList();
            systemIds.AddRange(bestBuyPricesResult.Select(x => x.SystemId));
            List<SolarSystem_V_Row> systems = _DBService.GetSolarSystems(systemIds);
            
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

        public async Task<IActionResult> Ships(string name, string race, string group)
        {
            List<string> shipRaces = _DBService.GetAllShipRaces();
            List<string> shipGroups = _DBService.GetAllShipGroups();

            string queryRace = race;
            string queryGroup = group;
            string queryName = name;
            if (race == "All") queryRace = null;
            if (group == "All") queryGroup = null;
            if (String.IsNullOrEmpty(name)) queryName = null;

            List<ItemType_V_Row> ships = _DBService.GetAllShipsForGroupRaceAndName(queryName, queryRace, queryGroup);

            var model = new ShipsPageViewModel
            {
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
                modules = _DBService.SearchModules(query);
            }
            else // Return all modules
            {
                modules = _DBService.GetAllModules();
            }

            var model = new ModulesPageViewModel
            {
                Query = query,
                Modules = modules
            };

            return View(model);
        }

        public async Task<IActionResult> Ores(string view, string marketGroup, string group, string name)
        {
            List<string> marketGroups = _DBService.GetOreMarketGroups();
            List<string> groups = _DBService.GetOreGroups();

            string queryMarketGroup = marketGroup;
            string queryGroup = group;
            string queryName = name;
            if (marketGroup == "All") queryMarketGroup = null;
            if (group == "All") queryGroup = null;
            if (String.IsNullOrEmpty(name)) queryName = null;

            List<Ore_V_Row> ores = _DBService.GetOres(queryMarketGroup, queryGroup, queryName);

            if (view == null) view = "Table";

            ores = ores.OrderBy(x => x.Name).ToList();

            var model = new OresPageViewModel
            {
                MarketGroups = marketGroups,
                Groups = groups,
                View = view,
                QueryMarketGroup = queryMarketGroup,
                QueryGroup = queryGroup,
                QueryName = name,
                Ores = ores
            };

            return View(model);
        }
    }
}
