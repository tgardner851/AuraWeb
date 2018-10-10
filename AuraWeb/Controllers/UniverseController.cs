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
        private readonly DBService _DBService; 

        public UniverseController(ILogger<UniverseController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
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

        public async Task<IActionResult> Index()
        {
            var model = new UniversePageViewModel
            {

            };

            return View(model);
        }

        #region Regions
        public async Task<IActionResult> Regions(string query)
        {
            List<Region_V_Row> regions = new List<Region_V_Row>();

            if (String.IsNullOrWhiteSpace(query)) {
                regions = _DBService.GetAllRegions();
            }
            else {
                regions = _DBService.SearchRegions(query);
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
            var region = _DBService.GetRegion(id);
            var regionApi = await _ESIClient.Universe.GetRegionInfoV1Async(id);
            var constellations = _DBService.GetConstellationsForRegion(id);
            

            var model = new UniverseRegionInfoPageViewModel
            {
                Region = region,
                Region_API = regionApi.Model,
                Constellations = constellations
            };

            return View(model);
        }
        #endregion

        #region Constellations
        public async Task<IActionResult> Constellations(string query)
        {
            List<Constellation_V_Row> constellations = new List<Constellation_V_Row>();

            if (String.IsNullOrWhiteSpace(query)){
                constellations = _DBService.GetAllConstellations();
            }
            else {
                constellations = _DBService.SearchConstellations(query);
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
            var constellation = _DBService.GetConstellation(id);
            var systems = _DBService.GetSolarSystemsForConstellation(id);

            var model = new UniverseConstellationInfoPageViewModel
            {
                Constellation = constellation,
                Systems = systems
            };

            return View(model);
        }
        #endregion

        #region Systems
        public async Task<IActionResult> Systems(string query)
        {
            List<SolarSystem_V_Row> systems = new List<SolarSystem_V_Row>();

            if (String.IsNullOrWhiteSpace(query))
            {
                systems = _DBService.GetAllSolarSystems();
            }
            else
            {
                systems = _DBService.SearchSolarSystems(query);
            }

            var model = new UniverseSystemsPageViewModel
            {
                Query = query,
                Systems = systems
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SystemInfoOpenInfoWindowForItemType(UniverseSystemInfoPageViewModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, model.OpenInfoModel.ItemTypeId);
            return RedirectToAction("SystemInfo", new { id = model.OpenInfoModel.SystemId });
        }

        [HttpPost]
        public async Task<ActionResult> SystemInfoSetSystemAsWaypoint(UniverseSystemInfoPageViewModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.SetAutopilotWaypointV2Async(auth, model.SetDestination.AddToBeginning, model.SetDestination.ClearOtherWaypoints, model.SetDestination.DestinationId);
            return RedirectToAction("SystemInfo", new { id = model.SetDestination.DestinationId });
        }

        public async Task<IActionResult> SystemInfo(int id)
        {
            SolarSystem_V_Row solarSystem = _DBService.GetSolarSystem(id);

            // TODO: Eventually move this to SDE calls
            var solarSystemApi = await _ESIClient.Universe.GetSolarSystemInfoV4Async(id);
            var star = await _ESIClient.Universe.GetStarInfoV1Async(solarSystemApi.Model.StarId);
            List<Stargate> stargates = new List<Stargate>();
            foreach (int stargateId in solarSystemApi.Model.Stargates)
            {
                var stargate = await _ESIClient.Universe.GetStargateInfoV1Async(stargateId);
                stargates.Add(stargate.Model);
            }

            List<Station_V_Row> stations = _DBService.GetStationsForSolarSystem(id);

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
        #endregion

        #region Stations
        [HttpPost]
        public async Task<ActionResult> StationInfoOpenInfoWindowForItemType(UniverseStationInfoPageViewModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, model.OpenInfoModel.ItemTypeId);
            return RedirectToAction("StationInfo", new { id = model.OpenInfoModel.SystemId });
        }

        [HttpPost]
        public async Task<ActionResult> StationInfoSetSystemAsWaypoint(UniverseStationInfoPageViewModel model)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.SetAutopilotWaypointV2Async(auth, model.SetDestination.AddToBeginning, model.SetDestination.ClearOtherWaypoints, model.SetDestination.DestinationId);
            return RedirectToAction("StationInfo", new { id = model.SetDestination.DestinationId });
        }

        public async Task<IActionResult> Stations(string query)
        {
            List<Station_V_Row> stations = new List<Station_V_Row>();

            if (String.IsNullOrWhiteSpace(query))
            {
                stations = _DBService.GetAllStations();
            }
            else
            {
                stations = _DBService.SearchStations(query);
            }

            var model = new UniverseStationsPageViewModel
            {
                Query = query,
                Stations = stations
            };

            return View(model);
        }

        public async Task<IActionResult> StationInfo(int id)
        {
            Station_V_Row station = _DBService.GetStation(id);

            var model = new UniverseStationInfoPageViewModel
            {
                Station = station,
                SetDestination = new UniverseSetDestinationModel(),
                OpenInfoModel = new UniverseSystemInfoItemTypeOpenInfoModel()
            };

            return View(model);
        }
        #endregion

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
                    SolarSystem_V_Row system = _DBService.GetSolarSystem(fromId);
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
                    Station_V_Row station = _DBService.GetStation(fromId);
                    if (station != null)
                    {
                        from = new JumpRouteModel();
                        // Need to find the system in which the station resides
                        SolarSystem_V_Row systemForStation = _DBService.GetSolarSystem(station.SolarSystemId);
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
                    var systems = _DBService.SearchSolarSystems(fromQuery);
                    foreach (var s in systems)
                    {
                        fromOpts.Add(new JumpRouteModel()
                        {
                            Id = s.Id,
                            Type = "System",
                            Name = s.Name
                        });
                    }
                    var stations = _DBService.SearchStations(fromQuery);
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
                    SolarSystem_V_Row system = _DBService.GetSolarSystem(toId);
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
                    Station_V_Row station = _DBService.GetStation(toId);
                    if (station != null)
                    {
                        to = new JumpRouteModel();
                        // Need to find the system in which the station resides
                        SolarSystem_V_Row systemForStation = _DBService.GetSolarSystem(station.SolarSystemId);
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
                    var systems = _DBService.SearchSolarSystems(toQuery);
                    foreach (var s in systems)
                    {
                        toOpts.Add(new JumpRouteModel()
                        {
                            Id = s.Id,
                            Type = "System",
                            Name = s.Name
                        });
                    }
                    var stations = _DBService.SearchStations(toQuery);
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
                    SolarSystem_V_Row system = _DBService.GetSolarSystem(j);
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
