using System.Collections.Generic;
using Microsoft.Marketplace.SaaS.Models;

namespace LandingPage.ViewModels.Publisher
{
    public class IndexViewModel
    {
        public IList<Subscription> Subscriptions { get; internal set; }
    }
}
