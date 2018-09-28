using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models;
using EVEStandard.Models.API;
using eZet.EveLib.EveWhoModule;
using eZet.EveLib.ZKillboardModule;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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

        public async Task<ActionResult> SearchOpenInfoWindow(int id, string query)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, id);
            return RedirectToAction("SearchResults", new { query = query });
        }

        public async Task<ActionResult> SearchOpenMarketWindow(int id, string query)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenMarketDetailsV1Async(auth, id);
            return RedirectToAction("SearchResults", new { query = query });
        }

        public async Task<IActionResult> SearchResults(string query)
        {
            // Redirect to Jump Routes if the query syntax matches
            if (!String.IsNullOrWhiteSpace(query))
            {
                Regex rgx = new Regex(@"(.*) > (.*)");
                List<Match> rgxMatches = rgx.Matches(query).ToList();
                if (rgxMatches != null && rgxMatches.Count > 0 && rgxMatches[0].Groups.Count == 3) {
                    string fromQuery = rgxMatches[0].Groups[1].Value;
                    string toQuery = rgxMatches[0].Groups[2].Value;
                    return RedirectToAction("JumpRoutes", "Universe", new { fromQuery = fromQuery, toQuery = toQuery });
                }
            }

            int count = -1; // -1 at the end implies that the query was not provided
            List<Region_V_Row> regions = new List<Region_V_Row>();
            List<Constellation_V_Row> constellations = new List<Constellation_V_Row>();
            List<SolarSystem_V_Row> solarSystems = new List<SolarSystem_V_Row>();
            List<Station_V_Row> stations = new List<Station_V_Row>();
            List<ItemType_V_Row> itemTypes = new List<ItemType_V_Row>();
            List<CharacterDataModel> characters = new List<CharacterDataModel>();
            
            if(!String.IsNullOrWhiteSpace(query))
            {
                // Attempt to parse as int to check for specific id searches that are not broad
                int id = 0;
                Int32.TryParse(query, out id);
                if (id > 0)
                {
                    try
                    {
                        var characterApi = await _ESIClient.Character.GetCharacterPublicInfoV4Async(id);
                        CharacterInfo characterApiModel = characterApi.Model;
                        characters.Add(new CharacterDataModel()
                        {
                            Id = id,
                            Character = characterApiModel
                        });
                    }
                    catch(Exception e)
                    {
                        // Do nothing. Character isn't valid
                    }

                    regions = new List<Region_V_Row>() { _SDEService.GetRegion(id) };
                    constellations = new List<Constellation_V_Row>() { _SDEService.GetConstellation(id) };
                    solarSystems = new List<SolarSystem_V_Row>() { _SDEService.GetSolarSystem(id) };
                    stations = new List<Station_V_Row>() { _SDEService.GetStation(id) };
                }
                else // For services that do not support id search
                {
                    // Search Universe
                    regions = _SDEService.SearchRegions(query);
                    constellations = _SDEService.SearchConstellations(query);
                    solarSystems = _SDEService.SearchSolarSystems(query);
                    stations = _SDEService.SearchStations(query);
                    // Search Item Types
                    itemTypes = _SDEService.SearchItemTypes(query);
                    // Search API
                    try
                    {
                        AuthDTO auth = GetAuth(_ESIClient);
                        _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

                        // TODO: Support all available search categories (agent, alliance, character, constellation, corporation, faction, inventory_type, region, solar_system, station, structure)
                        //https://esi.evetech.net/ui#/Search/get_characters_character_id_search

                        var searchApi = await _ESIClient.Search.SearchCharacterV3Async(auth, new List<string>() { "character" }, query);
                        var searchApiModel = searchApi.Model;

                        // Process Characters
                        List<int> characterIds = searchApiModel.Character;
                        foreach(int characterId in characterIds)
                        {
                            var characterIdSearch = await _ESIClient.Character.GetCharacterPublicInfoV4Async(characterId);
                            CharacterInfo characterFromSearch = characterIdSearch.Model;
                            characters.Add(new CharacterDataModel()
                            {
                                Id = characterId,
                                Character = characterFromSearch
                            });
                        }
                    }
                    catch(Exception e)
                    {
                        // Not logged in, won't bother searching
                    }
                }

                count = regions.Count() +
                    constellations.Count() +
                    solarSystems.Count() +
                    stations.Count() +
                    itemTypes.Count() +
                    characters.Count();
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
                Characters = characters
            };

            return View(model);
        }
    }
}
