using AuraWeb.Models;
using AuraWeb.Services;
using EVEStandard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class ItemTypeController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<ItemTypeController> _Log;
        private readonly EVEStandardAPI _ESIClient;
        private readonly string _SDEFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadUrl;
        private readonly SDEService _SDEService;

        public ItemTypeController(ILogger<ItemTypeController> logger, IConfiguration configuration, EVEStandardAPI esiClient)
        {
            _Log = logger;
            _Config = configuration;
            _SDEFileName = _Config["SDEFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadUrl = _Config["SDEDownloadURL"];
            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempFileName, _SDEDownloadUrl);
            this._ESIClient = esiClient;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ItemTypePageViewModel
            {

            };

            return View(model);
        }

        public async Task<IActionResult> ItemTypeInfo(int id)
        {
            ItemType_V_Row itemType = _SDEService.GetItemType(id);

            // TODO: SDE views don't handle this right now
            var itemTypeApi = await _ESIClient.Universe.GetTypeInfoV3Async(id);
            EVEStandard.Models.Type itemTypeApiModel = itemTypeApi.Model;

            var model = new ItemTypeInfoPageViewModel
            {
                ItemType = itemType,
                ItemType_API = itemTypeApiModel
            };

            return View(model);
        }
    }
}
