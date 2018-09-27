using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models.API;
using eZet.EveLib.EveWhoModule;
using eZet.EveLib.ZKillboardModule;
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
        private readonly string _SDEDownloadUrl;
        private readonly string _SDEBackupFileName;
        private readonly string _SDETempCompressedFileName;
        private readonly string _SDETempFileName;
        private readonly SDEService _SDEService;

        public SearchController(ILogger<SearchController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            
            _SDEFileName = _Config["SDEFileName"];
            _SDEBackupFileName = _Config["SDEBackupFileName"];
            _SDETempCompressedFileName = _Config["SDETempCompressedFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempCompressedFileName, _SDETempFileName, _SDEBackupFileName, _SDEDownloadUrl);

            this._ESIClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new SearchPageViewModel()
            {

            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SearchOpenInfoWindow(int id, string query)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, id);
            return RedirectToAction("Index", new { query = query });
        }

        [HttpPost]
        public async Task<ActionResult> SearchOpenMarketWindow(int id, string query)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenMarketDetailsV1Async(auth, id);
            return RedirectToAction("Index", new { query = query });
        }

        public async Task<IActionResult> SearchResults(string query)
        {
            int count = -1; // -1 at the end implies that the query was not provided
            List<Region_V_Row> regions = new List<Region_V_Row>();
            List<Constellation_V_Row> constellations = new List<Constellation_V_Row>();
            List<SolarSystem_V_Row> solarSystems = new List<SolarSystem_V_Row>();
            List<Station_V_Row> stations = new List<Station_V_Row>();
            List<ItemType_V_Row> itemTypes = new List<ItemType_V_Row>();
            EVEStandard.Models.CharacterInfo character = null;

            if(!String.IsNullOrWhiteSpace(query))
            {
                // Search Universe
                regions = _SDEService.SearchRegions(query);
                constellations = _SDEService.SearchConstellations(query);
                solarSystems = _SDEService.SearchSolarSystems(query);
                stations = _SDEService.SearchStations(query);
                // Search Item Types
                itemTypes = _SDEService.SearchItemTypes(query);

                // Attempt to parse as int to check for specific id searches that are not broad
                int id = 0;
                Int32.TryParse(query, out id);
                if (id > 0)
                {
                    try
                    {
                        var characterApi = await _ESIClient.Character.GetCharacterPublicInfoV4Async(id);
                        character = characterApi.Model;
                    }
                    catch(Exception e)
                    {
                        // Do nothing. Character isn't valid
                    }
                }
                else // For services that do not support id search
                {
                    // TODO: More services
                }

                count = regions.Count() +
                    constellations.Count() +
                    solarSystems.Count() +
                    stations.Count() +
                    itemTypes.Count() +
                    (character != null ? 1 : 0);
            }

            
            var model = new SearchResultsPageViewModel
            {
                Query = query,
                ResultCount = count,
                Regions = regions,
                Constellations = constellations,
                SolarSystems = solarSystems,
                Stations = stations,
                ItemTypes = itemTypes,
                Character = character
            };

            return View(model);
        }
    }
}
