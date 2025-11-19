using Microsoft.AspNetCore.Mvc;

namespace SurveyApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
