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
        private readonly string _SDEFileName;
        private readonly string _SDETempFileName;
        private readonly string _SDEDownloadURL;

        public ManagementController(ILogger<ManagementController> logger, IConfiguration configuration)
        {
            _Log = logger;
            _Config = configuration;

            _SDEFileName = _Config["SDEFileName"];
            _SDETempFileName = _Config["SDETempFileName"];
            _SDEDownloadURL = _Config["SDEDownloadURL"];

            _SDEService = new SDEService(_Log, _SDEFileName, _SDETempFileName);
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
            bool sdeInitialized = false;
            try
            {
                _SDEService.Initialize(_SDEDownloadURL);
                sdeInitialized = true;
            }
            catch(Exception e)
            {
                sdeInitialized = false;
            }
            var model = new ManagementPageViewModel()
            {
                SDEExists = _SDEService.SDEExists(),
                SDEInitialized = sdeInitialized
            };
            return View("Index", model);
        }
    }
}
