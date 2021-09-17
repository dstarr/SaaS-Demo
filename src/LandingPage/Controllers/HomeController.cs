using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LandingPage.ViewModels;
using LandingPage.ViewModels.Home;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace LandingPage.Controllers
{
    //[Authorize]
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly GraphServiceClient _graphServiceClient;

        public HomeController(
            ILogger<HomeController> logger, 
            IConfiguration config,
            GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _config = config;
            _graphServiceClient = graphServiceClient;
        }

        [AuthorizeForScopes(Scopes = new string[] { "user.read" })]
        public async Task<IActionResult> Index()
        {
            var graphApiUser = await _graphServiceClient.Me.Request().GetAsync();

            IndexViewModel model = new IndexViewModel()
            {
                UserClaims = this.User.Claims,
                
                DisplayName = graphApiUser.DisplayName,
                GivenName = graphApiUser.GivenName,
                Surname = graphApiUser.Surname,
                Mail = graphApiUser.Mail,
                Department = graphApiUser.Department,
                JobTitle = graphApiUser.JobTitle
            };

            return View(model);
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