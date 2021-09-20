using Microsoft.AspNetCore.Mvc;

namespace LandingPage.Controllers
{
    public class SubscriptionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
