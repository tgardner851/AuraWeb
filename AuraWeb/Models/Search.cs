using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class SearchPageViewModel
    {

    }
    
    public class SearchResultsPageViewModel
    {
        public string Query { get; set; }
        public int ResultCount { get; set; }
        public List<Region_V_Row> Regions { get; set; }
        public List<Constellation_V_Row> Constellations { get; set; }
        public List<SolarSystem_V_Row> SolarSystems { get; set; }
        public List<Station_V_Row> Stations { get; set; }
        public List<ItemType_V_Row> ItemTypes { get; set; }
        public List<CharacterDataModel> Characters { get; set; }
    }
}
