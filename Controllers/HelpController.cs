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

        // Survey Revision Help
        public IActionResult SurveyRevision()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            ViewBag.UserRole = HttpContext.Session.GetString("RoleId");
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

        // Admin Functions Help - Restricted to Admin users only
        public IActionResult AdminFunctions()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "UserLogin");
            }

            // Check if user has admin access (RoleId 101 = Super Admin, 102 = Admin)
            var roleIdStr = HttpContext.Session.GetString("RoleId");
            int roleId = string.IsNullOrEmpty(roleIdStr) ? 0 : Convert.ToInt32(roleIdStr);
            
            if (roleId != 101 && roleId != 102)
            {
                TempData["ResultMessage"] = "<strong>Access Denied!</strong> You do not have permission to view Admin help documentation.";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index");
            }

            ViewBag.UserRole = roleId;
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
