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

        [HttpPost]
        public async Task<IActionResult> JumpRouteSearchForm(string fromQuery, int fromId, string toQuery, int toId)
        {
            List<JumpRouteModel> fromOpts = new List<JumpRouteModel>();
            JumpRouteModel from = null;
            List<JumpRouteModel> toOpts = new List<JumpRouteModel>();
            JumpRouteModel to = null;
            if (fromId >= 0) {
                from = new JumpRouteModel();
                SolarSystem_V_Row system = _SDEService.GetSolarSystem(fromId);
                Station_V_Row station = null;
                if (system == null) {
                    station = _SDEService.GetStation(fromId);
                    from.Id = station.Id;
                    from.Type = "Station";
                    from.Name = station.Name;
                }
                else {
                    from.Id = system.Id;
                    from.Type = "System";
                    from.Name = system.Name;
                }
            }
            else {
                if (!String.IsNullOrWhiteSpace(fromQuery)) {
                    var systems = _SDEService.SearchSolarSystems(fromQuery);
                    foreach(var s in systems) {
                      fromOpts.Add(new JumpRouteModel(){
                          Id = s.Id,
                          Type = "System",
                          Name = s.Name
                      });
                    }
                    var stations = _SDEService.SearchStations(fromQuery);
                    foreach(var s in stations) {
                      fromOpts.Add(new JumpRouteModel(){
                          Id = s.Id,
                          Type = "Station",
                          Name = s.Name
                      });
                    }
                }
            }
            if (toId >= 0) {
                to = new JumpRouteModel();
                SolarSystem_V_Row system = _SDEService.GetSolarSystem(toId);
                Station_V_Row station = null;
                if (system == null) {
                    station = _SDEService.GetStation(toId);
                    to.Id = station.Id;
                    to.Type = "Station";
                    to.Name = station.Name;
                }
                else {
                    to.Id = system.Id;
                    to.Type = "System";
                    to.Name = system.Name;
                }
            }
            else {
                if (!String.IsNullOrWhiteSpace(toQuery)) {
                    var systems = _SDEService.SearchSolarSystems(toQuery);
                    foreach(var s in systems) {
                      toOpts.Add(new JumpRouteModel(){
                          Id = s.Id,
                          Type = "System",
                          Name = s.Name
                      });
                    }
                    var stations = _SDEService.SearchStations(toQuery);
                    foreach(var s in stations) {
                      toOpts.Add(new JumpRouteModel(){
                          Id = s.Id,
                          Type = "Station",
                          Name = s.Name
                      });
                    }
                }
            }
            bool calculate = (from != null && to != null);
            List<int> jumps = new List<int>();
            List<Stargate> stargateJumps = new List<Stargate>();
            if (calculate) {
                var jumpsApi = await _ESIClient.Routes.GetRouteV1Async(from.Id, to.Id);
                jumps = jumpsApi.Model;
                foreach(int j in jumps) {
                    var stargateApi = await _ESIClient.Universe.GetStargateInfoV1Async(j);
                    Stargate stargate = stargateApi.Model;
                    stargateJumps.Add(stargate);
                }
            }

            UniverseJumpRoutesModel model = new UniverseJumpRoutesModel()
            {
                Jumps = stargateJumps,
                From = from,
                FromQuery = fromQuery,
                FromResults = fromOpts,
                To = to,
                ToQuery = toQuery,
                ToResults = toOpts
            };

            return RedirectToAction("JumpRoutes", new { query = model });
        }

        public async Task<IActionResult> JumpRoutes(UniverseJumpRoutesModel query)
        {
            if (query == null) query = new UniverseJumpRoutesModel();
            if (query.Calculate) 
            {

            }
            var model = new UniverseJumpRoutesPageViewModel 
            {
                Form = query
            };
            return View(model);
        }
    }
}
