using AuraWeb.Models;
using EVEStandard;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    [Authorize]
    public class SecureController : _BaseController
    {
        private readonly EVEStandardAPI esiClient;

        public SecureController(EVEStandardAPI esiClient)
        {
            this.esiClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            AuthDTO auth = GetAuth(esiClient);

            var characterInfo = await esiClient.Character.GetCharacterPublicInfoV4Async(CharacterId);
            var corporationInfo = await esiClient.Corporation.GetCorporationInfoV4Async((int)characterInfo.Model.CorporationId);
            var locationInfo = await esiClient.Location.GetCharacterLocationV1Async(auth);
            var location = await esiClient.Universe.GetSolarSystemInfoV4Async(locationInfo.Model.SolarSystemId);

            var model = new SecurePageViewModel
            {
                CharacterName = characterInfo.Model.Name,
                CorporationName = corporationInfo.Model.Name,
                CharacterLocation = location.Model.Name
            };

            return View(model);
        }
    }
}