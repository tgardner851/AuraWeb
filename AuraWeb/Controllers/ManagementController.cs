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

        public ManagementController(ILogger<ManagementController> logger, IConfiguration configuration)
        {
            _Log = logger;
            _Config = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ManagementPageViewModel()
            {
                
            };
            return View(model);
        }
    }
}
