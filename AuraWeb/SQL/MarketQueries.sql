select * from RegionMarketTypeIds;
select * from RegionMarketOrders;
select * from MarketAveragePrices;

-- Latest Market Average Prices
select a.TimeStamp, a.TypeId, a.AdjustedPrice, a.AveragePrice
from MarketAveragePrices as a
join (
	select max("Timestamp") as "Timestamp", TypeId
	from MarketAveragePrices
	group by TypeId
) as b on b."Timestamp" = a."Timestamp"
	and b.TypeId = a.TypeId

-- Best Sell Price for All Items
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

-- Best 20 Sell Prices (Specific Item)
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 1 and TypeId = 627
	order by price desc
	limit 20
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc

-- Best Buy Price for All Items
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

-- Best 20 Buy Prices (Specific Item)
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 0 and TypeId = 627
	order by price desc
	limit 20
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc