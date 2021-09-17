using System.Collections.Generic;
using System.Security.Claims;

namespace LandingPage.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Claim> UserClaims { get; internal set; }

        public string Department { get; internal set; }
        public string DisplayName { get; internal set; }
        public string GivenName { get; internal set; }
        public string JobTitle { get; internal set; }
        public string Mail { get; internal set; }
        public string Surname { get; internal set; }
    }
}
