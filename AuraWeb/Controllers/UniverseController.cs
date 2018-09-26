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
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadUrl;
        private readonly UniverseService _UniverseService;
        private readonly SDEService _SDEService;

        public UniverseController(ILogger<UniverseController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            _SDEFileName = _Config["SDEFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _UniverseService = new UniverseService(_Log, _SDEFileName);
            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempFileName, _SDEDownloadUrl);
            this._ESIClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new UniversePageViewModel
            {

            };

            return View(model);
        }

        // TODO: Use SDE
        public async Task<IActionResult> Regions()
        {
            List<Region> regions = new List<Region>();
            var universeRegions = await _ESIClient.Universe.GetRegionsV1Async();
            List<int> regionIds = universeRegions.Model;
            foreach (int regionId in regionIds)
            {
                var universeRegionInfo = await _ESIClient.Universe.GetRegionInfoV1Async(regionId);
                regions.Add(universeRegionInfo.Model);
            }

            var model = new UniverseRegionsPageViewModel
            {
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

        // TODO: Use SDE
        public async Task<IActionResult> Constellations()
        {
            var universeConstellations = await _ESIClient.Universe.GetConstellationsV1Async();
            List<int> constellationIds = universeConstellations.Model;
            List<Constellation> constellations = new List<Constellation>();
            foreach (int constellationId in constellationIds)
            {
                var universeConstellation = await _ESIClient.Universe.GetConstellationV1Async(constellationId);
                constellations.Add(universeConstellation.Model);
            }

            var model = new UniverseConstellationsPageViewModel
            {
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
        public async Task<ActionResult> SetSystemAsWaypoint(UniverseSetDestinationModel setDestination)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.SetAutopilotWaypointV2Async(auth, setDestination.AddToBeginning, setDestination.ClearOtherWaypoints, setDestination.DestinationId);
            return RedirectToAction("SystemInfo", new { id = setDestination.DestinationId });
        }

        public async Task<IActionResult> SystemInfo(int id)
        {
            var solarSystem = _SDEService.GetSolarSystem(id);
            var solarSystemApi = await _ESIClient.Universe.GetSolarSystemInfoV4Async(id);
            var star = await _ESIClient.Universe.GetStarInfoV1Async(solarSystemApi.Model.StarId);
            List<Stargate> stargates = new List<Stargate>();
            foreach (int stargateId in solarSystemApi.Model.Stargates)
            {
                var stargate = await _ESIClient.Universe.GetStargateInfoV1Async(stargateId);
                stargates.Add(stargate.Model);
            }
            // TODO: Convert this to SDE data
            List<Station> stations = new List<Station>();
            foreach (int stationId in solarSystemApi.Model.Stations)
            {
                var station = await _ESIClient.Universe.GetStationInfoV2Async(stationId);
                stations.Add(station.Model);
            }


            UniverseSetDestinationModel setDestination = new UniverseSetDestinationModel()
            {
                DestinationId = id
            };
            var model = new UniverseSystemInfoPageViewModel
            {
                System = solarSystem,
                System_API = solarSystemApi.Model,
                Star = star.Model,
                Stargates = stargates,
                Stations = stations,
                SetDestination = setDestination
            };

            return View(model);
        }
    }
}
