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
            // Search Universe
            var regions = _SDEService.SearchRegions(query);
            var constellations = _SDEService.SearchConstellations(query);
            var solarSystems = _SDEService.SearchSolarSystems(query);
            var stations = _SDEService.SearchStations(query);
            // Search Item Types
            var itemTypes = _SDEService.SearchItemTypes(query);

            var model = new SearchPageViewModel
            {
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
