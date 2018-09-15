using AuraWeb.Models;
using ICSharpCode.SharpZipLib.BZip2;
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
        private SQLiteService _SQLiteService;
        private string _SDEFileName;

        public SDEService()
        {
            _SDEFileName = Common.SQLiteSDEPath;
        }

        #region Download
        public void Download(string refreshTempDir, string sdeAddress, bool _verbose)
        {
            // Get the filename to download to (with path). Make sure to use same path as config and exe
            string sdePath = _SDEFileName;
            string sdeTempPath = Common.SQLiteSDETempExtractionPath + "sde.sqlite";
            if (!String.IsNullOrWhiteSpace(refreshTempDir)) // Use the temp dir
            {
                sdeTempPath = refreshTempDir;
            }

            //Out.Info(String.Format("Downloading SDE from URL '{0}' to temp file '{1}'...", sdeAddress, sdeTempPath), _verbose);

            // Create the temp directory if needed
            string sdeTempDir = new FileInfo(sdeTempPath).Directory.FullName;
            Directory.CreateDirectory(sdeTempDir);
            //Out.Debug(String.Format("Created temp directory '{0}'.", sdeTempDir), _verbose);

            // Check if the SDE exists and make a backup
            bool sdeExists = false;
            string existingSDEBackupPath = new FileInfo(sdePath).Directory.FullName + "\\sde.sqlite.backup";
            // Rename existing DB as a backup, if it exists
            if (File.Exists(sdePath)) // Existing SDE
            {
                sdeExists = true;
                File.Move(sdePath, existingSDEBackupPath);
                //Out.Debug(String.Format("Moved existing SDE from '{0}' to '{1}'.", sdePath, existingSDEBackupPath), _verbose);
            }

            // Attempt the download synchronously
            try
            {
                //Out.Info(String.Format("Downloading file '{0}' to '{1}'.", sdeAddress, sdeTempPath), _verbose);
                Downloader dl = new Downloader(sdeAddress, sdeTempPath, _verbose);
                dl.DownloadFile();
            }
            catch (Exception e)
            {
                //Out.Error(String.Format("Failed to download SDE from address '{0}' to temp path '{1}'", sdeAddress, sdeTempPath), _verbose, e);
                // Change the backup back
                File.Move(existingSDEBackupPath, sdePath);
                //Out.Debug(String.Format("Moved backup SDE from '{0}' to '{1}'.", existingSDEBackupPath, sdePath), _verbose);
                return;
            }
            //Out.Info(String.Format("Finished downloading SDE from URL '{0}' to temp file '{1}'. Took {2} minutes to complete.", sdeAddress, sdeTempPath, Math.Round(sdeDownloadTimer.Elapsed.TotalMinutes, 2).ToString("0.##")), _verbose);

            // File should be .bz2
            FileInfo sdeTemp = new FileInfo(sdeTempPath);

            // Decompress!
            using (FileStream sdeTempStream = sdeTemp.OpenRead())
            {
                using (FileStream sdeStream = File.Create(sdePath))
                {
                    try
                    {
                        //Out.Info(String.Format("Decompressing .bz2 file '{0}' to '{1}'...", sdeTempPath, sdePath), _verbose);
                        Stopwatch sdeDecompressionTimer = new Stopwatch();
                        sdeDecompressionTimer.Start();
                        BZip2.Decompress(sdeTempStream, sdeStream, true);
                        sdeDecompressionTimer.Stop();
                        //Out.Info(String.Format("Finished decompressing .bz2 file '{0}' to '{1}'. Took {2} minutes to complete.", sdeTempPath, sdePath, Math.Round(sdeDecompressionTimer.Elapsed.TotalMinutes, 2).ToString("0.##")), _verbose);
                    }
                    catch (Exception e)
                    {
                        //Out.Error(String.Format("Failed to decompress sde temp file '{0}'", sdeTempPath), _verbose, e);
                        // Change the backup back
                        File.Move(existingSDEBackupPath, sdePath);
                        //Out.Debug(String.Format("Moved backup SDE from '{0}' to '{1}'.", existingSDEBackupPath, sdePath), _verbose);
                        return;
                    }
                }
            }

            // Verify file extracted properly, byte size should be > 1024 at minimum
            FileInfo sdeFile = new FileInfo(sdePath);
            if (sdeFile.Length < 1024)
            {
                //Out.Error("File extraction likely failed. File size was less than 1024 bytes.", _verbose);
                sdeFile.Delete(); // Delete the file, as it's invalid
                                  // Change the backup back
                File.Move(existingSDEBackupPath, sdePath);
                //Out.Debug(String.Format("Moved backup SDE from '{0}' to '{1}'.", existingSDEBackupPath, sdePath), _verbose);
                return;
            }

            // Delete temp directory and all files inside
            string dirDel = new FileInfo(sdeTempPath).Directory.FullName;
            Directory.Delete(dirDel, true);
            //Out.Debug(String.Format("Deleted SDE download temp directory and files within '{0}'.", Common.SQLiteSDETempExtractionPath), _verbose);

            // Delete the backup if necessary
            if (sdeExists)
            {
                File.Delete(existingSDEBackupPath);
                //Out.Debug(String.Format("Deleted old existing SDE '{0}'", existingSDEBackupPath), _verbose);
            }

            //Out.Info(String.Format("SDE refreshed. Took {0} minutes.", Math.Round(sdeRefreshTimer.Elapsed.TotalMinutes, 2).ToString("0.##")), _verbose);
        }
        #endregion

        #region SQLite DB
        private void InitSQLiteService()
        {
            _SQLiteService = new SQLiteService(_SDEFileName);
        }

        public void CreateViews(bool _verbose)
        {
            List<string> viewCreationSQLSequence = SQL.SDEViews.Sequence;
            //Out.Debug(String.Format("Will execute {0} SQL scripts. Starting...", viewCreationSQLSequence.Count), _verbose);
            _SQLiteService = new SQLiteService(_SDEFileName);
            _SQLiteService.ExecuteMultiple(viewCreationSQLSequence);
            //Out.Info("Created SDE Views.", _verbose);
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

            //Out.Debug(String.Format("Generated WHERE clause for ItemTypes filter: '{0}'", wc.ToString()), _verbose);

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
