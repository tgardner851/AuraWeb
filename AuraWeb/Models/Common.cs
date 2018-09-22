using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class InsertDTO
    {
        public string SQL { get; set; }
        public object Parameters { get; set; }
    }
}
