/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;
select * from MarketOpportunitiesDetail;



select 
	TypeId, TypeName, MarketGroupName, GroupName, GroupCategoryName,
	BuyId,
	BuyRegionId,
	BuyRegionName
from MarketOpportunitiesDetail where PriceDiff > 10000000


select Id, Name, MarketGroup_Name, Group_Name, Group_Category_Name from ItemTypes_V;

MarketGroupName varchar, GroupName varchar, Group_Category_Name varchar,

select * from MarketOpportunitiesDetail