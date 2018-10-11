/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;

create table if not exists MarketOpportunities (
	Id int primary key,
	Name varchar,
	GroupName varchar,
	GroupCategoryName varchar,
	MarketGroupName varchar,
	MarketGroupDescription varchar,
	MarketGroupIconFile varchar,
	AveragePrice number,
	AdjustedPrice number,
	AveragesLastUpdated datetime,
	BestBuyOrderId int,
	BestBuyRegionId int,
	BestBuyRegionName varchar,
	BestBuySystemId int,
	BestBuySystemName varchar,
	BestBuyLocationId int,
	BestBuyStationName varchar,
	BestBuyRange varchar,
	BestBuyDuration number,
	BestBuyIssued datetime,
	BestBuyMinVolume number,
	BestBuyVolumeRemain number,
	BestBuyPrice number,
	BestSellOrderId int,
	BestSellRegionId int,
	BestSellRegionName varchar,
	BestSellSystemId int,
	BestSellSystemName varchar,
	BestSellLocationId int,
	BestSellStationName varchar,
	BestSellRange varchar,
	BestSellDuration number,
	BestSellIssued datetime,
	BestSellMinVolume number,
	BestSellVolumeRemain number,
	BestSellPrice number
)


select distinct 
	a.Id,
	a.Name,
	a.Group_Name GroupName,
	a.Group_Category_Name GroupCategoryName,
	a.MarketGroup_Name MarketGroupName,
	a.MarketGroup_Description MarketGroupDescription,
	a.MarketGroup_Icon_File MarketGroupIconFile,
	b.AveragePrice,
	b.AdjustedPrice,
	b.LastUpdated AveragesLastUpdated,
	c.OrderId BestBuyOrderId,
	c.RegionId BestBuyRegionId,
	c.RegionName BestBuyRegionName,
	c.SystemId BestBuySystemId,
	c.SystemName BestBuySystemName,
	c.LocationId BestBuyLocationId,
	c.StationName BestBuyStationName,
	c.RangeName BestBuyRange,
	c.Duration BestBuyDuration,
	c.Issued BestBuyIssued,
	c.MinVolume BestBuyMinVolume,
	c.VolumeRemain BestBuyVolumeRemain,
	c.Price BestBuyPrice,
	d.OrderId BestSellOrderId,
	d.RegionId BestSellRegionId,
	d.RegionName BestSellRegionName,
	d.SystemId BestSellSystemId,
	d.SystemName BestSellSystemName,
	d.LocationId BestSellLocationId,
	d.StationName BestSellStationName,
	d.RangeName BestSellRange,
	d.Duration BestSellDuration,
	d.Issued BestSellIssued,
	d.MinVolume BestSellMinVolume,
	d.VolumeRemain BestSellVolumeRemain,
	d.Price BestSellPrice,
	DateTime('now') LastUpdatedDate
from ItemTypes_V as a 
left join MarketAveragesRecent_V as b on b.TypeId = a.Id
left join MarketBestBuyPrices_V as c on c.TypeId = a.Id
left join MarketBestSellPrices_V as d on d.TypeId = a.Id
where a.Id = 606