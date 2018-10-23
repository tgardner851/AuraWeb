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
	recentAverages.LastUpdated,
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
where type.Id = 606
;






/*
 * CHARACTER
 */
select * from Characters;