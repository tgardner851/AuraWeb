using AuraWeb.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Services
{
    public class UniverseService
    {
        private readonly ILogger _Log;
        private readonly string _SDEPath;
        private readonly SQLiteService _SQLiteService;

        public UniverseService(ILogger logger, string sdePath)
        {
            _Log = logger;
            _SDEPath = sdePath;
            _SQLiteService = new SQLiteService(_SDEPath);
        }

        public SolarSystemDTO GetSystemInfo(int systemId)
        {
            string sql = @"
select
	r.solarSystemID as Id,
	r.regionID as RegionId,
	(select regionName from mapRegions where regionID = r.regionID) as RegionName,
	r.constellationID as ConstellationId,
	(select constellationName from mapConstellations where constellationID = r.constellationID) as ConstellationName,
	r.solarSystemName as Name,
	r.x as Position_X,
	r.y as Position_Y,
	r.z as Position_Z,
	r.xMin as Position_XMin,
	r.xMax as Position_XMax,
	r.yMin as Position_YMin,
	r.yMax as Position_YMax,
	r.zMin as Position_ZMin,
	r.zMax as Position_ZMax,
	r.luminosity as Luminosity,
	r.border as Border,
	r.fringe as Fringe,
	r.corridor as Corridor,
	r.hub as Hub,
	r.international as International,
	r.regional as Regional,
	r.security as Security_Security,
	r.securityClass as Security_SecurityClass,
	r.factionID as FactionId,
	(select factionName from chrFactions where factionID = r.factionID) as FactionName,
	r.radius as Radius,
	r.sunTypeID as SunTypeId
from mapSolarSystems as r
where r.solarSystemID = @id
";
            SolarSystemDTO result = _SQLiteService.SelectSingle<SolarSystemDTO>(sql, new { id = systemId });
            return result;
        }
    }
}
