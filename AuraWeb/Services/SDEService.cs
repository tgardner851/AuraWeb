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
        private readonly string _SDETempFileName;

        public SDEService(ILogger logger, string sdeFileName, string sdeTempFileName)
        {
            _Log = logger;
            _SDEFileName = sdeFileName;
            _SDETempFileName = sdeTempFileName;
            _SQLiteService = new SQLiteService(sdeFileName);
        }
        
        public void Initialize(string sdeAddress)
        {
            Download(sdeAddress);
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
                string sql = "SELECT* FROM ItemTypes_V where itemtype_id = 46075";
                object test_THORAX = _SQLiteService.SelectSingle<object>(sql);
                if (test_THORAX == null)
                {
                    _Log.LogDebug("Selected test item from SDE to test, result was null. The database is likely empty but the schema intact.");
                    return false;
                }
                else
                {
                    try
                    {
                        ItemType test_THORAX_cast = (ItemType)test_THORAX;
                        if (test_THORAX_cast == null)
                        {
                            _Log.LogDebug("Attempted to cast test object to relevant type, but failed. This may be an issue with the model or the object received.");
                            return false;
                        }
                        else return true; // Success path
                    }
                    catch(Exception e)
                    {
                        _Log.LogDebug("Attempted to cast test object to relevant type, but received an exception. This may be an issue with the model or the object received.");
                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                _Log.LogDebug("Selected test item from SDE to test, result was an exception. The database likely doesn't have the relevant views created properly.");
                return false;
            }
        }

        #region Download
        public void Download(string sdeAddress)
        {
            // Get the filename to download to (with path). Make sure to use same path as config and exe
            string sdePath = _SDEFileName;
            string sdeTempPath = _SDETempFileName;

            _Log.LogInformation(String.Format("Downloading SDE from URL '{0}' to temp file '{1}'...", sdeAddress, sdeTempPath));

            // Create the temp directory if needed
            string sdeTempDir = new FileInfo(sdeTempPath).Directory.FullName;
            Directory.CreateDirectory(sdeTempDir);
            _Log.LogDebug(String.Format("Created temp directory '{0}'.", sdeTempDir));

            // Check if the SDE exists and make a backup
            bool sdeExists = false;
            string existingSDEBackupPath = new FileInfo(sdePath).Directory.FullName + "\\sde.sqlite.backup";
            bool backupExists = false;
            // Rename existing DB as a backup, if it exists
            if (File.Exists(sdePath)) // Existing SDE
            {
                sdeExists = true;
                File.Move(sdePath, existingSDEBackupPath);
                backupExists = true;
                _Log.LogDebug(String.Format("Moved existing SDE from '{0}' to '{1}'.", sdePath, existingSDEBackupPath));
            }
            backupExists = new FileInfo(existingSDEBackupPath).Exists; // Just in case?

            // Attempt the download synchronously
            try
            {
                _Log.LogInformation(String.Format("Downloading file '{0}' to '{1}'.", sdeAddress, sdeTempPath));
                Downloader dl = new Downloader(sdeAddress, sdeTempPath);
                dl.DownloadFile();
            }
            catch (Exception e)
            {
                _Log.LogError(String.Format("Failed to download SDE from address '{0}' to temp path '{1}'", sdeAddress, sdeTempPath), e);
                if (backupExists)
                {
                    // Change the backup back
                    File.Move(existingSDEBackupPath, sdePath);
                    _Log.LogDebug(String.Format("Moved backup SDE from '{0}' to '{1}'.", existingSDEBackupPath, sdePath));
                }
                throw;
            }
            _Log.LogInformation(String.Format("Finished downloading SDE from URL '{0}' to temp file '{1}'.", sdeAddress, sdeTempPath));

            // File should be .bz2
            FileInfo sdeTemp = new FileInfo(sdeTempPath);

            // Decompress!
            using (FileStream sdeTempStream = sdeTemp.OpenRead())
            {
                using (FileStream sdeStream = File.Create(sdePath))
                {
                    try
                    {
                        _Log.LogInformation(String.Format("Decompressing .bz2 file '{0}' to '{1}'...", sdeTempPath, sdePath));
                        Stopwatch sdeDecompressionTimer = new Stopwatch();
                        sdeDecompressionTimer.Start();
                        BZip2.Decompress(sdeTempStream, sdeStream, true);
                        sdeDecompressionTimer.Stop();
                        _Log.LogInformation(String.Format("Finished decompressing .bz2 file '{0}' to '{1}'. Took {2} minutes to complete.", sdeTempPath, sdePath, Math.Round(sdeDecompressionTimer.Elapsed.TotalMinutes, 2).ToString("0.##")));
                    }
                    catch (Exception e)
                    {
                        _Log.LogError(String.Format("Failed to decompress sde temp file '{0}'", sdeTempPath), e);
                        if (backupExists)
                        {
                            // Change the backup back
                            File.Move(existingSDEBackupPath, sdePath);
                            _Log.LogDebug(String.Format("Moved backup SDE from '{0}' to '{1}'.", existingSDEBackupPath, sdePath));
                        }
                        throw;
                    }
                }
            }

            // Verify file extracted properly, byte size should be > 1024 at minimum
            FileInfo sdeFile = new FileInfo(sdePath);
            if (sdeFile.Length < 1024)
            {
                _Log.LogError("File extraction likely failed. File size was less than 1024 bytes.");
                sdeFile.Delete(); // Delete the file, as it's invalid
                if (backupExists)
                {
                    // Change the backup back
                    File.Move(existingSDEBackupPath, sdePath);
                    _Log.LogDebug(String.Format("Moved backup SDE from '{0}' to '{1}'.", existingSDEBackupPath, sdePath));
                }
                throw new Exception("SDE File length was under 1024 bytes.");
            }

            // Delete temp directory and all files inside
            string dirDel = new FileInfo(sdeTempPath).Directory.FullName;
            Directory.Delete(dirDel, true);
            _Log.LogDebug(String.Format("Deleted SDE download temp directory and files within '{0}'.", _SDETempFileName));

            // Delete the backup if necessary
            if (sdeExists)
            {
                if (backupExists)
                {
                    File.Delete(existingSDEBackupPath);
                    _Log.LogDebug(String.Format("Deleted old existing SDE '{0}'", existingSDEBackupPath));
                }
            }

            _Log.LogInformation(String.Format("SDE refreshed."));
        }
        #endregion

        #region SQLite DB
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

        public List<ItemType> GetItemTypesAll()
        {
            _SQLiteService = new SQLiteService(_SDEFileName);
            List<ItemType> result = new List<ItemType>();
            result = _SQLiteService.SelectMultiple<ItemType>(SQL.SDEViews.SELECT_ALL_ITEMTYPES);
            return result;
        }

        public List<ItemType> GetItemTypesSearch(string query, bool _verbose)
        {
            InitSQLiteService();
            List<ItemType> result = new List<ItemType>();
            result = _SQLiteService.SelectMultiple<ItemType>(SQL.SDEViews.SELECT_ITEMTYPES_SEARCH, new { Query = query }, _verbose);
            return result;
        }

        public List<ItemType> GetItemTypesFilter(string filter, bool _verbose)
        {
            // Split out KV Pairs
            Dictionary<string, string> filterKVPairs = filter.Split(';')
                .Select(x => x.Split('='))
                .Where(x => x.Length == 2)
                .ToDictionary(x => x.First(), x => x.Last());

            // Assemble the WHERE clause
            StringBuilder wc = new StringBuilder();
            wc.Append(" where ");
            int counter = 0;
            foreach (KeyValuePair<string, string> kvPair in filterKVPairs)
            {
                string key = kvPair.Key;
                string value = kvPair.Value;

                if (counter > 0) wc.Append(" and ");

                wc.Append(key); // " where key" or " and key"

                // Split out value in case multiple were provided
                if (value.Split(',').Length > 1) // Multiple provided
                {
                    wc.Append(" in ("); // " where key in (" or " and key in ("
                    wc.Append(value); // " where key in (value,value" or " and key in (value,value"
                    wc.Append(")"); // " where key in (value,value)" or " and key in (value,value)"
                }
                else // Single provided
                {
                    wc.Append("="); // " where key=" or " and key="
                    wc.Append(value); // " where key=value" or " and key=value"
                }
                counter++;
            }
            wc.Append(";"); // " where key=value and key in (value,value) and key=value;"

            _Log.LogDebug(String.Format("Generated WHERE clause for ItemTypes filter: '{0}'", wc.ToString()), _verbose);

            // Append WHERE clause to SELECT
            string sql = SQL.SDEViews.SELECT_BASE_ITEMTYPES + wc.ToString();

            InitSQLiteService();
            List<ItemType> result = new List<ItemType>();
            result = _SQLiteService.SelectMultiple<ItemType>(sql, null, _verbose);
            return result;
        }
        #endregion
    }
}
