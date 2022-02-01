using System.Collections.Generic;
using Microsoft.Marketplace.SaaS.Models;

namespace PublisherPortal.ViewModels.Meters;

public class IndexViewModel
{
    public Subscription Subscription { get; set; }

    public IReadOnlyList<Plan> Plans { get; set; }
}
