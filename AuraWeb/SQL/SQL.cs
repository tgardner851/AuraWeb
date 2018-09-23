using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.SQL
{
    public static class SDEViews
    {
        #region SQL
        private static string DROP_ITEMTYPES_V = "drop view if exists ItemTypes_V;";
        private static string DROP_MAP_V = "drop view if exists Map_V;";
        private static string DROP_CERTIFICATES_V = "drop view if exists Certificates_V;";
        #region Create Table Scripts
        private static string CREATE_ITEMTYPES_V = @"
/*
 * 
 * ITEM TYPES
 * 
 */
create view ItemTypes_V AS
select 
	type.typeID ID,
	type.typeName Name,
	type.description Description,
	type.mass Mass,
	type.volume Volume,
	type.capacity Capacity,
	type.portionSize PortionSize,
	typeRace.raceID Race_ID,
	typeRace.raceName Race_Name,
	typeRace.description Race_Description,
	typeRaceIcon.iconID Race_Icon_ID,
	typeRaceIcon.iconFile Race_Icon_File,
	typeRaceIcon.description Race_Icon_Description,
	typeRace.shortDescription Race_ShortDescription,
	type.basePrice BasePrice,
	type.published Published,
	typeMktGrp.marketGroupID MarketGroup_ID,
	typeMktGrp.parentGroupID MarketGroup_ParentID, /* Missing Join */
	typeMktGrp.marketGroupName MarketGroup_Name,
	typeMktGrp.description MarketGroup_Description,
	typeMktGrpIcon.iconID MarketGroup_Icon_ID,
	typeMktGrpIcon.iconFile MarketGroup_Icon_File,
	typeMktGrpIcon.description MarketGroup_Icon_Description,
	typeMktGrp.hasTypes MarketGroup_HasTypes,
	typeIcon.iconID Icon_ID,
	typeIcon.iconFile Icon_File,
	typeIcon.description Icon_Description,
	type.soundID SoundID, /* Missing Join */
	type.graphicID GraphicID, /* Missing Join */
	typeGrp.groupID Group_ID,
	typeGrp.groupName Group_Name,
	typeGrpIcon.iconID Group_Icon_ID,
	typeGrpIcon.iconFile Group_Icon_File,
	typeGrpIcon.description Group_Icon_Description,
	typeGrp.useBasePrice Group_UseBasePrice,
	typeGrp.anchored Group_Anchored,
	typeGrp.anchorable Group_Anchorable,
	typeGrp.fittableNonSingleton Group_FittableNonSingleton,
	typeGrp.published Group_Published,
	typeGrpCat.categoryID Group_Category_ID,
	typeGrpCat.categoryName Group_Category_Name,
	typeGrpCatIcon.iconID Group_Category_Icon_ID,
	typeGrpCatIcon.iconFile Group_Category_Icon_File,
	typeGrpCatIcon.description Group_Category_Icon_Description,
	typeGrpCat.published Group_Category_Published,
	typeMeta.parentTypeID Meta_ParentType_ID, /* Missing Join */
	typeMetaGrp.metaGroupID Meta_Group_ID,
	typeMetaGrp.metaGroupName Meta_Group_Name,
	typeMetaGrp.description Meta_Group_Description,
	typeMetaGrpIcon.iconID Meta_Group_Icon_ID,
	typeMetaGrpIcon.iconFile Meta_Group_Icon_File,
	typeMetaGrpIcon.description Meta_Group_Icon_Description,
	--typeVolume.volume Volume, /* There's already a volume column! */
	typeContraband.factionID Contraband_Faction_ID, 
	typeContraband.standingLoss Contraband_StandingLoss,
	typeContraband.confiscateMinSec Contraband_ConfiscateMinSec,
	typeContraband.fineByValue Contraband_FineByValue,
	typeContraband.attackMinSec Contraband_AttackMinSec,
	typeTrait.traitID Trait_ID,
	typeTrait.skillID Trait_Skill_ID,
	typeTrait.bonus Trait_Bonus,
	typeTrait.bonusText Trait_BonusText,
	typeTraitUnit.unitID Trait_Unit_ID,
	typeTraitUnit.unitName Trait_Unit_Name,
	typeTraitUnit.displayName Trait_DisplayName,
	typeTraitUnit.description Trait_Description
from invTypes type
left join invGroups typeGrp on typeGrp.groupID = type.groupID
left join invCategories typeGrpCat on typeGrpCat.categoryID = typeGrp.categoryID
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
left join invTraits typeTrait on typeTrait.typeID = type.typeID
left join eveUnits typeTraitUnit on typeTraitUnit.unitID = typeTrait.unitID
;
";
        private static string CREATE_MAP_V = @"
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
        private static string CREATE_CERTIFICATES_V = @"
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
        #endregion

        #region Select Scripts
        #region Base Select Scripts
        public static string SELECT_BASE_ITEMTYPES = @"SELECT * FROM ItemTypes_V";
        public static string SELECT_BASE_ITEMTYPES_BACKUP = @"
SELECT ID, Name, Description, Mass, Volume, Capacity, PortionSize, 
        RaceName as Race_RaceName, RaceDescription, RaceIconFile, RaceIconDescription, RaceShortDescription, 
    BasePrice, Published, 
        MarketGroupParentID, MarketGroupName, MarketGroupDescription, 
            MarketGroupIconFile, MarketGroupIconDescription, 
        MarketGroupHasTypes, 
    IconFile, IconDescription, SoundID, GraphicID, 
        GroupCategoryName, 
            GroupCategoryIconFile, GroupCategoryIconDescription, 
        GroupCategoryPublished, 
    GroupName, 
        GroupIconFile, GroupIconDescription, 
    GroupUseBasePrice, GroupAnchored, GroupAnchorable, GroupFittableNonSingleton, GroupPublished, 
    MetaParentTypeID, 
        MetaGroupName, MetaGroupDescription, 
            MetaGroupIconFile, MetaGroupIconDescription, 
    ContrabandFactionID, ContrabandStandingLoss, ContrabandConfiscateMinSec, ContrabandFineByValue, ContrabandAttackMinSec, 
    TraitSkillID, TraitBonus, TraitBonusText, TraitUnitName, TraitDisplayName, TraitDescription
FROM ItemTypes_V
";
        #endregion

        #region Item Types
        public static string SELECT_ALL_ITEMTYPES = SELECT_BASE_ITEMTYPES + ";";

        public static string SELECT_ITEMTYPES_ID = SELECT_BASE_ITEMTYPES + @" where (ID = @ID);";

        /// <summary>
        /// Requires @Query string
        /// </summary>
        public static string SELECT_ITEMTYPES_SEARCH = SELECT_BASE_ITEMTYPES + @" where (Name LIKE @Query OR Description LIKE @Query);";
        #endregion
        #endregion


        #endregion

        public static List<string> Sequence = new List<string>()
        {
            DROP_ITEMTYPES_V,
            DROP_MAP_V,
            DROP_CERTIFICATES_V,
            CREATE_ITEMTYPES_V,
            CREATE_MAP_V,
            CREATE_CERTIFICATES_V
        };
    }
}
