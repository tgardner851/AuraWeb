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
    }
}