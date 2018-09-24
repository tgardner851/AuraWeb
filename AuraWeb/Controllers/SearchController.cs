using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class SearchController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<SearchController> _Log;
        private readonly EVEStandardAPI _ESIClient;
        private readonly string _SDEFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadUrl;
        private readonly SDEService _SDEService;

        public SearchController(ILogger<SearchController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            _SDEFileName = _Config["SDEFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempFileName, _SDEDownloadUrl);
            this._ESIClient = esiClient;
        }

        public async Task<IActionResult> Index(string query)
        {
            int count = -1; // -1 at the end implies that the query was not provided
            List<Region_V_Row> regions = new List<Region_V_Row>();
            List<Constellation_V_Row> constellations = new List<Constellation_V_Row>();
            List<SolarSystem_V_Row> solarSystems = new List<SolarSystem_V_Row>();
            List<Station_V_Row> stations = new List<Station_V_Row>();
            List<ItemType_V_Row> itemTypes = new List<ItemType_V_Row>();

            if(!String.IsNullOrWhiteSpace(query)) {
              // Search Universe
              regions = _SDEService.SearchRegions(query);
              constellations = _SDEService.SearchConstellations(query);
              solarSystems = _SDEService.SearchSolarSystems(query);
              stations = _SDEService.SearchStations(query);
              // Search Item Types
              itemTypes = _SDEService.SearchItemTypes(query);

              count = regions.Count() + constellations.Count() + solarSystems.Count() + stations.Count() + itemTypes.Count();
            }

            
            var model = new SearchPageViewModel
            {
                ResultCount = count,
                Regions = regions,
                Constellations = constellations,
                SolarSystems = solarSystems,
                Stations = stations,
                ItemTypes = itemTypes
            };

            return View(model);
        }
    }
}
