using AuraWeb.Models;
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    [Authorize]
    public class CharacterController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<HomeController> _Log;
        private readonly EVEStandardAPI esiClient;

        public CharacterController(ILogger<HomeController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            this.esiClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            var characterInfo = await esiClient.Character.GetCharacterPublicInfoV4Async(CharacterId);
            var corporationInfo = await esiClient.Corporation.GetCorporationInfoV4Async((int)characterInfo.Model.CorporationId);
            var locationInfo = await esiClient.Location.GetCharacterLocationV1Async(auth);
            var location = await esiClient.Universe.GetSolarSystemInfoV4Async(locationInfo.Model.SolarSystemId);

            var characterPortrait = await esiClient.Character.GetCharacterPortraitsV2Async(CharacterId);

            var model = new CharacterPageViewModel
            {
                CharacterName = characterInfo.Model.Name,
                CorporationName = corporationInfo.Model.Name,
                CharacterLocation = location.Model.Name,
                CharacterPortrait = characterPortrait.Model.Px512x512
            };

            return View(model);
        }

        public async Task<IActionResult> KillsLosses()
        {
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<KillmailIndex> killMailFromKillsLosses = new List<KillmailIndex>();
            var characterKillsLosses = await esiClient.Killmails.GetCharacterKillsAndLossesV1Async(auth, 1); // Get all the killmail ids from page 1
            killMailFromKillsLosses.AddRange(characterKillsLosses.Model);
            if (characterKillsLosses.MaxPages > 1) // If there are multiple pages, just get it all in one go
            {
                for (int x = 2; x < characterKillsLosses.MaxPages; x++)
                {
                    var k = await esiClient.Killmails.GetCharacterKillsAndLossesV1Async(auth, x);
                    killMailFromKillsLosses.AddRange(k.Model);
                }
            }
            // Now that all are obtained, get the kill mail details for each
            List<Killmail> killMails = new List<Killmail>();
            for (int x = 0; x < killMailFromKillsLosses.Count; x++)
            {
                KillmailIndex killmail = killMailFromKillsLosses[x];
                var m = await esiClient.Killmails.GetKillmailV1Async(killmail.KillmailId, killmail.KillmailHash);
                killMails.Add(m.Model);
            }

            var model = new CharacterKillsLossesViewModel
            {
                KillMails = killMails
            };

            return View(model);
        }

        public async Task<IActionResult> Bookmarks()
        {
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            List<BookmarkFolder> bookmarkFolders = new List<BookmarkFolder>();
            var characterBookmarkFolders = await esiClient.Bookmarks.ListBookmarkFoldersV2Async(auth, 1);
            bookmarkFolders.AddRange(characterBookmarkFolders.Model);
            if (characterBookmarkFolders.MaxPages > 1) // If there are multiple pages, just get it all in one go
            {
                for (int x = 2; x < characterBookmarkFolders.MaxPages; x++)
                {
                    var k = await esiClient.Bookmarks.ListBookmarkFoldersV2Async(auth, x);
                    bookmarkFolders.AddRange(k.Model);
                }
            }

            List<Bookmark> bookmarks = new List<Bookmark>();
            var characterBookmarks = await esiClient.Bookmarks.ListBookmarksV2Async(auth, 1);
            bookmarks.AddRange(characterBookmarks.Model);
            if (characterBookmarks.MaxPages > 1) // If there are multiple pages, just get it all in one go
            {
                for (int x = 2; x < characterBookmarks.MaxPages; x++)
                {
                    var k = await esiClient.Bookmarks.ListBookmarksV2Async(auth, x);
                    bookmarks.AddRange(k.Model);
                }
            }
            List<CharacterBookmarkDataModel> bookmarksViewModel = new List<CharacterBookmarkDataModel>();
            for(int x = 0; x < bookmarkFolders.Count; x++)
            {
                List<Bookmark> folderBookmarks = bookmarks.Where(y => y.FolderId == bookmarkFolders[x].FolderId).ToList();
                bookmarksViewModel.Add(new CharacterBookmarkDataModel()
                {
                    Folder = bookmarkFolders[x],
                    Bookmarks = folderBookmarks
                });
            }

            var model = new CharacterBookmarksViewModel
            {
                BookmarkFolders = bookmarksViewModel
            };

            return View(model);
        }

        public async Task<IActionResult> Fleet()
        {
            AuthDTO auth = GetAuth(esiClient);
            _Log.LogDebug(String.Format("Logged in to retrieve Character Info for Character Id: {0}", auth.CharacterId));

            bool characterInFleet = false;
            CharacterFleetInfo fleet = null;
            try
            {
                var characterFleet = await esiClient.Fleets.GetCharacterFleetInfoV1Async(auth);
                fleet = characterFleet.Model;
                if (fleet != null) characterInFleet = true;
                else characterInFleet = false;
            }
            catch(Exception e)
            {
                if(e.Message.Contains("Character is not in a fleet"))
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
    }
}