using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models;
using EVEStandard.Models.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class UniverseController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<UniverseController> _Log;
        private readonly EVEStandardAPI _ESIClient;
        private readonly string _SDEFileName;
        private readonly string _SDEDownloadUrl;
        private readonly string _SDEBackupFileName;
        private readonly string _SDETempCompressedFileName;
        private readonly string _SDETempFileName;
        private readonly SDEService _SDEService; 
        private readonly UniverseService _UniverseService;

        public UniverseController(ILogger<UniverseController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            
            _SDEFileName = _Config["SDEFileName"];
            _SDEBackupFileName = _Config["SDEBackupFileName"];
            _SDETempCompressedFileName = _Config["SDETempCompressedFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempCompressedFileName, _SDETempFileName, _SDEBackupFileName, _SDEDownloadUrl);

            _UniverseService = new UniverseService(_Log, _SDEFileName);
            this._ESIClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new UniversePageViewModel
            {

            };

            return View(model);
        }

        public async Task<IActionResult> Regions(string query)
        {
            List<Region_V_Row> regions = new List<Region_V_Row>();

            if (!String.IsNullOrWhiteSpace(query)) {
                regions = _SDEService.GetAllRegions();
            }
            else {
                regions = _SDEService.SearchRegions(query);
            }

            var model = new UniverseRegionsPageViewModel
            {
                Query = query,
                ResultCount = regions.Count,
                Regions = regions
            };

            return View(model);
        }

        public async Task<IActionResult> RegionInfo(int id)
        {
            var region = _SDEService.GetRegion(id);
            var regionApi = await _ESIClient.Universe.GetRegionInfoV1Async(id);
            var constellations = _SDEService.GetConstellationsForRegion(id);
            

            var model = new UniverseRegionInfoPageViewModel
            {
                Region = region,
                Region_API = regionApi.Model,
                Constellations = constellations
            };

            return View(model);
        }

        public async Task<IActionResult> Constellations(string query)
        {
            List<Constellation_V_Row> constellations = new List<Constellation_V_Row>();

            if (!String.IsNullOrWhiteSpace(query)){
                constellations = _SDEService.GetAllConstellations();
            }
            else {
                constellations = _SDEService.SearchConstellations(query);
            }

            var model = new UniverseConstellationsPageViewModel
            {
                Query = query,
                ResultCount = constellations.Count,
                Constellations = constellations
            };

            return View(model);
        }

        public async Task<IActionResult> ConstellationInfo(int id)
        {
            var constellation = _SDEService.GetConstellation(id);
            var systems = _SDEService.GetSolarSystemsForConstellation(id);

            var model = new UniverseConstellationInfoPageViewModel
            {
                Constellation = constellation,
                Systems = systems
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SystemInfoOpenInfoWindowForItemType(UniverseSystemInfoItemTypeOpenInfoModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, model.ItemTypeId);
            return RedirectToAction("SystemInfo", new { id = model.SystemId });
        }

        [HttpPost]
        public async Task<ActionResult> SystemInfoSetSystemAsWaypoint(UniverseSetDestinationModel setDestination)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.SetAutopilotWaypointV2Async(auth, setDestination.AddToBeginning, setDestination.ClearOtherWaypoints, setDestination.DestinationId);
            return RedirectToAction("SystemInfo", new { id = setDestination.DestinationId });
        }

        public async Task<IActionResult> SystemInfo(int id)
        {
            SolarSystem_V_Row solarSystem = _SDEService.GetSolarSystem(id);

            // TODO: Eventually move this to SDE calls
            var solarSystemApi = await _ESIClient.Universe.GetSolarSystemInfoV4Async(id);
            var star = await _ESIClient.Universe.GetStarInfoV1Async(solarSystemApi.Model.StarId);
            List<Stargate> stargates = new List<Stargate>();
            foreach (int stargateId in solarSystemApi.Model.Stargates)
            {
                var stargate = await _ESIClient.Universe.GetStargateInfoV1Async(stargateId);
                stargates.Add(stargate.Model);
            }

            List<Station_V_Row> stations = _SDEService.GetStationsForSolarSystem(id);

            var model = new UniverseSystemInfoPageViewModel
            {
                System = solarSystem,
                System_API = solarSystemApi.Model,
                Star = star.Model,
                Stargates = stargates,
                Stations = stations,
                SetDestination = new UniverseSetDestinationModel(),
                OpenInfoModel = new UniverseSystemInfoItemTypeOpenInfoModel()
            };

            return View(model);
        }
    }
}
