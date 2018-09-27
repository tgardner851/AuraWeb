using AuraWeb.Models;
using ICSharpCode.SharpZipLib.BZip2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraWeb.Services
{
    public class SDEService
    {
        private readonly ILogger _Log;
        private SQLiteService _SQLiteService;
        private readonly string _SDEFileName;
        private readonly string _SDETempCompressedFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEBackupFileName;
        private readonly string _SDEDownloadUrl;

        public SDEService(ILogger logger, string sdeFileName, string sdeTempCompressedFileName, string sdeTempFileName, string sdeBackupFileName, string sdeDownloadUrl)
        {
            _Log = logger;
            _SDEFileName = sdeFileName;
            _SDETempCompressedFileName = sdeTempCompressedFileName;
            _SDETempFileName = sdeTempFileName;
            _SDEBackupFileName = sdeBackupFileName;
            _SDEDownloadUrl = sdeDownloadUrl;
            _SQLiteService = new SQLiteService(sdeFileName);
        }
        
        #region Download
        private void Download()
        {
            // GetById the filename to download to (with path). Make sure to use same path as config and exe
            string sdePath = _SDEFileName;
            string sdeTempCompressedPath = _SDETempCompressedFileName;
            string sdeTempPath = _SDETempFileName;
            string sdeBackupFileName = _SDEBackupFileName;
            string sdeAddress = _SDEDownloadUrl;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            _Log.LogInformation(String.Format("Downloading SDE from URL '{0}' to temp file '{1}'...", sdeAddress, sdeTempCompressedPath));

            // Create the temp directory if needed
            string sdeTempDir = new FileInfo(sdeTempCompressedPath).Directory.FullName;
            Directory.CreateDirectory(sdeTempDir);
            _Log.LogDebug(String.Format("Created temp directory '{0}'.", sdeTempDir));

            // Check if the SDE exists and make a backup
            bool sdeExists = false;
            //string existingSDEBackupPath = new FileInfo(sdePath).Directory.FullName + "\\sde.sqlite.backup";
            bool backupExists = false;
            // Rename existing DB as a backup, if it exists
            if (File.Exists(sdePath)) // Existing SDE
            {
                sdeExists = true;
                File.Copy(sdePath, sdeBackupFileName); // Copy file as backup, keep original in place
                backupExists = true;
                _Log.LogDebug(String.Format("Copied existing SDE from '{0}' to '{1}'.", sdePath, sdeBackupFileName));
            }
            backupExists = new FileInfo(sdeBackupFileName).Exists; // Just in case?

            // Attempt the download synchronously to the temp path
            try
            {
                _Log.LogInformation(String.Format("Downloading file '{0}' to '{1}'.", sdeAddress, sdeTempCompressedPath));
                Downloader dl = new Downloader(sdeAddress, sdeTempCompressedPath);
                dl.DownloadFile();
            }
            catch (Exception e)
            {
                _Log.LogError(String.Format("Failed to download SDE from address '{0}' to temp path '{1}'", sdeAddress, sdeTempCompressedPath), e);
                if (backupExists)
                {
                    // Delete the backup, the original file exists
                    File.Delete(sdeBackupFileName);
                    _Log.LogDebug(String.Format("Deleted backup SDE from {0}, kept orginal.", sdeBackupFileName));
                }
                throw;
            }
            _Log.LogInformation(String.Format("Finished downloading SDE from URL '{0}' to temp file '{1}'.", sdeAddress, sdeTempCompressedPath));

            // File should be .bz2
            FileInfo sdeTemp = new FileInfo(sdeTempCompressedPath);

            // Decompress!
            using (FileStream sdeTempStream = sdeTemp.OpenRead())
            {
                using (FileStream sdeStream = File.Create(sdeTempPath))
                {
                    try
                    {
                        _Log.LogInformation(String.Format("Decompressing .bz2 file '{0}' to '{1}'...", sdeTempCompressedPath, sdeTempPath));
                        Stopwatch sdeDecompressionTimer = new Stopwatch();
                        sdeDecompressionTimer.Start();
                        BZip2.Decompress(sdeTempStream, sdeStream, true);
                        sdeDecompressionTimer.Stop();
                        _Log.LogInformation(String.Format("Finished decompressing .bz2 file '{0}' to '{1}'. Took {2} minutes to complete.", sdeTempCompressedPath, sdeTempPath, Math.Round(sdeDecompressionTimer.Elapsed.TotalMinutes, 2).ToString("0.##")));
                    }
                    catch (Exception e)
                    {
                        _Log.LogError(String.Format("Failed to decompress sde temp file '{0}'", sdeTempPath), e);
                        if (backupExists)
                        {
                            // Delete the backup, the original file exists
                            File.Delete(sdeBackupFileName);
                            _Log.LogDebug(String.Format("Deleted backup SDE from {0}, kept orginal.", sdeBackupFileName));
                        }
                        throw;
                    }
                }
            }

            // Verify file extracted properly, byte size should be > 1024 at minimum
            FileInfo sdeTempFile = new FileInfo(sdeTempPath);
            if (sdeTempFile.Length < 1024)
            {
                _Log.LogError("File extraction likely failed. File size was less than 1024 bytes.");
                sdeTempFile.Delete(); // Delete the file, as it's invalid
                if (backupExists)
                {
                    // Delete the backup, the original file exists
                    File.Delete(sdeBackupFileName);
                    _Log.LogDebug(String.Format("Deleted backup SDE from {0}, kept orginal.", sdeBackupFileName));
                }
                throw new Exception("SDE File length was under 1024 bytes.");
            }
            
            // Delete the backup if necessary
            if (sdeExists)
            {
                if (backupExists)
                {
                    File.Delete(sdeBackupFileName);
                    _Log.LogDebug(String.Format("Deleted old existing SDE '{0}'", sdeBackupFileName));
                }
            }

            // Replace old SDE with new SDE
            for(int x = 0; x < 3; x++) // Try three times if it fails
            {
                bool success = false;
                try
                {
                    File.Delete(sdePath);
                    File.Copy(sdeTempPath, sdePath);
                    _Log.LogDebug(String.Format("Replaced old SDE with new at '{0}'.", sdePath));
                    success = true;
                }
                catch(Exception e)
                {
                    success = false;
                    continue;
                }
                if (success) break;
            }

            // Delete temp directory and all files inside
            string dirDel = new FileInfo(sdeTempPath).Directory.FullName;
            Directory.Delete(dirDel, true);
            _Log.LogDebug(String.Format("Deleted SDE download temp directory and files within '{0}'.", _SDETempFileName));

            sw.Stop();

            _Log.LogInformation(String.Format("SDE refreshed. Entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }
        #endregion

        #region SQLite DB
        public void Initialize()
        {
            Download();
            CreateViews();
        }

        public bool SDEExists()
        {
            string sdePath = _SDEFileName;
            FileInfo sdeFile = new FileInfo(sdePath);
            if (!sdeFile.Exists) return false;
            // Query the DB against the views. If there is an error, or if the result is null, there is an issue (No Thorax!?)
            try
            {
                string sql = "SELECT* FROM ItemTypes_V where id = 46075";
                ItemType_V_Row test_THORAX = _SQLiteService.SelectSingle<ItemType_V_Row>(sql);
                if (test_THORAX == null)
                {
                    _Log.LogDebug("Selected test item from SDE to test, result was null. The database is likely empty but the schema intact, or there was a failure casting or mapping the appropriate object.");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                _Log.LogDebug("Selected test item from SDE to test, result was an exception. The database likely doesn't have the relevant views created properly, or there was a failure casting or mapping the appropriate object.");
                return false;
            }
        }

        private void InitSQLiteService()
        {
            _SQLiteService = new SQLiteService(_SDEFileName);
        }

        public void CreateViews()
        {
            List<string> viewCreationSQLSequence = SQL.SDEViews.Sequence;
            _Log.LogDebug(String.Format("Will execute {0} SQL scripts. Starting...", viewCreationSQLSequence.Count));
            _SQLiteService = new SQLiteService(_SDEFileName);
            _SQLiteService.ExecuteMultiple(viewCreationSQLSequence);
            _Log.LogInformation("Created SDE Views.");
        }
        #endregion

        #region Queries
        // TODO: Deprecate GetTypeNames()
        public List<TypeNameDTO> GetTypeNames()
        {
            _SQLiteService = new SQLiteService(_SDEFileName);
            List<TypeNameDTO> result = new List<TypeNameDTO>();
            string sql = "select typeId Id, typeName Name from invTypes";
            result = _SQLiteService.SelectMultiple<TypeNameDTO>(sql);
            // The below will not work, since there is a limit on parameters
            //string sql = "select typeId Id, typeName Name from invTypes where typeId in @typeIds";
            //result = _SQLiteService.SelectMultiple<TypeNameDTO>(sql, new { typeIds = typeIds });
            return result;
        }

        public List<T> Search<T>(string sql, string query)
        {
            query = query.Trim();
            List<T> result = new List<T>();
            string id = query; // For Id searches
            query = String.Format("%{0}%", query); // Format query for LIKE operator
            result = _SQLiteService.SelectMultiple<T>(sql, new { id = id, query = query });
            return result;
        }

        // For getting by primary id
        public T GetById<T>(string sql, int id)
        {
            return _SQLiteService.SelectSingle<T>(sql, new { id = id });
        }

        // For getting by foreign id
        public List<T> GetMultipleById<T>(string sql, int id)
        {
            return _SQLiteService.SelectMultiple<T>(sql, new { id = id });
        }

        // For getting my primary ids
        public List<T> GetByMultipleIds<T>(string sql, List<int> ids)
        {
            return _SQLiteService.SelectMultiple<T>(sql, new { ids = ids });
        }

        // For getting my primary ids (with long data type)
        public List<T> GetByMultipleIdsLong<T>(string sql, List<long> ids)
        {
            return _SQLiteService.SelectMultiple<T>(sql, new { ids = ids });
        }

        // For getting all rows (no parameters)
        public List<T> GetMultiple<T>(string sql)
        {
            return _SQLiteService.SelectMultiple<T>(sql);
        }

        #region Universe
        #region Regions
        public List<Region_V_Row> SearchRegions(string query)
        {
            string sql = @"
select * from Regions_V where 
    Name like @query 
    or Id like @query
    or FactionName like @query
order by Name
;";
            return Search<Region_V_Row>(sql, query);
        }

        public Region_V_Row GetRegion(int id)
        {
            string sql = @"
select * from Regions_V where id = @id
;";
            return GetById<Region_V_Row>(sql, id);
        }

        public List<Region_V_Row> GetRegions(List<int> ids)
        {
            string sql = @"
select * from Regions_V where id in @ids
;";
            return GetByMultipleIds<Region_V_Row>(sql, ids);
        }

        public List<Region_V_Row> GetAllRegions()
        {
            string sql = @"select * from Regions_V";
            return GetMultiple<Region_V_Row>(sql);
        }
        #endregion

        #region Constellations
        public List<Constellation_V_Row> SearchConstellations(string query)
        {
            string sql = @"
select * from Constellations_V where 
    Name like @query 
    or Id like @query
    or RegionName like @query
    or FactionName like @query
order by Name
;";
            return Search<Constellation_V_Row>(sql, query);
        }

        public Constellation_V_Row GetConstellation(int id)
        {
            string sql = @"
select * from Constellations_V where id = @id
;";
            return GetById<Constellation_V_Row>(sql, id);
        }

        public List<Constellation_V_Row> GetConstellations(List<int> ids)
        {
            string sql = @"
select * from Constellations_V where id in @ids
;";
            return GetByMultipleIds<Constellation_V_Row>(sql, ids);
        }

        public List<Constellation_V_Row> GetConstellationsForRegion(int id)
        {
            string sql = @"
select * from Constellations_V where RegionId = @id
;";
            return GetMultipleById<Constellation_V_Row>(sql, id);
        }

        public List<Constellation_V_Row> GetAllConstellations()
        {
            string sql = @"select * from Constellations_V";
            return GetMultiple<Constellation_V_Row>(sql);
        }
        #endregion

        #region Solar Systems
        public List<SolarSystem_V_Row> SearchSolarSystems(string query)
        {
            string sql = @"
select * from SolarSystems_V where 
    Name like @query 
    or Id like @query
    or RegionName like @query
    or ConstellationName like @query
    or FactionName like @query
order by Name
;";
            return Search<SolarSystem_V_Row>(sql, query);
        }

        public SolarSystem_V_Row GetSolarSystem(int id)
        {
            string sql = @"
select * from SolarSystems_V where id = @id
;";
            return GetById<SolarSystem_V_Row>(sql, id);
        }

        public List<SolarSystem_V_Row> GetSolarSystems(List<int> ids)
        {
            string sql = @"
select * from SolarSystems_V where id in @ids
;";
            return GetByMultipleIds<SolarSystem_V_Row>(sql, ids);
        }

        public List<SolarSystem_V_Row> GetSolarSystemsForConstellation(int id)
        {
            string sql = @"
select * from SolarSystems_V where ConstellationId = @id
;";
            return GetMultipleById<SolarSystem_V_Row>(sql, id);
        }

        public List<SolarSystem_V_Row> GetAllSolarSystems()
        {
            string sql = @"select * from SolarSystems_V";
            return GetMultiple<SolarSystem_V_Row>(sql);
        }
        #endregion

        #region Stations
        public List<Station_V_Row> SearchStations(string query)
        {
            string sql = @"
select * from Stations_V where 
    Name like @query  
    or Id like @query
    or SolarSystemName like @query
    or ConstellationName like @query
    or RegionName like @query
    or OperationName like @query
order by Name
;";
            return Search<Station_V_Row>(sql, query);
        }

        public Station_V_Row GetStation(int id)
        {
            string sql = @"
select * from Stations_V where id = @id
;";
            return GetById<Station_V_Row>(sql, id);
        }

        public List<Station_V_Row> GetStations(List<int> ids)
        {
            string sql = @"
select * from Stations_V where id in @ids
;";
            return GetByMultipleIds<Station_V_Row>(sql, ids);
        }

        public List<Station_V_Row> GetStationsLong(List<long> ids)
        {
            string sql = @"
select * from Stations_V where id in @ids
;";
            return GetByMultipleIdsLong<Station_V_Row>(sql, ids);
        }

        public List<Station_V_Row> GetAllStations()
        {
            string sql = @"select * from Stations_V";
            return GetMultiple<Station_V_Row>(sql);
        }
        #endregion
        #endregion

        #region Item Types
        public List<ItemType_V_Row> SearchItemTypes(string query)
        {
            string sql = @"
select * from ItemTypes_V where 
    Name like @query  
    or Id like @query
    or Race_Name like @query
    or MarketGroup_Name like @query
    or Group_Name like @query
    or Group_Category_Name like @query
    or Meta_Group_Name like @query
order by Name
;";
            return Search<ItemType_V_Row>(sql, query);
        }

        public ItemType_V_Row GetItemType(int id)
        {
            string sql = @"
select * from ItemTypes_V where id = @id
;";
            return GetById<ItemType_V_Row>(sql, id);
        }

        public List<ItemType_V_Row> GetItemTypes(List<int> ids)
        {
            string sql = @"
select * from ItemTypes_V where id in @ids
;";
            return GetByMultipleIds<ItemType_V_Row>(sql, ids);
        }

        public List<ItemType_V_Row> GetAllItemTypes()
        {
            string sql = @"select * from ItemTypes_V";
            return GetMultiple<ItemType_V_Row>(sql);
        }
        #endregion


        #endregion
    }
}
