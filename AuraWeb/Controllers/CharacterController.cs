using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Models;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    [Authorize]
    public class CharacterController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<CharacterController> _Log;
        private readonly EVEStandardAPI _ESIClient;
        private readonly DBService _DBService;

        public CharacterController(ILogger<CharacterController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this._ESIClient = esiClient;

            string dbFileName = _Config["DBFileName"];
            string sdeFileName = _Config["SDEFileName"];
            string sdeTempCompressedFileName = _Config["SDETempCompressedFileName"];
            string sdeTempFileName = _Config["SDETempFileName"];
            string sdeDownloadUrl = _Config["SDEDownloadURL"];
            _DBService = new DBService(_Log, dbFileName, sdeFileName, sdeTempCompressedFileName, sdeTempFileName, sdeDownloadUrl);
        }

        public async Task<ActionResult> CharacterOpenInfoWindow(int id)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));
            await _ESIClient.UserInterface.OpenInformationWindowV1Async(auth, id);
            return RedirectToAction("Index", new { id = id });
        }

        public async Task<IActionResult> Index(int id)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            Fatigue jumpFatigue = null;
            EVEStandard.Models.System locationSystem = null;
            
            if (id <= 0) // Use own Character info
            {
                id = CharacterId; // Set id and use that

                var characterJumpFatigue = await _ESIClient.Character.GetJumpFatigueV1Async(auth);
                jumpFatigue = characterJumpFatigue.Model;

                var characterLocationApi = await _ESIClient.Location.GetCharacterLocationV1Async(auth);
                CharacterLocation characterLocation = characterLocationApi.Model;
                var locationSystemApi = await _ESIClient.Universe.GetSolarSystemInfoV4Async(characterLocation.SolarSystemId);
                locationSystem = locationSystemApi.Model;
            }

            Character_Row character = _DBService.GetCharacterPublicInfo(id); //await _ESIClient.Character.GetCharacterPublicInfoV4Async(id);
            var portrait = await _ESIClient.Character.GetCharacterPortraitsV2Async(id);
            var corporation = await _ESIClient.Corporation.GetCorporationInfoV4Async((int)character.CorporationId);

            List<SkillQueueDataModel> skillsQueue = await GetSkillQueue(auth, id);

            List<CharacterBookmarkDataModel> bookmarksViewModel = await GetBookmarks(auth);
            bookmarksViewModel = bookmarksViewModel.OrderBy(x => x.Folder.Name).ToList();

            var model = new CharacterPageViewModel
            {
                Id = id,
                Character = character,
                Portrait = portrait.Model,
                Corporation = corporation.Model,
                LocationSystem = locationSystem,
                CharacterJumpFatigue = jumpFatigue,
                SkillsQueue = skillsQueue,
                Bookmarks = bookmarksViewModel
            };
            
            return View(model);
        }

        public async Task<IActionResult> KillsLosses()
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<KillmailIndex> killMailFromKillsLosses = new List<KillmailIndex>();
            var characterKillsLosses = await _ESIClient.Killmails.GetCharacterKillsAndLossesV1Async(auth, 1); // GetById all the killmail ids from page 1
            killMailFromKillsLosses.AddRange(characterKillsLosses.Model);
            if (characterKillsLosses.MaxPages > 1) // If there are multiple pages, just get it all in one go
            {
                for (int x = 2; x < characterKillsLosses.MaxPages; x++)
                {
                    var k = await _ESIClient.Killmails.GetCharacterKillsAndLossesV1Async(auth, x);
                    killMailFromKillsLosses.AddRange(k.Model);
                }
            }
            // Now that all are obtained, get the kill mail details for each
            List<Killmail> killMails = new List<Killmail>();
            for (int x = 0; x < killMailFromKillsLosses.Count; x++)
            {
                KillmailIndex killmail = killMailFromKillsLosses[x];
                var m = await _ESIClient.Killmails.GetKillmailV1Async(killmail.KillmailId, killmail.KillmailHash);
                killMails.Add(m.Model);
            }

            var model = new CharacterKillsLossesViewModel
            {
                KillMails = killMails
            };

            return View(model);
        }

        private async Task<List<CharacterBookmarkDataModel>> GetBookmarks(AuthDTO auth)
        {
            List<BookmarkFolder> bookmarkFolders = new List<BookmarkFolder>();
            var characterBookmarkFolders = await _ESIClient.Bookmarks.ListBookmarkFoldersV2Async(auth, 1);
            bookmarkFolders.AddRange(characterBookmarkFolders.Model);
            if (characterBookmarkFolders.MaxPages > 1) // If there are multiple pages, just get it all in one go
            {
                for (int x = 2; x < characterBookmarkFolders.MaxPages; x++)
                {
                    var k = await _ESIClient.Bookmarks.ListBookmarkFoldersV2Async(auth, x);
                    bookmarkFolders.AddRange(k.Model);
                }
            }

            List<Bookmark> bookmarksResult = new List<Bookmark>();
            var characterBookmarks = await _ESIClient.Bookmarks.ListBookmarksV2Async(auth, 1);
            bookmarksResult.AddRange(characterBookmarks.Model);
            if (characterBookmarks.MaxPages > 1) // If there are multiple pages, just get it all in one go
            {
                for (int x = 2; x < characterBookmarks.MaxPages; x++)
                {
                    var k = await _ESIClient.Bookmarks.ListBookmarksV2Async(auth, x);
                    bookmarksResult.AddRange(k.Model);
                }
            }
            List<BookmarkDataModel> bookmarks = new List<BookmarkDataModel>();
            // Get all Location Ids, and Type Ids 
            List<long> bookmarkLocationIds = bookmarksResult.Select(x => x.LocationId).ToList();
            List<int> bookmarkTypeIds = bookmarksResult.Where(x => x.Item != null).Select(x => x.Item.TypeId).ToList();

            // Assume the locations are stations...
            List<Station_V_Row> stations = _DBService.GetStationsLong(bookmarkLocationIds);
            List<SolarSystem_V_Row> systems = _DBService.GetSolarSystemsLong(bookmarkLocationIds);
            List<ItemType_V_Row> itemTypes = _DBService.GetItemTypes(bookmarkTypeIds);

            foreach (Bookmark b in bookmarksResult)
            {
                Station_V_Row station = stations.Where(x => x.Id == b.LocationId).FirstOrDefault();
                SolarSystem_V_Row system = systems.Where(x => x.Id == b.LocationId).FirstOrDefault();
                string itemTypeName = String.Empty;
                if (b.Item != null) itemTypeName = itemTypes.Where(x => x.Id == b.Item.TypeId).Select(x => x.Name).FirstOrDefault();

                bookmarks.Add(new BookmarkDataModel()
                {
                    Id = b.BookmarkId,
                    Coordinates = b.Coordinates,
                    Created = b.Created,
                    FolderId = b.FolderId,
                    ItemTypeId = (b.Item != null ? b.Item.TypeId : 0),
                    ItemTypeName = itemTypeName,
                    Label = b.Label,
                    LocationId = b.LocationId,
                    SystemId = system != null ? system.Id : -1,
                    SystemName = system != null ? system.Name : String.Empty,
                    StationId = station != null ? station.Id : -1,
                    StationName = station != null ? station.Name : String.Empty,
                    Notes = b.Notes
                });
            }

            List<CharacterBookmarkDataModel> bookmarksViewModel = new List<CharacterBookmarkDataModel>();
            for (int x = 0; x < bookmarkFolders.Count; x++)
            {
                List<BookmarkDataModel> folderBookmarks = bookmarks.Where(y => y.FolderId == bookmarkFolders[x].FolderId).ToList();

                bookmarksViewModel.Add(new CharacterBookmarkDataModel()
                {
                    Folder = bookmarkFolders[x],
                    Bookmarks = folderBookmarks
                });
            }
            return bookmarksViewModel;
        }

        public async Task<IActionResult> Bookmarks(string view, string folder, string name)
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<CharacterBookmarkDataModel> bookmarksViewModel = await GetBookmarks(auth);
            List<BookmarkFolder> bookmarksFolders = bookmarksViewModel.Select(x => x.Folder).ToList();
            List<string> bookmarksFoldersString = bookmarksFolders.Select(x => x.Name).Distinct().ToList();
            bookmarksFoldersString.Sort();

            if (view == null) view = "Table";
            if (folder != null && folder != "All")
            {
                bookmarksViewModel = bookmarksViewModel.Where(x => x.Folder.Name == folder).ToList();
            }
            else
            {
                folder = "All";
            }
            if (name != null)
            {
                bookmarksViewModel = bookmarksViewModel.Where(x => x.Bookmarks.Any(y => y.Label.ToLower().StartsWith(name.ToLower()) || y.Label.ToLower().EndsWith(name.ToLower()) || y.Label.ToLower().Contains(name.ToLower()))).ToList();
            }

            var model = new CharacterBookmarksViewModel
            {
                FolderNames = bookmarksFoldersString,
                View = view,
                QueryFolder = folder,
                QueryName = name,
                Bookmarks = bookmarksViewModel
            };

            return View(model);
        }

        public async Task<IActionResult> Fleet()
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            bool characterInFleet = false;
            CharacterFleetInfo fleet = null;
            try
            {
                var characterFleet = await _ESIClient.Fleets.GetCharacterFleetInfoV1Async(auth);
                fleet = characterFleet.Model;
                if (fleet != null) characterInFleet = true;
                else characterInFleet = false;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Character is not in a fleet"))
                {
                    characterInFleet = false;
                }
                characterInFleet = false; // Do it anyway
            }

            var model = new CharacterFleetViewModel()
            {
                CharacterInFleet = characterInFleet,
                Fleet = fleet
            };

            return View(model);
        }

        private async Task<List<SkillQueueDataModel>> GetSkillQueue(AuthDTO auth, int characterId)
        {
            if (characterId <= 0 || characterId != auth.CharacterId) return new List<SkillQueueDataModel>(); // This only works for current logged in character

            var characterSkillsQueueApi = await _ESIClient.Skills.GetCharacterSkillQueueV2Async(auth);
            List<SkillQueue> skillsQueueApiModel = characterSkillsQueueApi.Model;
            skillsQueueApiModel = skillsQueueApiModel.Where(x => x.FinishDate == null || x.FinishDate >= DateTime.Now.AddHours(-6)).ToList(); // Get only the skills that are completed within 6 hours from now (to current planned)
            List<SkillQueueDataModel> skillsQueue = new List<SkillQueueDataModel>();
            foreach (SkillQueue skillApi in skillsQueueApiModel)
            {
                Skill_V_Row skill = _DBService.GetSkillForIdAndSkillLevel(skillApi.SkillId, skillApi.FinishedLevel);
                skillsQueue.Add(new SkillQueueDataModel()
                {
                    Sequence = skillApi.QueuePosition,
                    Skill = skill,
                    Skill_API = skillApi
                });
            }
            skillsQueue = skillsQueue.OrderBy(x => x.Sequence).ToList();
            return skillsQueue;
        }

        public async Task<IActionResult> Skills()
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<SkillQueueDataModel> skillsQueue = await GetSkillQueue(auth, auth.CharacterId);

            var characterFinishedSkillsApi = await _ESIClient.Skills.GetCharacterSkillsV4Async(auth);
            CharacterSkills characterFinishedSkillsApiModel = characterFinishedSkillsApi.Model;
            List<SkillFinishedSkillDataModel> skills = new List<SkillFinishedSkillDataModel>();
            foreach(Skill skillApi in characterFinishedSkillsApiModel.Skills)
            {
                Skill_V_Row skill = _DBService.GetSkillForIdAndSkillLevel(skillApi.SkillId, skillApi.TrainedSkillLevel);
                skills.Add(new SkillFinishedSkillDataModel()
                {
                    Skill = skill,
                    Skill_API = skillApi
                });
            }
            SkillFinishedDataModel skillsDataModel = new SkillFinishedDataModel()
            {
                TotalSp = characterFinishedSkillsApiModel.TotalSp,
                UnallocatedSp = characterFinishedSkillsApiModel.UnallocatedSp,
                Skills = skills
            };

            var model = new CharacterSkillsViewModel()
            {
                SkillQueue = skillsQueue,
                Skills = skillsDataModel
            };

            return View(model);
        }

        public async Task<IActionResult> Assets()
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<AssetDataModel> assets = new List<AssetDataModel>();
            List<Asset> assets_api = new List<Asset>();
            var assetsApi = await _ESIClient.Assets.GetCharacterAssetsV3Async(auth, 1);
            var assetsApiModel = assetsApi.Model;
            assets_api = assetsApiModel;
            if (assetsApi.MaxPages > 1)
            {
                for(int x = 2; x < assetsApi.MaxPages; x++)
                {
                    assetsApi = await _ESIClient.Assets.GetCharacterAssetsV3Async(auth, x);
                    assets_api.AddRange(assetsApi.Model);
                }
            }
            // Get all ItemTypes, Systems, and Stations at once (quicker)
            List<int> itemTypeIds = assets_api.Select(x => x.TypeId).Distinct().ToList();
            List<ItemType_V_Row> itemTypes = _DBService.GetItemTypes(itemTypeIds);
            List<int> locationIds = assets_api.Select(x => (int)x.LocationId).Distinct().ToList();
            List<SolarSystem_V_Row> solarSystems = _DBService.GetSolarSystems(locationIds);
            List<Station_V_Row> stations = _DBService.GetStations(locationIds);
            for(int x = 0; x < assets_api.Count; x++)
            {
                Asset asset = assets_api[x];
                ItemType_V_Row itemType = itemTypes.Where(b => b.Id == asset.TypeId).FirstOrDefault();
                SolarSystem_V_Row system = null;
                Station_V_Row station = null;
                if (asset.LocationType == EVEStandard.Enumerations.LocationTypeEnum.solar_system)
                {
                    system = solarSystems.Where(b => b.Id == (int)asset.LocationId).FirstOrDefault();
                }
                else if (asset.LocationType == EVEStandard.Enumerations.LocationTypeEnum.station)
                {
                    station = stations.Where(b => b.Id == (int)asset.LocationId).FirstOrDefault();
                }
                AssetDataModel a = new AssetDataModel()
                {
                    Asset_API = asset,
                    ItemType = itemType,
                    System = system,
                    Station = station
                };
                assets.Add(a);
            }

            var model = new AssetsPageViewModel()
            {
                Assets = assets
            };

            return View(model);
        }

        public async Task<IActionResult> Wallet()
        {
            AuthDTO auth = GetAuth(_ESIClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<CharacterWalletJournal> walletJournalEntries = new List<CharacterWalletJournal>();
            // TODO: THE BELOW IS UNFORTUNATELY FUCKING BROKEN
            /*
            var walletJournalApi = await _ESIClient.Wallet.GetCharacterWalletJournalV4Async(auth, 1);
            walletJournalEntries = walletJournalApi.Model;
            if (walletJournalApi.MaxPages > 1)
            {
                for (int x = 2; x < walletJournalApi.MaxPages; x++)
                {
                    walletJournalApi = await _ESIClient.Wallet.GetCharacterWalletJournalV4Async(auth, x);
                    walletJournalEntries.AddRange(walletJournalApi.Model);
                }
            }
            */

            List<WalletTransaction> walletTransactions = new List<WalletTransaction>();
            var walletTransactionsApi = await _ESIClient.Wallet.GetCharacterWalletTransactionsV1Async(auth, 0);
            walletTransactions = walletTransactionsApi.Model;
            if (walletTransactionsApi.MaxPages > 1)
            {
                for (int x = 2; x < walletTransactionsApi.MaxPages; x++)
                {
                    walletTransactionsApi = await _ESIClient.Wallet.GetCharacterWalletTransactionsV1Async(auth, x);
                    walletTransactions.AddRange(walletTransactionsApi.Model);
                }
            }

            var model = new WalletPageViewModel()
            {
                Journal = walletJournalEntries,
                Transactions = walletTransactions
            };

            return View(model);
        }
    }
}