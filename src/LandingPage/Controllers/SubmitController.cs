using Microsoft.AspNetCore.Mvc;

namespace LandingPage.Controllers
{
    public class SubmitController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
