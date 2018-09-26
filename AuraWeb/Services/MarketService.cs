using AuraWeb.Models;
using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWeb.Services
{
    public class MarketService
    {
        private readonly ILogger _Log;
        private readonly string _MarketDbPath;
        private readonly SQLiteService _SQLiteService;

        private const string INSERT_REGIONMARKET_TYPEID = @"
INSERT INTO RegionMarketTypeIds (RegionId, TypeId)
VALUES (@RegionId, @TypeId)
";
        private const string INSERT_MARKET_ORDER = @"
INSERT INTO RegionMarketOrders (RegionId, OrderId, TypeId, SystemId, LocationId, 
Range, IsBuyOrder, Duration, Issued, MinVolume, VolumeRemain, VolumeTotal, Price)
VALUES (@RegionId, @OrderId, @TypeId, @SystemId, @LocationId, 
@Range, @IsBuyOrder, @Duration, @Issued, @MinVolume, @VolumeRemain, @VolumeTotal, @Price)
";
        private const string INSERT_MARKET_AVERAGE_PRICE = @"
INSERT INTO MarketAveragePrices (Timestamp, TypeId, AdjustedPrice, AveragePrice)
VALUES (@Timestamp, @TypeId, @AdjustedPrice, @AveragePrice)
";

        private const int SECONDS_TIMEOUT = 240;

        private const int SECONDS_BETWEEN_ACTIONS = 10;
        private const int SECONDS_BETWEEN_REGIONS = 10;
        private int MS_BETWEEN_ACTIONS = SECONDS_BETWEEN_ACTIONS * 1000;
        private int MS_BETWEEN_REGIONS = SECONDS_BETWEEN_REGIONS * 1000;

        public MarketService(ILogger logger, string marketDbPath)
        {
            _Log = logger;
            _MarketDbPath = marketDbPath; // @"C:\AuraWeb\Data\market.sqlite";
            _SQLiteService = new SQLiteService(_MarketDbPath);
        }

        private void CreateDb()
        {
            if (!DBExists()) // Create the DB if needed
            {
                FileInfo fi = new FileInfo(_MarketDbPath);
                fi.Directory.Create();
                FileStream fs = File.Create(_MarketDbPath);
                fs.Close();
                _Log.LogDebug(String.Format("Created Market Database '{0}'.", _MarketDbPath));
            }
        }

        public void DownloadMarket()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _Log.LogInformation(String.Format("[Will create or use Market Database located at '{0}'.", _MarketDbPath));
            CreateDb();
            CreateTables();
            ClearTables();

            DownloadAndSaveMarketPrices().Wait();

            sw.Stop();

            _Log.LogDebug(String.Format("Market download finished; entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }

        private bool DBExists()
        {
            FileInfo dbFile = new FileInfo(_MarketDbPath);
            if (!dbFile.Exists) return false;

            // TODO: Implement this with a model and actually select

            // Query the DB against the views. If there is an error, or if the result is null, there is an issue (No Thorax!?)
            return true;
            /*
            try
            {
                string sql = "SELECT* FROM ItemTypes_V where id = 46075";
                ItemType test_THORAX = _SQLiteService.SelectSingle<ItemType>(sql);
                if (test_THORAX == null)
                {
                    //_Log.LogDebug("Selected test item from SDE to test, result was null. The database is likely empty but the schema intact, or there was a failure casting or mapping the appropriate object.");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                //_Log.LogDebug("Selected test item from SDE to test, result was an exception. The database likely doesn't have the relevant views created properly, or there was a failure casting or mapping the appropriate object.");
                return false;
            }
            */
        }

        private void CreateTables()
        {
            const string CREATE_TABLE_REGION_MARKET_TYPEIDS = @"
CREATE TABLE IF NOT EXISTS RegionMarketTypeIds 
(RegionId int not null, TypeId int not null)";
            const string CREATE_TABLE_REGION_MARKET_ORDERS = @"
CREATE TABLE IF NOT EXISTS RegionMarketOrders 
(RegionId int not null, OrderId int not null, TypeId int not null, SystemId int not null, LocationId int not null, 
Range text, IsBuyOrder int not null, Duration int, Issued text not null, MinVolume int, VolumeRemain int, 
VolumeTotal int, Price int not null)";
            const string CREATE_TABLE_MARKET_AVERAGE_PRICES = @"
CREATE TABLE IF NOT EXISTS MarketAveragePrices
(Timestamp datetime not null, TypeId int not null, AdjustedPrice int, AveragePrice int)";

            _SQLiteService.ExecuteMultiple(new List<string>() { CREATE_TABLE_REGION_MARKET_TYPEIDS, CREATE_TABLE_REGION_MARKET_ORDERS, CREATE_TABLE_MARKET_AVERAGE_PRICES });
            _Log.LogDebug("Created Tables in Market Database.");
        }

        private void ClearTables()
        {
            const string DELETE_FROM_TABLE_REGION_MARKET_TYPEIDS = @"
DELETE FROM RegionMarketTypeIds 
";
            const string DELETE_FROM_TABLE_REGION_MARKET_ORDERS = @"
DELETE FROM RegionMarketOrders 
";
// Delete strategy not applicable for MarketAveragePrices

            _SQLiteService.ExecuteMultiple(new List<string>() { DELETE_FROM_TABLE_REGION_MARKET_TYPEIDS, DELETE_FROM_TABLE_REGION_MARKET_ORDERS });
            _Log.LogDebug("Deleted all rows from Tables in Market Database.");
        }

        private EVEStandardAPI GetESIClient()
        {
            return new EVEStandardAPI(
                "AuraWebMarketDownloader",                      // User agent
                DataSource.Tranquility,                         // Server [Tranquility/Singularity]
                TimeSpan.FromSeconds(SECONDS_TIMEOUT)           // Timeout
            );
        }

        private async Task DownloadAndSaveMarketPrices()
        {
            var _ESIClient = GetESIClient();

            Stopwatch sw = new Stopwatch();

            #region Handle Market Average Prices
            sw = new Stopwatch();
            sw.Start();
            _Log.LogDebug("Getting Market Average Prices");
            var marketPricesApi = await _ESIClient.Market.ListMarketPricesV1Async();
            List<MarketPrice> marketPrices = marketPricesApi.Model;
            sw.Stop();
            _Log.LogDebug(String.Format("Finished getting Market Average Prices. Result count is {0}. Took {1} seconds.", marketPrices.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
            // Persist to database
            try
            {
                sw = new Stopwatch();
                sw.Start();
                List<InsertDTO> marketAveragePricesInsert = new List<InsertDTO>();
                foreach (MarketPrice market in marketPrices)
                {
                    object parameter = new
                    {
                        Timestamp = DateTime.Now,
                        TypeId = market.TypeId,
                        AdjustedPrice = market.AdjustedPrice,
                        AveragePrice = market.AveragePrice
                    };
                    marketAveragePricesInsert.Add(new InsertDTO()
                    {
                        SQL = INSERT_MARKET_AVERAGE_PRICE,
                        Parameters = parameter
                    });
                }
                List<string> marketAveragePricesInsertSql = marketAveragePricesInsert.Select(a => a.SQL).ToList();
                List<object> marketAveragePricesInsertParameters = marketAveragePricesInsert.Select(a => a.Parameters).ToList();
                _SQLiteService.ExecuteMultiple(marketAveragePricesInsertSql, marketAveragePricesInsertParameters);
                sw.Stop();
                _Log.LogDebug(String.Format("Inserted Market Average Prices to database. Took {0} seconds.", sw.Elapsed.TotalSeconds.ToString("##.##")));
            }
            catch (Exception e)
            {
                _Log.LogError(e, String.Format("Failed to insert Market Average Prices. Will proceed. Error: {0}", e.Message));
            }
            #endregion


            #region Get Region Ids
            List<int> regionIds = new List<int>();
            var regionIdsResult = await _ESIClient.Universe.GetRegionsV1Async();
            regionIds = regionIdsResult.Model;
            _Log.LogDebug(String.Format("Found {0} Regions to process in Market.", regionIds.Count));
            #endregion


            await DownloadAndSaveMarketPricesForRegion(_ESIClient, regionIds);

        }

        private async Task DownloadAndSaveMarketPricesForJita()
        {
            var _ESIClient = GetESIClient();
            int JiraRegionId = 30000142;
            await DownloadAndSaveMarketPricesForRegion(_ESIClient, new List<int>() { JiraRegionId });
        }

        private async Task DownloadAndSaveMarketPricesForRegion(EVEStandardAPI _ESIClient, List<int> regionIds)
        {
            Stopwatch sw = new Stopwatch();

            for (int x = 0; x < regionIds.Count; x++) // Loop through the regions
            {
                int regionId = regionIds[x];
                _Log.LogDebug(String.Format("Processing Region Id {0} for Market ({1} of {2})...", regionId, x + 1, regionIds.Count));
                sw = new Stopwatch();

                #region Get Type Ids in Region Market
                sw = new Stopwatch();
                sw.Start();
                List<long> typeIdsInRegion = new List<long>();
                var typeIdsInRegionResult = await _ESIClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, 1); // GetById the results for page 1
                typeIdsInRegion = typeIdsInRegionResult.Model; // Assign the result
                if (typeIdsInRegionResult.MaxPages > 1) // Handle multiple pages
                {
                    for (int a = 2; a <= typeIdsInRegionResult.MaxPages; a++)
                    {
                        typeIdsInRegionResult = await _ESIClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, a); // GetById the results for page a
                        typeIdsInRegion.AddRange(typeIdsInRegionResult.Model); // Add the results to the master list
                    }
                }
                sw.Stop();
                _Log.LogDebug(String.Format("Finished getting Type Ids in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, typeIdsInRegionResult.MaxPages, typeIdsInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                // Persist to database
                try
                {
                    sw = new Stopwatch();
                    sw.Start();
                    List<InsertDTO> marketTypeIdInserts = new List<InsertDTO>();
                    foreach (long typeId in typeIdsInRegion)
                    {
                        object parameter = new
                        {
                            RegionId = regionId,
                            TypeId = typeId
                        };
                        marketTypeIdInserts.Add(new InsertDTO()
                        {
                            SQL = INSERT_REGIONMARKET_TYPEID,
                            Parameters = parameter
                        });
                    }
                    List<string> marketTypeIdInsertSql = marketTypeIdInserts.Select(a => a.SQL).ToList();
                    List<object> marketTypeIdInsertParameters = marketTypeIdInserts.Select(a => a.Parameters).ToList();
                    _SQLiteService.ExecuteMultiple(marketTypeIdInsertSql, marketTypeIdInsertParameters);
                    sw.Stop();
                    _Log.LogDebug(String.Format("Inserted Type Ids in Market for Region {0} to database. Took {1} seconds.", regionId, sw.Elapsed.TotalSeconds.ToString("##.##")));
                }
                catch (Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to insert Type Ids in Market for Region {0}. Will proceed. Error: {1}", regionId, e.Message));
                }
                #endregion

                // Give the servers a break
                _Log.LogDebug(String.Format("Sleeping for {0} seconds before processing next Market request...", SECONDS_BETWEEN_ACTIONS.ToString()));
                Thread.Sleep(MS_BETWEEN_ACTIONS);

                #region Get Orders in Region Market
                sw = new Stopwatch();
                sw.Start();
                List<MarketOrder> ordersInRegion = new List<MarketOrder>();
                var ordersInRegionResult = await _ESIClient.Market.ListOrdersInRegionV1Async(regionId, null, 1); // GetById the results for page 1
                ordersInRegion = ordersInRegionResult.Model; // Assign the result
                if (ordersInRegionResult.MaxPages > 1) // Handle multiple pages
                {
                    for (int a = 2; a <= ordersInRegionResult.MaxPages; a++)
                    {
                        ordersInRegionResult = await _ESIClient.Market.ListOrdersInRegionV1Async(regionId, null, a); // GetById the results for page a
                        ordersInRegion.AddRange(ordersInRegionResult.Model); // Add the results to the master list
                    }
                }
                sw.Stop();
                _Log.LogDebug(String.Format("Finished getting Orders in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, ordersInRegionResult.MaxPages, ordersInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                // Persist to database
                try
                {
                    sw = new Stopwatch();
                    sw.Start();
                    List<InsertDTO> marketOrderInserts = new List<InsertDTO>();
                    foreach (MarketOrder order in ordersInRegion)
                    {
                        string key = String.Format("{0}-{1}-{2}", regionId, order.OrderId, order.TypeId);

                        object parameter = new
                        {
                            RegionId = regionId,
                            OrderId = order.OrderId,
                            TypeId = order.TypeId,
                            SystemId = order.SystemId,
                            LocationId = order.LocationId,
                            Range = order.Range,
                            IsBuyOrder = order.IsBuyOrder == true ? 1 : 0,
                            Duration = order.Duration,
                            Issued = order.Issued,
                            MinVolume = order.MinVolume,
                            VolumeRemain = order.VolumeRemain,
                            VolumeTotal = order.VolumeTotal,
                            Price = order.Price
                        };
                        marketOrderInserts.Add(new InsertDTO()
                        {
                            SQL = INSERT_MARKET_ORDER,
                            Parameters = parameter
                        });
                    }
                    List<string> marketOrderInsertSql = marketOrderInserts.Select(a => a.SQL).ToList();
                    List<object> marketOrderInsertParameters = marketOrderInserts.Select(a => a.Parameters).ToList();
                    _SQLiteService.ExecuteMultiple(marketOrderInsertSql, marketOrderInsertParameters);
                    sw.Stop();
                    _Log.LogDebug(String.Format("Inserted Orders in Market for Region {0} to database. Took {1} seconds.", regionId, sw.Elapsed.TotalSeconds.ToString("##.##")));
                }
                catch (Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to insert Orders in Market for Region {0}. Will proceed. Error: {1}", regionId, e.Message));
                }
                #endregion

                // Give the servers a break
                _Log.LogDebug(String.Format("Sleeping for {0} seconds before processing next Market request...", SECONDS_BETWEEN_ACTIONS.ToString()));
                //Thread.Sleep(MS_BETWEEN_ACTIONS);

                /*
                    * The below currently not working. Either fix with a pull request, or reference link below
                    * to use the API itself
                    * https://esi.evetech.net/ui#/
                    */
                // var marketGroups = await _ESIClient.Market.GetItemGroupsV1Async();

                // TODO: Consider doing this by type id in region at some point //_ESIClient.Market.ListHistoricalMarketStatisticsInRegionV1Async

                double percentComplete = ((x + 1) / regionIds.Count) * 100;
                _Log.LogInformation(String.Format("Finished Processing Region Id {0} for Market ({1} of {2}) ({1}%)", regionId, x + 1, regionIds.Count, percentComplete.ToString("##.##")));

                // Give the servers a break, this time only if not the last one
                if (x != regionIds.Count - 1)
                {
                    _Log.LogDebug(String.Format("Sleeping for {0} seconds before processing next Market Region batch request...", SECONDS_BETWEEN_REGIONS.ToString()));
                    Thread.Sleep(MS_BETWEEN_REGIONS);
                }
            }
        }

        public MarketAveragePrices_Row GetAveragePriceForTypeId(int id)
        {
            const string sql = @"
select a.TimeStamp, a.TypeId, a.AdjustedPrice, a.AveragePrice
from MarketAveragePrices as a
join (
	select max(""Timestamp"") as ""Timestamp"", TypeId
    from MarketAveragePrices
    where TypeId = @id
    group by TypeId
) as b on b.""Timestamp"" = a.""Timestamp""
    and b.TypeId = a.TypeId
where a.TypeId = @id
";
            MarketAveragePrices_Row result = _SQLiteService.SelectSingle<MarketAveragePrices_Row>(sql, new { id = id });
            return result;
        }

        public List<MarketAveragePrices_Row> GetAveragePrices()
        {
            const string sql = @"
select a.TimeStamp, a.TypeId, a.AdjustedPrice, a.AveragePrice
from MarketAveragePrices as a
join (
	select max(""Timestamp"") as ""Timestamp"", TypeId
    from MarketAveragePrices
    group by TypeId
) as b on b.""Timestamp"" = a.""Timestamp""
    and b.TypeId = a.TypeId
order by a.TypeId
";
            List<MarketAveragePrices_Row> result = new List<MarketAveragePrices_Row>();
            result = _SQLiteService.SelectMultiple<MarketAveragePrices_Row>(sql);
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestSellPrices()
        {
            const string sql = @"
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
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql);
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestSellPricesForTypeId(int typeid, int limit = 20)
        {
            const string sql = @"
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 1 and TypeId = @typeid
	order by price desc
	limit @limit
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql, new { typeid = typeid, limit = limit });
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestBuyPrices()
        {
            const string sql = @"
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
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql);
            return result;
        }

        public List<RegionMarketOrdersRow> GetBestBuyPricesForTypeId(int typeid, int limit = 20)
        {
            const string sql = @"
select *
from RegionMarketOrders
where rowid in (
	select rowid from RegionMarketOrders
	where IsBuyOrder = 0 and TypeId = @typeid
	order by price desc
	limit @limit
)
order by TypeId asc, RegionId asc, SystemId asc, LocationId asc
";
            List<RegionMarketOrdersRow> result = new List<RegionMarketOrdersRow>();
            result = _SQLiteService.SelectMultiple<RegionMarketOrdersRow>(sql, new { typeid = typeid, limit = limit });
            return result;
        }
    }
}
