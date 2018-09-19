using AuraWeb.Models;
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
        private readonly EVEStandardAPI esiClient;

        public UniverseController(ILogger<UniverseController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this.esiClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new UniversePageViewModel
            {

            };

            return View(model);
        }

        public async Task<IActionResult> Regions()
        {
            List<Region> regions = new List<Region>();
            var universeRegions = await esiClient.Universe.GetRegionsV1Async();
            List<int> regionIds = universeRegions.Model;
            foreach (int regionId in regionIds)
            {
                var universeRegionInfo = await esiClient.Universe.GetRegionInfoV1Async(regionId);
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
            var universeRegionInfo = await esiClient.Universe.GetRegionInfoV1Async(id);
            Region regionInfo = universeRegionInfo.Model;
            List<Constellation> constellations = new List<Constellation>();
            foreach(int constellationId in regionInfo.Constellations)
            {
                var regionConstellation = await esiClient.Universe.GetConstellationV1Async(constellationId);
                constellations.Add(regionConstellation.Model);
            }

            var model = new UniverseRegionInfoPageViewModel
            {
                Region = regionInfo,
                Constellations = constellations
            };

            return View(model);
        }

        public async Task<IActionResult> Constellations()
        {
            var universeConstellations = await esiClient.Universe.GetConstellationsV1Async();
            List<int> constellationIds = universeConstellations.Model;
            List<Constellation> constellations = new List<Constellation>();
            foreach (int constellationId in constellationIds)
            {
                var universeConstellation = await esiClient.Universe.GetConstellationV1Async(constellationId);
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
            var constellation = await esiClient.Universe.GetConstellationV1Async(id);
            List<EVEStandard.Models.System> systems = new List<EVEStandard.Models.System>();
            foreach(int systemId in constellation.Model.Systems)
            {
                var system = await esiClient.Universe.GetSolarSystemInfoV4Async(systemId);
                systems.Add(system.Model);
            }

            var model = new UniverseConstellationInfoPageViewModel
            {
                Constellation = constellation.Model,
                Systems = systems
            };

            return View(model);
        }

        public async Task<IActionResult> SystemInfo(int id)
        {
            var system = await esiClient.Universe.GetSolarSystemInfoV4Async(id);
            var star = await esiClient.Universe.GetStarInfoV1Async(system.Model.StarId);
            List<Stargate> stargates = new List<Stargate>();
            foreach(int stargateId in system.Model.Stargates)
            {
                var stargate = await esiClient.Universe.GetStargateInfoV1Async(stargateId);
                stargates.Add(stargate.Model);
            }
            List<Station> stations = new List<Station>();
            foreach(int stationId in system.Model.Stations)
            {
                var station = await esiClient.Universe.GetStationInfoV2Async(stationId);
                stations.Add(station.Model);
            }
            
            var model = new UniverseSystemInfoPageViewModel
            {
                System = system.Model,
                Star = star.Model,
                Stargates = stargates,
                Stations = stations
            };

            return View(model);
        }
    }
}
