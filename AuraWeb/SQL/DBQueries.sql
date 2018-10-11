/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;


select distinct 
	a.Id,
	a.Name,
	b.*,
	c.*,
	d.*
from ItemTypes_V as a 
left join MarketAveragesRecent_V as b on b.TypeId = a.Id
left join MarketBestBuyPrices_V as c on c.TypeId = a.Id
left join MarketBestSellPrices_V as d on d.TypeId = a.Id

where a.Id = 606