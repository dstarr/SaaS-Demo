using System.Collections.Generic;
using Microsoft.Marketplace.SaaS.Models;

namespace LandingPage.ViewModels.Publisher
{
    public class SubscriptionViewModel
    {
        public Subscription Subscription {  get; internal set; }
        public IReadOnlyList<Plan> Plans { get; set; }
    }
}
