using AuraWeb.Services;
using EVEStandard;
using EVEStandard.Enumerations;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security;

namespace AuraWeb
{
    public class Startup
    {
        public Startup(ILogger<Startup> logger, IConfiguration configuration, IHostingEnvironment env)
        {
            Logger = logger;
            Configuration = configuration;
            HostingEnv = env;
        }

        public ILogger<Startup> Logger { get; set; }
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnv { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add cookie authentication and set the login url
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                });

            // Register your application at: https://developers.eveonline.com/applications to obtain client ID and secret key and add them to user secrets
            // by right-clicking the solution and selecting Manage User Secrets.
            // Also, modify the callback URL in appsettings.json to match with your environment.

            // Initialize the client
            var esiClient = new EVEStandardAPI(
                    "AuraWeb",                      // User agent
                    DataSource.Tranquility,         // Server [Tranquility/Singularity]
                    TimeSpan.FromSeconds(30),       // Timeout
                    Configuration["SSOCallbackUrl"],
                    Configuration["ClientId"],
                    Configuration["SecretKey"]);

            // Register with DI container
            services.AddSingleton<EVEStandardAPI>(esiClient);

            services.AddDistributedMemoryCache();

            // Session is required 
            services.AddSession();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddHangfire(c => c.UseMemoryStorage());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });
            app.UseCookiePolicy();

            app.UseAuthentication();
            app.UseSession();

            // Localization
            // https://stackoverflow.com/questions/38945076/number-format-does-not-work-in-asp-net-core
            var dtf = new DateTimeFormatInfo
            {
                ShortDatePattern = "yyyy-MM-dd",
                LongDatePattern = "yyyy-MM-dd HH:mm",
                ShortTimePattern = "HH:mm",
                LongTimePattern = "HH:mm"
            };
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US") { DateTimeFormat = dtf },
                new CultureInfo("en") { DateTimeFormat = dtf }
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            #region Hangfire
            GlobalConfiguration.Configuration.UseMemoryStorage();
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                WorkerCount = 1
            });
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });

            // Setup tasks for recurring schedules through Hangfire
            #region Recurring Jobs
            // SDE Downloader
            RecurringJob.AddOrUpdate(
                () => DownloadSDE(),
                Cron.Daily(23)); // Run every night at 11PM
            // Market Downloader
            RecurringJob.AddOrUpdate(
                () => DownloadMarketData(),
                Cron.HourInterval(2)); // Run every two hours
            // Market Downloader for Jita
            RecurringJob.AddOrUpdate(
                () => DownloadMarketDataForJita(),
                Cron.MinuteInterval(15)); // Run every 15 minutes
            // IEC Downloader
            RecurringJob.AddOrUpdate(
                () => DownloadIEC(),
                Cron.Monthly(2)); // Run on the 2nd day of every month
            #endregion
            #endregion

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        #region Recurring Tasks
        [AutomaticRetry(Attempts = 2)]
        public void DownloadSDE()
        {
            string sdeFileName = Configuration["SDEFileName"];
            string sdeBackupFileName = Configuration["SDEBackupFileName"];
            string sdeTempCompressedFileName = Configuration["SDETempCompressedFileName"];
            string sdeTempFileName = Configuration["SDETempFileName"];
            string sdeDownloadUrl = Configuration["SDEDownloadURL"];
            
            SDEService _SDEService = new SDEService(Logger, sdeFileName, sdeTempCompressedFileName, sdeTempFileName, sdeBackupFileName, sdeDownloadUrl);
            _SDEService.Initialize();
        }

        [AutomaticRetry(Attempts = 1)]
        public void DownloadMarketData()
        {
            string marketDbPath = Configuration["MarketFileName"];
            MarketService _MarketService = new MarketService(Logger, marketDbPath);
            _MarketService.DownloadMarket();
        }

        // TODO: Fix
        [AutomaticRetry(Attempts = 0)]
        public void DownloadMarketDataForJita()
        {
            string marketDbPath = Configuration["MarketFileName"];
            MarketService _MarketService = new MarketService(Logger, marketDbPath);
            _MarketService.DownloadMarket(true);
        }

        [AutomaticRetry(Attempts = 1)]
        public void DownloadIEC()
        {
            string iecPath = Configuration["IECPath"];
            string iecDownloadUrl = Configuration["IECDownloadURL"];
            string webRootPath = HostingEnv.WebRootPath;
            IECService _IECService = new IECService(Logger, iecPath, iecDownloadUrl, webRootPath);
            _IECService.DownloadIEC();
        }
        #endregion

        /// <summary>
        /// Will only allow access to the Hangfire dashboard if authorized by EVE
        /// </summary>
        public class MyAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                var httpContext = context.GetHttpContext();

                // Allow only the specified Characters to access
                return httpContext.User.Identity.IsAuthenticated && httpContext.User.HasClaim("Admin", "true");
            }
        }
    }
}
