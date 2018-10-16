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
    public static class SDEHelpers
    {
        public static string FormatString_Range(string range)
        {
            string result = String.Empty;
            int rangeInt = -1;
            Int32.TryParse(range, out rangeInt);
            if (rangeInt > 0) result = String.Format("{0} Jumps", rangeInt);
            else result = range.FirstCharToUpper();
            return result;
        }
    }

    public static class DBSQL
    {
        #region CREATE_BASE_TABLES
        public static List<string> SEQUENCE_CREATE_BASE_TABLES = new List<string>()
          {
              CREATE_BASE_TABLE_REGION_MARKET_TYPEIDS,
              CREATE_BASE_TABLE_REGION_MARKET_ORDERS,
              CREATE_BASE_TABLE_MARKET_AVERAGE_PRICES,
              CREATE_BASE_TABLE_CHARACTERS,
              CREATE_BASE_TABLE_MARKET_OPPORTUNITIES,
              CREATE_BASE_TABLE_MARKET_OPPORTUNITIES_DETAIL
          };
        public const string CREATE_BASE_TABLE_REGION_MARKET_TYPEIDS = @"
CREATE TABLE IF NOT EXISTS RegionMarketTypeIds 
(Id varchar primary key, RegionId int not null, TypeId int not null)";
        public const string CREATE_BASE_TABLE_REGION_MARKET_ORDERS = @"
CREATE TABLE IF NOT EXISTS RegionMarketOrders 
(Id varchar primary key, RegionId int not null, OrderId int not null, TypeId int not null, SystemId int not null, LocationId int not null, 
Range text, IsBuyOrder int not null, Duration int, Issued text not null, MinVolume int, VolumeRemain int, 
VolumeTotal int, Price decimal not null)";
        public const string CREATE_BASE_TABLE_MARKET_AVERAGE_PRICES = @"
CREATE TABLE IF NOT EXISTS MarketAveragePrices
(Id int primary key, TypeId int not null, AdjustedPrice decimal, AveragePrice decimal, Timestamp datetime not null)";
        public const string CREATE_BASE_TABLE_CHARACTERS = @"
CREATE TABLE IF NOT EXISTS Characters
(Id int primary key, Name varchar not null, Description varchar, Gender varchar, BirthDate datetime not null, SecurityStatus decimal, RaceId int, 
AncestryId int, BloodlineId int, AllianceId int, CorporationId int, FactionId int, LastUpdateDate datetime)";
        public const string CREATE_BASE_TABLE_MARKET_OPPORTUNITIES = @"
CREATE TABLE IF NOT EXISTS MarketOpportunities 
(TypeId int primary key, BuyId varchar not null, BuyPrice number not null, SellId varchar not null, 
SellPrice number not null, PriceDiff number not null)";
        public const string CREATE_BASE_TABLE_MARKET_OPPORTUNITIES_DETAIL = @"
CREATE TABLE IF NOT EXISTS MarketOpportunitiesDetail
(TypeId int primary key, TypeName varchar, MarketGroupName varchar, GroupName varchar, GroupCategoryName varchar,
BuyId varchar not null, BuyRegionId int not null, BuyRegionName varchar, BuyOrderId int not null,
BuySystemId int not null, BuySystemName varchar, BuyLocationId int not null, BuyStationName varchar, BuyRange varchar not null, 
BuyDuration int not null, BuyIssued datetime not null, BuyMinVolume int, BuyVolumeRemain int, BuyVolumeTotal int, BuyPrice number not null, 
SellId varchar not null, SellRegionId int not null, SellRegionName varchar, SellOrderId int not null,
SellSystemId int not null, SellSystemName varchar, SellLocationId int not null, SellStationName varchar, SellRange varchar not null, 
SellDuration int not null, SellIssued datetime not null, SellMinVolume int, SellVolumeRemain int, SellVolumeTotal int, SellPrice number not null, 
PriceDiff number not null)";
        #endregion

        #region CREATE_BASE_INDEXES
        public static List<string> SEQUENCE_CREATE_BASE_INDEXES = new List<string>()
        {
            CREATE_BASE_INDEX_CHARACTERS_NAME,
            CREATE_BASE_INDEX_CHARACTERS_CORPORATIONID,
            CREATE_BASE_INDEX_CHARACTERS_FACTIONID,
            CREATE_BASE_INDEX_MARKETAVERAGEPRICES_TYPEID,
            CREATE_BASE_INDEX_MARKETAVERAGEPRICES_TIMESTAMP,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_REGIONID,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_ORDERID,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_TYPEID,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_SYSTEMID,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_LOCATIONID,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_ISBUYORDER,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_VOLUMEREMAIN,
            CREATE_BASE_INDEX_REGIONMARKETORDERS_TYPEIDISBUYORDER,
            CREATE_BASE_INDEX_REGIONMARKETTYPEIDS_REGIONID,
            CREATE_BASE_INDEX_REGIONMARKETTYPEIDS_TYPEID,
            CREATE_BASE_INDEX_MARKETOPPORTUNITIES_TYPEID,
            CREATE_BASE_INDEX_MARKETOPPORTUNITIES_BUYID,
            CREATE_BASE_INDEX_MARKETOPPORTUNITIES_SELLID,
            CREATE_BASE_INDEX_MARKETOPPORTUNITIES_PRICEDIFF,
            CREATE_BASE_INDEX_MARKETOPPORTUNITIESDETAIL_TYPEID
        };
        public const string CREATE_BASE_INDEX_CHARACTERS_NAME = @"CREATE INDEX IF NOT EXISTS ""ix_Characters_Name"" ON ""Characters"" (""Name"")";
        public const string CREATE_BASE_INDEX_CHARACTERS_CORPORATIONID = @"CREATE INDEX IF NOT EXISTS ""ix_Characters_CorporationId"" ON ""Characters"" (""CorporationId"")";
        public const string CREATE_BASE_INDEX_CHARACTERS_FACTIONID = @"CREATE INDEX IF NOT EXISTS ""ix_Characters_FactionId"" ON ""Characters"" (""FactionId"")";
        public const string CREATE_BASE_INDEX_MARKETAVERAGEPRICES_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_MarketAveragePrices_TypeId"" ON ""MarketAveragePrices"" (""TypeId"")";
        public const string CREATE_BASE_INDEX_MARKETAVERAGEPRICES_TIMESTAMP = @"CREATE INDEX IF NOT EXISTS ""ix_MarketAveragePrices_Timestamp"" ON ""MarketAveragePrices"" (""Timestamp"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_REGIONID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_RegionId"" ON ""RegionMarketOrders"" (""RegionId"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_ORDERID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_OrderId"" ON ""RegionMarketOrders"" (""OrderId"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_TypeId"" ON ""RegionMarketOrders"" (""TypeId"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_SYSTEMID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_SystemId"" ON ""RegionMarketOrders"" (""SystemId"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_LOCATIONID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_LocationId"" ON ""RegionMarketOrders"" (""LocationId"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_ISBUYORDER = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_IsBuyOrder"" ON ""RegionMarketOrders"" (""IsBuyOrder"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_VOLUMEREMAIN = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_VolumeRemain"" ON ""RegionMarketOrders"" (""VolumeRemain"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETORDERS_TYPEIDISBUYORDER = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketOrders_TypeIdIsBuyOrder"" ON ""RegionMarketTypeIds"" (""TypeId"", ""IsBuyOrder"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETTYPEIDS_REGIONID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketTypeIds_RegionId"" ON ""RegionMarketTypeIds"" (""RegionId"")";
        public const string CREATE_BASE_INDEX_REGIONMARKETTYPEIDS_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_RegionMarketTypeIds_TypeId"" ON ""RegionMarketTypeIds"" (""TypeId"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIES_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunities_TypeId"" ON MarketOpportunities (""TypeId"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIES_BUYID = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunities_BuyId"" ON MarketOpportunities (""BuyId"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIES_SELLID = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunities_SellId"" ON MarketOpportunities (""SellId"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIES_PRICEDIFF = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunities_PriceDiff"" ON MarketOpportunities (""PriceDiff"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIESDETAIL_TYPEID = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunitiesDetail_TypeId"" ON MarketOpportunitiesDetail (""TypeId"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIESDETAIL_PRICEDIFF = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunitiesDetail_PriceDiff"" ON MarketOpportunitiesDetail (""PriceDiff"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIESDETAIL_MARKETGROUPNAME = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunitiesDetail_MarketGroupName"" ON MarketOpportunitiesDetail (""MarketGroupName"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIESDETAIL_GROUPNAME = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunitiesDetail_GroupName"" ON MarketOpportunitiesDetail (""GroupName"")";
        public const string CREATE_BASE_INDEX_MARKETOPPORTUNITIESDETAIL_GROUPCATEGORYNAME = @"CREATE INDEX IF NOT EXISTS ""ix_MarketOpportunitiesDetail_GroupCategoryName"" ON MarketOpportunitiesDetail (""GroupCategoryName"")";
        #endregion

        #region CREATE_BASE_VIEWS
        public static List<string> SEQUENCE_CREATE_BASE_VIEWS = new List<string>()
        {
            CREATE_BASE_VIEW_MARKET_AVERAGES_RECENT,
            CREATE_BASE_VIEW_MARKET_BEST_SELL_PRICES,
            CREATE_BASE_VIEW_MARKET_BEST_BUY_PRICES
        };
        public const string CREATE_BASE_VIEW_MARKET_AVERAGES_RECENT = @"
create view if not exists MarketAveragesRecent_V as 
select 
	a.TypeId,
	(select distinct Name from ItemTypes_V where Id = a.TypeId) as TypeName,
	a.AveragePrice,
	a.AdjustedPrice, 
	a.TimeStamp as LastUpdated
from MarketAveragePrices as a
join (
	select max(""Timestamp"") as ""Timestamp"", TypeId
	from MarketAveragePrices

    group by TypeId
) as b on b.""Timestamp"" = a.""Timestamp""
	and b.TypeId = a.TypeId
";
        public const string CREATE_BASE_VIEW_MARKET_BEST_SELL_PRICES = @"
create view if not exists MarketBestSellPrices_V as 
select 
	TypeId,
	(select distinct Name from ItemTypes_V where Id = TypeId) TypeName,
	OrderId,
	RegionId,
	(select distinct Name from Regions_V where Id = RegionId) RegionName,
	SystemId,
	(select distinct Name from SolarSystems_V where Id = SystemId) SystemName,
	LocationId,
	(select distinct Name from Stations_V where Id = LocationId) StationName,
	case when Range = 'station' then 'Station'
		when Range = 'solarsystem' then 'System'
		when Range = 'region' then 'Region'
		when Range = '1' then '1 Jump'
		else (Range || ' Jumps')
	end RangeName,
	Duration,
	Issued,
	MinVolume,
	VolumeRemain,
	Price
from RegionMarketOrders
where rowid in (
	select rowid from (
		select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
		where IsBuyOrder = 1
		group by TypeId
	)
)
order by TypeId asc, Price desc, RegionId asc, SystemId asc, LocationId asc
";
        public const string CREATE_BASE_VIEW_MARKET_BEST_BUY_PRICES = @"
create view if not exists MarketBestBuyPrices_V as 
select 
	TypeId,
	(select distinct Name from ItemTypes_V where Id = TypeId) TypeName,
	OrderId,
	RegionId,
	(select distinct Name from Regions_V where Id = RegionId) RegionName,
	SystemId,
	(select distinct Name from SolarSystems_V where Id = SystemId) SystemName,
	LocationId,
	(select distinct Name from Stations_V where Id = LocationId) StationName,
	case when Range = 'station' then 'Station'
		when Range = 'solarsystem' then 'System'
		when Range = 'region' then 'Region'
		when Range = '1' then '1 Jump'
		else (Range || ' Jumps')
	end RangeName,
	Duration,
	Issued,
	MinVolume,
	VolumeRemain,
	Price
from RegionMarketOrders
where rowid in (
	select rowid from (
		select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
		where IsBuyOrder = 0
		group by TypeId
	)
)
order by TypeId asc, Price desc, RegionId asc, SystemId asc, LocationId asc
";
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
INSERT OR REPLACE INTO Characters (Id, Name, Description, Gender, BirthDate, SecurityStatus,
RaceId, AncestryId, BloodlineId, AllianceId, CorporationId, FactionId, LastUpdateDate)
VALUES (
    @Id,
    COALESCE((SELECT Name FROM Characters WHERE Id = @Id), @Name),
    COALESCE((SELECT Description FROM Characters WHERE Id = @Id), @Description),
    COALESCE((SELECT Gender FROM Characters WHERE Id = @Id), @Gender),
    COALESCE((SELECT BirthDate FROM Characters WHERE Id = @Id), @BirthDate),
    COALESCE((SELECT SecurityStatus FROM Characters WHERE Id = @Id), @SecurityStatus),
    COALESCE((SELECT RaceId FROM Characters WHERE Id = @Id), @RaceId),
    COALESCE((SELECT AncestryId FROM Characters WHERE Id = @Id), @AncestryId),
    COALESCE((SELECT BloodlineId FROM Characters WHERE Id = @Id), @BloodlineId),
    COALESCE((SELECT AllianceId FROM Characters WHERE Id = @Id), @AllianceId),
    COALESCE((SELECT CorporationId FROM Characters WHERE Id = @Id), @CorporationId),
    COALESCE((SELECT FactionId FROM Characters WHERE Id = @Id), @FactionId),
    COALESCE((SELECT LastUpdateDate FROM Characters WHERE Id = @Id), @LastUpdateDate)
)
";
        #endregion

        #region SDE
        public static List<string> SEQUENCE_SDE_VIEWS_DROP = new List<string>()
        {
            DROP_ITEMTYPES_V,
            DROP_MAP_V,
            DROP_CERTIFICATES_V,
            DROP_REGIONS_V,
            DROP_CONSTELLATIONS_V,
            DROP_SOLARSYSTEMS_V,
            DROP_STATIONS_V,
            DROP_STATIONSERVICES_V,
            DROP_SKILLS_V
        };
        public static List<string> SEQUENCE_SDE_VIEWS_CREATE = new List<string>()
        {
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
        public const string DROP_ITEMTYPES_V = "drop view if exists ItemTypes_V;";
        public const string DROP_MAP_V = "drop view if exists Map_V;";
        public const string DROP_CERTIFICATES_V = "drop view if exists Certificates_V;";
        public const string DROP_REGIONS_V = "drop view if exists Regions_V;";
        public const string DROP_CONSTELLATIONS_V = "drop view if exists Constellations_V;";
        public const string DROP_SOLARSYSTEMS_V = "drop view if exists SolarSytems_V;";
        public const string DROP_STATIONS_V = "drop view if exists Stations_V;";
        public const string DROP_STATIONSERVICES_V = "drop view if exists StationServices_V;";
        public const string DROP_SKILLS_V = "drop view if exists Skills_V";
        #endregion

        #region SDE VIEW CREATES
        public const string CREATE_ITEMTYPES_V = @"
/*
 * 
 * ITEM TYPES
 * 
 */
create view if not exists ItemTypes_V AS
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
        public const string CREATE_MAP_V = @"
/*
 * 
 * MAP
 * 
 */
create view if not exists Map_V AS
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
        public const string CREATE_CERTIFICATES_V = @"
/*
 * 
 * CERTIFICATES
 * 
 */
create view if not exists Certificates_V AS
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
        public const string CREATE_REGIONS_V = @"
/* 
 * 
 * REGIONS
 * 
 */
create view if not exists Regions_V AS
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
	IFNULL((select factionName from chrFactions where factionID = r.factionID), 'None') as FactionName,
	r.radius as Radius
from mapRegions as r
";
        public const string CREATE_CONSTELLATIONS_V = @"
/* 
 * 
 * CONSTELLATIONS 
 * 
 */
create view if not exists Constellations_V AS
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
	IFNULL((select factionName from chrFactions where factionID = c.factionID), 'None') as FactionName,
	c.radius as Radius
from mapConstellations as c
;
";
        public const string CREATE_SOLARSYSTEMS_V = @"
/* 
 * 
 * SOLAR SYSTEM 
 * 
 */
create view if not exists SolarSystems_V AS
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
	IFNULL(s.securityClass, 'None') as Security_Class,
	s.factionID as FactionId,
	IFNULL((select factionName from chrFactions where factionID = s.factionID), 'None') as FactionName,
	s.radius as Radius,
	s.sunTypeID as SunTypeId
from mapSolarSystems as s
;
";
        public const string CREATE_STATIONS_V = @"
/*
 * 
 * STATIONS
 * 
 */
create view if not exists Stations_V AS
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
        public const string CREATE_STATIONSERVICES_V = @"
/*
 * 
 * STATION SERVICES
 * 
 */
create view if not exists StationServices_V AS
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
        public const string CREATE_SKILLS_V = @"
/*
 * 
 * SKILLS
 * 
 */
create view if not exists Skills_V as 
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
        public const string CREATE_ORES_V = @"
/*
 *
 * ORES
 *
 */
create view if not exists Ores_V as 
select
	a.*,
	buy.OrderId BuyOrderId,
	buy.RegionId BuyRegionId,
	buy.RegionName BuyRegionName,
	buy.SystemId BuySystemId,
	buy.SystemName BuySystemName,
	buy.LocationId BuyLocationId,
	buy.StationName BuyStationName,
	buy.RangeName BuyRangeName,
	buy.Duration BuyDuration,
	buy.Issued BuyIssued,
	buy.MinVolume BuyMinVolume,
	buy.VolumeRemain BuyVolumeRemain,
	buy.Price BuyPrice,
	sell.OrderId SellOrderId,
	sell.RegionId SellRegionId,
	sell.RegionName SellRegionName,
	sell.SystemId SellSystemId,
	sell.SystemName SellSystemName,
	sell.LocationId SellLocationId,
	sell.StationName SellStationName,
	sell.RangeName SellRangeName,
	sell.Duration SellDuration,
	sell.Issued SellIssued,
	sell.MinVolume SellMinVolume,
	sell.VolumeRemain SellVolumeRemain,
	sell.Price SellPrice
from ItemTypes_V as a
left join MarketBestBuyPrices_V as buy on buy.TypeId = a.Id
left join MarketBestSellPrices_V as sell on sell.TypeId = a.Id
where a.Group_Category_Name = 'Asteroid'
";
        #endregion

        #region SDE INDEX CREATES
        public static List<string> SEQUENCE_SDE_INDEXES_CREATE = new List<string>()
        {
            CREATE_INDEX_TYPENAME,
            CREATE_INDEX_RACENAME,
            CREATE_INDEX_MARKETGROUPNAME,
            CREATE_INDEX_GROUPNAME,
            CREATE_INDEX_CATEGORYNAME,
            CREATE_INDEX_METAGROUPNAME,
            CREATE_INDEX_ATTRIBUTENAME,
            CREATE_INDEX_ATTRIBUTEDISPLAYNAME,
            CREATE_INDEX_EFFECTNAME,
            CREATE_INDEX_REGIONNAME,
            CREATE_INDEX_CONSTELLATIONNAME,
            CREATE_INDEX_SOLARSYSTEMNAME,
            CREATE_INDEX_STATIONNAME,
            CREATE_INDEX_STATIONSERVICENAME
        };
        public const string CREATE_INDEX_TYPENAME = @"CREATE INDEX IF NOT EXISTS ""ix_invTypes_typeName"" ON invTypes (""typeName"")";
        public const string CREATE_INDEX_RACENAME = @"CREATE INDEX IF NOT EXISTS ""ix_chrRaces_raceName"" ON chrRaces (""raceName"")";
        public const string CREATE_INDEX_MARKETGROUPNAME = @"CREATE INDEX IF NOT EXISTS ""ix_invMarketGroups_marketGroupName"" ON invMarketGroups (""marketGroupName"")";
        public const string CREATE_INDEX_GROUPNAME = @"CREATE INDEX IF NOT EXISTS ""ix_invGroups_groupName"" ON invGroups (""typeNamegroupName"")";
        public const string CREATE_INDEX_CATEGORYNAME = @"CREATE INDEX IF NOT EXISTS ""ix_invCategories_categoryName"" ON invCategories (""categoryName"")";
        public const string CREATE_INDEX_METAGROUPNAME = @"CREATE INDEX IF NOT EXISTS ""ix_invMetaGroups_metaGroupName"" ON invMetaGroups (""metaGroupName"")";
        public const string CREATE_INDEX_ATTRIBUTENAME = @"CREATE INDEX IF NOT EXISTS ""ix_dgmAttributeTypes_attributeName"" ON dgmAttributeTypes (""attributeName"")";
        public const string CREATE_INDEX_ATTRIBUTEDISPLAYNAME = @"CREATE INDEX IF NOT EXISTS ""ix_dgmAttributeTypes_displayName"" ON dgmAttributeTypes (""displayName"")";
        public const string CREATE_INDEX_EFFECTNAME = @"CREATE INDEX IF NOT EXISTS ""ix_dgmEffects_effectName"" ON dgmEffects (""effectName"")";
        public const string CREATE_INDEX_REGIONNAME = @"CREATE INDEX IF NOT EXISTS ""ix_mapRegions_regionName"" ON mapRegions (""regionName"")";
        public const string CREATE_INDEX_CONSTELLATIONNAME = @"CREATE INDEX IF NOT EXISTS ""ix_mapConstellations_constellationName"" ON mapConstellations (""constellationName"")";
        public const string CREATE_INDEX_SOLARSYSTEMNAME = @"CREATE INDEX IF NOT EXISTS ""ix_mapSolarSystems_solarSystemName"" ON mapSolarSystems (""solarSystemName"")";
        public const string CREATE_INDEX_STATIONNAME = @"CREATE INDEX IF NOT EXISTS ""ix_staStations_stationName"" ON staStations (""stationName"")";
        public const string CREATE_INDEX_STATIONSERVICENAME = @"CREATE INDEX IF NOT EXISTS ""ix_staServices_serviceName"" ON staServices (""serviceName"")";
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
        private const int SECONDS_BETWEEN_ACTIONS = 1;
        private const int SECONDS_BETWEEN_REGIONS = 1;
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
            CreateBaseTables();
            CreateBaseIndexes();
            CreateBaseViews();
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

        private void CreateBaseTables()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Creating Base Tables for Database...");
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_CREATE_BASE_TABLES);
            sw.Stop();
            _Log.LogInformation(String.Format("Created Base Tables for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }

        private void CreateBaseIndexes()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Creating Base Indexes for Database...");
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_CREATE_BASE_INDEXES);
            sw.Stop();
            _Log.LogInformation(String.Format("Created Base Indexes for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }

        private void CreateBaseViews()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Creating Base Views for Database...");
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_CREATE_BASE_VIEWS);
            sw.Stop();
            _Log.LogInformation(String.Format("Created Base Views for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
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

            PopulateStatsTables();

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

        private void PopulateStatsTables()
        {
            Stopwatch sw = new Stopwatch();
            
            List<string> sql = new List<string>();
            string deleteSql = @"delete from MarketOpportunities";
            string insertSql = @"
insert into MarketOpportunities
select 
	a.TypeId as TypeId,
	a.Id as BuyId, 
	a.MaxPrice as BuyPrice,
	b.Id as SellId,
	b.MinPrice as SellPrice,
	(a.MaxPrice - b.MinPrice) as PriceDiff
from (
	select s.Id, s.TypeId, max(s.Price) as MaxPrice
	from RegionMarketOrders as s
	where s.IsBuyOrder = 1
	group by s.TypeId
) as a
join (
	select b.Id, b.TypeId, min(b.Price) as MinPrice 
	from RegionMarketOrders as b
	where b.IsBuyOrder = 0
	group by b.TypeId
) as b on b.TypeId = a.TypeId
";
            sql.Add(deleteSql);
            sql.Add(insertSql);
            _Log.LogDebug(String.Format("Beginning to delete rows from MarketOpportunities and re-populate..."));
            _SQLiteService.ExecuteMultiple(sql);
            _Log.LogDebug(String.Format("Finished deleting rows from MarketOpportunities and re-populating."));

            sql = new List<string>();
            deleteSql = @"delete from MarketOpportunitiesDetail";
            insertSql = @"
insert into MarketOpportunitiesDetail
select distinct
	a.TypeId,
	b.Name as TypeName,
    IFNULL(b.MarketGroup_Name, 'None') as MarketGroupName, 
    IFNULL(b.Group_Name, 'None') as GroupName, 
    IFNULL(b.Group_Category_Name, 'None') as GroupCategoryName,
	a.BuyId,
	buy.RegionId as BuyRegionId,
	buyRegion.Name as BuyRegionName,
	buy.OrderId as BuyOrderId,
	buy.SystemId as BuySystemId,
	buySystem.Name as BuySystemName,
	buy.LocationId as BuyLocationId,
	buyStation.Name as BuyStationName,
	case when buy.Range = 'station' then 'Station'
		when buy.Range = 'solarsystem' then 'System'
		when buy.Range = 'region' then 'Region'
		when buy.Range = '1' then '1 Jump'
		else (buy.Range || ' Jumps') end as BuyRange,
	buy.Duration as BuyDuration,
	buy.Issued as BuyIssued,
	buy.MinVolume as BuyMinVolume,
	buy.VolumeRemain as BuyVolumeRemain,
	buy.VolumeTotal as BuyVolumeTotal,
	a.BuyPrice,
	a.SellId,
	sell.RegionId as SellRegionId,
	sellRegion.Name as SellRegionName,
	sell.OrderId as SellOrderId,
	sell.SystemId as SellSystemId,
	sellSystem.Name as SellSystemName,
	sell.LocationId as SellLocationId,
	sellStation.Name as SellStationName,
	case when sell.Range = 'station' then 'Station'
		when sell.Range = 'solarsystem' then 'System'
		when sell.Range = 'region' then 'Region'
		when sell.Range = '1' then '1 Jump'
		else (sell.Range || ' Jumps') end as SellRange,
	sell.Duration as SellDuration,
	sell.Issued as SellIssued,
	sell.MinVolume as SellMinVolume,
	sell.VolumeRemain as SellVolumeRemain,
	sell.VolumeTotal as SellVolumeTotal,
	a.SellPrice,
	a.PriceDiff
from MarketOpportunities as a
join ItemTypes_V as b on b.Id = a.TypeId 
join RegionMarketOrders as buy on buy.Id = a.BuyId 
join Regions_V as buyRegion on buyRegion.Id = buy.RegionId
join SolarSystems_V as buySystem on buySystem.Id = buy.SystemId
join RegionMarketOrders as sell on sell.Id = a.SellId
join Regions_V as sellRegion on sellRegion.Id = sell.RegionId
join SolarSystems_V as sellSystem on sellSystem.Id = sell.SystemId
left join Stations_V as buyStation on buyStation.Id = buy.LocationId 
left join Stations_V as sellStation on sellStation.Id = sell.LocationId 
order by PriceDiff desc
";
            sql.Add(deleteSql);
            sql.Add(insertSql);
            _Log.LogDebug(String.Format("Beginning to delete rows from MarketOpportunitiesDetail and re-populate..."));
            _SQLiteService.ExecuteMultiple(sql);
            _Log.LogDebug(String.Format("Finished deleting rows from MarketOpportunitiesDetail and re-populating."));

            sw.Stop();
            _Log.LogInformation(String.Format("Finished deleting rows from MarketOpportunities,MarketOpportunitiesDetail and re-populated. Entire process took {0} seconds.", Math.Round(sw.Elapsed.TotalSeconds, 2).ToString("##.##")));
        }
        #endregion

        #region SDE
        private bool SDEExists()
        {
            return new FileInfo(_SDEFileName).Exists;
        }

        private void CreateSDEIndexes()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug(String.Format("Preparing to create {0} indexes. Starting...", DBSQL.SEQUENCE_SDE_INDEXES_CREATE.Count));
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_SDE_INDEXES_CREATE);
            sw.Stop();
            _Log.LogInformation(String.Format("Created SDE Indexes for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
        }

        private void CreateSDEViews()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug(String.Format("Will execute {0} SQL scripts. Starting...", DBSQL.SEQUENCE_SDE_VIEWS_DROP.Count + DBSQL.SEQUENCE_SDE_VIEWS_CREATE.Count));
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_SDE_VIEWS_DROP);
            _SQLiteService.ExecuteMultiple(DBSQL.SEQUENCE_SDE_VIEWS_CREATE);
            sw.Stop();
            _Log.LogInformation(String.Format("Created SDE Views for Database. Process took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
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
            CreateSDEIndexes();
            CreateSDEViews();

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
            for (int x = 0; x < 3; x++) // Try three times if it fails
            {
                bool success = false;
                try
                {
                    if (File.Exists(sdePath)) // Existing SDE, delete it
                    {
                        File.Delete(sdePath);
                    }
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
                _Log.LogError(String.Format("SDE at path '{0}' does not exist. Cannot migrate SDE to AuraWeb database.", _SDEFileName));
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

        #region Character
        public Character_Row GetCharacterPublicInfo(int id, bool forceRefresh = false) 
        {
            _Log.LogDebug(String.Format("Getting Character Public Info for Id '{0}'...", id.ToString()));
            string sql = @"select * from Characters where Id = @Id";
            // Look for the public info in the database and check age
            Character_Row character = _SQLiteService.SelectSingle<Character_Row>(sql, new { Id = id });
            if (forceRefresh || character == null || character.LastUpdateDate == null || (DateTime.Now - character.LastUpdateDate).TotalHours > 1) // If character is one hour old, go get again
            {
                _Log.LogDebug(String.Format("Character Public Info for Id '{0}' was either not in database, or was out of date.", id.ToString()));
                // Get character from ESI API
                var characterApi = _ESIClient.Character.GetCharacterPublicInfoV4Async(id).Result;
                var characterApiModel = characterApi.Model;
                // Insert into the database
                _SQLiteService.Execute(DBSQL.INSERT_CHARACTER, new {
                    Id = id,
                    Name = characterApiModel.Name,
                    Description = characterApiModel.Description,
                    Gender = characterApiModel.Gender,
                    BirthDate = characterApiModel.Birthday,
                    SecurityStatus = characterApiModel.SecurityStatus,
                    RaceId = characterApiModel.RaceId,
                    AncestryId = characterApiModel.AncestryId,
                    BloodlineId = characterApiModel.BloodlineId,
                    AllianceId = characterApiModel.AllianceId,
                    CorporationId = characterApiModel.CorporationId,
                    FactionId = characterApiModel.FactionId,
                    LastUpdateDate = DateTime.Now
                });
                _Log.LogDebug(String.Format("Updated Character Public Info for Id '{0}'.", id.ToString()));
                // Re-get the character from the DB and return it
                character = _SQLiteService.SelectSingle<Character_Row>(sql, new { Id = id });
                return character;
            }
            else return character; // Character data is fresh enough, return it
        }
        #endregion

        #region Queries
        // TODO: Deprecate GetTypeNames()
        public List<TypeNameDTO> GetTypeNames()
        {
            _SQLiteService = new SQLiteService(_SDEFileName);
            List<TypeNameDTO> result = new List<TypeNameDTO>();
            string sql = "select typeId Id, typeName Name from invTypes";
            result = _SQLiteService.SelectMultiple<TypeNameDTO>(sql);
            // The below will not work, since there is a limit on parameters
            //string sql = "select typeId Id, typeName Name from invTypes where typeId in @typeIds";
            //result = _SQLiteService.SelectMultiple<TypeNameDTO>(sql, new { typeIds = typeIds });
            return result;
        }

        public List<T> Search<T>(string sql, string query)
        {
            query = query.Trim();
            List<T> result = new List<T>();
            string id = query; // For Id searches
            query = String.Format("%{0}%", query); // Format query for LIKE operator
            result = _SQLiteService.SelectMultiple<T>(sql, new { id = id, query = query });
            return result;
        }

        // For getting by primary id
        public T GetById<T>(string sql, int id)
        {
            return _SQLiteService.SelectSingle<T>(sql, new { id = id });
        }

        // For getting by foreign id
        public List<T> GetMultipleById<T>(string sql, int id)
        {
            return _SQLiteService.SelectMultiple<T>(sql, new { id = id });
        }

        // For getting my primary ids
        public List<T> GetByMultipleIds<T>(string sql, List<int> ids)
        {
            return _SQLiteService.SelectMultiple<T>(sql, new { ids = ids });
        }

        // For getting my primary ids (with long data type)
        public List<T> GetByMultipleIdsLong<T>(string sql, List<long> ids)
        {
            return _SQLiteService.SelectMultiple<T>(sql, new { ids = ids });
        }

        // For getting all rows (no parameters)
        public List<T> GetMultiple<T>(string sql, bool useSlapper = true)
        {
            return _SQLiteService.SelectMultiple<T>(sql, null, useSlapper);
        }

        #region Universe
        #region Regions
        public List<Region_V_Row> SearchRegions(string query)
        {
            string sql = @"
select * from Regions_V where 
    Name like @query 
    or Id like @query
    or FactionName like @query
order by Name
;";
            return Search<Region_V_Row>(sql, query);
        }

        public Region_V_Row GetRegion(int id)
        {
            string sql = @"
select * from Regions_V where id = @id
;";
            return GetById<Region_V_Row>(sql, id);
        }

        public List<Region_V_Row> GetRegions(List<int> ids)
        {
            string sql = @"
select * from Regions_V where id in @ids
;";
            return GetByMultipleIds<Region_V_Row>(sql, ids);
        }

        public List<Region_V_Row> GetRegions(string factionName, string name)
        {
            if (name != null) name = String.Format("%{0}%", name);
            string sql = @"
select * from Regions_V where 1=1
    and IFNULL(FactionName, 'None') = IFNULL(@factionName, IFNULL(FactionName, 'None'))
    and (@name IS NULL OR Name like @name)
order by Name
;";
            return _SQLiteService.SelectMultiple<Region_V_Row>(sql, new { factionName = factionName, name = name });
        }

        public List<Region_V_Row> GetAllRegions()
        {
            string sql = @"select * from Regions_V";
            return GetMultiple<Region_V_Row>(sql);
        }

        public List<string> GetRegionFactions()
        {
            string sql = @"
select distinct FactionName from Regions_V order by FactionName
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetAllRegionNames()
        {
            string sql = @"
select distinct Name from Regions_V order by Name
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }
        #endregion

        #region Constellations
        public List<Constellation_V_Row> SearchConstellations(string query)
        {
            string sql = @"
select * from Constellations_V where 
    Name like @query 
    or Id like @query
    or RegionName like @query
    or FactionName like @query
order by Name
;";
            return Search<Constellation_V_Row>(sql, query);
        }

        public Constellation_V_Row GetConstellation(int id)
        {
            string sql = @"
select * from Constellations_V where id = @id
;";
            return GetById<Constellation_V_Row>(sql, id);
        }

        public List<Constellation_V_Row> GetConstellations(List<int> ids)
        {
            string sql = @"
select * from Constellations_V where id in @ids
;";
            return GetByMultipleIds<Constellation_V_Row>(sql, ids);
        }

        public List<Constellation_V_Row> GetConstellations(string regionName, string factionName, string name)
        {
            if (name != null) name = String.Format("%{0}%", name);
            string sql = @"
select * from Constellations_V where 1=1
    and IFNULL(RegionName, 'None') = IFNULL(@regionName, IFNULL(RegionName, 'None'))
    and IFNULL(FactionName, 'None') = IFNULL(@factionName, IFNULL(FactionName, 'None'))
    and (@name IS NULL OR Name like @name)
order by Name
;";
            return _SQLiteService.SelectMultiple<Constellation_V_Row>(sql, new { regionName = regionName, factionName = factionName, name = name });
        }

        public List<Constellation_V_Row> GetConstellationsForRegion(int id)
        {
            string sql = @"
select * from Constellations_V where RegionId = @id
;";
            return GetMultipleById<Constellation_V_Row>(sql, id);
        }

        public List<Constellation_V_Row> GetAllConstellations()
        {
            string sql = @"select * from Constellations_V";
            return GetMultiple<Constellation_V_Row>(sql);
        }

        public List<string> GetConstellationFactionNames()
        {
            string sql = @"
select distinct FactionName from Constellations_V order by FactionName
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetAllConstellationNames()
        {
            string sql = @"
select distinct Name from Constellations_V order by Name
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }
        #endregion

        #region Solar Systems
        public List<SolarSystem_V_Row> SearchSolarSystems(string query)
        {
            string sql = @"
select * from SolarSystems_V where 
    Name like @query 
    or Id like @query
    or RegionName like @query
    or ConstellationName like @query
    or FactionName like @query
order by Name
;";
            return Search<SolarSystem_V_Row>(sql, query);
        }

        public SolarSystem_V_Row GetSolarSystem(int id)
        {
            string sql = @"
select * from SolarSystems_V where id = @id
;";
            return GetById<SolarSystem_V_Row>(sql, id);
        }

        public List<SolarSystem_V_Row> GetSolarSystems(List<int> ids)
        {
            string sql = @"
select * from SolarSystems_V where id in @ids
;";
            return GetByMultipleIds<SolarSystem_V_Row>(sql, ids);
        }

        public List<SolarSystem_V_Row> GetSolarSystemsLong(List<long> ids)
        {
            string sql = @"
select * from SolarSystems_V where id in @ids
;";
            return _SQLiteService.SelectMultiple<SolarSystem_V_Row>(sql, new { ids = ids });
        }

        public List<SolarSystem_V_Row> GetSolarSystems(string regionName, string constellationName, string factionName, string securityClass, string name)
        {
            if (name != null) name = String.Format("%{0}%", name);
            string sql = @"
select * from SolarSystems_V where 1=1
    and IFNULL(RegionName, 'None') = IFNULL(@regionName, IFNULL(RegionName, 'None'))
    and IFNULL(ConstellationName, 'None') = IFNULL(@constellationName, IFNULL(ConstellationName, 'None'))
    and IFNULL(FactionName, 'None') = IFNULL(@factionName, IFNULL(FactionName, 'None'))
    and IFNULL(Security_Class, 'None') = IFNULL(@securityClass, IFNULL(Security_Class, 'None'))
    and (@name IS NULL OR Name like @name)
order by Name
;";
            return _SQLiteService.SelectMultiple<SolarSystem_V_Row>(sql, new { regionName = regionName, constellationName = constellationName, factionName = factionName, securityClass = securityClass, name = name });
        }

        public List<SolarSystem_V_Row> GetSolarSystemsForConstellation(int id)
        {
            string sql = @"
select * from SolarSystems_V where ConstellationId = @id
;";
            return GetMultipleById<SolarSystem_V_Row>(sql, id);
        }

        public List<SolarSystem_V_Row> GetAllSolarSystems()
        {
            string sql = @"select * from SolarSystems_V";
            return GetMultiple<SolarSystem_V_Row>(sql);
        }

        public List<string> GetSolarSystemFactionNames()
        {
            string sql = @"
select distinct FactionName from SolarSystems_V order by FactionName
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetSolarSystemSecurityClasses()
        {
            string sql = @"
select distinct Security_Class from SolarSystems_V order by Security_Class
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetSolarSystemNames()
        {
            string sql = @"
select distinct Name from SolarSystems_V order by Name
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }
        #endregion

        #region Stations
        public List<Station_V_Row> SearchStations(string query)
        {
            string sql = @"
select * from Stations_V where 
    Name like @query  
    or Id like @query
    or SolarSystemName like @query
    or ConstellationName like @query
    or RegionName like @query
    or OperationName like @query
order by Name
;";
            return Search<Station_V_Row>(sql, query);
        }

        public Station_V_Row GetStation(int id)
        {
            string sql = @"
select * from Stations_V where id = @id
;";
            return GetById<Station_V_Row>(sql, id);
        }

        public List<Station_V_Row> GetStations(List<int> ids)
        {
            string sql = @"
select * from Stations_V where id in @ids
;";
            return GetByMultipleIds<Station_V_Row>(sql, ids);
        }

        public List<Station_V_Row> GetStationsLong(List<long> ids)
        {
            string sql = @"
select * from Stations_V where id in @ids
;";
            return GetByMultipleIdsLong<Station_V_Row>(sql, ids);
        }

        public List<Station_V_Row> GetStations(string regionName, string constellationName, string solarSystemName, string operationName, string name)
        {
            if (name != null) name = String.Format("%{0}%", name);
            string sql = @"
select * from Stations_V where 1=1
    and IFNULL(RegionName, 'None') = IFNULL(@regionName, IFNULL(RegionName, 'None'))
    and IFNULL(ConstellationName, 'None') = IFNULL(@constellationName, IFNULL(ConstellationName, 'None'))
    and IFNULL(SolarSystemName, 'None') = IFNULL(@solarSystemName, IFNULL(SolarSystemName, 'None'))
    and IFNULL(OperationName, 'None') = IFNULL(@operationName, IFNULL(OperationName, 'None'))
    and (@name IS NULL OR Name like @name)
order by Name
;";
            return _SQLiteService.SelectMultiple<Station_V_Row>(sql, new { regionName = regionName, constellationName = constellationName, solarSystemName = solarSystemName, operationName = operationName, name = name });
        }

        public List<Station_V_Row> GetStationsForSolarSystem(int id)
        {
            string sql = @"
select * from Stations_V where SolarSystemId = @id
;";
            return GetMultipleById<Station_V_Row>(sql, id);
        }

        public List<Station_V_Row> GetAllStations()
        {
            string sql = @"select * from Stations_V";
            return GetMultiple<Station_V_Row>(sql);
        }

        public List<string> GetStationSecurityClasses()
        {
            string sql = @"
select distinct Security_Class from Stations_V order by Security_Class
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetStationOperationNames()
        {
            string sql = @"
select distinct OperationName from Stations_V order by OperationName
";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }
        #endregion
        #endregion

        #region Item Types
        public List<ItemType_V_Row> SearchItemTypes(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public ItemType_V_Row GetItemType(int id)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and id = @id
order by Attributes_Category_Name asc, Attributes_Id asc, Effects_Id asc
;";
            return GetById<ItemType_V_Row>(sql, id);
        }

        public List<ItemType_V_Row> GetItemTypes(List<int> ids)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and id in @ids
;";
            return GetByMultipleIds<ItemType_V_Row>(sql, ids);
        }

        public List<ItemType_V_Row> GetItemTypes(string raceName, string marketGroupName, string groupName, string groupCategoryName, string metaGroupName, string name)
        {
            if (name != null) name = String.Format("%{0}%", name);
            string sql = @"
select * from ItemTypes_V where 1=1
    and IFNULL(Race_Name, 'None') = IFNULL(@raceName, IFNULL(Race_Name, 'None'))
    and IFNULL(MarketGroup_Name, 'None') = IFNULL(@marketGroupName, IFNULL(MarketGroup_Name, 'None'))
    and IFNULL(Group_Name, 'None') = IFNULL(@groupName, IFNULL(Group_Name, 'None'))
    and IFNULL(Group_Category_Name, 'None') = IFNULL(@groupCategoryName, IFNULL(Group_Category_Name, 'None'))
    and IFNULL(Meta_Group_Name, 'None') = IFNULL(@metaGroupName, IFNULL(Meta_Group_Name, 'None'))
    and (@name IS NULL OR Name like @name)
order by Name
";
            return _SQLiteService.SelectMultiple<ItemType_V_Row>(sql, new { raceName = raceName, marketGroupName = marketGroupName, groupName = groupName, groupCategoryName = groupCategoryName, metaGroupName = metaGroupName, name = name });
        }

        public List<ItemType_V_Row> GetAllItemTypes()
        {
            string sql = @"select * from ItemTypes_V";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<string> GetItemTypeRaceNames()
        {
            string sql = @"
select distinct Race_Name from ItemTypes_V order by Race_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetItemTypeMarketGroupNames()
        {
            string sql = @"
select distinct MarketGroup_Name from ItemTypes_V order by MarketGroup_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetItemTypeGroupNames()
        {
            string sql = @"
select distinct Group_Name from ItemTypes_V order by Group_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetItemTypeGroupCategoryNames()
        {
            string sql = @"
select distinct Group_Category_Name from ItemTypes_V order by Group_Category_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetItemTypeMetaGroupNames()
        {
            string sql = @"
select distinct Meta_Group_Name from ItemTypes_V order by Meta_Group_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        #region Ships
        public List<ItemType_V_Row> SearchShips(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Ship'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public List<ItemType_V_Row> GetAllShips()
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Ship'
order by Name";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<string> GetAllShipGroups()
        {
            string sql = @"
select distinct Group_Name from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Ship' 
order by Group_Name";
            return GetMultiple<string>(sql, false);
        }

        public List<string> GetAllShipRaces()
        {
            string sql = @"
select distinct Race_Name from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Ship'
order by Group_Name";
            return GetMultiple<string>(sql, false);
        }

        public List<ItemType_V_Row> GetAllShipsForGroupRaceAndName(string name, string raceName, string groupName)
        {
            if (groupName == null && raceName == null && name == null) return new List<ItemType_V_Row>(); // If all are null, just return an empty list
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Ship'
    and IFNULL(Group_Name, 'None') = IFNULL(@groupName, IFNULL(Group_Name, 'None'))
    and IFNULL(Race_Name, 'None') = IFNULL(@raceName, IFNULL(Race_Name, 'None'))
    and (@name IS NULL OR @name = '') OR Name like @name
order by Name";
            return _SQLiteService.SelectMultiple<ItemType_V_Row>(sql, new { groupName = groupName, raceName = raceName, name = name });
        }
        #endregion

        #region Modules
        public List<ItemType_V_Row> SearchModules(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Module'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public List<ItemType_V_Row> SearchHighPowerModules(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Module'
    and Effects_Name = 'hiPower'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public List<ItemType_V_Row> SearchMediumPowerModules(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Module'
    and Effects_Name = 'medPower'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public List<ItemType_V_Row> SearchLowPowerModules(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Module'
    and Effects_Name = 'lowPower'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public List<ItemType_V_Row> SearchRigModules(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Module'
    and Effects_Name = 'rigSlot'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public List<ItemType_V_Row> GetAllModules()
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Module'
order by Name";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<ItemType_V_Row> GetAllHighPowerModules()
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Module'
    and Effects_Name = 'hiPower'
order by Name";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<ItemType_V_Row> GetAllMediumPowerModules()
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Module'
    and Effects_Name = 'medPower'
order by Name";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<ItemType_V_Row> GetAllLowPowerModules()
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Module'
    and Effects_Name = 'lowPower'
order by Name";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<ItemType_V_Row> GetAllRigModules()
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Module'
    and Effects_Name = 'rigSlot'
order by Name";
            return GetMultiple<ItemType_V_Row>(sql);
        }
        #endregion

        #region Ore
        public List<ItemType_V_Row> SearchOre(string query)
        {
            string sql = @"
select * from ItemTypes_V where 1=1
    and Group_Category_Name = 'Asteroid'
    and (
        Name like @query  
        or Id like @query
    )
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public ItemType_V_Row GetOre(int id)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Asteroid'
    and id = @id
order by Attributes_Category_Name asc, Attributes_Id asc, Effects_Id asc
;";
            return GetById<ItemType_V_Row>(sql, id);
        }

        public List<ItemType_V_Row> GetOres(List<int> ids)
        {
            string sql = @"
select * from ItemTypes_V where 1=1 
    and Group_Category_Name = 'Asteroid'
    and id in @ids
;";
            return GetByMultipleIds<ItemType_V_Row>(sql, ids);
        }

        public List<ItemType_V_Row> GetAllOre()
        {
            string sql = @"select * from ItemTypes_V where Group_Category_Name = 'Asteroid'";
            return GetMultiple<ItemType_V_Row>(sql);
        }

        public List<Ore_V_Row> GetOres(string marketGroup = null, string group = null, string name = null)
        {
            string sql = @"
select * from Ores_V 
where 1=1
    and IFNULL(MarketGroup_Name, 'None') = IFNULL(@marketGroup, IFNULL(MarketGroup_Name, 'None'))
    and IFNULL(Group_Name, 'None') = IFNULL(@group, IFNULL(Group_Name, 'None'))
    and (@name IS NULL OR @name = '') OR Name like @name
";
            return _SQLiteService.SelectMultiple<Ore_V_Row>(sql, new { marketGroup = marketGroup, group = group, name = name });
        }

        public List<string> GetOreMarketGroups()
        {
            string sql = @"select distinct MarketGroup_Name from ItemTypes_V where Group_Category_Name = 'Asteroid' order by MarketGroup_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }

        public List<string> GetOreGroups()
        {
            string sql = @"select distinct Group_Name from ItemTypes_V where Group_Category_Name = 'Asteroid' order by Group_Name";
            return _SQLiteService.SelectMultiple<string>(sql, null, false);
        }
        #endregion
        #endregion

        #region Skills
        public List<Skill_V_Row> SearchSkills(string query)
        {
            string sql = @"
select * from Skills_V where 1=1
    and (
        Cert_Name like @query  
        or Id like @query
    )
order by Cert_Name, SkillLevelInt
;";
            return Search<Skill_V_Row>(sql, query);
        }

        public Skill_V_Row GetSkill(int id)
        {
            string sql = @"
select * from Skills_V where 1=1
    and Id = @id
;";
            return GetById<Skill_V_Row>(sql, id);
        }

        public List<Skill_V_Row> GetSkills(List<int> ids)
        {
            string sql = @"
select * from Skills_V where 1=1 
    and Id in @ids
;";
            return GetByMultipleIds<Skill_V_Row>(sql, ids);
        }

        public List<Skill_V_Row> GetAllSkills()
        {
            string sql = @"select * from Skills_V";
            return GetMultiple<Skill_V_Row>(sql);
        }

        public Skill_V_Row GetSkillForIdAndSkillLevel(int id, int skillLevel)
        {
            string sql = @"
select * from Skills_V where 1=1 
    and Id = @id and SkillLevelInt = @skilllevel
;";
            return _SQLiteService.SelectSingle<Skill_V_Row>(sql, new { id = id, skilllevel = skillLevel });
        }
        #endregion

        #region Market
        public MarketAveragePrices_Row GetAveragePriceForTypeId(int id)
        {
            const string sql = @"
select a.TimeStamp, a.TypeId, a.AdjustedPrice, a.AveragePrice
from MarketAveragePrices as a
join (
	select max(""Timestamp"") as ""Timestamp"", TypeId
    from MarketAveragePrices
    where TypeId = @id
    group by TypeId
) as b on b.""Timestamp"" = a.""Timestamp""
    and b.TypeId = a.TypeId
where a.TypeId = @id
";
            MarketAveragePrices_Row result = _SQLiteService.SelectSingle<MarketAveragePrices_Row>(sql, new { id = id });
            return result;
        }

        public List<MarketAveragePrices_Row> GetAveragePrices()
        {
            const string sql = @"
select a.TimeStamp, a.TypeId, a.AdjustedPrice, a.AveragePrice
from MarketAveragePrices as a
join (
	select max(""Timestamp"") as ""Timestamp"", TypeId
    from MarketAveragePrices
    group by TypeId
) as b on b.""Timestamp"" = a.""Timestamp""
    and b.TypeId = a.TypeId
order by a.TypeId
";
            List<MarketAveragePrices_Row> result = new List<MarketAveragePrices_Row>();
            result = _SQLiteService.SelectMultiple<MarketAveragePrices_Row>(sql);
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestSellPrices()
        {
            const string sql = @"
select *
from RegionMarketOrders
where rowid in (
	select rowid from (
		select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
		where IsBuyOrder = 1
		group by TypeId
	)
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql);
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestSellPricesForTypeId(int typeid, int limit = 20)
        {
            const string sql = @"
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 1 and TypeId = @typeid
	order by Price desc
	limit @limit
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql, new { typeid = typeid, limit = limit });
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestBuyPrices()
        {
            const string sql = @"
select *
from RegionMarketOrders
where rowid in (
	select rowid from (
		select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
		where IsBuyOrder = 0
		group by TypeId
	)
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql);
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestBuyPricesForTypeId(int typeid, int limit = 20)
        {
            const string sql = @"
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 0 and TypeId = @typeid
	order by Price desc
	limit @limit
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql, new { typeid = typeid, limit = limit });
            return result;
        }

        public List<MarketOpportunitiesDetail_Row> GetMarketOpportunities(int priceDiffAbove = 1000000, string marketGroupName = null, string groupName = null, string groupCategoryName = null) // Diff of > 1 mil by default
        {
            const string sql = @"
select * from MarketOpportunitiesDetail 
where 1=1 
    and PriceDiff >= @priceDiffAbove
    and MarketGroupName = IFNULL(@marketGroupName, MarketGroupName)
    and GroupName = IFNULL(@groupName, GroupName)
    and GroupCategoryName = IFNULL(@groupCategoryName, GroupCategoryName)
"; 
            List<MarketOpportunitiesDetail_Row> result = new List<MarketOpportunitiesDetail_Row>();
            result = _SQLiteService.SelectMultiple<MarketOpportunitiesDetail_Row>(sql, new { priceDiffAbove = priceDiffAbove, marketGroupName = marketGroupName, groupName = groupName, groupCategoryName = groupCategoryName });
            return result;
        }

        public List<string> GetMarketOpportunityMarketGroups()
        {
            const string sql = @"select distinct MarketGroupName from MarketOpportunitiesDetail order by MarketGroupName";
            List<string> result = new List<string>();
            result = _SQLiteService.SelectMultiple<string>(sql, null, false);
            return result;
        }

        public List<string> GetMarketOpportunityGroups()
        {
            const string sql = @"select distinct GroupName from MarketOpportunitiesDetail order by GroupName";
            List<string> result = new List<string>();
            result = _SQLiteService.SelectMultiple<string>(sql, null, false);
            return result;
        }

        public List<string> GetMarketOpportunityGroupCategories()
        {
            const string sql = @"select distinct GroupCategoryName from MarketOpportunitiesDetail order by GroupCategoryName";
            List<string> result = new List<string>();
            result = _SQLiteService.SelectMultiple<string>(sql, null, false);
            return result;
        }
        #endregion
        #endregion
    }
}
