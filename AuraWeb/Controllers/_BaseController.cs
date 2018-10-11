using AuraWeb.Models;
using EVEStandard;
using EVEStandard.Models.API;
using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class _BaseController : Controller
    {
        public int CharacterId
        {
            get
            {
                return Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
        }

        public AuthDTO GetAuth(EVEStandardAPI esiClient)
        {
            int characterId = Int32.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            // This only here to force checking of auth
            ESIModelDTO<EVEStandard.Models.CharacterInfo> characterInfo = esiClient.Character.GetCharacterPublicInfoV4Async(characterId).Result;

            var auth = new AuthDTO
            {
                AccessToken = new AccessTokenDetails
                {
                    AccessToken = User.FindFirstValue("AccessToken"),
                    ExpiresUtc = DateTime.Parse(User.FindFirstValue("AccessTokenExpiry")),
                    RefreshToken = User.FindFirstValue("RefreshToken")
                },
                CharacterId = characterId,
                Scopes = User.FindFirstValue("Scopes")
            };

            return auth;
        }
    }
}
