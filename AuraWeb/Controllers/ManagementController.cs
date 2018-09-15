using AuraWeb.Models;
using AuraWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuraWeb.Controllers
{
    public class ManagementController : _BaseController
    {
        private readonly IConfiguration _Config;
        private readonly ILogger<ManagementController> _Log;
        private readonly SDEService _SDEService;

        public ManagementController(ILogger<ManagementController> logger, IConfiguration configuration)
        {
            _Log = logger;
            _Config = configuration;
            _SDEService = new SDEService(_Log, _Config["SDEFileName"], _Config["SDETempFileName"]);
        }

        public async Task<IActionResult> Index()
        {
            var model = new ManagementPageViewModel()
            {
                SDEExists = _SDEService.SDEExists()
            };
            return View(model);
        }

        public ActionResult RefreshSDE()
        {
            _SDEService.Download(_Config["SDE_DOWNLOAD_URL"]);
            return View();
        }
    }
}
