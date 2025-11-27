using AnalyticaDocs.Models;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace AnalyticaDocs.Controllers
{
    public class UserLoginController : Controller
    {
        private readonly IAdmin _repository;
        public UserLoginController(IAdmin repository)
        {
            _repository = repository;
        }
        public IActionResult Index()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            return View("LoginBasic",new UserLoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index([Bind("LoginId,LoginPassword")] UserLoginModel loginData)
        {
            if (loginData.LoginId.IsNullOrEmpty() || loginData.LoginPassword.IsNullOrEmpty())
            {
                // Return view with validation errors to show red borders
                return View("LoginBasic", loginData);
            }

            var user = _repository.GetLoginUser(loginData);
            if (user != null)
            {
                if (user.ISActive != "Y")
                {
                    TempData["LoginFailed"] = "<strong>Access denied! </strong> Your account has been locked. Please Contact admin.";
                    return View("LoginBasic", loginData);
                }
                HttpContext.Session.SetString("UserID", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.LoginName.ToString());
                HttpContext.Session.SetString("RoleId", value: user.RoleId.ToString());

                // Store profile picture URL in session if available
                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    HttpContext.Session.SetString("ProfilePictureUrl", user.ProfilePictureUrl);
                }

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                TempData["LoginMessage"] = "Login failed. Please check your Login ID and password.";
                return View("LoginBasic", loginData);
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
