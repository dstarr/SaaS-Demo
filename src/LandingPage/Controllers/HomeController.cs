using LandingPage.ViewModels.Home;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Marketplace.SaaS;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace LandingPage.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;
        private readonly GraphServiceClient _graphServiceClient;

        public HomeController(
            IMarketplaceSaaSClient marketplaceSaaSClient,
            GraphServiceClient graphServiceClient)
        {
            _marketplaceSaaSClient = marketplaceSaaSClient;
            _graphServiceClient = graphServiceClient;
        }

        /// <summary>
        /// Shows all information associated with the user, the request, and the subscription.
        /// </summary>
        /// <param name="token">THe marketplace purchase ID token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
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
            var subscriptionPlans = (await _marketplaceSaaSClient.Fulfillment.ListAvailablePlansAsync(resolvedSubscription.Id.Value, cancellationToken: cancellationToken)).Value;
            var operationList = (await _marketplaceSaaSClient.Operations.ListOperationsAsync(resolvedSubscription.Id.Value, cancellationToken: cancellationToken)).Value;

            // get graph current user data
            var graphApiUser = await _graphServiceClient.Me.Request().GetAsync();
            
            // build the model
            var model = new IndexViewModel()
            {
                PurchaseIdToken = HttpUtility.UrlDecode(token),
                UserClaims = this.User.Claims,
                GraphUser = graphApiUser,
                Subscription = resolvedSubscription.Subscription,
                OperationList = operationList,
                SubscriptionPlans = subscriptionPlans
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}