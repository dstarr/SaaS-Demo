using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LandingPage.ViewModels;
using LandingPage.ViewModels.Publisher;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;

namespace LandingPage.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    // [Route("/Publisher")]
    public class PublisherController : Controller
    {
        [Route("/Publisher")]
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

        private readonly ILogger<PublisherController> _logger;
        private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;

        public PublisherController(
            ILogger<PublisherController> logger,
            IMarketplaceSaaSClient marketplaceSaaSClient)
        {
            this._logger = logger;
            this._marketplaceSaaSClient = marketplaceSaaSClient;
        }

        // shows subscription details
        [Route("/Suscription/{id}")]
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
                Plans = plans.Plans
            };

            return View(model);
        }

        // this action will mark the subscription State as Subscribed
        [Route("/Activate/{id}/{planId}")]
        public async Task<IActionResult> ActivateAsync(Guid id, string planId, CancellationToken cancellationToken)
        {
            SubscriberPlan subscriberPlan = new SubscriberPlan()
            {
                PlanId = planId,
            };

            _ = await _marketplaceSaaSClient.Fulfillment.ActivateSubscriptionAsync(id, subscriberPlan, cancellationToken: cancellationToken);

            return this.RedirectToAction("Subscription", new { id = id });

        }

        // this action will mark the subscription State as Unsubscribed
        [Route("/Delete")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                _ = await _marketplaceSaaSClient.Fulfillment.DeleteSubscriptionAsync(id, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return this.RedirectToAction("Subscription", new { id = id });
        }

        [Route("/ChangePlan/{SubscriptionId}/{planId}")]
        public IActionResult ChangePlan(Guid subscriptionId, string planId, CancellationToken cancellationToken)
        {
            _logger.LogCritical("GOT IT: " + subscriptionId + " | " + planId);

            var subscriberPlan = new SubscriberPlan()
            {
                PlanId = planId,
            };

            string operationId = this._marketplaceSaaSClient.Fulfillment.UpdateSubscription(subscriptionId, subscriberPlan, cancellationToken: cancellationToken);

            _logger.LogCritical($"OPERATION ID: {operationId}");


            return this.RedirectToAction("Operations", new { subscriptionId = subscriptionId, operationId = operationId });
        }

        [HttpGet]
        [Route("/Operations/{subscriptionId}/{operationId}")]
        public async Task<IActionResult> OperationsAsync(Guid subscriptionId, Guid operationId, CancellationToken cancellationToken)
        {
            var subscription = (await _marketplaceSaaSClient.Fulfillment.GetSubscriptionAsync(subscriptionId, cancellationToken: cancellationToken)).Value;
            var subscriptionOperations = (await _marketplaceSaaSClient.Operations.ListOperationsAsync(subscriptionId, cancellationToken: cancellationToken)).Value;
            var operationStatus = (await _marketplaceSaaSClient.Operations.GetOperationStatusAsync(subscriptionId, operationId, cancellationToken: cancellationToken)).Value;

            var model = new OperationsViewModel()
            {
                Subscription = subscription,
                SubscriptionOperations = subscriptionOperations.Operations,
                OperationStatus = operationStatus
            };

            return View(model);
        }

        [HttpGet]
        [Route("/Update/{subscriptionId:Guid}/{planId}/{operationId:Guid}")]
        public async Task<IActionResult> UpdateAsync(Guid subscriptionId, string planId, Guid operationId, CancellationToken cancellationToken)
        {

            _logger.LogCritical($"SUBSCRIPTION ID: UpdateAsync: {subscriptionId}");
            _logger.LogCritical($"OPERATION ID: UpdateAsync: {operationId}");
            _logger.LogCritical($"PLAN ID: UpdateAsync: {planId}"); 
            
            var updateOperation = new UpdateOperation()
            {
                PlanId = planId,
                Status = UpdateOperationStatusEnum.Success
            };

            var status = (await _marketplaceSaaSClient.Operations.UpdateOperationStatusAsync(subscriptionId,
                    operationId, updateOperation, cancellationToken: cancellationToken)).Status;

            


            _logger.LogInformation($"STATUS UpdateAsync: {status}");

            return this.RedirectToAction("Subscription", new { id = subscriptionId });
        }
    }
}
