using AuraWeb.Models;
using ICSharpCode.SharpZipLib.BZip2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models;
using System.Threading;

namespace AuraWeb.Services
{
    public static class DBSQL
    {
        #region CREATE_TABLE
        public static List<string> SEQUENCE_CREATE_TABLES = new List<string>()
          {
              CREATE_TABLE_REGION_MARKET_TYPEIDS,
              CREATE_TABLE_REGION_MARKET_ORDERS,
              CREATE_TABLE_MARKET_AVERAGE_PRICES,
              CREATE_TABLE_CHARACTERS
          };
        public const string CREATE_TABLE_REGION_MARKET_TYPEIDS = @"
  CREATE TABLE IF NOT EXISTS RegionMarketTypeIds 
  (Id varchar primary key, RegionId int not null, TypeId int not null)";
        public const string CREATE_TABLE_REGION_MARKET_ORDERS = @"
  CREATE TABLE IF NOT EXISTS RegionMarketOrders 
  (Id varchar primary key, RegionId int not null, OrderId int not null, TypeId int not null, SystemId int not null, LocationId int not null, 
  Range text, IsBuyOrder int not null, Duration int, Issued text not null, MinVolume int, VolumeRemain int, 
  VolumeTotal int, Price int not null)";
        public const string CREATE_TABLE_MARKET_AVERAGE_PRICES = @"
  CREATE TABLE IF NOT EXISTS MarketAveragePrices
  (Id int primary key, TypeId int not null, AdjustedPrice int, AveragePrice int, Timestamp datetime not null)";
        public const string CREATE_TABLE_CHARACTERS = @"
  CREATE TABLE IF NOT EXISTS Characters
  (Id int primary key, Name varchar not null, Description varchar, Gender varchar, Birthday datetime not null, SecurityStatus int, RaceId int, 
  AncestryId int, BloodlineId int, AllianceId int, CorporationId int, FactionId int, SearchDate datetime)";
        #endregion

