﻿using Microsoft.AspNetCore.Authorization;
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
    [Route("~/")]
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

        /// <summary>
        /// Shows subscription details
        /// </summary>
        /// <param name="id">The subscription ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        [Route("Subscription/{id}")]
        public async Task<IActionResult> SubscriptionAsync(Guid id, CancellationToken cancellationToken)
        {

            Subscription subscription = (await _marketplaceSaaSClient.Fulfillment.GetSubscriptionAsync(id, cancellationToken: cancellationToken)).Value;
            SubscriptionPlans plans = (await _marketplaceSaaSClient.Fulfillment.ListAvailablePlansAsync(id, cancellationToken: cancellationToken)).Value;

            var model = new SubscriptionViewModel()
            {

                Subscription = subscription,
                Plans = plans.Plans
            };

            return View(model);
        }

        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// This action will mark the subscription State as Subscribed
        /// </summary>
        /// <param name="id">The subscription ID</param>
        /// <param name="planId">The plan ID as defined in Partner Center</param>
        /// <param name="cancellationToken">Cancelltion token</param>
        /// <returns>IActionResult</returns>
        [Route("Activate/{id}/{planId}")]
        public async Task<IActionResult> ActivateAsync(Guid id, string planId, CancellationToken cancellationToken)
        {
            SubscriberPlan subscriberPlan = new SubscriberPlan()
            {
                PlanId = planId,
            };

            _ = await _marketplaceSaaSClient.Fulfillment.ActivateSubscriptionAsync(id, subscriberPlan, cancellationToken: cancellationToken);

            return this.RedirectToAction("Subscription", new { id = id });

        }


    }
}
