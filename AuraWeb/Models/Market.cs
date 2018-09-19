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
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public double? AdjustedPrice { get; set; }
        [DisplayFormat(DataFormatString = "{0:N3}")]
        public double? AveragePrice { get; set; }
    }
}