        #region CREATE_INDEX
        public static List<string> SEQUENCE_CREATE_INDEXES = new List<string>()
        {
            CREATE_INDEX_CHARACTERS_NAME,
            CREATE_INDEX_CHARACTERS_CORPORATIONID,
            CREATE_INDEX_CHARACTERS_FACTIONID,
            CREATE_INDEX_MARKETAVERAGEPRICES_TYPEID,
            CREATE_INDEX_MARKETAVERAGEPRICES_TIMESTAMP,
            CREATE_INDEX_REGIONMARKETORDERS_REGIONID,
            CREATE_INDEX_REGIONMARKETORDERS_ORDERID,
            CREATE_INDEX_REGIONMARKETORDERS_TYPEID,
            CREATE_INDEX_REGIONMARKETORDERS_SYSTEMID,
            CREATE_INDEX_REGIONMARKETORDERS_LOCATIONID,
            CREATE_INDEX_REGIONMARKETORDERS_ISBUYORDER,
            CREATE_INDEX_REGIONMARKETORDERS_VOLUMEREMAIN,
            CREATE_INDEX_REGIONMARKETTYPEIDS_REGIONID,
            CREATE_INDEX_REGIONMARKETTYPEIDS_TYPEID
        };
        public const string CREATE_INDEX_CHARACTERS_NAME = @"CREATE INDEX IF NOT EXISTS ""ix_Characters_Name"" ON ""Characters"" (""Name"")";
        public const string CREATE_INDEX_CHARACTERS_CORPORATIONID = @"CREATE INDEX IF NOT EXISTS ""ix_Characters_CorporationId"" ON ""Characters"" (""CorporationId"")";
        public const string CREATE_INDEX_CHARACTERS_FACTIONID = @"CREATE INDEX IF NOT EXISTS ""ix_Characters_FactionId"" ON ""Characters"" (""FactionId"")";
        public const string CREATE_INDEX_MARKETAVERAGEPRICES_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_MarketAveragePrices_TypeId"" ON ""MarketAveragePrices"" (""TypeId"")";
        public const string CREATE_INDEX_MARKETAVERAGEPRICES_TIMESTAMP = @"CREATE INDEX IF NOT EXISTS ""ix_MarketAveragePrices_Timestamp"" ON ""MarketAveragePrices"" (""Timestamp"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_REGIONID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_RegionId"" ON ""RegionMarketOrders"" (""RegionId"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_ORDERID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_OrderId"" ON ""RegionMarketOrders"" (""OrderId"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_TypeId"" ON ""RegionMarketOrders"" (""TypeId"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_SYSTEMID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_SystemId"" ON ""RegionMarketOrders"" (""SystemId"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_LOCATIONID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_LocationId"" ON ""RegionMarketOrders"" (""LocationId"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_ISBUYORDER = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_IsBuyOrder"" ON ""RegionMarketOrders"" (""IsBuyOrder"")";
        public const string CREATE_INDEX_REGIONMARKETORDERS_VOLUMEREMAIN = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_VolumeRemain"" ON ""RegionMarketOrders"" (""VolumeRemain"")";
        public const string CREATE_INDEX_REGIONMARKETTYPEIDS_REGIONID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketTypeIds_RegionId"" ON ""RegionMarketTypeIds"" (""RegionId"")";
        public const string CREATE_INDEX_REGIONMARKETTYPEIDS_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketTypeIds_TypeId"" ON ""RegionMarketTypeIds"" (""TypeId"")";
        #endregion

        #region INSERT
        public const string INSERT_REGIONMARKET_TYPEID = @"
  INSERT OR REPLACE INTO RegionMarketTypeIds (Id, RegionId, TypeId)
  VALUES (
    @Id, 
    COALESCE((SELECT RegionId FROM RegionMarketTypeIds WHERE Id = @Id), @RegionId),
    COALESCE((SELECT TypeId FROM RegionMarketTypeIds WHERE Id = @Id), @TypeId)
  )
  ";
        public const string INSERT_MARKET_ORDER = @"
  INSERT OR REPLACE INTO RegionMarketOrders (Id, RegionId, OrderId, TypeId, SystemId, 
  LocationId, Range, IsBuyOrder, Duration, Issued, MinVolume, VolumeRemain, VolumeTotal, 
  Price)
  VALUES (
    @Id, 
    COALESCE((SELECT RegionId FROM RegionMarketOrders WHERE Id = @Id), @RegionId),
    COALESCE((SELECT OrderId FROM RegionMarketOrders WHERE Id = @Id), @OrderId),
    COALESCE((SELECT TypeId FROM RegionMarketOrders WHERE Id = @Id), @TypeId),
    COALESCE((SELECT SystemId FROM RegionMarketOrders WHERE Id = @Id), @SystemId),
    COALESCE((SELECT LocationId FROM RegionMarketOrders WHERE Id = @Id), @LocationId),
    COALESCE((SELECT Range FROM RegionMarketOrders WHERE Id = @Id), @Range),
    COALESCE((SELECT IsBuyOrder FROM RegionMarketOrders WHERE Id = @Id), @IsBuyOrder),
    COALESCE((SELECT Duration FROM RegionMarketOrders WHERE Id = @Id), @Duration),
    COALESCE((SELECT Issued FROM RegionMarketOrders WHERE Id = @Id), @Issued),
    COALESCE((SELECT MinVolume FROM RegionMarketOrders WHERE Id = @Id), @MinVolume),
    COALESCE((SELECT VolumeRemain FROM RegionMarketOrders WHERE Id = @Id), @VolumeRemain),
    COALESCE((SELECT VolumeTotal FROM RegionMarketOrders WHERE Id = @Id), @VolumeTotal),
    COALESCE((SELECT Price FROM RegionMarketOrders WHERE Id = @Id), @Price)
  )
  ";
        /// <summary>
        /// Note: This table does not really have uniqueness to replace except for the timestamp.
        /// Unless you can time travel, this shouldn't become an issue.
        /// </summary>
        public const string INSERT_MARKET_AVERAGE_PRICE = @"
  INSERT INTO MarketAveragePrices (Timestamp, TypeId, AdjustedPrice, AveragePrice)
  VALUES (@Timestamp, @TypeId, @AdjustedPrice, @AveragePrice)
  ";
        public const string INSERT_CHARACTER = @"
  INSERT OR REPLACE INTO Characters (Id, Name, Description, Gender, Birthday, 
  SecurityStatus, RaceId, AncestryId, BloodlineId, AllianceId, CorporationId, 
  FactionId, SearchDate)      
  VALUES (
    @Id, 
    COALESCE((SELECT Name FROM Characters WHERE Id = @Id), @Name),
    COALESCE((SELECT Description FROM Characters WHERE Id = @Id), @Description),
    COALESCE((SELECT Gender FROM Characters WHERE Id = @Id), @Gender),
    COALESCE((SELECT Birthday FROM Characters WHERE Id = @Id), @Birthday),
    COALESCE((SELECT SecurityStatus FROM Characters WHERE Id = @Id), @SecurityStatus),
    COALESCE((SELECT RaceId FROM Characters WHERE Id = @Id), @RaceId),
    COALESCE((SELECT AncestryId FROM Characters WHERE Id = @Id), @AncestryId),
    COALESCE((SELECT BloodlineId FROM Characters WHERE Id = @Id), @BloodlineId),
    COALESCE((SELECT AllianceId FROM Characters WHERE Id = @Id), @AllianceId),
    COALESCE((SELECT CorporationId FROM Characters WHERE Id = @Id), @CorporationId),
    COALESCE((SELECT FactionId FROM Characters WHERE Id = @Id), @FactionId),
    COALESCE((SELECT SearchDate FROM Characters WHERE Id = @Id), @SearchDate)
  )
  ";
        #endregion

        #region SDE
        public static List<string> SEQUENCE_SDE = new List<string>()
        {
            DROP_ITEMTYPES_V,
            DROP_MAP_V,
            DROP_CERTIFICATES_V,
            DROP_REGIONS_V,
            DROP_CONSTELLATIONS_V,
            DROP_SOLARSYSTEMS_V,
            DROP_STATIONS_V,
            DROP_STATIONSERVICES_V,
            DROP_SKILLS_V,
            CREATE_ITEMTYPES_V,
            CREATE_MAP_V,
            CREATE_CERTIFICATES_V,
            CREATE_REGIONS_V,
            CREATE_CONSTELLATIONS_V,
            CREATE_SOLARSYSTEMS_V,
            CREATE_STATIONS_V,
            CREATE_STATIONSERVICES_V,
            CREATE_SKILLS_V
        };

        #region SDE VIEW DROPS
        public static string DROP_ITEMTYPES_V = "drop view if exists ItemTypes_V;";
        public static string DROP_MAP_V = "drop view if exists Map_V;";
        public static string DROP_CERTIFICATES_V = "drop view if exists Certificates_V;";
        public static string DROP_REGIONS_V = "drop view if exists Regions_V;";
        public static string DROP_CONSTELLATIONS_V = "drop view if exists Constellations_V;";
        public static string DROP_SOLARSYSTEMS_V = "drop view if exists SolarSytems_V;";
        public static string DROP_STATIONS_V = "drop view if exists Stations_V;";
        public static string DROP_STATIONSERVICES_V = "drop view if exists StationServices_V;";
        public static string DROP_SKILLS_V = "drop view if exists Skills_V";
        #endregion

        #region SDE VIEW CREATES
        public static string CREATE_ITEMTYPES_V = @"
/*
 * 
 * ITEM TYPES
 * 
 */
create view ItemTypes_V AS
select 
	type.typeID Id,
	type.typeName Name,
	type.description Description,
	type.mass Mass,
	type.volume Volume,
	type.capacity Capacity,
	type.portionSize PortionSize,
	typeRace.raceID Race_Id,
	typeRace.raceName Race_Name,
	typeRace.description Race_Description,
	typeRaceIcon.iconID Race_Icon_Id,
	typeRaceIcon.iconFile Race_Icon_File,
	typeRaceIcon.description Race_Icon_Description,
	typeRace.shortDescription Race_ShortDescription,
	type.basePrice BasePrice,
	type.published Published,
	typeMktGrp.marketGroupID MarketGroup_Id,
	typeMktGrp.parentGroupID MarketGroup_ParentId, /* Missing Join */
	typeMktGrp.marketGroupName MarketGroup_Name,
	typeMktGrp.description MarketGroup_Description,
	typeMktGrpIcon.iconID MarketGroup_Icon_Id,
	typeMktGrpIcon.iconFile MarketGroup_Icon_File,
	typeMktGrpIcon.description MarketGroup_Icon_Description,
	typeMktGrp.hasTypes MarketGroup_HasTypes,
	typeIcon.iconID Icon_Id,
	typeIcon.iconFile Icon_File,
	typeIcon.description Icon_Description,
	type.soundID SoundId, /* Missing Join */
	type.graphicID GraphicId, /* Missing Join */
	typeGrp.groupID Group_Id,
	typeGrp.groupName Group_Name,
	typeGrpIcon.iconID Group_Icon_Id,
	typeGrpIcon.iconFile Group_Icon_File,
	typeGrpIcon.description Group_Icon_Description,
	typeGrp.useBasePrice Group_UseBasePrice,
	typeGrp.anchored Group_Anchored,
	typeGrp.anchorable Group_Anchorable,
	typeGrp.fittableNonSingleton Group_FittableNonSingleton,
	typeGrp.published Group_Published,
	typeGrpCat.categoryID Group_Category_Id,
	typeGrpCat.categoryName Group_Category_Name,
	typeGrpCatIcon.iconID Group_Category_Icon_Id,
	typeGrpCatIcon.iconFile Group_Category_Icon_File,
	typeGrpCatIcon.description Group_Category_Icon_Description,
	typeGrpCat.published Group_Category_Published,
	typeMeta.parentTypeID Meta_ParentType_Id, /* Missing Join */
	typeMetaGrp.metaGroupID Meta_Group_Id,
	typeMetaGrp.metaGroupName Meta_Group_Name,
	typeMetaGrp.description Meta_Group_Description,
	typeMetaGrpIcon.iconID Meta_Group_Icon_Id,
	typeMetaGrpIcon.iconFile Meta_Group_Icon_File,
	typeMetaGrpIcon.description Meta_Group_Icon_Description,
	--typeVolume.volume Volume, /* There's already a volume column! */
	typeContraband.factionID Contraband_Faction_Id, 
	typeContraband.standingLoss Contraband_StandingLoss,
	typeContraband.confiscateMinSec Contraband_ConfiscateMinSec,
	typeContraband.fineByValue Contraband_FineByValue,
	typeContraband.attackMinSec Contraband_AttackMinSec,
	typeAttr.attributeID Attributes_Id,
	typeAttr.valueInt Attributes_ValueInt,
	typeAttr.valueFloat Attributes_ValueFloat,
	typeAttrType.attributeName Attributes_Name,
	typeAttrType.description Attributes_Description,
	typeAttrType.iconID Attributes_Icon_Id,
	typeAttrTypeIcon.iconFile Attributes_Icon_File,
	typeAttrTypeIcon.description Attributes_Icon_Description,
	typeAttrType.defaultValue Attributes_DefaultValue,
	typeAttrType.published Attributes_Published,
	typeAttrType.displayName Attributes_DisplayName,
	typeAttrType.unitID Attributes_Unit_Id,
	typeAttrTypeUnit.unitName Attributes_Unit_Name,
	typeAttrTypeUnit.displayName Attributes_Unit_DisplayName,
	typeAttrTypeUnit.description Attributes_Unit_Description,
	typeAttrType.stackable Attributes_Stackable,
	typeAttrType.highIsGood Attributes_HighIsGood,
	typeAttrType.categoryID Attributes_Category_Id,
	typeAttrTypeCategory.categoryName Attributes_Category_Name,
	typeAttrTypeCategory.categoryDescription Attributes_Category_Description,
	typeEffects.effectID Effects_Id,
	typeEffects.isDefault Effects_IsDefault,
	typeEffectsInfo.effectName Effects_Name,
	typeEffectsInfo.effectCategory Effects_Category,
	typeEffectsInfo.preExpression Effects_PreExpression,
	typeEffectsInfo.postExpression Effects_PostExpression,
	typeEffectsInfo.description Effects_Description,
	typeEffectsInfo.guid Effects_Guid,
	typeEffectsInfo.iconID Effects_Icon_Id,
	typeEffectsInfoIcon.iconFile Effects_Icon_File,
	typeEffectsInfoIcon.description Effects_Icon_Description,
	typeEffectsInfo.isOffensive Effects_IsOffensive,
	typeEffectsInfo.isAssistance Effects_IsAssistance,
	typeEffectsInfo.durationAttributeID Effects_DurationAttributeId, /* Missing Join */
	typeEffectsInfo.trackingSpeedAttributeID Effects_TrackingSpeedAttributeId, /* Missing Join */
	typeEffectsInfo.dischargeAttributeID Effects_DischargeAttributeId, /* Missing Join */
	typeEffectsInfo.rangeAttributeID Effects_RangeAttributeId, /* Missing Join */
	typeEffectsInfo.falloffAttributeID Effects_FalloffAttributeId, /* Missing Join */
	typeEffectsInfo.disallowAutoRepeat Effects_DisallowAutoRepeat,
	typeEffectsInfo.published Effects_Published,
	typeEffectsInfo.displayName Effects_DisplayName,
	typeEffectsInfo.isWarpSafe Effects_IsWarpSafe,
	typeEffectsInfo.rangeChance Effects_RangeChance,
	typeEffectsInfo.electronicChance Effects_ElectronicChance,
	typeEffectsInfo.propulsionChance Effects_PropulsionChance,
	typeEffectsInfo.distribution Effects_Distribution,
	typeEffectsInfo.sfxName Effects_SfxName,
	typeEffectsInfo.npcUsageChanceAttributeID Effects_NpcUsageChanceAttributeId,
	typeEffectsInfo.npcActivationChanceAttributeID Effects_NpcActivationChanceAttributeId,
	typeEffectsInfo.fittingUsageChanceAttributeID Effects_FittingUsageChanceAttributeId,
	typeEffectsInfo.modifierInfo Effects_ModifierInfo
from invTypes type
left join invGroups typeGrp on typeGrp.groupID = type.groupID
	and typeGrp.published = 1
left join invCategories typeGrpCat on typeGrpCat.categoryID = typeGrp.categoryID
	and typeGrpCat.published = 1
left join eveIcons typeGrpCatIcon on typeGrpCatIcon.iconID = typeGrpCat.iconID
left join eveIcons typeGrpIcon on typeGrpIcon.iconID = typeGrp.iconID
left join chrRaces typeRace on typeRace.raceID = type.raceID
left join eveIcons typeRaceIcon on typeRaceIcon.iconID = typeRace.iconID
left join invMarketGroups typeMktGrp on typeMktGrp.marketGroupID = type.marketGroupID
left join eveIcons typeMktGrpIcon on typeMktGrpIcon.iconID = typeMktGrp.iconID
left join eveIcons typeIcon on typeIcon.iconID = type.iconID
left join invMetaTypes typeMeta on typeMeta.typeId = type.typeID
left join invMetaGroups typeMetaGrp on typeMetaGrp.metaGroupID = typeMeta.metaGroupID
left join eveIcons typeMetaGrpIcon on typeMetaGrpIcon.iconID = typeMetaGrp.iconID
left join invVolumes typeVolume on typeVolume.typeID = type.typeID
left join invContrabandTypes typeContraband on typeContraband.typeID = type.typeID
left join dgmTypeAttributes typeAttr on typeAttr.typeID = type.typeID
left join dgmAttributeTypes typeAttrType on typeAttrType.attributeID = typeAttr.attributeID
	and typeAttrType.published = 1
left join eveIcons typeAttrTypeIcon on typeAttrTypeIcon.iconID = typeAttrType.iconID
left join eveUnits typeAttrTypeUnit on typeAttrTypeUnit.unitID = typeAttrType.unitID
left join dgmAttributeCategories typeAttrTypeCategory on typeAttrTypeCategory.categoryID = typeAttrType.categoryID
left join dgmTypeEffects typeEffects on typeEffects.typeID = type.typeID
left join dgmEffects typeEffectsInfo on typeEffectsInfo.effectID = typeEffects.effectID
	and typeEffectsInfo.published = 1
left join eveIcons typeEffectsInfoIcon on typeEffectsInfoIcon.iconID = typeEffectsInfo.iconID
where type.published = 1
;
";
        public static string CREATE_MAP_V = @"
/*
 * 
 * MAP
 * 
 */
create view Map_V AS
select
	rgn.regionID RegionID,
	rgn.regionName RegionName,
	rgn.x RegionX,
	rgn.y RegionY,
	rgn.z RegionZ,
	rgn.xMin RegionXMin,
	rgn.xMax RegionXMax,
	rgn.yMin RegionYMin,
	rgn.yMax RegionYMax,
	rgn.zMin RegionZMin,
	rgn.zMax RegionZMax,
	rgn.factionID FactionID, /* Missing Join */
	rgn.radius Radius,
	rgnCnstln.constellationID ConstellationID,
	rgnCnstln.constellationName ConstellationName,
	rgnCnstln.x ConstellationX,
	rgnCnstln.y ConstellationY,
	rgnCnstln.z ConstellationZ,
	rgnCnstln.xMin ConstellationXMin,
	rgnCnstln.xMax ConstellationXMax,
	rgnCnstln.yMin ConstellationYMin,
	rgnCnstln.yMax ConstellationYMax,
	rgnCnstln.zMin ConstellationZMin,
	rgnCnstln.zMax ConstellationZMax,
	rgnCnstln.factionID ConstellationFactionID, /* Missing Join */
	rgnCnstln.radius ConstellationRadius,
	rgnCnstlnSolSys.solarSystemID SolarSystemID,
	rgnCnstlnSolSys.solarSystemName SolarSystemName,
	rgnCnstlnSolSys.x SolarSystemX,
	rgnCnstlnSolSys.y SolarSystemY,
	rgnCnstlnSolSys.z SolarSystemZ,
	rgnCnstlnSolSys.xMin SolarSystemXMin,
	rgnCnstlnSolSys.xMax SolarSystemXMax,
	rgnCnstlnSolSys.yMin SolarSystemYMin,
	rgnCnstlnSolSys.yMax SolarSystemYMax,
	rgnCnstlnSolSys.zMin SolarSystemZMin,
	rgnCnstlnSolSys.zMax SolarSystemZMax,
	rgnCnstlnSolSys.luminosity SolarSystemLuminosity,
	rgnCnstlnSolSys.border SolarSystemBorder,
	rgnCnstlnSolSys.fringe SolarSystemFringe,
	rgnCnstlnSolSys.corridor SolarSystemCorridor,
	rgnCnstlnSolSys.hub SolarSystemHub,
	rgnCnstlnSolSys.international SolarSystemInternational,
	rgnCnstlnSolSys.regional SolarSystemRegional,
	rgnCnstlnSolSys.constellation SolarSystemConstellation,
	rgnCnstlnSolSys.security SolarSystemSecurity,
	rgnCnstlnSolSys.factionID SolarSystemFactionID, /* Missing Join */
	rgnCnstlnSolSys.radius SolarSystemRadius,
	rgnCnstlnSolSys.sunTypeID SolarSystemSunTypeID,
	rgnCnstlnSolSys.securityClass SolarSystemSecurityClass,
	rgnCnstlnSolSysPlace.itemID PlaceItemID, /* Missing Join */
	rgnCnstlnSolSysPlaceName.itemName PlaceItemName,
	rgnCnstlnSolSysPlace.typeID PlaceTypeID, /* Mission Join */
	rgnCnstlnSolSysPlace.ownerID PlaceOwnerID, /* Missing Join */
	rgnCnstlnSolSysPlace.flagID PlaceFlagID, /* Missing Join */
	rgnCnstlnSolSysPlace.quantity PlaceQuantity,
	rgnCnstlnSolSysStation.stationID StationID,
	rgnCnstlnSolSysStation.security StationSecurity,
	rgnCnstlnSolSysStation.dockingCostPerVolume StationDockingCostPerVolume,
	rgnCnstlnSolSysStation.maxShipVolumeDockable StationMaxShipVolumeDockable,
	rgnCnstlnSolSysStation.officeRentalCost StationOfficeRentalCost,
	rgnCnstlnSolSysStation.operationID StationOperationID, /* Missing Join */
	--rgnCnstlnSolSysStationType.stationTypeID StationTypeID,
	rgnCnstlnSolSysStationType.dockEntryX StationTypeDockEntryX,
	rgnCnstlnSolSysStationType.dockEntryY StationTypeDockEntryY,
	rgnCnstlnSolSysStationType.dockEntryZ StationTypeDockEntryZ,
	rgnCnstlnSolSysStationType.dockOrientationX StationTypeDockOrientationX,
	rgnCnstlnSolSysStationType.dockOrientationY StationTypeDockOrientationY,
	rgnCnstlnSolSysStationType.dockOrientationZ StationTypeDockOrientationZ,
	rgnCnstlnSolSysStationType.operationID StationTypeOperationID, /* Missing Join */
	rgnCnstlnSolSysStationType.officeSlots StationTypeOfficeSlots,
	rgnCnstlnSolSysStationType.reprocessingEfficiency StationTypeReprocessingEfficiency,
	rgnCnstlnSolSysStationType.conquerable StationTypeConquerable,
	rgnCnstlnSolSysStation.corporationID StationCorporationID, /* Missing Join */
	rgnCnstlnSolSysStation.stationName StationName,
	rgnCnstlnSolSysStation.x StationX,
	rgnCnstlnSolSysStation.y StationY,
	rgnCnstlnSolSysStation.z StationZ,
	rgnCnstlnSolSysStation.reprocessingEfficiency StationReprocessingEfficiency,
	rgnCnstlnSolSysStation.reprocessingStationsTake StationReprocessingStationsTake,
	rgnCnstlnSolSysStation.reprocessingHangarFlag StationReprocessingHangarFlag
from mapRegions rgn
left join mapConstellations rgnCnstln on rgnCnstln.regionID = rgn.regionID
left join mapSolarSystems rgnCnstlnSolSys on rgnCnstlnSolSys.regionID = rgn.regionID
	and rgnCnstlnSolSys.constellationID = rgnCnstln.constellationID
left join invItems rgnCnstlnSolSysPlace on rgnCnstlnSolSysPlace.locationID = rgnCnstlnSolSys.solarSystemID
left join invNames rgnCnstlnSolSysPlaceName on rgnCnstlnSolSysPlaceName.itemID = rgnCnstlnSolSysPlace.itemID
left join staStations rgnCnstlnSolSysStation on rgnCnstlnSolSysStation.regionID = rgn.regionID 
	and rgnCnstlnSolSysStation.constellationID = rgnCnstln.constellationID
	and rgnCnstlnSolSysStation.solarSystemID = rgnCnstlnSolSys.solarSystemID
	and rgnCnstlnSolSysStation.stationID = rgnCnstlnSolSysPlace.itemID
left join staStationTypes rgnCnstlnSolSysStationType on rgnCnstlnSolSysStationType.stationTypeID = rgnCnstlnSolSysStation.stationTypeID
--where SolarSystemName LIKE ('%clellinon%') 
	--and regionConstellationSolarSystemStationStationName = 'Clellinon VI - Moon 11 - Center for Advanced Studies School'
	--and StationId = '60015036'
;
";
        public static string CREATE_CERTIFICATES_V = @"
/*
 * 
 * CERTIFICATES
 * 
 */
create view Certificates_V AS
select 
	crt.certID ID,
	crt.description Description,
	--crtGrp.groupID GroupID,
	crtGrp.categoryID GroupCategoryID,
	crtGrp.groupName GroupName,
	--crtGrpIcon.iconID GroupIconID,
	crtGrpIcon.iconFile GroupIconFile,
	crtGrpIcon.description GroupIconDescription,
	crtGrp.useBasePrice GroupUseBasePrice,
	crtGrp.anchored GroupAnchored,
	crtGrp.anchorable GroupAnchorable,
	crtGrp.fittableNonSingleton GroupFittableNonSingleton,
	crtGrp.published GroupPublished,
	crt.name Name,
	crtMstr.typeID MasteryTypeID,
	crtMstr.masteryLevel MasteryLevel,
	skl.skillID SkillID,
	skl.certLevelInt SkillCertLevelInt,
	skl.skillLevel SkillCertLevel,
	skl.certLevelText SkillCertLevelText
from certCerts crt
left join certSkills skl on skl.certID = crt.certID
left join invGroups crtGrp on crtGrp.groupID = crt.groupID
left join eveIcons crtGrpIcon on crtGrpIcon.iconID = crtGrp.iconID
left join certMasteries crtMstr on crtMstr.certID = crt.certID
;
";
        public static string CREATE_REGIONS_V = @"
/* 
 * 
 * REGIONS
 * 
 */
create view Regions_V AS
select 
	r.regionID as Id,
	r.regionName as Name,
	r.x as Position_X,
	r.y as Position_Y,
	r.z as Position_Z,
	r.xMin as Position_XMin,
	r.xMax as Position_XMax,
	r.yMin as Position_YMin,
	r.yMax as Position_YMax,
	r.zMin as Position_ZMin,
	r.zMax as Position_ZMax,
	r.factionID as FactionId,
	(select factionName from chrFactions where factionID = r.factionID) as FactionName,
	r.radius as Radius
from mapRegions as r
";
        public static string CREATE_CONSTELLATIONS_V = @"
/* 
 * 
 * CONSTELLATIONS 
 * 
 */
create view Constellations_V AS
select
	c.constellationID as Id,
	c.regionID as RegionId,
	(select regionName from mapRegions where regionID = c.regionID) as RegionName,
	c.constellationName as Name,
	c.x as Position_X,
	c.y as Position_Y,
	c.z as Position_Z,
	c.xMin as Position_XMin,
	c.xMax as Position_XMax,
	c.yMin as Position_YMin,
	c.yMax as Position_YMax,
	c.zMin as Position_ZMin,
	c.zMax as Position_ZMax,
	c.factionID as FactionId,
	(select factionName from chrFactions where factionID = c.factionID) as FactionName,
	c.radius as Radius
from mapConstellations as c
;
";
        public static string CREATE_SOLARSYSTEMS_V = @"
/* 
 * 
 * SOLAR SYSTEM 
 * 
 */
create view SolarSystems_V AS
select
	s.solarSystemID as Id,
	s.regionID as RegionId,
	(select regionName from mapRegions where regionID = s.regionID) as RegionName,
	s.constellationID as ConstellationId,
	(select constellationName from mapConstellations where constellationID = s.constellationID) as ConstellationName,
	s.solarSystemName as Name,
	s.x as Position_X,
	s.y as Position_Y,
	s.z as Position_Z,
	s.xMin as Position_XMin,
	s.xMax as Position_XMax,
	s.yMin as Position_YMin,
	s.yMax as Position_YMax,
	s.zMin as Position_ZMin,
	s.zMax as Position_ZMax,
	s.luminosity as Luminosity,
	s.border as Border,
	s.fringe as Fringe,
	s.corridor as Corridor,
	s.hub as Hub,
	s.international as International,
	s.regional as Regional,
	s.security as Security_Status,
	s.securityClass as Security_Class,
	s.factionID as FactionId,
	(select factionName from chrFactions where factionID = s.factionID) as FactionName,
	s.radius as Radius,
	s.sunTypeID as SunTypeId
from mapSolarSystems as s
;
";
        public static string CREATE_STATIONS_V = @"
/*
 * 
 * STATIONS
 * 
 */
create view Stations_V AS
select 
	s.stationID as Id,
	s.solarSystemID as SolarSystemId,
	(select solarSystemName from mapSolarSystems where solarSystemID = s.solarSystemID) as SolarSystemName,
	s.constellationID as ConstellationId,
	(select constellationName from mapConstellations where constellationID = s.constellationID) as ConstellationName,
	s.regionID as RegionId,
	(select regionName from mapRegions where regionID = s.regionID) as RegionName,
	s.stationName as Name,
	s.x as Position_X,
	s.y as Position_Y,
	s.z as Position_Z,
	s.security as Security_Status,
	s.dockingCostPerVolume as DockingCostPerVolume,
	s.maxShipVolumeDockable as MaxShipVolumeDockable,
	t.officeSlots as OfficeSlots,
	s.officeRentalCost as OfficeRentalCost,
	s.reprocessingEfficiency as ReprocessingEfficiency,
	s.reprocessingStationsTake as ReprocessingStationsTake,
	s.reprocessingHangarFlag as ReprocessingHangarFlag,
	s.operationID as OperationId,
	o.activityID as OperationActivityId,
	o.operationName as OperationName,
	o.description as OperationDescription,
	s.stationTypeID as StationTypeId,
	t.dockEntryX as Dock_EntryX,
	t.dockEntryY as Dock_EntryY,
	t.dockEntryZ as Dock_EntryZ,
	t.dockOrientationX as Dock_OrientationX,
	t.dockOrientationY as Dock_OrientationY,
	t.dockOrientationZ as Dock_OrientationZ,
	t.conquerable as Conquerable,
	s.corporationID as CorporationId
from staStations as s
left join staOperations as o on o.operationID = s.operationID
left join staStationTypes as t on t.stationTypeID = s.stationTypeID
;
";
        public static string CREATE_STATIONSERVICES_V = @"
/*
 * 
 * STATION SERVICES
 * 
 */
create view StationServices_V AS
select 
	s.stationID as StationId,
	s.stationName as StationName,
	os.serviceID as ServiceId,
	sv.serviceName as Name,
	sv.description as Description
from staStations as s
join staOperations as o on o.operationID = s.operationID
join staOperationServices as os on os.operationID = o.operationID
join staServices as sv on sv.serviceID = os.serviceID
;
";
        public static string CREATE_SKILLS_V = @"
/*
 * 
 * SKILLS
 * 
 */
create view Skills_V as 
select
	skl.skillID Id,
	skl.certLevelInt SkillLevelInt,
	skl.skillLevel SkillCertLevel,
	skl.certLevelText SkillCertLevelText,
	crt.certID Cert_Id,
	crt.name Cert_Name,
	crt.description Cert_Description,
	crt.groupID Cert_Group_Id,
	crtGrp.categoryID Cert_Group_Category_Id,
	crtGrp.groupName Cert_Group_Name,
	crtGrp.iconID Cert_Group_Icon_Id,
	crtGrpIcon.iconFile Cert_Group_Icon_File,
	crtGrpIcon.description Cert_Group_Icon_Description,
	crtGrp.useBasePrice Cert_Group_UseBasePrice,
	crtGrp.anchored Cert_Group_Anchored,
	crtGrp.anchorable Cert_Group_Anchorable,
	crtGrp.fittableNonSingleton Cert_Group_FittableNonSingleton,
	crtGrp.published Cert_Group_Published
from certSkills skl
join certCerts crt on crt.certID = skl.certID
left join invGroups crtGrp on crtGrp.groupID = crt.groupID
	and crtGrp.published = 1
left join eveIcons crtGrpIcon on crtGrpIcon.iconID = crtGrp.iconID
;
";
        #endregion
        
        #endregion
    }

    public class DBService
    {
        private readonly ILogger _Log;
        private readonly EVEStandardAPI _ESIClient;
        private SQLiteService _SQLiteService;
        private readonly string _DBFileName;
        private readonly string _SDEFileName;
        private readonly string _SDETempCompressedFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadUrl;
        private const int JITA_REGION_ID = 10000002; // SystemId: 30000142;
        private const int SECONDS_TIMEOUT = 240;
        private const int SECONDS_BETWEEN_ACTIONS = 4;
        private const int SECONDS_BETWEEN_REGIONS = 4;
        private int MS_BETWEEN_ACTIONS = SECONDS_BETWEEN_ACTIONS * 1000;
        private int MS_BETWEEN_REGIONS = SECONDS_BETWEEN_REGIONS * 1000;

        public DBService(ILogger logger, string dbFileName, string sdeFileName, string sdeTempCompressedFileName, string sdeTempFileName, string sdeDownloadUrl)
        {
            _Log = logger;
            _DBFileName = dbFileName;
            _SQLiteService = new SQLiteService(dbFileName);
            _SDEFileName = sdeFileName;
            _SDETempCompressedFileName = sdeTempCompressedFileName;
            _SDETempFileName = sdeTempFileName;
            _SDEDownloadUrl = sdeDownloadUrl;
            _ESIClient = new EVEStandardAPI(
                "AuraWebMarketDownloader",                      // User agent
                DataSource.Tranquility,                         // Server [Tranquility/Singularity]
                TimeSpan.FromSeconds(SECONDS_TIMEOUT)           // Timeout
            );
        }

        private void Initialize()
        {
            CreateDb();
            CreateTables();
            CreateIndexes();
        }

        #region DB Actions
        private bool DBExists()
        {
            FileInfo dbFile = new FileInfo(_DBFileName);
            if (!dbFile.Exists) return false;
            if (dbFile.Length < 1024) return false; // Byte size should be > 1024 at minimum
            return true;
        }

        private void CreateDb()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Creating Database...");
            if (!DBExists()) // Create the DB if needed
            {
                FileInfo fi = new FileInfo(_DBFileName);
                fi.Directory.Create();
                FileStream fs = File.Create(_DBFileName);
                fs.Close();
                _Log.LogDebug(String.Format("Created Database '{0}'.", _DBFileName));
            }
            sw.Stop();
            _Log.LogInformation(String.Format("Created Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }

        private void CreateTables()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Creating Base Tables for Database...");
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_CREATE_TABLES);
            sw.Stop();
            _Log.LogInformation(String.Format("Created Tables for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }

        private void CreateIndexes()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Creating Base Indexes for Database...");
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_CREATE_INDEXES);
            sw.Stop();
            _Log.LogInformation(String.Format("Created Tables for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }

        private void CreateSDEViews()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug(String.Format("Will execute {0} SQL scripts. Starting...", DBSQL.SEQUENCE_SDE.Count));
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_SDE);
            sw.Stop();
            _Log.LogInformation(String.Format("Created SDE Views for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }
        #endregion

        #region Market
        public void DownloadMarket(bool jitaPricesOnly = false)
        {
            Initialize();

            if (jitaPricesOnly) _Log.LogInformation("Beginning Market Download for Jita Region...");
            else _Log.LogInformation("Beginning Market Download for all Regions...");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (jitaPricesOnly) DownloadAndSaveMarketPricesForJita();
            else DownloadAndSaveMarketPrices();

            sw.Stop();

            _Log.LogDebug(String.Format("Market download finished; entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }
        
        private void DownloadAndSaveMarketPrices()
        {
            Stopwatch sw = new Stopwatch();

            #region Handle Market Average Prices
            sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Getting Market Average Prices");
            var marketPricesApi = _ESIClient.Market.ListMarketPricesV1Async().Result;
            List<MarketPrice> marketPrices = marketPricesApi.Model;
            sw.Stop();
            _Log.LogDebug(String.Format("Finished getting Market Average Prices. Result count is {0}. Took {1} seconds.", marketPrices.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
            // Persist to database
            DateTime timestamp = DateTime.Now;
            try
            {
                sw = new Stopwatch();
                sw.Start();
                List<InsertDTO> marketAveragePricesInsert = new List<InsertDTO>();
                foreach (MarketPrice market in marketPrices)
                {
                    object parameter = new
                    {
                        Timestamp = timestamp,
                        TypeId = market.TypeId,
                        AdjustedPrice = market.AdjustedPrice,
                        AveragePrice = market.AveragePrice
                    };
                    marketAveragePricesInsert.Add(new InsertDTO()
                    {
                        SQL = DBSQL.INSERT_MARKET_AVERAGE_PRICE,
                        Parameters = parameter
                    });
                }
                List<string> marketAveragePricesInsertSql = marketAveragePricesInsert.Select(a => a.SQL).ToList();
                List<object> marketAveragePricesInsertParameters = marketAveragePricesInsert.Select(a => a.Parameters).ToList();
                _SQLiteService.ExecuteMultiple(marketAveragePricesInsertSql, marketAveragePricesInsertParameters);
                sw.Stop();
                _Log.LogDebug(String.Format("Inserted Market Average Prices to database. Took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
            }
            catch (Exception e)
            {
                _Log.LogError(e, String.Format("Failed to insert Market Average Prices. Will proceed. Error: {0}", e.Message));
            }
            #endregion

            #region Get Region Ids
            List<int> regionIds = new List<int>();
            var regionIdsResult = _ESIClient.Universe.GetRegionsV1Async().Result;
            regionIds = regionIdsResult.Model;
            _Log.LogDebug(String.Format("Found {0} Regions to process in Market.", regionIds.Count));
            #endregion

            DownloadAndSaveMarketPricesForRegion(regionIds);
        }

        private void DownloadAndSaveMarketPricesForJita()
        {
            DownloadAndSaveMarketPricesForRegion(new List<int>() { JITA_REGION_ID });
        }

        private void DownloadAndSaveMarketPricesForRegion(List<int> regionIds)
        {
            Stopwatch sw = new Stopwatch();

            for (int x = 0; x < regionIds.Count; x++) // Loop through the regions
            {
                int regionId = regionIds[x];
                _Log.LogDebug(String.Format("Processing Region Id {0} for Market ({1} of {2})...", regionId, x + 1, regionIds.Count));
                sw = new Stopwatch();

                #region Get Type Ids in Region Market
                sw = new Stopwatch();
                sw.Start();
                List<long> typeIdsInRegion = new List<long>();
                var typeIdsInRegionResult = _ESIClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, 1).Result; // GetById the results for page 1
                typeIdsInRegion = typeIdsInRegionResult.Model; // Assign the result
                if (typeIdsInRegionResult.MaxPages > 1) // Handle multiple pages
                {
                    for (int a = 2; a <= typeIdsInRegionResult.MaxPages; a++)
                    {
                        // Try twice
                        try
                        {
                            typeIdsInRegionResult = _ESIClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, a).Result; // GetById the results for page a
                        }
                        catch(Exception e)
                        {
                            try
                            {
                                typeIdsInRegionResult = _ESIClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, a).Result; // GetById the results for page a
                            }
                            catch(Exception e2)
                            {
                                throw;
                            }
                            throw;
                        }
                        typeIdsInRegion.AddRange(typeIdsInRegionResult.Model); // Add the results to the master list
                    }
                }
                typeIdsInRegion = typeIdsInRegion.Distinct().ToList(); // Remove duplicates to avoid SQL errors
                sw.Stop();
                _Log.LogDebug(String.Format("Finished getting Type Ids in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, typeIdsInRegionResult.MaxPages, typeIdsInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                // Persist to database
                try
                {
                    sw = new Stopwatch();
                    sw.Start();
                    List<InsertDTO> marketTypeIdInserts = new List<InsertDTO>();
                    foreach (long typeId in typeIdsInRegion)
                    {
                        string key = String.Format("{0}-{1}", regionId, typeId);
                        object parameter = new
                        {
                            Id = key,
                            RegionId = regionId,
                            TypeId = typeId
                        };
                        marketTypeIdInserts.Add(new InsertDTO()
                        {
                            SQL = DBSQL.INSERT_REGIONMARKET_TYPEID,
                            Parameters = parameter
                        });
                    }
                    List<string> marketTypeIdInsertSql = marketTypeIdInserts.Select(a => a.SQL).ToList();
                    List<object> marketTypeIdInsertParameters = marketTypeIdInserts.Select(a => a.Parameters).ToList();
                    _SQLiteService.ExecuteMultiple(marketTypeIdInsertSql, marketTypeIdInsertParameters);
                    sw.Stop();
                    _Log.LogDebug(String.Format("Inserted Type Ids in Market for Region {0} to database. Took {1} seconds.", regionId, sw.Elapsed.TotalSeconds.ToString("##.##")));
                }
                catch (Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to insert Type Ids in Market for Region {0}. Will proceed. Error: {1}", regionId, e.Message));
                }
                #endregion

                // Give the servers a break
                _Log.LogDebug(String.Format("Sleeping for {0} seconds before processing next Market request...", SECONDS_BETWEEN_ACTIONS.ToString()));
                Thread.Sleep(MS_BETWEEN_ACTIONS);

                #region Get Orders in Region Market
                sw = new Stopwatch();
                sw.Start();
                List<MarketOrder> ordersInRegion = new List<MarketOrder>();
                var ordersInRegionResult = _ESIClient.Market.ListOrdersInRegionV1Async(regionId, null, 1).Result; // GetById the results for page 1
                ordersInRegion = ordersInRegionResult.Model; // Assign the result
                if (ordersInRegionResult.MaxPages > 1) // Handle multiple pages
                {
                    for (int a = 2; a <= ordersInRegionResult.MaxPages; a++)
                    {
                        // Try twice
                        try
                        {
                            ordersInRegionResult = _ESIClient.Market.ListOrdersInRegionV1Async(regionId, null, a).Result; // GetById the results for page a
                        }
                        catch(Exception e)
                        {
                            try
                            {
                                ordersInRegionResult = _ESIClient.Market.ListOrdersInRegionV1Async(regionId, null, a).Result; // GetById the results for page a
                            }
                            catch(Exception e2)
                            {
                                throw;
                            }
                            throw;
                        }
                        ordersInRegion.AddRange(ordersInRegionResult.Model); // Add the results to the master list
                    }
                }
                ordersInRegion = ordersInRegion.Distinct().ToList(); // Remove duplicates to avoid SQL errors
                sw.Stop();
                _Log.LogDebug(String.Format("Finished getting Orders in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, ordersInRegionResult.MaxPages, ordersInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                // Persist to database
                try
                {
                    sw = new Stopwatch();
                    sw.Start();
                    List<InsertDTO> marketOrderInserts = new List<InsertDTO>();
                    foreach (MarketOrder order in ordersInRegion)
                    {
                        string key = String.Format("{0}-{1}-{2}", regionId, order.OrderId, order.TypeId);
                        object parameter = new
                        {
                            Id = key,
                            RegionId = regionId,
                            OrderId = order.OrderId,
                            TypeId = order.TypeId,
                            SystemId = order.SystemId,
                            LocationId = order.LocationId,
                            Range = order.Range,
                            IsBuyOrder = order.IsBuyOrder == true ? 1 : 0,
                            Duration = order.Duration,
                            Issued = order.Issued,
                            MinVolume = order.MinVolume,
                            VolumeRemain = order.VolumeRemain,
                            VolumeTotal = order.VolumeTotal,
                            Price = order.Price
                        };
                        marketOrderInserts.Add(new InsertDTO()
                        {
                            SQL = DBSQL.INSERT_MARKET_ORDER,
                            Parameters = parameter
                        });
                    }
                    List<string> marketOrderInsertSql = marketOrderInserts.Select(a => a.SQL).ToList();
                    List<object> marketOrderInsertParameters = marketOrderInserts.Select(a => a.Parameters).ToList();
                    _SQLiteService.ExecuteMultiple(marketOrderInsertSql, marketOrderInsertParameters);
                    sw.Stop();
                    _Log.LogDebug(String.Format("Inserted Orders in Market for Region {0} to database. Took {1} seconds.", regionId, sw.Elapsed.TotalSeconds.ToString("##.##")));
                }
                catch (Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to insert Orders in Market for Region {0}. Will proceed. Error: {1}", regionId, e.Message));
                }
                #endregion

                // Give the servers a break
                _Log.LogDebug(String.Format("Sleeping for {0} seconds before processing next Market request...", SECONDS_BETWEEN_ACTIONS.ToString()));
                //Thread.Sleep(MS_BETWEEN_ACTIONS);

                /*
                    * The below currently not working. Either fix with a pull request, or reference link below
                    * to use the API itself
                    * https://esi.evetech.net/ui#/
                    */
                // var marketGroups = await _ESIClient.Market.GetItemGroupsV1Async();

                // TODO: Consider doing this by type id in region at some point //_ESIClient.Market.ListHistoricalMarketStatisticsInRegionV1Async

                double percentComplete = ((double)((x + 1) / regionIds.Count)) * 100;
                _Log.LogInformation(String.Format("Finished Processing Region Id {0} for Market ({1} of {2}) ({1}%)", regionId, x + 1, regionIds.Count, percentComplete.ToString("##.##")));

                // Give the servers a break, this time only if not the last one
                if (x != regionIds.Count - 1)
                {
                    _Log.LogDebug(String.Format("Sleeping for {0} seconds before processing next Market Region batch request...", SECONDS_BETWEEN_REGIONS.ToString()));
                    Thread.Sleep(MS_BETWEEN_REGIONS);
                }
            }
        }
        #endregion

        #region SDE
        private bool SDEExists()
        {
            return new FileInfo(_SDEFileName).Exists;
        }

        /// <summary>
        /// Downloads the SDE and imports it to the master AuraWeb database.
        /// </summary>
        public void RefreshSDEData()
        {
            Initialize();

            _Log.LogInformation("Beginning SDE Data Refresh...");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            DownloadAndSaveSDE();
            MigrateSDEToDB();

            sw.Stop();

            _Log.LogDebug(String.Format("SDE data refresh process is finished. Entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }

        /// <summary>
        /// Download and save a new SDE from source.
        /// </summary>
        private void DownloadAndSaveSDE()
        {
            // GetById the filename to download to (with path). Make sure to use same path as config and exe
            string sdePath = _SDEFileName;
            string sdeTempCompressedPath = _SDETempCompressedFileName;
            string sdeTempPath = _SDETempFileName;
            string sdeAddress = _SDEDownloadUrl;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            _Log.LogInformation(String.Format("Downloading SDE from URL '{0}' to temp file '{1}'...", sdeAddress, sdeTempCompressedPath));

            // Create the temp directory if needed
            string sdeTempDir = new FileInfo(sdeTempCompressedPath).Directory.FullName;
            Directory.CreateDirectory(sdeTempDir);
            _Log.LogDebug(String.Format("Created temp directory '{0}'.", sdeTempDir));

            // Attempt the download synchronously to the temp path
            try
            {
                _Log.LogInformation(String.Format("Downloading file '{0}' to '{1}'...", sdeAddress, sdeTempCompressedPath));
                Downloader dl = new Downloader(sdeAddress, sdeTempCompressedPath);
                dl.DownloadFile();
            }
            catch (Exception e)
            {
                _Log.LogError(e, String.Format("Failed to download SDE from address '{0}' to temp path '{1}'", sdeAddress, sdeTempCompressedPath), e);
                throw;
            }
            _Log.LogInformation(String.Format("Finished downloading SDE from URL '{0}' to temp file '{1}'.", sdeAddress, sdeTempCompressedPath));

            // File should be .bz2
            FileInfo sdeTemp = new FileInfo(sdeTempCompressedPath);

            // Decompress!
            using (FileStream sdeTempStream = sdeTemp.OpenRead())
            {
                using (FileStream sdeStream = File.Create(sdeTempPath))
                {
                    try
                    {
                        _Log.LogInformation(String.Format("Decompressing .bz2 file '{0}' to '{1}'...", sdeTempCompressedPath, sdeTempPath));
                        Stopwatch sdeDecompressionTimer = new Stopwatch();
                        sdeDecompressionTimer.Start();
                        BZip2.Decompress(sdeTempStream, sdeStream, true);
                        sdeDecompressionTimer.Stop();
                        _Log.LogInformation(String.Format("Finished decompressing .bz2 file '{0}' to '{1}'. Took {2} minutes to complete.", sdeTempCompressedPath, sdeTempPath, Math.Round(sdeDecompressionTimer.Elapsed.TotalMinutes, 2).ToString("0.##")));
                    }
                    catch (Exception e)
                    {
                        _Log.LogError(e, String.Format("Failed to decompress sde temp file '{0}'", sdeTempPath), e);
                        throw;
                    }
                }
            }

            // Verify file extracted properly, byte size should be > 1024 at minimum
            FileInfo sdeTempFile = new FileInfo(sdeTempPath);
            if (sdeTempFile.Length < 1024)
            {
                _Log.LogError("File extraction likely failed. File size was less than 1024 bytes.");
                sdeTempFile.Delete(); // Delete the file, as it's invalid
                throw new Exception("SDE File length was under 1024 bytes.");
            }

            // Replace old SDE with new SDE, if needed
            if (File.Exists(sdePath)) // Existing SDE, delete it
            {
                for (int x = 0; x < 3; x++) // Try three times if it fails
                {
                    bool success = false;
                    try
                    {
                        File.Delete(sdePath);
                        File.Copy(sdeTempPath, sdePath);
                        _Log.LogDebug(String.Format("Replaced old SDE with new at '{0}'.", sdePath));
                        success = true;
                    }
                    catch (Exception e)
                    {
                        success = false;
                        continue;
                    }
                    if (success) break;
                }
            }

            // Delete temp directory and all files inside
            string dirDel = new FileInfo(sdeTempPath).Directory.FullName;
            Directory.Delete(dirDel, true);
            _Log.LogDebug(String.Format("Deleted SDE download temp directory and files within '{0}'.", _SDETempFileName));

            sw.Stop();

            _Log.LogInformation(String.Format("SDE downloaded. Entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }

        // https://www.c-sharpcorner.com/article/merge-two-sqlite-databases-in-windows-runtime-apps/
        // https://stackoverflow.com/questions/4544083/merging-two-sqlite-database-files-c-net
        // https://stackoverflow.com/questions/12211717/how-to-dump-sqlite-in-memory-database-into-file-with-ado-net
        /// <summary>
        /// Migrate the SDE file to the master AuraWeb database.
        /// </summary>
        private void MigrateSDEToDB()
        {
            if (!SDEExists())
            {
                _Log.LogError(String.Format("SDE at path '{0}' does not exist.", _SDEFileName));
                return;
            }

            Stopwatch sw = new Stopwatch();

            // First, create the tables

            sw.Start();
            _Log.LogDebug(String.Format("Getting scripts from existing SDE to recreate in the AuraWeb database..."));

            SQLiteService _SDESqliteService = new SQLiteService(_SDEFileName);
            // Get Tables
            string sqlForTables = @"
select type as Type, name as Name, tbl_name as TableName, sql as SQL
from sqlite_master 
where name not like 'sqlite_%' and type = 'table';
";
            List<SQLiteMigrationObject> Tables = _SDESqliteService.SelectMultiple<SQLiteMigrationObject>(sqlForTables); // Get the scripts from the SDE
            List<string> TablesSQL = Tables.Select(x => x.SQL).ToList(); // Select the SQL
            // Need to correct each to avoid errors with insert if the record already exists
            for(int x = 0; x < TablesSQL.Count; x++)
            {
                TablesSQL[x] = TablesSQL[x].Replace("CREATE TABLE", "CREATE TABLE IF NOT EXISTS");
            }
            // Get Indexes
            string sqlForIndexes = @"
select type as Type, name as Name, tbl_name as TableName, sql as SQL
from sqlite_master 
where name not like 'sqlite_%' and type = 'index';
";
            List<SQLiteMigrationObject> Indexes = _SDESqliteService.SelectMultiple<SQLiteMigrationObject>(sqlForIndexes); // Get the scripts from the SDE
            List<string> IndexesSQL = Indexes.Select(x => x.SQL).ToList(); // Select the SQL
            // Need to repair each to avoid errors with insert if the record already exists
            for (int x = 0; x < IndexesSQL.Count; x++)
            {
                IndexesSQL[x] = IndexesSQL[x].Replace("CREATE UNIQUE INDEX", "CREATE UNIQUE INDEX IF NOT EXISTS");
                IndexesSQL[x] = IndexesSQL[x].Replace("CREATE INDEX", "CREATE INDEX IF NOT EXISTS");
            }

            _Log.LogDebug(String.Format("Found {0} Tables and {1} Indexes to process.", TablesSQL.Count, IndexesSQL.Count));

            _Log.LogDebug(String.Format("Executing all scripts..."));
            
            _SQLiteService.ExecuteMultiple(TablesSQL); // Execute on the master db
            _SQLiteService.ExecuteMultiple(IndexesSQL); // Execute on the master db
            sw.Stop();

            _Log.LogInformation(String.Format("Finished executing create scripts from SDE. Entire process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));

            // Now move the data!

            sw = new Stopwatch();
            _Log.LogInformation(String.Format("Beginning migration of SDE data..."));
            sw.Start();

            // Attach the SDE database to a connection using the AuraWeb database, and move the data
            string sqlAttach = String.Format("ATTACH '{0}' AS SDE", _SDEFileName);

            List<string> _MigrationScriptList = new List<string>(); // These SQL statements will be executed in order
            _MigrationScriptList.Add(sqlAttach);

            // Need a list tables
            List<string> TableNames = Tables.Select(x => x.TableName).ToList();

            // Now generate a Delete for each Table in the AuraWeb database, and an Insert from the attached SDE db to the AuraWeb db
            for(int x = 0; x < TableNames.Count; x++)
            {
                string tableName = TableNames[x];
                string deleteSql = String.Format("DELETE FROM {0}", tableName);
                string insertSql = String.Format("INSERT INTO {0} SELECT * FROM SDE.{0}", tableName);
                _MigrationScriptList.Add(deleteSql);
                _MigrationScriptList.Add(insertSql);
            }

            // By this point the migration script list contains an attach, and a delete then insert for each table
            _SQLiteService.ExecuteMultiple(_MigrationScriptList);

            sw.Stop();

            _Log.LogInformation(String.Format("Finished executing migration scripts for SDE. Entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }

        // Used for the following query used in schema/table generation from SDE
        /*
select type as Type, name as Name, tbl_name as TableName, sql as SQL
from sqlite_master 
where name not like 'sqlite_%' ;
        */
        private class SQLiteMigrationObject
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string TableName { get; set; }
            public string SQL { get; set; }
        }
        #endregion
    }
}