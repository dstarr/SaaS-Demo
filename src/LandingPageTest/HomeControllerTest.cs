using Xunit;
using src.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LandingPageTest
{
    public class HomeControllerTest
    {
        HomeController _controller;
        ControllerContext _controllerContext;

        public HomeControllerTest()
        {
            _controller = new HomeController(new NullLogger<HomeController>());

            _controllerContext = _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "username")
                    }, "someAuthTypeName"))
                }
            };
        }


        [Fact]
        public void Index()
        {
            IActionResult result = _controller.Index();

            Assert.NotNull(result);
        }

        [Fact]
        public void Privacy()
        {
            IActionResult result = _controller.Privacy();

            Assert.NotNull(result);
        }
    }
}
