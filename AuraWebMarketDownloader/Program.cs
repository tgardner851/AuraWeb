using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWebMarketDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            // FOR TESTING
            args = new string[1];
            args[0] = @"C:\AuraWeb\Data\market.sqlite";

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("[ERROR] Market DB directory not provided. Expected first argument to be a path.");
                Environment.Exit(3); // Why 3? because
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            string MARKET_DB_PATH = args[0];
            Console.WriteLine(String.Format("[INFO] Will create or use Market Database located at '{0}'.", MARKET_DB_PATH));
            if (!DBExists(MARKET_DB_PATH)) // Create the DB if needed
            { 
                FileStream fs = File.Create(MARKET_DB_PATH);
                fs.Close();
                Console.WriteLine("[DATABASE] Created Market Database '{0}'.", MARKET_DB_PATH);
            }
            CreateTables(MARKET_DB_PATH);
            ClearTables(MARKET_DB_PATH);

            DownloadAndSaveMarketPrices(MARKET_DB_PATH).Wait();

            sw.Stop();

            Console.WriteLine(String.Format("Application finished and ready to terminate. Entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));

            // TEMP FOR TESTING
            Console.ReadKey();
        }

        private static bool DBExists(string dbPath)
        {
            FileInfo dbFile = new FileInfo(dbPath);
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

        private static void CreateTables(string dbPath)
        {
            string CREATE_TABLE_REGION_MARKET_TYPEIDS = @"
CREATE TABLE IF NOT EXISTS RegionMarketTypeIds 
(RegionId int not null, TypeId int not null)";
            string CREATE_TABLE_REGION_MARKET_ORDERS = @"
CREATE TABLE IF NOT EXISTS RegionMarketOrders 
(RegionId int not null, OrderId int not null, TypeId int not null, SystemId int not null, LocationId int not null, 
Range text, IsBuyOrder int not null, Duration int, Issued text not null, MinVolume int, VolumeRemain int, 
VolumeTotal int, Price int not null)";

            SQLiteService _SQLiteService = new SQLiteService(dbPath);
            _SQLiteService.ExecuteMultiple(new List<string>() { CREATE_TABLE_REGION_MARKET_TYPEIDS, CREATE_TABLE_REGION_MARKET_ORDERS });
            Console.WriteLine("[DATABASE] Created Tables.");
        }

        private static void ClearTables(string dbPath)
        {
            string DELETE_FROM_TABLE_REGION_MARKET_TYPEIDS = @"
DELETE FROM RegionMarketTypeIds 
";
            string DELETE_FROM_TABLE_REGION_MARKET_ORDERS = @"
DELETE FROM RegionMarketOrders 
";

            SQLiteService _SQLiteService = new SQLiteService(dbPath);
            _SQLiteService.ExecuteMultiple(new List<string>() { DELETE_FROM_TABLE_REGION_MARKET_TYPEIDS, DELETE_FROM_TABLE_REGION_MARKET_ORDERS });
            Console.WriteLine("[DATABASE] Deleted all rows from Tables.");
        }

        private static async Task DownloadAndSaveMarketPrices(string dbPath)
        {
            int SECONDS_TIMEOUT = 120;

            int SECONDS_BETWEEN_ACTIONS = 5;
            int SECONDS_BETWEEN_REGIONS = 5;
            int MS_BETWEEN_ACTIONS = SECONDS_BETWEEN_ACTIONS * 1000;
            int MS_BETWEEN_REGIONS = SECONDS_BETWEEN_REGIONS * 1000;

            string INSERT_REGIONMARKET_TYPEID = @"
INSERT INTO RegionMarketTypeIds (RegionId, TypeId)
VALUES (@RegionId, @TypeId)
";
            string INSERT_MARKET_ORDER = @"
INSERT INTO RegionMarketOrders (RegionId, OrderId, TypeId, SystemId, LocationId, 
Range, IsBuyOrder, Duration, Issued, MinVolume, VolumeRemain, VolumeTotal, Price)
VALUES (@RegionId, @OrderId, @TypeId, @SystemId, @LocationId, 
@Range, @IsBuyOrder, @Duration, @Issued, @MinVolume, @VolumeRemain, @VolumeTotal, @Price)
";
            SQLiteService _SQLiteService = new SQLiteService(dbPath);

            #region Download and Save
            try
            {
                var esiClient = new EVEStandardAPI(
                "AuraWebMarketDownloader",                      // User agent
                DataSource.Tranquility,                         // Server [Tranquility/Singularity]
                TimeSpan.FromSeconds(SECONDS_TIMEOUT)           // Timeout
            );

                #region Get Region Ids
                List<int> regionIds = new List<int>();
                var regionIdsResult = await esiClient.Universe.GetRegionsV1Async();
                regionIds = regionIdsResult.Model;
                Console.WriteLine(String.Format("[REGION IDS] Found {0} Regions to process.", regionIds.Count));
                #endregion

                for (int x = 0; x <= regionIds.Count; x++) // Loop through the regions
                {
                    int regionId = regionIds[x];
                    Console.WriteLine(String.Format("[REGION {0}] Processing Region Id {0} ({1} of {2})...", regionId, x + 1, regionIds.Count));
                    Stopwatch sw = new Stopwatch();

                    #region Get Type Ids in Region Market
                    sw = new Stopwatch();
                    sw.Start();
                    List<long> typeIdsInRegion = new List<long>();
                    var typeIdsInRegionResult = await esiClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, 1); // Get the results for page 1
                    typeIdsInRegion = typeIdsInRegionResult.Model; // Assign the result
                    if (typeIdsInRegionResult.MaxPages > 1) // Handle multiple pages
                    {
                        for (int a = 2; a <= typeIdsInRegionResult.MaxPages; a++)
                        {
                            typeIdsInRegionResult = await esiClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, a); // Get the results for page a
                            typeIdsInRegion.AddRange(typeIdsInRegionResult.Model); // Add the results to the master list
                        }
                    }
                    sw.Stop();
                    Console.WriteLine(String.Format("[REGION {0}] Finished getting Type Ids in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, typeIdsInRegionResult.MaxPages, typeIdsInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
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
                        Console.WriteLine(String.Format("[REGION {0}] Inserted Type Ids in Market for Region {0} to database. Took {1} seconds.", regionId, sw.Elapsed.TotalSeconds.ToString("##.##")));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(String.Format("[ERROR] Failed to insert Type Ids in Market for Region {0}. Will proceed. Error: {1}", regionId, e.Message));
                    }
                    #endregion

                    // Give the servers a break
                    Console.WriteLine(String.Format("[SLEEP] Sleeping for {0} seconds...zzzz", SECONDS_BETWEEN_ACTIONS.ToString()));
                    Thread.Sleep(MS_BETWEEN_ACTIONS);

                    #region Get Orders in Region Market
                    sw = new Stopwatch();
                    sw.Start();
                    List<MarketOrder> ordersInRegion = new List<MarketOrder>();
                    var ordersInRegionResult = await esiClient.Market.ListOrdersInRegionV1Async(regionId, null, 1); // Get the results for page 1
                    ordersInRegion = ordersInRegionResult.Model; // Assign the result
                    if (ordersInRegionResult.MaxPages > 1) // Handle multiple pages
                    {
                        for (int a = 2; a <= ordersInRegionResult.MaxPages; a++)
                        {
                            ordersInRegionResult = await esiClient.Market.ListOrdersInRegionV1Async(regionId, null, a); // Get the results for page a
                            ordersInRegion.AddRange(ordersInRegionResult.Model); // Add the results to the master list
                        }
                    }
                    sw.Stop();
                    Console.WriteLine(String.Format("[REGION {0}] Finished getting Orders in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, ordersInRegionResult.MaxPages, ordersInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                    // Persist to database
                    try
                    {
                        sw = new Stopwatch();
                        sw.Start();
                        List<InsertDTO> marketOrderInserts = new List<InsertDTO>();
                        foreach (MarketOrder order in ordersInRegion)
                        {
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
                        Console.WriteLine(String.Format("[REGION {0}] Inserted Orders in Market for Region {0} to database. Took {1} seconds.", regionId, sw.Elapsed.TotalSeconds.ToString("##.##")));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(String.Format("[ERROR] Failed to insert Orders in Market for Region {0}. Will proceed. Error: {1}", regionId, e.Message));
                    }
                    #endregion

                    // Give the servers a break
                    //Console.WriteLine(String.Format("[SLEEP] Sleeping for {0} seconds...zzzz", SECONDS_BETWEEN_ACTIONS.ToString()));
                    //Thread.Sleep(MS_BETWEEN_ACTIONS);

                    /*
                     * The below currently not working. Either fix with a pull request, or reference link below
                     * to use the API itself
                     * https://esi.evetech.net/ui#/
                     */
                    // var marketGroups = await esiClient.Market.GetItemGroupsV1Async();

                    // TODO: Consider doing this by type id in region at some point //esiClient.Market.ListHistoricalMarketStatisticsInRegionV1Async

                    double percentComplete = ((x + 1) / regionIds.Count) * 100;
                    Console.WriteLine(String.Format("[REGION {0}] Finished Processing Region Id {0} ({1}%)", regionId, percentComplete.ToString("##.##")));

                    // Give the servers a break, this time only if not the last one
                    if (x < regionIds.Count)
                    {
                        Console.WriteLine(String.Format("[SLEEP] Sleeping for {0} seconds...zzzz", SECONDS_BETWEEN_REGIONS.ToString()));
                        Thread.Sleep(MS_BETWEEN_REGIONS);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("[ERROR] Error occurred: ", e.Message));
                Environment.Exit(400);
            }
            #endregion
        }
    }
}
