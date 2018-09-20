using EVEStandard;
using EVEStandard.Enumerations;
using EVEStandard.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWebMarketDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            string thisPath = Assembly.GetExecutingAssembly().Location;
            string thisDir = System.IO.Path.GetDirectoryName(thisPath);

            string MARKET_DB_PATH = Path.Combine(thisDir, "market.sqlite");
            if (!DBExists(MARKET_DB_PATH)) // Create the DB if needed
            { 
                FileStream fs = File.Create(MARKET_DB_PATH);
                fs.Close();
            }

            DownloadAndSaveMarketPrices(MARKET_DB_PATH).Wait();
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

        private static void CreateDb()
        {

        }

        private static async Task DownloadAndSaveMarketPrices(string dbPath)
        {
            int SECONDS_BETWEEN_ACTIONS = 15;
            int SECONDS_BETWEEN_REGIONS = 30;
            int MS_BETWEEN_ACTIONS = SECONDS_BETWEEN_ACTIONS * 1000;
            int MS_BETWEEN_REGIONS = SECONDS_BETWEEN_REGIONS * 1000;

            #region Set DB
            try
            {

            }
            catch(Exception e)
            {

            }
            #endregion

            #region Download and Save
            try
            {
                var esiClient = new EVEStandardAPI(
                "AuraWebMarketDownloader",   // User agent
                DataSource.Tranquility,             // Server [Tranquility/Singularity]
                TimeSpan.FromSeconds(30)            // Timeout
            );

                #region Get Region Ids
                List<int> regionIds = new List<int>();
                var regionIdsResult = await esiClient.Universe.GetRegionsV1Async();
                regionIds = regionIdsResult.Model;
                Console.WriteLine(("Found {0} Regions to process.", regionIds.Count));
                #endregion

                for (int x = 0; x < regionIds.Count; x++) // Loop through the regions
                {
                    int regionId = regionIds[x];
                    Console.WriteLine(String.Format("Processing Region Id {0} ({1} of {2})...", regionId, x + 1, regionIds.Count));
                    Stopwatch sw = new Stopwatch();

                    #region Get Type Ids in Region Market
                    sw = new Stopwatch();
                    sw.Start();
                    List<long> typeIdsInRegion = new List<long>();
                    var typeIdsInRegionResult = await esiClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, 1); // Get the results for page 1
                    typeIdsInRegion = typeIdsInRegionResult.Model; // Assign the result
                    if (typeIdsInRegionResult.MaxPages > 1) // Handle multiple pages
                    {
                        for (int a = 2; a < typeIdsInRegionResult.MaxPages; a++)
                        {
                            typeIdsInRegionResult = await esiClient.Market.ListTypeIdsRelevantToMarketV1Async(regionId, a); // Get the results for page a
                            typeIdsInRegion.AddRange(typeIdsInRegionResult.Model); // Add the results to the master list
                        }
                    }
                    // TODO: Persist to Database
                    sw.Stop();
                    Console.WriteLine(String.Format("Finished getting Type Ids in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, typeIdsInRegionResult.MaxPages, typeIdsInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                    #endregion

                    // Give the servers a break
                    Console.WriteLine(String.Format("Sleeping for {0} seconds...zzzz", SECONDS_BETWEEN_ACTIONS.ToString()));
                    Thread.Sleep(MS_BETWEEN_ACTIONS);

                    #region Get Orders in Region Market
                    sw = new Stopwatch();
                    sw.Start();
                    List<MarketOrder> ordersInRegion = new List<MarketOrder>();
                    var ordersInRegionResult = await esiClient.Market.ListOrdersInRegionV1Async(regionId, null, 1); // Get the results for page 1
                    ordersInRegion = ordersInRegionResult.Model; // Assign the result
                    if (ordersInRegionResult.MaxPages > 1) // Handle multiple pages
                    {
                        for (int a = 2; a < ordersInRegionResult.MaxPages; a++)
                        {
                            ordersInRegionResult = await esiClient.Market.ListOrdersInRegionV1Async(regionId, null, a); // Get the results for page a
                            ordersInRegion.AddRange(ordersInRegionResult.Model); // Add the results to the master list
                        }
                    }
                    // TODO: Persist to Database
                    sw.Stop();
                    Console.WriteLine(String.Format("Finished getting Orders in Market for Region {0}. Processed {1} pages. Result count is {2}. Took {3} seconds.", regionId, ordersInRegionResult.MaxPages, ordersInRegion.Count, sw.Elapsed.TotalSeconds.ToString("##.##")));
                    #endregion

                    // Give the servers a break
                    Console.WriteLine(String.Format("Sleeping for {0} seconds...zzzz", SECONDS_BETWEEN_ACTIONS.ToString()));
                    Thread.Sleep(MS_BETWEEN_ACTIONS);

                    /*
                     * The below currently not working. Either fix with a pull request, or reference link below
                     * to use the API itself
                     * https://esi.evetech.net/ui#/
                     */
                    // var marketGroups = await esiClient.Market.GetItemGroupsV1Async();







                    // TODO: Consider doing this by type id in region at some point //esiClient.Market.ListHistoricalMarketStatisticsInRegionV1Async



                    double percentComplete = (x + 1) / regionIds.Count;
                    Console.WriteLine(String.Format("Finished Processing Region Id {0} ({1}%)", regionId, percentComplete.ToString("##.#")));

                    // Give the servers a break
                    Console.WriteLine(String.Format("Sleeping for {0} seconds...zzzz", SECONDS_BETWEEN_REGIONS.ToString()));
                    Thread.Sleep(MS_BETWEEN_REGIONS);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Error occurred: ", e.Message));
                Environment.Exit(400);
            }
            #endregion
        }
    }
}
