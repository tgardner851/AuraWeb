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

            if (String.IsNullOrWhiteSpace(query)) {
                regions = _SDEService.GetAllRegions();
            }
            else {
                regions = _SDEService.SearchRegions(query);
            }

            var model = new UniverseRegionsPageViewModel
            {
                Query = query,
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

            if (String.IsNullOrWhiteSpace(query)){
                constellations = _SDEService.GetAllConstellations();
            }
            else {
                constellations = _SDEService.SearchConstellations(query);
            }

            var model = new UniverseConstellationsPageViewModel
            {
                Query = query,
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

        public async Task<IActionResult> Systems(string query)
        {
            List<SolarSystem_V_Row> systems = new List<SolarSystem_V_Row>();

            if (String.IsNullOrWhiteSpace(query))
            {
                systems = _SDEService.GetAllSolarSystems();
            }
            else
            {
                systems = _SDEService.SearchSolarSystems(query);
            }

            var model = new UniverseSystemsPageViewModel
            {
                Query = query,
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

        public async Task<IActionResult> JumpRoutes(string fromQuery, int fromId, string fromType, string toQuery, int toId, string toType)
        {
            List<JumpRouteModel> fromOpts = new List<JumpRouteModel>();
            JumpRouteModel from = null;
            List<JumpRouteModel> toOpts = new List<JumpRouteModel>();
            JumpRouteModel to = null;
            if (fromId > 0) // If id was provided, search for it
            {
                if (fromType == "System")
                {
                    SolarSystem_V_Row system = _SDEService.GetSolarSystem(fromId);
                    if (system != null)
                    {
                        from = new JumpRouteModel();
                        from.Id = system.Id;
                        from.Type = "System";
                        from.Name = system.Name;
                    }
                }
                else if (fromType == "Station")
                {
                    Station_V_Row station = _SDEService.GetStation(fromId);
                    if (station != null)
                    {
                        from = new JumpRouteModel();
                        // Need to find the system in which the station resides
                        SolarSystem_V_Row systemForStation = _SDEService.GetSolarSystem(station.SolarSystemId);
                        from.Id = systemForStation.Id;
                        from.Type = "Station";
                        from.Name = systemForStation.Name;
                    }
                }
            }
            else // Id not provided, so search for entries via query
            {
                if (!String.IsNullOrWhiteSpace(fromQuery))
                {
                    var systems = _SDEService.SearchSolarSystems(fromQuery);
                    foreach (var s in systems)
                    {
                        fromOpts.Add(new JumpRouteModel()
                        {
                            Id = s.Id,
                            Type = "System",
                            Name = s.Name
                        });
                    }
                    var stations = _SDEService.SearchStations(fromQuery);
                    foreach (var s in stations)
                    {
                        fromOpts.Add(new JumpRouteModel()
                        {
                            Id = s.Id,
                            Type = "Station",
                            Name = s.Name
                        });
                    }
                }
            }
            if (toId > 0) // If id was provided, search for it
            {
                if (toType == "System")
                {
                    SolarSystem_V_Row system = _SDEService.GetSolarSystem(toId);
                    if (system != null)
                    {
                        to = new JumpRouteModel();
                        to.Id = system.Id;
                        to.Type = "System";
                        to.Name = system.Name;
                    }
                }
                else if (toType == "Station")
                {
                    Station_V_Row station = _SDEService.GetStation(toId);
                    if (station != null)
                    {
                        to = new JumpRouteModel();
                        // Need to find the system in which the station resides
                        SolarSystem_V_Row systemForStation = _SDEService.GetSolarSystem(station.SolarSystemId);
                        to.Id = systemForStation.Id;
                        to.Type = "Station";
                        to.Name = systemForStation.Name;
                    }
                }
            }
            else // Id not provided, so search for entries via query
            {
                if (!String.IsNullOrWhiteSpace(toQuery))
                {
                    var systems = _SDEService.SearchSolarSystems(toQuery);
                    foreach (var s in systems)
                    {
                        toOpts.Add(new JumpRouteModel()
                        {
                            Id = s.Id,
                            Type = "System",
                            Name = s.Name
                        });
                    }
                    var stations = _SDEService.SearchStations(toQuery);
                    foreach (var s in stations)
                    {
                        toOpts.Add(new JumpRouteModel()
                        {
                            Id = s.Id,
                            Type = "Station",
                            Name = s.Name
                        });
                    }
                }
            }
            bool calculate = (from != null && to != null);
            List<int> jumps = new List<int>();
            List<SolarSystem_V_Row> systemJumps = new List<SolarSystem_V_Row>();
            if (calculate)
            {
                var jumpsApi = await _ESIClient.Routes.GetRouteV1Async(from.Id, to.Id);
                jumps = jumpsApi.Model;
                foreach (int j in jumps)
                {
                    SolarSystem_V_Row system = _SDEService.GetSolarSystem(j);
                    systemJumps.Add(system);
                }
            }

            UniverseJumpRoutesModel dataModel = new UniverseJumpRoutesModel()
            {
                Jumps = systemJumps,
                From = from,
                FromId = fromId,
                FromQuery = fromQuery,
                FromType = fromType,
                FromResults = fromOpts,
                To = to,
                ToId = toId,
                ToQuery = toQuery,
                ToType = toType,
                ToResults = toOpts
            };

            var model = new UniverseJumpRoutesPageViewModel 
            {
                Form = dataModel
            };
            return View(model);
        }
    }
}
