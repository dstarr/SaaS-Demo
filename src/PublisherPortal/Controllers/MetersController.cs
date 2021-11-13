using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Marketplace.SaaS;

namespace PublisherPortal.Controllers;

[Authorize]
[Route("~/Meters")]
public class MetersController : Controller
{
    private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;
    private readonly ILogger<HomeController> _logger;

    public MetersController(
        IMarketplaceSaaSClient marketplaceSaaSClient,
        ILogger<HomeController> logger)

    {
        _marketplaceSaaSClient = marketplaceSaaSClient;
        _logger = logger;
    }


    [Route("Subscription/{id}")]
    public async Task<IActionResult> IndexAsync(Guid id, CancellationToken cancellationToken)
    {

        var subscription = _marketplaceSaaSClient.Fulfillment.GetSubscriptionAsync(id, cancellationToken: cancellationToken);

        return View();
    }
}