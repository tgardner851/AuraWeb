/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;
select * from MarketOpportunities;
select * from MarketOpportunitiesDetail;


select * from Characters;


select * from invTypes order by typeID; 


select 
	buy.TypeId as TypeId,
	buy.Id as BuyId, 
	buy.Price as BuyPrice,
	sell.Id as SellId,
	sell.Price as SellPrice,
	(sell.Price - buy.Price) as PriceDiff
from (
	select s.Id, s.TypeId, min(s.Price) as Price
	from RegionMarketOrders as s
	where s.IsBuyOrder = 0
	group by s.TypeId
) as buy
join (
	select b.Id, b.TypeId, max(b.Price) as Price 
	from RegionMarketOrders as b
	where b.IsBuyOrder = 1
	group by b.TypeId
) as sell on sell.TypeId = buy.TypeId
where sell.Price > buy.Price