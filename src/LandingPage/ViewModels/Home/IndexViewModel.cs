using System.Collections.Generic;
using System.Security.Claims;

namespace LandingPage.ViewModels.Home
{
    public class IndexViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NameFromOpenIdConnect { get; set; }
        public string EmailFromClaims { get; set; }
        public string EmailFromGraph { get; set; }
        public string NameFromGraph { get; set; }
        public string UserPrincipalName { get; set; }
        public string PurchaserEmail { get; set; }
        public string AccessToken { get; set; }
        public string NameFromClaims { get; set; }
        public IEnumerable<Claim> UserClaims { get; internal set; }
    }
}
