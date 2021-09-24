using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LandingPage.ViewModels;
using LandingPage.ViewModels.Publisher;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;

namespace LandingPage.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class PublisherController : Controller
    {
        private readonly ILogger<PublisherController> _logger;
        private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;

        public PublisherController(
            ILogger<PublisherController> logger,
            IMarketplaceSaaSClient marketplaceSaaSClient)
        {
            this._logger = logger;
            this._marketplaceSaaSClient = marketplaceSaaSClient;
        }

        // shows a list of all subscriptions
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

        // shows subscription details
        public async Task<IActionResult> SubscriptionAsync(Guid id, CancellationToken cancellationToken)
        {

            Subscription subscription = null;
            SubscriptionPlans plans = null;

            try
            {
                subscription = (await _marketplaceSaaSClient.Fulfillment.GetSubscriptionAsync(id, cancellationToken: cancellationToken)).Value;
                plans = (await _marketplaceSaaSClient.Fulfillment.ListAvailablePlansAsync(id, cancellationToken: cancellationToken)).Value;
            }
            catch (Exception exception)
            {
                ErrorViewModel errorViewModel = new ErrorViewModel()
                {
                    Description = "Error calling SaaS Fulfillment API",
                    ExceptionMessage = exception.Message
                };

                return View("Error", errorViewModel);

            }

            var model = new SubscriptionViewModel()
            {
                
                Subscription = subscription,
                Plans = plans
            };

            return View(model);
        }

        // this action will mark the subscription State as Subscribed
        [Route("/Publisher/Activate/{id}/{planId}")]
        public async Task<IActionResult> ActivateAsync(Guid id, string planId, CancellationToken cancellationToken)
        {
            SubscriberPlan subscriberPlan = new SubscriberPlan()
            {
                PlanId= planId,
            };
            
            _ = await _marketplaceSaaSClient.Fulfillment.ActivateSubscriptionAsync(id, subscriberPlan, cancellationToken: cancellationToken);

            return this.RedirectToAction("Subscription", new { id = id });

        }

        
        // this action will mark the subscription State as Unsubscribed
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            try { 
                _ = await _marketplaceSaaSClient.Fulfillment.DeleteSubscriptionAsync(id, cancellationToken: cancellationToken);
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            
            return this.RedirectToAction("Subscription", new { id = id });
        }
    }
}
