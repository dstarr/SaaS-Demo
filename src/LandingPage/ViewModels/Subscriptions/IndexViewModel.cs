using Microsoft.Marketplace.SaaS.Models;
using System.Collections.Generic;

namespace LandingPage.ViewModels.Subscriptions
{
    public class IndexViewModel
    {
        public IList<Subscription> Subscriptions { get; internal set; }
    }
}
