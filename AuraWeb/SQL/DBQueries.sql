/*
 * MARKET
 */
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 1 and TypeId = 627
	order by price desc
	limit 20
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc