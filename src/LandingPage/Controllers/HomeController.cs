using System.Diagnostics;
using Azure.Identity;
using LandingPage.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace.SaaS;

namespace LandingPage.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private MarketplaceSaaSClient _marketplaceClient;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _marketplaceClient = new MarketplaceSaaSClient(new ClientSecretCredential(_config["TenantId"],
                _config["ClientId"], _config["clientSecret"]));
        }

        public IActionResult Index()
        {
            var userName = User.Identity.Name;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}