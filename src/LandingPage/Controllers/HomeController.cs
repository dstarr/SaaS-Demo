using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LandingPage.ViewModels;
using LandingPage.ViewModels.Home;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;

namespace LandingPage.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;
        private readonly GraphServiceClient _graphServiceClient;

        public HomeController(
            ILogger<HomeController> logger, 
            IMarketplaceSaaSClient marketplaceSaaSClient,
            GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _marketplaceSaaSClient = marketplaceSaaSClient;
            _graphServiceClient = graphServiceClient;
        }

        [AuthorizeForScopes(Scopes = new string[] { "user.read" })]
        public async Task<IActionResult> IndexAsync(string token, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(token))
            {
                this.ModelState.AddModelError(string.Empty, "Token URL parameter cannot be empty");
                this.ViewBag.Message = "Token URL parameter cannot be empty";
                return this.View();
            }

            // resolve the subscription using the marketplace purchase id token
            var resolvedSubscription = (await _marketplaceSaaSClient.Fulfillment.ResolveAsync(token, cancellationToken: cancellationToken)).Value;
            //var subscriptionPlans = (await _marketplaceSaaSClient.Fulfillment.ListAvailablePlansAsync(resolvedSubscription.Id.Value, cancellationToken: cancellationToken)).Value;
            var operationList = (await _marketplaceSaaSClient.Operations.ListOperationsAsync(resolvedSubscription.Id.Value, cancellationToken: cancellationToken)).Value;



            // get graph data
            var graphApiUser = await _graphServiceClient.Me.Request().GetAsync();
            
            // build the model
            var model = new IndexViewModel()
            {
                PurchaseIdToken = token,
                UserClaims = this.User.Claims,
                GraphValues = new GraphValuesViewModel
                {
                    DisplayName = graphApiUser.DisplayName,
                    GivenName = graphApiUser.GivenName,
                    Surname = graphApiUser.Surname,
                    Mail = graphApiUser.Mail,
                    JobTitle = graphApiUser.JobTitle                    
                },
                Subscription = resolvedSubscription.Subscription,
                OperationList = operationList,
                //SubscriptionPlans = subscriptionPlans
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