using EVEStandard;
using EVEStandard.Models.SSO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly EVEStandardAPI _ESIClient;

        private static string SSOStateKey = "SSOState";

        public AuthController(EVEStandardAPI esiClient)
        {
            this._ESIClient = esiClient;
        }

        public IActionResult Login(string returnUrl = null)
        {
            // Scopes are required for API calls but not for authentication, dummy scope is inserted to workaround an issue in the library
            var scopes = new List<string>()
            {
                "publicData",
                "esi-calendar.respond_calendar_events.v1",
                "esi-calendar.read_calendar_events.v1",
                "esi-location.read_location.v1",
                "esi-location.read_ship_type.v1",
                "esi-mail.organize_mail.v1",
                "esi-mail.read_mail.v1",
                "esi-mail.send_mail.v1",
                "esi-skills.read_skills.v1",
                "esi-skills.read_skillqueue.v1",
                "esi-wallet.read_character_wallet.v1",
                "esi-wallet.read_corporation_wallet.v1",
                "esi-search.search_structures.v1",
                "esi-clones.read_clones.v1",
                "esi-characters.read_contacts.v1",
                "esi-universe.read_structures.v1",
                "esi-bookmarks.read_character_bookmarks.v1",
                "esi-killmails.read_killmails.v1",
                "esi-corporations.read_corporation_membership.v1",
                "esi-assets.read_assets.v1",
                "esi-planets.manage_planets.v1",
                "esi-fleets.read_fleet.v1",
                "esi-fleets.write_fleet.v1",
                "esi-ui.open_window.v1",
                "esi-ui.write_waypoint.v1",
                "esi-characters.write_contacts.v1",
                "esi-fittings.read_fittings.v1",
                "esi-fittings.write_fittings.v1",
                "esi-markets.structure_markets.v1",
                "esi-corporations.read_structures.v1",
                "esi-corporations.write_structures.v1",
                "esi-characters.read_loyalty.v1",
                "esi-characters.read_opportunities.v1",
                "esi-characters.read_chat_channels.v1",
                "esi-characters.read_medals.v1",
                "esi-characters.read_standings.v1",
                "esi-characters.read_agents_research.v1",
                "esi-industry.read_character_jobs.v1",
                "esi-markets.read_character_orders.v1",
                "esi-characters.read_blueprints.v1",
                "esi-characters.read_corporation_roles.v1",
                "esi-location.read_online.v1",
                "esi-contracts.read_character_contracts.v1",
                "esi-clones.read_implants.v1",
                "esi-characters.read_fatigue.v1",
                "esi-killmails.read_corporation_killmails.v1",
                "esi-corporations.track_members.v1",
                "esi-wallet.read_corporation_wallets.v1",
                "esi-characters.read_notifications.v1",
                "esi-corporations.read_divisions.v1",
                "esi-corporations.read_contacts.v1",
                "esi-assets.read_corporation_assets.v1",
                "esi-corporations.read_titles.v1",
                "esi-corporations.read_blueprints.v1",
                "esi-bookmarks.read_corporation_bookmarks.v1",
                "esi-contracts.read_corporation_contracts.v1",
                "esi-corporations.read_standings.v1",
                "esi-corporations.read_starbases.v1",
                "esi-industry.read_corporation_jobs.v1",
                "esi-markets.read_corporation_orders.v1",
                "esi-corporations.read_container_logs.v1",
                "esi-industry.read_character_mining.v1",
                "esi-industry.read_corporation_mining.v1",
                "esi-planets.read_customs_offices.v1",
                "esi-corporations.read_facilities.v1",
                "esi-corporations.read_medals.v1",
                "esi-characters.read_titles.v1",
                "esi-alliances.read_contacts.v1",
                "esi-characters.read_fw_stats.v1",
                "esi-corporations.read_fw_stats.v1",
                "esi-corporations.read_outposts.v1",
                "esi-characterstats.read.v1"
            };

            string state;

            if (!String.IsNullOrEmpty(returnUrl))
            {
                state = Base64UrlTextEncoder.Encode(Encoding.ASCII.GetBytes(returnUrl));
            }
            else
            {
                state = Guid.NewGuid().ToString();
            }

            HttpContext.Session.SetString(SSOStateKey, state);

            var authorization = _ESIClient.SSO.AuthorizeToEVEUri(scopes, state);
            return Redirect(authorization.SignInURI);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Callback(string code, string state)
        {
            // BIG NO-NO BELOW, COMMENTED OUT CODE BECAUSE THE SESSION WAS CHANGING BETWEEN AUTHORIZATION!
            var authorization = new Authorization
            {
                AuthorizationCode = code,
                ExpectedState = state, //HttpContext.Session.GetString(SSOStateKey),
                ReturnedState = state
            };

            var accessToken = await _ESIClient.SSO.VerifyAuthorizationAsync(authorization);
            var character = await _ESIClient.SSO.GetCharacterDetailsAsync(accessToken.AccessToken);

            await SignInAsync(accessToken, character);
                       
            if (Guid.TryParse(state, out Guid stateGuid))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var returnUrl = Encoding.ASCII.GetString(Base64UrlTextEncoder.Decode(state));
                return Redirect(returnUrl);
            }            
        }

        private async Task SignInAsync(AccessTokenDetails accessToken, CharacterDetails character)
        {
            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, character.CharacterId.ToString()),
                new Claim(ClaimTypes.Name, character.CharacterName),
                new Claim("AccessToken", accessToken.AccessToken),
                new Claim("RefreshToken", accessToken.RefreshToken ?? ""),
                new Claim("AccessTokenExpiry", accessToken.ExpiresUtc.ToString()),
                new Claim("Scopes", character.Scopes)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddHours(24) });
        }
    }
}