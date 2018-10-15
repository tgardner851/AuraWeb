/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;
select * from MarketOpportunitiesDetail;


select
	a.*,
	buy.OrderId BuyOrderId,
	buy.RegionId BuyRegionId,
	buy.RegionName BuyRegionName,
	buy.SystemId BuySystemId,
	buy.SystemName BuySystemName,
	buy.LocationId BuyLocationId,
	buy.StationName BuyStationName,
	buy.RangeName BuyRangeName,
	buy.Duration BuyDuration,
	buy.Issued BuyIssued,
	buy.MinVolume BuyMinVolume,
	buy.VolumeRemain BuyVolumeRemain,
	buy.Price BuyPrice,
	sell.OrderId SellOrderId,
	sell.RegionId SellRegionId,
	sell.RegionName SellRegionName,
	sell.SystemId SellSystemId,
	sell.SystemName SellSystemName,
	sell.LocationId SellLocationId,
	sell.StationName SellStationName,
	sell.RangeName SellRangeName,
	sell.Duration SellDuration,
	sell.Issued SellIssued,
	sell.MinVolume SellMinVolume,
	sell.VolumeRemain SellVolumeRemain,
	sell.Price SellPrice
from ItemTypes_V as a
left join MarketBestBuyPrices_V as buy on buy.TypeId = a.Id
left join MarketBestSellPrices_V as sell on sell.TypeId = a.Id
where a.Group_Category_Name = 'Asteroid'


select distinct Group_Category_Name from ItemTypes_V where Group_Category_Name = 'Asteroid' order by Group_Name
;