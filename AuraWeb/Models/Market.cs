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
        public string TypeName { get; set; }
        public double? AdjustedPrice { get; set; }
        public double? AveragePrice { get; set; }
    }

    public class MarketBestPricesPageViewModel
    {
        public List<RegionMarketOrder> Orders { get; set; }
    }

    public class RegionMarketTypeIdsRow
    {
        public int RegionId { get; set; }
        public int TypeId { get; set; }
    }

    public class RegionMarketOrdersRow
    {
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
}
