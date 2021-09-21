
using LandingPage.ViewModels.Subscriptions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace.SaaS;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Marketplace.SaaS.Models;
using System.Collections.Generic;
using System;

namespace LandingPage
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class SubscriptionsController : Controller
    {
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;

        public SubscriptionsController(
            ILogger<SubscriptionsController> logger,
            IMarketplaceSaaSClient marketplaceSaaSClient)
        {
            this._logger = logger;
            this._marketplaceSaaSClient = marketplaceSaaSClient;
        }

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

                public async Task<IActionResult> SubAsync(Guid id, CancellationToken cancellationToken)
        {
            var subscription = (await _marketplaceSaaSClient.Fulfillment.GetSubscriptionAsync(id, cancellationToken: cancellationToken)).Value;

            var model = new SubscriptionViewModel()
            {
                Subscription = subscription
            };

            return View(model);
        }
    }
}
