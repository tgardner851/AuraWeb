using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class MarketPageViewModel
    {
        public List<MarketModel> Prices { get; set; }
    }

    public class MarketModel
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public double? AdjustedPrice { get; set; }
        public double? AveragePrice { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MarketBestPricesPageViewModel
    {
        public List<RegionMarketOrder> Orders { get; set; }
    }

    public class MarketOpportunitiesPageViewModel
    {
        public List<string> MarketGroups { get; set; }
        public List<string> Groups { get; set; }
        public List<string> GroupCategories { get; set; }
        public string View { get; set; }
        public int QueryThreshold { get; set; }
        public string QueryMarketGroupName { get; set; }
        public string QueryGroupName { get; set; }
        public string QueryGroupCategoryName { get; set; }
        public List<MarketOpportunitiesDetail_Row> Opportunities { get; set; }
    }

    // TODO: Rename to match SDE naming convention
    public class RegionMarketTypeIdsRow
    {
        public int RegionId { get; set; }
        public int TypeId { get; set; }
    }

    // TODO: Rename to match SDE naming convention
    public class RegionMarketOrdersRow
    {
        public string Id { get; set; }
        public int RegionId { get; set; }
        public int OrderId { get; set; }
        public int TypeId { get; set; }
        public int SystemId { get; set; }
        public int LocationId { get; set; }
        public string Range { get; set; }
        public int IsBuyOrder { get; set; }
        public int Duration { get; set; }
        public DateTime Issued { get; set; }
        public int MinVolume { get; set; }
        public int VolumeRemain { get; set; }
        public int VolumeTotal { get; set; }
        public double Price { get; set; }
    }

    public class RegionMarketOrdersModel
    {
        public int SystemId { get; set; }
        public string SystemName { get; set; }
        public string Range { get; set; }
        public double Price { get; set; }
    }

    // TODO: Rename to match SDE naming convention
    public class RegionMarketOrder
    {
        public RegionMarketOrder(RegionMarketOrdersRow row, string typeName)
        {
            RegionId = row.RegionId;
            OrderId = row.OrderId;
            TypeId = row.TypeId;
            TypeName = typeName;
            SystemId = row.SystemId;
            LocationId = row.LocationId;
            Range = row.Range;
            IsBuyOrder = row.IsBuyOrder;
            Duration = row.Duration;
            Issued = row.Issued;
            MinVolume = row.MinVolume;
            VolumeRemain = row.VolumeRemain;
            VolumeTotal = row.VolumeTotal;
            Price = row.Price;
        }

        public int RegionId { get; set; }
        public int OrderId { get; set; }
        public int TypeId { get; set; }

        public string TypeName { get; set; }

        public int SystemId { get; set; }
        public int LocationId { get; set; }
        public string Range { get; set; }
        public int IsBuyOrder { get; set; }
        public int Duration { get; set; }
        public DateTime Issued { get; set; }
        public int MinVolume { get; set; }
        public int VolumeRemain { get; set; }
        public int VolumeTotal { get; set; }
        public double Price { get; set; }
    }

    public class MarketAveragePrices_Row
    {
        public DateTime Timestamp { get; set; }
        public int TypeId{ get; set; }
        public double? AdjustedPrice { get; set; }
        public double? AveragePrice { get; set; }
    }

    public class MarketOpportunitiesDetail_Row
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string MarketGroupName { get; set; }
        public string GroupName { get; set; }
        public string GroupCategoryName { get; set; }
        public string BuyId { get; set; }
        public int BuyRegionId { get; set; }
        public string BuyRegionName { get; set; }
        public int BuyOrderId { get; set; }
        public int BuySystemId { get; set; }
        public string BuySystemName { get; set; }
        public int BuyLocationId { get; set; }
        public string BuyStationName { get; set; }
        public string BuyRange { get; set; }
        public int BuyDuration { get; set; }
        public DateTime BuyIssued { get; set; }
        public int BuyMinVolume { get; set; }
        public int BuyVolumeRemain { get; set; }
        public int BuyVolumeTotal { get; set; }
        public double BuyPrice { get; set; }
        public string SellId { get; set; }
        public int SellRegionId { get; set; }
        public string SellRegionName { get; set; }
        public int SellOrderId { get; set; }
        public int SellSystemId { get; set; }
        public string SellSystemName { get; set; }
        public int SellLocationId { get; set; }
        public string SellStationName { get; set; }
        public string SellRange { get; set; }
        public int SellDuration { get; set; }
        public DateTime SellIssued { get; set; }
        public int SellMinVolume { get; set; }
        public int SellVolumeRemain { get; set; }
        public int SellVolumeTotal { get; set; }
        public double SellPrice { get; set; }
        public double PriceDiff { get; set; }
    }
}
