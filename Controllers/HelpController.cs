using Microsoft.AspNetCore.Mvc;
using SurveyApp.Repo;

namespace SurveyApp.Controllers
{
    public class HelpController : Controller
    {
        // Main Help Index
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            ViewBag.UserRole = HttpContext.Session.GetString("RoleId");
            return View();
        }

        // Survey Creation Help
        public IActionResult SurveyCreation()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // Survey Assignment Help
        public IActionResult SurveyAssignment()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // Survey Execution Help
        public IActionResult SurveyExecution()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // Survey Submission Help
        public IActionResult SurveySubmission()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // Reports Help
        public IActionResult Reports()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // Admin Functions Help
        public IActionResult AdminFunctions()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // Quick Start Guide
        public IActionResult QuickStart()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }

        // FAQ
        public IActionResult FAQ()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            return View();
        }
    }
}
