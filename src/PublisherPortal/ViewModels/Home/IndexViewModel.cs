using System.Collections.Generic;
using Microsoft.Marketplace.SaaS.Models;

namespace PublisherPortal.ViewModels.Home;

public class IndexViewModel
{
    public Subscription Subscription { get; internal set; }
    public bool HasMeters { get; internal set; }
}

