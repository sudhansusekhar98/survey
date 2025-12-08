using AnalyticaDocs.Models;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using AnalyticaDocs.Util;
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

                // Check if user must change password (admin set temporary password)
                if (user.MustChangePassword)
                {
                    HttpContext.Session.SetString("MustChangePassword", "true");
                    return RedirectToAction("ForceChangePassword");
                }

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                TempData["LoginMessage"] = "Login failed. Please check your Login ID and password.";
                return View("LoginBasic", loginData);
            }

        }

        /// <summary>
        /// Force change password page - shown when admin sets a temporary password
        /// </summary>
        public IActionResult ForceChangePassword()
        {
            var mustChange = HttpContext.Session.GetString("MustChangePassword");
            if (mustChange != "true")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View("ForceChangePassword");
        }

        /// <summary>
        /// Handle force change password submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForceChangePassword(string newPassword, string confirmPassword)
        {
            var mustChange = HttpContext.Session.GetString("MustChangePassword");
            if (mustChange != "true")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["ErrorMessage"] = "Password fields cannot be empty.";
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View("ForceChangePassword");
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View("ForceChangePassword");
            }

            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Password must be at least 6 characters long.";
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View("ForceChangePassword");
            }

            var userIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Index");
            }

            int userId = Convert.ToInt32(userIdStr);

            try
            {
                // Update password directly (bypassing current password check since this is a forced change)
                using var con = new Microsoft.Data.SqlClient.SqlConnection(DBConnection.ConnectionString);
                con.Open();
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
                    UPDATE LoginMaster 
                    SET LoginPassword = @Password, 
                        MustChangePassword = 0 
                    WHERE UserID = @UserID", con);
                
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Password", newPassword);

                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    HttpContext.Session.Remove("MustChangePassword");
                    TempData["SuccessMessage"] = "Password changed successfully! Welcome to the Survey Application.";
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to change password. Please try again.";
                    ViewBag.UserName = HttpContext.Session.GetString("UserName");
                    return View("ForceChangePassword");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                return View("ForceChangePassword");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
