using Microsoft.Marketplace.SaaS.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace LandingPage.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Claim> UserClaims { get; internal set; }

        public GraphValuesViewModel GraphValues {  get; internal set; }

        public Subscription Subscription { get; internal set; }
        public string PurchaseIdToken { get; internal set; }
    }
}
