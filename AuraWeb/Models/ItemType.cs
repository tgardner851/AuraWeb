using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class ItemTypePageViewModel
    {
    }

    public class ItemTypesPageViewModel
    {
        public string Query { get; set; }
        public List<ItemType_V_Row> ItemTypes { get; set; }
    }

    public class ItemTypeInfoPageViewModel
    {
        public long ItemTypeId { get; set; }
        public ItemType_V_Row ItemType { get; set; }
        public EVEStandard.Models.Type ItemType_API { get; set; }
        public MarketAveragePrices_Row AveragePrice { get; set; }
        public List<RegionMarketOrdersModel> BestSellPrices { get; set; }
        public List<RegionMarketOrdersModel> BestBuyPrices { get; set; }

        public ItemTypeInfoOpenMarketModel OpenMarketModel { get; set; }
        public ItemTypeInfoOpenInfoModel OpenInfoModel { get; set; }
    }

    public class ItemTypeInfoOpenMarketModel
    {
        public long ItemTypeId { get; set; }
    }

    public class ItemTypeInfoOpenInfoModel
    {
        public long ItemTypeId { get; set; }
    }

    public class ShipsPageViewModel
    {
        public string QueryName { get; set; }
        public string QueryRace { get; set; }
        public string QueryGroup { get; set; }
        public List<string> ShipRaces { get; set; }
        public List<string> ShipGroups { get; set; }
        public List<ItemType_V_Row> Ships { get; set; }
    }

    public class ModulesPageViewModel
    {
        public string Query { get; set; }
        public List<ItemType_V_Row> Modules { get; set; }
    }

    public class OresPageViewModel
    {
        public List<string> MarketGroups { get; set; }
        public List<string> Groups { get; set; }
        public string View { get; set; }
        public string QueryMarketGroup { get; set; }
        public string QueryGroup { get; set; }
        public string QueryName { get; set; }
        public List<Ore_V_Row> Ores { get; set; }
    }
}
