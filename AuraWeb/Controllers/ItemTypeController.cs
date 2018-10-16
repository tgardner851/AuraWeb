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

        public async Task<IActionResult> ItemTypes(string raceName, string marketGroupName, string groupName, string groupCategoryName, string metaGroupName, string name)
        {
            List<string> races = _DBService.GetItemTypeRaceNames();
            List<string> marketGroups = _DBService.GetItemTypeMarketGroupNames();
            List<string> groups = _DBService.GetItemTypeGroupNames();
            List<string> groupCategories = _DBService.GetItemTypeGroupCategoryNames();
            List<string> metaGroups = _DBService.GetItemTypeMetaGroupNames();
            List<ItemType_V_Row> itemTypes = new List<ItemType_V_Row>();

            if (raceName == null) raceName = "All";
            if (marketGroupName == null) marketGroupName = "All";
            if (groupName == null) groupName = "All";
            if (groupCategoryName == null) groupCategoryName = "All";
            if (metaGroupName == null) metaGroupName = "All";
            string queryRaceName = raceName;
            string queryMarketGroupName = marketGroupName;
            string queryGroupName = groupName;
            string queryGroupCategoryName = groupCategoryName;
            string queryMetaGroupName = metaGroupName;
            string queryName = name;
            if (raceName == "All") queryRaceName = null;
            if (marketGroupName == "All") queryMarketGroupName = null;
            if (groupName == "All") queryGroupName = null;
            if (groupCategoryName == "All") queryGroupCategoryName = null;
            if (metaGroupName == "All") queryMetaGroupName = null;

            itemTypes = _DBService.GetItemTypes(queryRaceName, queryMarketGroupName, queryGroupName, queryGroupCategoryName, queryMetaGroupName, queryName);

            var model = new ItemTypesPageViewModel
            {
                Races = races,
                MarketGroups = marketGroups,
                Groups = groups,
                GroupCategories = groupCategories,
                MetaGroups = metaGroups,
                QueryRaceName = raceName,
                QueryMarketGroupName = marketGroupName,
                QueryGroupName = groupName,
                QueryGroupCategoryName = groupCategoryName,
                QueryMetaGroupName = metaGroupName,
                QueryName = name,
                ItemTypes = itemTypes
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

        public async Task<IActionResult> Modules(string raceName, string marketGroupName, string groupName, string metaGroupName, string powerSlotName, string name)
        {
            List<string> races = _DBService.GetModuleRaceNames();
            List<string> marketGroups = _DBService.GetModuleMarketGroupNames();
            List<string> groups = _DBService.GetModuleGroupNames();
            List<string> metaGroups = _DBService.GetModuleMetaGroupNames();
            List<ItemType_V_Row> modules = new List<ItemType_V_Row>();

            if (raceName == null) raceName = "All";
            if (marketGroupName == null) marketGroupName = "All";
            if (groupName == null) groupName = "All";
            if (metaGroupName == null) metaGroupName = "All";
            if (powerSlotName == null) powerSlotName = "All";
            string queryRaceName = raceName;
            string queryMarketGroupName = marketGroupName;
            string queryGroupName = groupName;
            string queryMetaGroupName = metaGroupName;
            string queryPowerSlotName = powerSlotName;
            string queryName = name;
            if (raceName == "All") queryRaceName = null;
            if (marketGroupName == "All") queryMarketGroupName = null;
            if (groupName == "All") queryGroupName = null;
            if (metaGroupName == "All") queryMetaGroupName = null;
            if (powerSlotName == "All") queryPowerSlotName = null;

            modules = _DBService.GetModules(queryRaceName, queryMarketGroupName, queryGroupName, queryMetaGroupName, queryPowerSlotName, queryName);

            var model = new ModulesPageViewModel
            {
                Races = races,
                MarketGroups = marketGroups,
                Groups = groups,
                MetaGroups = metaGroups,
                QueryRaceName = raceName,
                QueryMarketGroupName = marketGroupName,
                QueryGroupName = groupName,
                QueryMetaGroupName = metaGroupName,
                QueryPowerSlotName = powerSlotName,
                QueryName = name,
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
