using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace.Metering;
using Microsoft.Marketplace.Metering.Models;
using Microsoft.Marketplace.SaaS;
using Microsoft.Marketplace.SaaS.Models;
using PublisherPortal.Controllers.ParameterModels;
using PublisherPortal.ViewModels.Meters;

namespace PublisherPortal.Controllers;

[Authorize]
[Route("~/Meters")]
public class MetersController : Controller
{
    private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;
    private readonly IMarketplaceMeteringClient _meteringClient;
    private readonly ILogger<HomeController> _logger;

    public MetersController(
        IMarketplaceSaaSClient marketplaceSaaSClient,
        IMarketplaceMeteringClient meteringClient,
        ILogger<HomeController> logger)

    {
        _marketplaceSaaSClient = marketplaceSaaSClient;
        _meteringClient = meteringClient;
        _logger = logger;
    }


    [Route("Subscription/{id}")]
    public async Task<IActionResult> IndexAsync(Guid id, CancellationToken cancellationToken)
    {
        var subscription = (await _marketplaceSaaSClient.Fulfillment.GetSubscriptionAsync(id, cancellationToken: cancellationToken)).Value;

        var plans = _marketplaceSaaSClient.Fulfillment.ListAvailablePlansAsync(subscription.Id.Value, cancellationToken: cancellationToken).Result.Value;

        var viewModel = new IndexViewModel()
        {
            Plans = plans.Plans,
            Subscription = subscription
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> InvokeMeterAsync([Bind] InvokeMeterParameter parameters, CancellationToken cancellationToken)
    {
        UsageEvent usageEvent = new UsageEvent()
        {
            PlanId = parameters.PlanId,
            ResourceId = parameters.SubscriptionId,
            Quantity = parameters.Quantity,
            Dimension = parameters.DimensionId,
            EffectiveStartTime = DateTimeOffset.Now.UtcDateTime,
        };
        
        _logger.Log(LogLevel.Information, "Invoking meter");

        UsageEventOkResponse result;
        

        try
        {

            result =
                (await _meteringClient.Metering.PostUsageEventAsync(usageEvent, cancellationToken: cancellationToken))
                .Value;

            var viewModel = new InvokeMeterViewModel()
            {
                DimensionId = result.Dimension,
                PlanId = result.PlanId,
                Quantity = result.Quantity,
                SubscriptionId = result.ResourceId,
                ResultStatus = result.Status,
            };
        }
        catch (RequestFailedException exception)
        {
            var viewModel = new InvokeMeterViewModel()
            {
                DimensionId = result.Dimension,
                PlanId = result.PlanId,
                Quantity = result.Quantity,
                SubscriptionId = result.ResourceId,
                ResultStatus = result.Status,
            };
        }





        return View(viewModel);
    }
}