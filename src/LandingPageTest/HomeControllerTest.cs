using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using LandingPage.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LandingPageTest
{
    public class HomeControllerTest
    {
        readonly HomeController _controller;
        ControllerContext _controllerContext;
        readonly IConfiguration _configuration;

        public HomeControllerTest(IConfiguration configuration)
        {
            _configuration = configuration;

            Mock _mockConfig = new Mock<IConfiguration>();


            _controller = new HomeController(new NullLogger<HomeController>(), _configuration);

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username")
            }, "someAuthTypeName"));

            _controllerContext = _controller.ControllerContext = context;
        }


        [Fact]
        public void Index()
        {
            IActionResult result = _controller.IndexAsync();

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
