select * from RegionMarketTypeIds

select * from RegionMarketOrders

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
	where IsBuyOrder = 1 and TypeId = 9678
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
	where IsBuyOrder = 0 and TypeId = 9678
	order by price desc
	limit 20
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc