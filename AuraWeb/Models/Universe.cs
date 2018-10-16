using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Models
{
    #region View Models
    public class UniversePageViewModel
    {

    }

    public class UniverseRegionsPageViewModel
    {
        public List<string> Factions { get; set; }
        public string QueryFactionName { get; set; }
        public string QueryName { get; set; }
        public List<Region_V_Row> Regions { get; set; }
    }

    public class UniverseRegionInfoPageViewModel
    {
        public Region_V_Row Region { get; set; }
        public Region Region_API { get; set; }
        public List<Constellation_V_Row> Constellations { get; set; }
    }

    public class UniverseConstellationsPageViewModel
    {
        public List<string> Regions { get; set; }
        public List<string> Factions { get; set; }
        public string QueryRegionName { get; set; }
        public string QueryFactionName { get; set; }
        public string QueryName { get; set; }
        public List<Constellation_V_Row> Constellations { get; set; }
    }

    public class UniverseConstellationInfoPageViewModel
    {
        public Constellation_V_Row Constellation { get; set; }
        public List<SolarSystem_V_Row> Systems { get; set; }
    }

    public class UniverseSystemsPageViewModel
    {
        public List<string> Regions { get; set; }
        public List<string> Constellations { get; set; }
        public List<string> Factions { get; set; }
        public List<string> SecurityClasses { get; set; }
        public string QueryRegionName { get; set; }
        public string QueryConstellationName { get; set; }
        public string QueryFactionName { get; set; }
        public string QuerySecurityClass { get; set; }
        public string QueryName { get; set; }
        public List<SolarSystem_V_Row> Systems { get; set; }
    }

    // TODO: Convert this to SDE data
    public class UniverseSystemInfoPageViewModel
    {
        public SolarSystem_V_Row System { get; set; }
        public EVEStandard.Models.System System_API { get; set; }
        public Star Star { get; set; }
        public List<Stargate> Stargates { get; set; }
        public List<Station_V_Row> Stations { get; set; }
        public UniverseSetDestinationModel SetDestination { get; set; }
        public UniverseSystemInfoItemTypeOpenInfoModel OpenInfoModel { get; set; }
    }
    #endregion

    public class UniverseSetDestinationModel
    {
        public long DestinationId { get; set; }
        public bool AddToBeginning { get; set; }
        public bool ClearOtherWaypoints { get; set; }
    }

    public class UniverseSystemInfoItemTypeOpenInfoModel
    {
        public int SystemId { get; set; }
        public long ItemTypeId { get; set; }
    }

    public class UniverseJumpRoutesPageViewModel
    {
        public UniverseJumpRoutesModel Form { get; set; }
        public UniverseSetDestinationModel SetDestination { get; set; }
    }

    public class UniverseJumpRoutesModel 
    {
        public List<SolarSystem_V_Row> Jumps { get; set; }
        public JumpRouteModel From { get; set; }
        public int FromId { get; set; }
        public string FromQuery { get; set; }
        public string FromType { get; set; }
        public List<JumpRouteModel> FromResults { get; set; }
        public JumpRouteModel To { get; set; }
        public int ToId { get; set; }
        public string ToQuery { get; set; }
        public string ToType { get; set; }
        public List<JumpRouteModel> ToResults { get; set; }
    }

    public class JumpRouteModel 
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class UniverseStationsPageViewModel
    {
        public List<string> Regions { get; set; }
        public List<string> Constellations { get; set; }
        public List<string> Systems { get; set; }
        public List<string> OperationNames { get; set; }
        public string QueryRegionName { get; set; }
        public string QueryConstellationName { get; set; }
        public string QuerySystemName { get; set; }
        public string QueryOperationName { get; set; }
        public string QueryName { get; set; }
        public List<Station_V_Row> Stations { get; set; }
    }

    public class UniverseStationInfoPageViewModel
    {
        public Station_V_Row Station { get; set; }
        public UniverseSetDestinationModel SetDestination { get; set; }
        public UniverseSystemInfoItemTypeOpenInfoModel OpenInfoModel { get; set; }
    }
}
