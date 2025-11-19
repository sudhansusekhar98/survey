using Microsoft.AspNetCore.Mvc;

namespace SurveyApp.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
