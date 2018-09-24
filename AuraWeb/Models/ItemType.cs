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
        public ItemType_V_Row ItemType { get; set; }
        public EVEStandard.Models.Type ItemType_API { get; set; }
    }
}
