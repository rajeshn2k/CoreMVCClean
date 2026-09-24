using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

namespace Core.MVC.Clean.Controllers
{
    public class PingController : Controller
    {
        private readonly IConfiguration configuration;

        public PingController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IActionResult Index()
        {
            var runtimeInfo = new Dictionary<string, string>
            {
                ["ProcessName"] = Process.GetCurrentProcess().ProcessName,
                ["HostTime"] = DateTime.Now.ToString(),
                ["ASPNETCORE_ENVIRONMENT"] =
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ["Book API Url"] = configuration["bookAPIUrl"],
                ["Person API Url"] = configuration["personAPIUrl"]
            };

            Log.Information("Runtime Environment: {@RuntimeInfo}", runtimeInfo);

            return View(runtimeInfo);
        }
    }
}