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

        #region Regions
        public async Task<IActionResult> Regions(string factionName, string name)
        {
            List<Region_V_Row> regions = new List<Region_V_Row>();
            List<string> factions = _DBService.GetRegionFactions();

            if (factionName == null) factionName = "All";
            string queryFactionName = factionName;
            if (factionName == "All") queryFactionName = null;

            regions = _DBService.GetRegions(queryFactionName, name);

            var model = new UniverseRegionsPageViewModel
            {
                Factions = factions,
                QueryFactionName = factionName,
                QueryName = name,
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
        public async Task<IActionResult> Constellations(string regionName, string factionName, string name)
        {
            List<string> regions = _DBService.GetAllRegionNames();
            List<string> factions = _DBService.GetConstellationFactionNames();
            List<Constellation_V_Row> constellations = new List<Constellation_V_Row>();

            if (regionName == null) regionName = "All";
            if (factionName == null) factionName = "All";
            string queryRegionName = regionName;
            string queryFactionName = factionName;
            string queryName = name;
            if (regionName == "All") queryRegionName = null;
            if (factionName == "All") queryFactionName = null;

            constellations = _DBService.GetConstellations(queryRegionName, queryFactionName, queryName);

            var model = new UniverseConstellationsPageViewModel
            {
                Regions = regions,
                Factions = factions,
                QueryRegionName = regionName,
                QueryFactionName = factionName,
                QueryName = name,
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
        public async Task<IActionResult> Systems(string regionName, string constellationName, string factionName, string securityClass, string name)
        {
            List<string> regions = _DBService.GetAllRegionNames();
            List<string> constellations = _DBService.GetAllConstellationNames();
            List<string> factions = _DBService.GetSolarSystemFactionNames();
            List<string> securityClasses = _DBService.GetSolarSystemSecurityClasses();
            List<SolarSystem_V_Row> systems = new List<SolarSystem_V_Row>();

            if (regionName == null) regionName = "All";
            if (constellationName == null) constellationName = "All";
            if (factionName == null) factionName = "All";
            if (securityClass == null) securityClass = "All";
            string queryRegionName = regionName;
            string queryConstellationName = constellationName;
            string queryFactionName = factionName;
            string querySecurityClass = securityClass;
            string queryName = name;
            if (regionName == "All") queryRegionName = null;
            if (constellationName == "All") queryConstellationName = null;
            if (factionName == "All") queryFactionName = null;
            if (securityClass == "All") querySecurityClass = null;

            systems = _DBService.GetSolarSystems(queryRegionName, queryConstellationName, queryFactionName, querySecurityClass, queryName);

            var model = new UniverseSystemsPageViewModel
            {
                Regions = regions,
                Constellations = constellations,
                Factions = factions,
                SecurityClasses = securityClasses,
                QueryRegionName = regionName,
                QueryConstellationName = constellationName,
                QueryFactionName = factionName, 
                QuerySecurityClass = securityClass,
                QueryName = name,
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

        public async Task<ActionResult> SetLocationAsWaypoint(int locationId)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.SetAutopilotWaypointV2Async(auth, false, false, locationId);
            return RedirectToAction("SystemInfo", new { id = locationId });
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

        public async Task<IActionResult> Stations(string regionName, string constellationName, string systemName, string operationName, string name)
        {
            List<string> regions = _DBService.GetAllRegionNames();
            List<string> constellations = _DBService.GetAllConstellationNames();
            List<string> systems = _DBService.GetSolarSystemNames();
            List<string> operationNames = _DBService.GetStationOperationNames();
            List<Station_V_Row> stations = new List<Station_V_Row>();

            if (regionName == null) regionName = "All";
            if (constellationName == null) constellationName = "All";
            if (systemName == null) systemName = "All";
            if (operationName == null) operationName = "All";
            string queryRegionName = regionName;
            string queryConstellationName = constellationName;
            string querySystemName = systemName;
            string queryOperationName = operationName;
            string queryName = name;
            if (regionName == "All") queryRegionName = null;
            if (constellationName == "All") queryConstellationName = null;
            if (systemName == "All") querySystemName = null;
            if (operationName == "All") queryOperationName = null;

            stations = _DBService.GetStations(queryRegionName, queryConstellationName, querySystemName, queryOperationName, queryName);

            var model = new UniverseStationsPageViewModel
            {
                Regions = regions,
                Constellations = constellations,
                Systems = systems,
                OperationNames = operationNames,
                QueryRegionName = regionName,
                QueryConstellationName = constellationName,
                QuerySystemName = systemName,
                QueryOperationName = operationName,
                QueryName = name,
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
