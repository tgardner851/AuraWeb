using AuraWeb.Models;
using EVEStandard;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    [Authorize]
    public class CharacterInfoController : _BaseController
    {
        private readonly ILogger<HomeController> _Log;
        private readonly EVEStandardAPI esiClient;

        public CharacterInfoController(ILogger<HomeController> logger, EVEStandardAPI esiClient)
        {
            _Log = logger;
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

            var model = new CharacterInfoPageViewModel
            {
                CharacterName = characterInfo.Model.Name,
                CorporationName = corporationInfo.Model.Name,
                CharacterLocation = location.Model.Name,
                CharacterPortrait = characterPortrait.Model.Px64x64
            };

            return View(model);
        }
    }
}