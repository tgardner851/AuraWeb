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


select distinct maxbuy.*, maxsell.*, item.Id, item.Name
from ItemTypes_V as item 
left join (
	select *
	from RegionMarketOrders
	where rowid in (
		select rowid from (
			select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
			where IsBuyOrder = 1 and TypeId = 37451
			group by TypeId
		)
	)
) as maxbuy on maxbuy.TypeId = item.Id
left join (
	select *
	from RegionMarketOrders
	where rowid in (
		select rowid from (
			select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
			where IsBuyOrder = 0 and TypeId = 37451
			group by TypeId
		)
	)
) as maxsell on maxsell.TypeId = item.Id
where item.Id = 37451;
--item.Name like ('%Mining Laser II%'); --37451


select *
from RegionMarketOrders
where rowid in (
	select rowid from (
		select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
		where IsBuyOrder = 1 and TypeId = 37451
		group by TypeId
	)
);

select *
from RegionMarketOrders
where rowid in (
	select rowid from (
		select rowid, TypeId, MAX(Price) AS Price from RegionMarketOrders
		where IsBuyOrder = 0 and TypeId = 37451
		group by TypeId
	)
)