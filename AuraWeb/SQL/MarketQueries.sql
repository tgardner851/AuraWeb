select * from RegionMarketTypeIds;
select * from RegionMarketOrders;
select * from MarketAveragePrices;

-- Latest Market Average Prices
select 
	a.TypeId,
	(select distinct Name from ItemTypes_V where Id = a.TypeId) as TypeName,
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

-- Best Sell Price for All Items
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

-- Best Buy Price for All Items
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

-- Best 20 Sell Prices (Specific Item)
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
	select rowid from RegionMarketOrders
	where IsBuyOrder = 1 and TypeId = 627
	order by price desc
	limit 20
)
order by TypeId asc, Price desc, RegionId asc, SystemId asc, LocationId asc

-- Best Buy Price for All Items
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

-- Best 20 Buy Prices (Specific Item)
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
	select rowid from RegionMarketOrders
	where IsBuyOrder = 0 and TypeId = 627
	order by price desc
	limit 20
)
order by TypeId asc, Price desc, RegionId asc, SystemId asc, LocationId asc