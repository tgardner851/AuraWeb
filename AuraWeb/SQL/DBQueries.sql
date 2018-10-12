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


explain query plan
select distinct 
	a.typeID,
	b.AveragePrice,
	b.AdjustedPrice,
	b.LastUpdated AveragesLastUpdated,
	c.OrderId BestBuyOrderId,
	c.RegionId BestBuyRegionId,
	--c.RegionName BestBuyRegionName,
	c.SystemId BestBuySystemId,
	--c.SystemName BestBuySystemName,
	c.LocationId BestBuyLocationId,
	--c.StationName BestBuyStationName,
	--c.RangeName BestBuyRange,
	c.Duration BestBuyDuration,
	c.Issued BestBuyIssued,
	c.MinVolume BestBuyMinVolume,
	c.VolumeRemain BestBuyVolumeRemain,
	c.Price BestBuyPrice,
	d.OrderId BestSellOrderId,
	d.RegionId BestSellRegionId,
	--d.RegionName BestSellRegionName,
	d.SystemId BestSellSystemId,
	--d.SystemName BestSellSystemName,
	d.LocationId BestSellLocationId,
	--d.StationName BestSellStationName,
	--d.RangeName BestSellRange,
	d.Duration BestSellDuration,
	d.Issued BestSellIssued,
	d.MinVolume BestSellMinVolume,
	d.VolumeRemain BestSellVolumeRemain,
	d.Price BestSellPrice,
	DateTime('now') LastUpdatedDate
from invTypes as a 
left join (
	select 
		a.TypeId,
		a.AveragePrice,
		a.AdjustedPrice, 
		a.TimeStamp as LastUpdated
	from MarketAveragePrices as a
	join (
		select max("Timestamp") as "Timestamp", TypeId
		from MarketAveragePrices
		group by TypeId
	) as b on b."Timestamp" = a."Timestamp"
		and b.TypeId = a.TypeId
) as b on b.TypeId = a.typeID
left join (
	select 
		TypeId,
		OrderId,
		RegionId,
		SystemId,
		LocationId,
		Range,
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
) as c on c.TypeId = a.typeID
left join (
	select 
		TypeId,
		OrderId,
		RegionId,
		SystemId,
		LocationId,
		Range,
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
) as d on d.TypeId = a.typeID
where a.typeID = 606;





select 
	a.*
from RegionMarketOrders as a
where a.Id in (
	select Id from RegionMarketOrders 
	where IsBuyOrder = 1 and TypeId = a.TypeId
	order by Price desc 
	limit 10
)
union all 
select 
	a.*
from RegionMarketOrders as a
where a.Id in (
	select Id from RegionMarketOrders 
	where IsBuyOrder = 0 and TypeId = a.TypeId
	order by Price desc 
	limit 10
)



select rowid, * from tmp_buy union all select rowid, * from tmp_sell;