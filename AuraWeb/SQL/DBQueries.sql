/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;
select * from MarketOpportunities;
select * from MarketOpportunitiesDetail;



select distinct
	type.Id as TypeId,
	type.Name as TypeName,
	recentAverages.AveragePrice,
	recentAverages.AdjustedPrice,
	recentAverages.LastUpdated as AveragesUpdated,
	bestBuy.RegionId,
	bestBuy.RegionName,
	bestBuy.SystemId,
	bestBuy.SystemName,
	bestBuy.LocationId,
	bestBuy.StationName,
	bestBuy.Price,
	bestSell.RegionId,
	bestSell.RegionName,
	bestSell.SystemId,
	bestSell.SystemName,
	bestSell.LocationId,
	bestSell.StationName,
	bestSell.Price
from ItemTypes_V as type 
left join MarketAveragesRecent_V as recentAverages on recentAverages.TypeId = type.Id 
left join MarketBestBuyPrices_V as bestBuy on bestBuy.TypeId = type.Id
left join MarketBestSellPrices_V as bestSell on bestSell.TypeId = type.Id
;


select * from ItemTypes_V where Id = 606 and Effects_Published = 1;

select * from ItemTypes_V where Id = 606;

select distinct Effects_Published from ItemTypes_V;



select 
	type.typeID Id,
	type.typeName Name,
	type.description Description,
	type.mass Mass,
	type.volume Volume,
	type.capacity Capacity,
	type.portionSize PortionSize,
	typeRace.raceID Race_Id,
	IFNULL(typeRace.raceName, 'None') Race_Name,
	typeRace.description Race_Description,
	typeRaceIcon.iconID Race_Icon_Id,
	typeRaceIcon.iconFile Race_Icon_File,
	typeRaceIcon.description Race_Icon_Description,
	typeRace.shortDescription Race_ShortDescription,
	type.basePrice BasePrice,
	type.published Published,
	typeMktGrp.marketGroupID MarketGroup_Id,
	typeMktGrp.parentGroupID MarketGroup_ParentId, /* Missing Join */
	IFNULL(typeMktGrp.marketGroupName, 'None') MarketGroup_Name,
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
	IFNULL(typeGrp.groupName, 'None') Group_Name,
	typeGrpIcon.iconID Group_Icon_Id,
	typeGrpIcon.iconFile Group_Icon_File,
	typeGrpIcon.description Group_Icon_Description,
	typeGrp.useBasePrice Group_UseBasePrice,
	typeGrp.anchored Group_Anchored,
	typeGrp.anchorable Group_Anchorable,
	typeGrp.fittableNonSingleton Group_FittableNonSingleton,
	typeGrp.published Group_Published,
	typeGrpCat.categoryID Group_Category_Id,
	IFNULL(typeGrpCat.categoryName, 'None') Group_Category_Name,
	typeGrpCatIcon.iconID Group_Category_Icon_Id,
	typeGrpCatIcon.iconFile Group_Category_Icon_File,
	typeGrpCatIcon.description Group_Category_Icon_Description,
	typeGrpCat.published Group_Category_Published,
	typeMeta.parentTypeID Meta_ParentType_Id, /* Missing Join */
	typeMetaGrp.metaGroupID Meta_Group_Id,
	IFNULL(typeMetaGrp.metaGroupName, 'None') Meta_Group_Name,
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
	IFNULL(typeAttrType.attributeName, 'None') Attributes_Name,
	typeAttrType.description Attributes_Description,
	typeAttrType.iconID Attributes_Icon_Id,
	typeAttrTypeIcon.iconFile Attributes_Icon_File,
	typeAttrTypeIcon.description Attributes_Icon_Description,
	typeAttrType.defaultValue Attributes_DefaultValue,
	typeAttrType.published Attributes_Published,
	IFNULL(typeAttrType.displayName, 'None') Attributes_DisplayName,
	typeAttrType.unitID Attributes_Unit_Id,
	typeAttrTypeUnit.unitName Attributes_Unit_Name,
	typeAttrTypeUnit.displayName Attributes_Unit_DisplayName,
	typeAttrTypeUnit.description Attributes_Unit_Description,
	typeAttrType.stackable Attributes_Stackable,
	typeAttrType.highIsGood Attributes_HighIsGood,
	typeAttrType.categoryID Attributes_Category_Id,
	IFNULL(typeAttrTypeCategory.categoryName, 'None') Attributes_Category_Name,
	typeAttrTypeCategory.categoryDescription Attributes_Category_Description,
	typeEffects.effectID Effects_Id,
	typeEffects.isDefault Effects_IsDefault,
	IFNULL(typeEffectsInfo.effectName, 'None') Effects_Name,
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
	IFNULL(typeEffectsInfo.displayName, 'None') Effects_DisplayName,
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
	and typeAttrType.published = 1 and typeAttrType.published is not null
left join eveIcons typeAttrTypeIcon on typeAttrTypeIcon.iconID = typeAttrType.iconID
left join eveUnits typeAttrTypeUnit on typeAttrTypeUnit.unitID = typeAttrType.unitID
left join dgmAttributeCategories typeAttrTypeCategory on typeAttrTypeCategory.categoryID = typeAttrType.categoryID
left join dgmTypeEffects typeEffects on typeEffects.typeID = type.typeID
left join dgmEffects typeEffectsInfo on typeEffectsInfo.effectID = typeEffects.effectID
	and typeEffectsInfo.published = 1 and typeEffectsInfo.published is not null
left join eveIcons typeEffectsInfoIcon on typeEffectsInfoIcon.iconID = typeEffectsInfo.iconID
where type.published = 1
	and type.typeID = 606







/*
 * CHARACTER
 */
select * from Characters;