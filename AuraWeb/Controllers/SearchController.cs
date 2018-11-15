using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models;
using EVEStandard.Models.API;
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
        private readonly DBService _DBService;

        public SearchController(ILogger<SearchController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this._ESIClient = esiClient;

            string dbFileName = _Config["DBFileName"];
            string sdeFileName = _Config["SDEFileName"];
            string sdeTempCompressedFileName = _Config["SDETempCompressedFileName"];
            string sdeTempFileName = _Config["SDETempFileName"];
            string sdeDownloadUrl = _Config["SDEDownloadURL"];
            _DBService = new DBService(_Log, dbFileName, sdeFileName, sdeTempCompressedFileName, sdeTempFileName, sdeDownloadUrl);
        }

        public async Task<IActionResult> Index(string queryScope, string query)
        {
            switch(queryScope)
            {
                case "ItemTypes":
                    return RedirectToAction("ItemTypes", "ItemType", new { name = query });
                case "Ores":
                    return RedirectToAction("Ores", "ItemType", new { name = query });
                case "Modules":
                    return RedirectToAction("Modules", "ItemType", new { name = query });
                case "Ships":
                    return RedirectToAction("Ships", "ItemType", new { name = query });
                case "Regions":
                    return RedirectToAction("Regions", "Universe", new { name = query });
                case "Constellations":
                    return RedirectToAction("Constellations", "Universe", new { name = query });
                case "Systems":
                    return RedirectToAction("Systems", "Universe", new { name = query });
                case "Stations":
                    return RedirectToAction("Stations", "Universe", new { name = query });
                case "Blueprints":
                    return RedirectToAction("ItemTypes", "ItemType", new { name = query, groupCategoryName = "Blueprint" });
                case "Charges":
                    return RedirectToAction("ItemTypes", "ItemType", new { name = query, groupCategoryName = "Charge" });
                default:
                    return RedirectToAction("Index", "Home");
            }
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
                        Character_Row character = _DBService.GetCharacterPublicInfo(id); //var characterApi = await _ESIClient.Character.GetCharacterPublicInfoV4Async(id);
                        characters.Add(new CharacterDataModel()
                        {
                            Id = id,
                            Character = character
                        });
                    }
                    catch(Exception e)
                    {
                        // Do nothing. Character isn't valid
                    }

                    regions = new List<Region_V_Row>() { _DBService.GetRegion(id) };
                    constellations = new List<Constellation_V_Row>() { _DBService.GetConstellation(id) };
                    solarSystems = new List<SolarSystem_V_Row>() { _DBService.GetSolarSystem(id) };
                    stations = new List<Station_V_Row>() { _DBService.GetStation(id) };
                }
                else // For services that do not support id search
                {
                    // Search Universe
                    regions = _DBService.SearchRegions(query);
                    constellations = _DBService.SearchConstellations(query);
                    solarSystems = _DBService.SearchSolarSystems(query);
                    stations = _DBService.SearchStations(query);
                    // Search Item Types
                    itemTypes = _DBService.SearchItemTypes(query);
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
                        for(int x = 0; x < characterIds.Count; x++)
                        {
                            if (x == 5) break; // Only do the first 5
                            int characterId = characterIds[x];
                            Character_Row characterFromSearch = _DBService.GetCharacterPublicInfo(characterId); //var characterIdSearch = await _ESIClient.Character.GetCharacterPublicInfoV4Async(characterId);
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
