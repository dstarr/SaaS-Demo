using System.Collections.Generic;
using System.Security.Claims;

namespace LandingPage.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Claim> UserClaims { get; internal set; }

        public GraphValuesViewModel GraphValues {  get; internal set; }

        public SubscriptionValuesViewModel SubscriptionValues {  get; internal set; }
    }
}
