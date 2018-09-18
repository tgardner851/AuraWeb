using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    public class UniversePageViewModel
    {

    }

    public class UniverseRegionsPageViewModel
    {
        public List<Region> Regions { get; set; }
    }

    public class UniverseRegionInfoPageViewModel
    {
        public Region Region { get; set; }
        public List<Constellation> Constellations { get; set; }
    }

    public class UniverseConstellationsPageViewModel
    {
        public List<Constellation> Constellations { get; set; }
    }

    public class UniverseConstellationInfoPageViewModel
    {
        public Constellation Constellation { get; set; }
        public List<EVEStandard.Models.System> Systems { get; set; }
    }
}
