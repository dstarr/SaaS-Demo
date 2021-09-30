using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PubisherPortal.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LandingPage.ViewModels.Publisher;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;
using PubisherPortal.ViewModels.Home;

namespace PubisherPortal.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IMarketplaceSaaSClient _marketplaceSaaSClient;

        public HomeController(
            ILogger<HomeController> logger,
            IMarketplaceSaaSClient marketplaceSaaSClient)
        {
            _logger = logger;
            _marketplaceSaaSClient = marketplaceSaaSClient;
        }

        /// <summary>
        /// Shows a list of all subscriptions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        public async Task<IActionResult> IndexAsync(CancellationToken cancellationToken)
        {
            IList<Subscription> subscriptionsList = new List<Subscription>();
            var subscriptions = _marketplaceSaaSClient.Fulfillment.ListSubscriptionsAsync(cancellationToken: cancellationToken);
            
            await foreach (var subscription in subscriptions)
            {
                subscriptionsList.Add(subscription);
            }
            
            var model = new IndexViewModel()
            {
                Subscriptions = subscriptionsList.OrderBy(s => s.Name).ToList<Subscription>()
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
