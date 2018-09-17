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
    [Authorize]
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
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));



            var model = new UniversePageViewModel
            {

            };

            return View(model);
        }

        public async Task<IActionResult> Regions()
        {
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

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
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

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





    }
}
