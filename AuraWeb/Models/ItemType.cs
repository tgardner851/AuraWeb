using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class ItemTypePageViewModel
    {
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
        public string Query { get; set; }
        public List<ItemType_V_Row> Ships { get; set; }
    }
}
