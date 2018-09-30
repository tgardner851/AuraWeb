using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AuraWeb.Services
{
    public class IECService
    {
        private readonly ILogger _Log;
        private readonly string _IECPath;
        private readonly string _IECDownloadUrl;
        private readonly string _WebRoutePath;

        public IECService(ILogger logger, string iecPath, string iecDownloadUrl, string webRoutePath)
        {
            _Log = logger;
            _IECPath = iecPath;
            _IECDownloadUrl = iecDownloadUrl;
            _WebRoutePath = webRoutePath;
        }

        public void DownloadIEC()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            _Log.LogInformation(String.Format("Beginning download process for IEC at URL '{0}'...", _IECDownloadUrl));

            // Create the directory if needed
            Directory.CreateDirectory(_IECPath);
            _Log.LogDebug(String.Format("Created directory '{0}'", _IECPath));
            // Create the temp directory
            string tempDir = Path.Combine(_IECPath, "tmp");

            // Delete temp dir
            if (new DirectoryInfo(tempDir).Exists)
            {
                try
                {
                    Directory.Delete(tempDir, true);
                    _Log.LogDebug(String.Format("Deleted temp directory '{0}'.", tempDir));
                }
                catch (Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to delete temp directory '{0}'.", tempDir));
                    throw;
                }
            }

            // Create the temp dir
            Directory.CreateDirectory(tempDir);
            _Log.LogDebug(String.Format("Created temp directory '{0}'", tempDir));

            _Log.LogInformation(String.Format("Checking IEC for URL '{0}'...", _IECDownloadUrl));

            // Unfortunately have to parse the page to find the links
            List<KeyValuePair<string, string>> iecCategories = new List<KeyValuePair<string, string>>();
            using (var hc = new HttpClient())
            {
                HttpResponseMessage response = hc.GetAsync(_IECDownloadUrl).Result;
                Stream stream = response.Content.ReadAsStreamAsync().Result;
                HtmlDocument _WebDocument = new HtmlDocument();
                _WebDocument.Load(stream);

                HtmlNodeCollection _webnodes = _WebDocument.DocumentNode.SelectNodes("//div[contains(@class,'content')]");

                if (_webnodes.Count >= 2)
                {
                    HtmlNode node = _webnodes[1];
                    bool iecHeaderFound = false;
                    for (int x = 0; x < node.ChildNodes.Count; x++)
                    {
                        HtmlNode innerNode = node.ChildNodes[x];
                        // Find the header, since everything is a big un-nested list
                        if(innerNode.Name == "h3" && innerNode.InnerText == "Image Export Collection (IEC)")
                        {
                            iecHeaderFound = true;
                        }
                        if(iecHeaderFound && innerNode.Name == "ul")
                        {
                            List<HtmlNode> iecLinkContainers = innerNode.ChildNodes.Where(a => a.Name == "li").ToList();
                            for(int y = 0; y < iecLinkContainers.Count; y++)
                            {
                                HtmlNode iecLinkContainer = iecLinkContainers[y];
                                if (iecLinkContainer.ChildNodes != null && iecLinkContainer.ChildNodes.Count > 0 && iecLinkContainer.ChildNodes[0].Name == "a")
                                {
                                    string title = iecLinkContainer.InnerText;
                                    string link = iecLinkContainer.ChildNodes[0].Attributes.Where(a => a.Name == "href").Select(a=>a.Value).FirstOrDefault();

                                    if (title.ToLower().Contains("icons"))
                                    {
                                        iecCategories.Add(new KeyValuePair<string, string>("icons", link));
                                    }
                                    else if (title.ToLower().Contains("renders"))
                                    {
                                        iecCategories.Add(new KeyValuePair<string, string>("renders", link));
                                    }
                                    else if (title.ToLower().Contains("types"))
                                    {
                                        iecCategories.Add(new KeyValuePair<string, string>("types", link));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _Log.LogInformation(String.Format("Found {0} categories to download.", iecCategories.Count));

            _Log.LogInformation(String.Format("Starting download for all ({0}) IEC categories...", iecCategories.Count));

            // Download and unzip files
            foreach(KeyValuePair<string, string> category in iecCategories)
            {
                string fileName = String.Format("{0}.zip", category.Key);
                string fullFileName = Path.Combine(tempDir, fileName);
                string fullFileDir = new FileInfo(fullFileName).Directory.FullName;

                // Download
                try
                {
                    _Log.LogInformation(String.Format("Downloading file '{0}' from address '{1}'.", fullFileName, category.Value));
                    Stopwatch catSw = new Stopwatch();
                    catSw.Start();
                    Downloader dl = new Downloader(category.Value, fullFileName);
                    dl.DownloadFile();
                    catSw.Stop();
                    _Log.LogInformation(String.Format("Finished downloading file '{0}' from address '{1}'. Took {2} seconds.", fullFileName, category.Value, Math.Round(catSw.Elapsed.TotalSeconds, 2).ToString("0.##")));
                }
                catch(Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to download IEC Category '{0}' from address '{1}'.", category.Key, category.Value));
                    throw; // Do not continue
                }

                // Unzip
                try
                {
                    Stopwatch decompressSw = new Stopwatch();
                    decompressSw.Start();
                    _Log.LogInformation(String.Format("Decompressing .zip file '{0}' to directory '{1}'...", fullFileName, fullFileDir));
                    ZipFile.ExtractToDirectory(fullFileName, fullFileDir);
                    decompressSw.Stop();
                    _Log.LogInformation(String.Format("Finished decompressing contents from file '{0}'. Took {1} seconds.", fullFileName, Math.Round(decompressSw.Elapsed.TotalSeconds, 2).ToString("0.##")));
                }
                catch (Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to decompress file '{0}' to directory '{1}'", fullFileName, fullFileDir));
                    throw; // Do not continue
                }

                // Delete .zip file
                try
                {
                    File.Delete(fullFileName);
                    _Log.LogDebug(String.Format("Deleted compressed file '{0}'.", fullFileName));
                }
                catch(Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to delete compressed file '{0}'. Will continue.", fullFileName));
                }
            }

            // Delete old files
            _Log.LogInformation("Removing existing IEC directories...");
            DirectoryInfo existingDir = new DirectoryInfo(_IECPath);
            List<DirectoryInfo> existingDirs = existingDir.EnumerateDirectories().ToList();
            DirectoryInfo tempDirInfo = new DirectoryInfo(tempDir);
            existingDirs = existingDirs.Where(x => x.Name != tempDirInfo.Name).ToList(); // Remove temp dir from this
            // Delete all
            foreach(DirectoryInfo di in existingDirs)
            {
                try
                {
                    Directory.Delete(di.FullName, true);
                    _Log.LogDebug(String.Format("Deleted directory '{0}'.", di.FullName));
                }
                catch(Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to delete directory '{0}'.", di.FullName));
                    throw; // Enough to fail
                }
            }

            // Move the files
            _Log.LogInformation("Moving newer IEC assets from '{0}' to '{1}'...", tempDir, _IECPath);
            List<DirectoryInfo> tempDirs = tempDirInfo.EnumerateDirectories().ToList();
            foreach(DirectoryInfo di in tempDirs)
            {
                string newPath = Path.Combine(_IECPath, di.Name);
                try
                {
                    Directory.Move(di.FullName, newPath);
                    _Log.LogDebug(String.Format("Moved directory '{0}' to '{1}'.", di.FullName, newPath));
                }
                catch(Exception e)
                {
                    _Log.LogError(e, String.Format("Failed to move directory from '{0}' to {1}.", di.FullName, newPath));
                    throw; // Enough to fail
                }
            }

            // Delete temp dir
            try
            {
                Directory.Delete(tempDir, true);
                _Log.LogDebug(String.Format("Deleted temp directory '{0}'.", tempDir));
            }
            catch(Exception e)
            {
                _Log.LogError(e, String.Format("Failed to delete temp directory '{0}'. Will continue.", tempDir));
            }

            // Now copy all the files to the deployed directory
            DirectoryInfo iceDirInfo = new DirectoryInfo(_IECPath);
            List<DirectoryInfo> iceDirs = iceDirInfo.EnumerateDirectories().ToList();
            string deployedPath = Path.Combine(_WebRoutePath, "images");
            deployedPath = Path.Combine(deployedPath, "iec");

            Directory.CreateDirectory(deployedPath);
            _Log.LogDebug(String.Format("Created directory in deployed path underneath wwwroot/images at '{0}'.", deployedPath));

            CopyAll(iceDirInfo, new DirectoryInfo(deployedPath));
            _Log.LogDebug(String.Format("Finished copying all files from '{0}' to '{1}'.", iceDirInfo.FullName, deployedPath));

            sw.Stop();
            _Log.LogInformation(String.Format("IEC refreshed. Entire process took {0} minutes.", sw.Elapsed.TotalMinutes.ToString("##.##")));
        }

        public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                _Log.LogDebug(@"Copying IEC file from '{0}' to '{1}\{2}'", source.FullName, target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
