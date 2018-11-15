/*
 * MARKET
 */
select * from MarketAveragesRecent_V;
select * from MarketBestBuyPrices_V;
select * from MarketBestSellPrices_V;
select * from MarketOpportunities;
select * from MarketOpportunitiesDetail;
select * from MarketStats;
select * from MarketStatsDetails;

/*
 * CHARACTER
 */
select * from Characters;


--select * from ItemTypes_V where Id = 38661
--37450
--=38660

select * from industryActivity
where typeID = 38660


select * from industryActivityProducts
where productTypeID = 37450

select typeID, activityID, productTypeID, COUNT(*)
FROM industryActivityProducts


select * from industryActivityMaterials
where typeID = 38660

select * from ramActivities


select * from industryActivityRaces
where typeID = 38660

select * from Skills_V



select distinct
	item.Id as Id,
	item.Name as Name,
	product.typeID as BlueprintId,
	productItem.Name as BlueprintName,
	product.activityID as ActivityId,
	productActivity.activityName as ActivityName,
	productActivity.iconNo as ActivityIconNo,
	productActivity.description as ActivityDescription,
	product.quantity as BlueprintQuantity,
	blueprint.maxProductionLimit as BlueprintProductionLimit,
	times.time as BlueprintTime,
	materials.materialTypeID as MaterialId,
	materialItem.Name as MaterialName,
	materials.quantity as MaterialQuantity,
	skillVal.*
from ItemTypes_V as item 
left join industryActivityProducts as product on product.productTypeID = item.Id
left join ramActivities as productActivity on productActivity.activityID = product.activityID
	and productActivity.published = 1
left join ItemTypes_V as productItem on productItem.Id = product.typeID
left join industryBlueprints as blueprint on blueprint.typeID = productItem.Id
left join industryActivity as times on times.typeID = product.typeID 
	and times.activityID = product.activityID
left join industryActivityMaterials as materials on materials.typeID = product.typeID 
	and materials.activityID = product.activityID
left join ItemTypes_V as materialItem on materialItem.Id = materials.materialTypeID
left join industryActivitySkills as skills on skills.typeID = product.typeID 
	and skills.activityID = product.activityID
left join Skills_V as skillVal on skillVal.Id = skills.skillID
	and skillVal.SkillLevelInt = skills.level
