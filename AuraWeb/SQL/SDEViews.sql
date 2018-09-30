drop view if exists ItemTypes_V;
drop view if exists Certificates_V;
drop view if exists Regions_V;
drop view if exists Constellations_V;
drop view if exists SolarSytems_V;
drop view if exists Stations_V;
drop view if exists StationServices_V;
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
left join dgmTypeAttributes typeAttr on typeAttr.typeID = type.typeID
left join dgmAttributeTypes typeAttrType on typeAttrType.attributeID = typeAttr.attributeID
left join eveIcons typeAttrTypeIcon on typeAttrTypeIcon.iconID = typeAttrType.iconID
left join eveUnits typeAttrTypeUnit on typeAttrTypeUnit.unitID = typeAttrType.unitID
left join dgmAttributeCategories typeAttrTypeCategory on typeAttrTypeCategory.categoryID = typeAttrType.categoryID
left join dgmTypeEffects typeEffects on typeEffects.typeID = type.typeID
left join dgmEffects typeEffectsInfo on typeEffectsInfo.effectID = typeEffects.effectID
left join eveIcons typeEffectsInfoIcon on typeEffectsInfoIcon.iconID = typeEffectsInfo.iconID
;
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
;
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
left join eveIcons crtGrpIcon on crtGrpIcon.iconID = crtGrp.iconID
;




select * from ItemTypes_V where Id = 606;
select * from Map_V;
select * from Certificates_V where SkillId = 11579;
select * from Regions_V;
select * from Constellations_V;
select * from SolarSystems_V;
select * from Stations_V;
select * from staOperations;
select * from staStationTypes where operationID is not null;
select * from staOperations;



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
	typeAttr.attributeID Attribute_Id,
	typeAttr.valueInt Attribute_ValueInt,
	typeAttr.valueFloat Attribute_ValueFloat,
	typeAttrType.attributeName Attribute_Name,
	typeAttrType.description Attribute_Description,
	typeAttrType.iconID Attribute_Icon_Id,
	typeAttrTypeIcon.iconFile Attribute_Icon_File,
	typeAttrTypeIcon.description Attribute_Icon_Description,
	typeAttrType.defaultValue Attribute_DefaultValue,
	typeAttrType.published Attribute_Published,
	typeAttrType.displayName Attribute_DisplayName,
	typeAttrType.unitID Attribute_Unit_Id,
	typeAttrTypeUnit.unitName Attribute_Unit_Name,
	typeAttrTypeUnit.displayName Attribute_Unit_DisplayName,
	typeAttrTypeUnit.description Attribute_Unit_Description,
	typeAttrType.stackable Attribute_Stackable,
	typeAttrType.highIsGood Attribute_HighIsGood,
	typeAttrType.categoryID Attribute_Category_Id,
	typeAttrTypeCategory.categoryName Attribute_Category_Name,
	typeAttrTypeCategory.categoryDescription Attribute_Category_Description,
	typeEffects.effectID Effect_Id,
	typeEffects.isDefault Effect_IsDefault,
	typeEffectsInfo.effectName Effect_Name,
	typeEffectsInfo.effectCategory Effect_Category,
	typeEffectsInfo.preExpression Effect_PreExpression,
	typeEffectsInfo.postExpression Effect_PostExpression,
	typeEffectsInfo.description Effect_Description,
	typeEffectsInfo.guid Effect_Guid,
	typeEffectsInfo.iconID Effect_Icon_Id,
	typeEffectsInfoIcon.iconFile Effect_Icon_File,
	typeEffectsInfoIcon.description Effect_Icon_Description,
	typeEffectsInfo.isOffensive Effect_IsOffensive,
	typeEffectsInfo.isAssistance Effect_IsAssistance,
	typeEffectsInfo.durationAttributeID Effect_DurationAttributeId, /* Missing Join */
	typeEffectsInfo.trackingSpeedAttributeID Effect_TrackingSpeedAttributeId, /* Missing Join */
	typeEffectsInfo.dischargeAttributeID Effect_DischargeAttributeId, /* Missing Join */
	typeEffectsInfo.rangeAttributeID Effect_RangeAttributeId, /* Missing Join */
	typeEffectsInfo.falloffAttributeID Effect_FalloffAttributeId, /* Missing Join */
	typeEffectsInfo.disallowAutoRepeat Effect_DisallowAutoRepeat,
	typeEffectsInfo.published Effect_Published,
	typeEffectsInfo.displayName Effect_DisplayName,
	typeEffectsInfo.isWarpSafe Effect_IsWarpSafe,
	typeEffectsInfo.rangeChance Effect_RangeChance,
	typeEffectsInfo.electronicChance Effect_ElectronicChance,
	typeEffectsInfo.propulsionChance Effect_PropulsionChance,
	typeEffectsInfo.distribution Effect_Distribution,
	typeEffectsInfo.sfxName Effect_SfxName,
	typeEffectsInfo.npcUsageChanceAttributeID Effect_NpcUsageChanceAttributeId,
	typeEffectsInfo.npcActivationChanceAttributeID Effect_NpcActivationChanceAttributeId,
	typeEffectsInfo.fittingUsageChanceAttributeID Effect_FittingUsageChanceAttributeId,
	typeEffectsInfo.modifierInfo Effect_ModifierInfo
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
left join dgmTypeAttributes typeAttr on typeAttr.typeID = type.typeID
left join dgmAttributeTypes typeAttrType on typeAttrType.attributeID = typeAttr.attributeID
left join eveIcons typeAttrTypeIcon on typeAttrTypeIcon.iconID = typeAttrType.iconID
left join eveUnits typeAttrTypeUnit on typeAttrTypeUnit.unitID = typeAttrType.unitID
left join dgmAttributeCategories typeAttrTypeCategory on typeAttrTypeCategory.categoryID = typeAttrType.categoryID
left join dgmTypeEffects typeEffects on typeEffects.typeID = type.typeID
left join dgmEffects typeEffectsInfo on typeEffectsInfo.effectID = typeEffects.effectID
left join eveIcons typeEffectsInfoIcon on typeEffectsInfoIcon.iconID = typeEffectsInfo.iconID
where type.typeID = 606
order by Attribute_Id


select * from dgmEffects;

