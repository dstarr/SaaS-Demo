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
    [Route("/Publisher")]
    public class PublisherController : Controller
    {

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

        /// <summary>
        /// This action will mark the subscription State as Unsubscribed
        /// </summary>
        /// <param name="id">The id of the subscription as a GUID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        [Route("Delete")]
        public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            _ = await _marketplaceSaaSClient.Fulfillment.DeleteSubscriptionAsync(id, cancellationToken: cancellationToken);

            return this.RedirectToAction("Index", new { id = id });
        }

        /// <summary>
        /// Changes plan for the subscription based on the plan ID
        /// </summary>
        /// <param name="subscriptionId">The subscription ID as a GUID</param>
        /// <param name="planId">The plan ID as assigned in Partner Center</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        [Route("ChangePlan/{SubscriptionId}/{planId}")]
        public IActionResult ChangePlan(Guid subscriptionId, string planId, CancellationToken cancellationToken)
        {
            var subscriberPlan = new SubscriberPlan()
            {
                PlanId = planId,
            };

            var operationId = this._marketplaceSaaSClient.Fulfillment.UpdateSubscription(subscriptionId, subscriberPlan, cancellationToken: cancellationToken);

            return this.RedirectToAction("Operations", new { subscriptionId = subscriptionId, operationId = operationId });
        }

        /// <summary>
        /// Gets a specific operation for a given subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="operationId">The operation's ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        [Route("Operations/{subscriptionId}/{operationId}")]
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

        /// <summary>
        /// Changes the plan for a given subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="planId">The target plan ID as defined in Partner Center</param>
        /// <param name="operationId">The ChangePlan operation ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>IActionResult</returns>
        [Route("Update/{subscriptionId:Guid}/{planId}/{operationId:Guid}")]
        public async Task<IActionResult> UpdateAsync(Guid subscriptionId, string planId, Guid operationId, CancellationToken cancellationToken)
        {
            var updateOperation = new UpdateOperation()
            {
                PlanId = planId,
                Status = UpdateOperationStatusEnum.Success
            };

            var status = (await _marketplaceSaaSClient.Operations.UpdateOperationStatusAsync(subscriptionId, 
                    operationId, updateOperation, cancellationToken: cancellationToken)).Status;
            
            return this.RedirectToAction("Subscription", new { id = subscriptionId });
        }
    }
}
