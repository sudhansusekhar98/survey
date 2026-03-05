using AnalyticaDocs.Models;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using AnalyticaDocs.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using SurveyApp.Repo;
using System.Diagnostics;

namespace AnalyticaDocs.Controllers
{
    public class UsersController : Controller
    {
        private readonly IAdmin _repository;
        private readonly ICommonUtil _util;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;

        public UsersController(IAdmin repository, ICommonUtil util, IEmailService emailService, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _util = util;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
        }
        public IActionResult Index()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "View");
            if (result != null) return result;

            ViewBag.DataForGrid = _repository.GetAllDetails();
            return View("Users", new UserModel());

        }

        public IActionResult GetUserModal(int id)
        {
            var user = _repository.GetUserById(id);
            return PartialView("~/Views/Users/_UserDetailModal.cshtml", user);
        }

        public IActionResult Create()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Create");


            if (result != null) return result;
            
            var model = new UserModel();
            // Get only employees who don't have user accounts yet
            var employees = _repository.GetAvailableEmployeesForUserCreation();
            model.EmployeeOptions = employees.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = e.EmpID.ToString(),
                Text = e.EmpName
            }).ToList();
            model.EmployeeOptions.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "", Value = "" });
            
            return View("Create", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Create");
            if (result != null) return result;

            if (!ModelState.IsValid)
            {
                // Get only employees who don't have user accounts yet
                var employees = _repository.GetAvailableEmployeesForUserCreation();
                user.EmployeeOptions = employees.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.EmpID.ToString(),
                    Text = e.EmpName
                }).ToList();
                user.EmployeeOptions.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "-- Select Employee --", Value = "" });
                return View("Create", user);
            }

            // Store plain-text password before AddUser hashes it inside the repo
            string plainTextPassword = user.LoginPassword;
            user.CreateBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            bool isSaved = _repository.AddUser(user);

            if (isSaved)
            {
                // Send email notification with temporary password to new user
                if (!string.IsNullOrEmpty(user.EmailID))
                {
                    bool emailSent = await _emailService.SendNewUserAccountNotificationAsync(
                        user.LoginName,
                        user.EmailID,
                        user.LoginId,
                        plainTextPassword
                    );
                    if (!emailSent)
                    {
                        TempData["EmailWarning"] = "User created but welcome email could not be sent. Please check SMTP settings.";
                    }
                }

                TempData["ResultType"] = "success";
                TempData["ResultMessage"] = "<strong>Success!</strong> User created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ResultType"] = "danger";
                TempData["ResultMessage"] = "<strong>Error!</strong> Record Not Save.";
                return View("Create", user);
            }
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
            if (result != null) return result;

            if (!id.HasValue)
                return RedirectToAction("Index");

            var user =  _repository.GetUserById(id.Value);
            if (user == null)
            {
                TempData["ResultMessage"] = "User not found.";
                return RedirectToAction("Index");
            }
            
            // Get available employees (excluding current user to allow keeping the same employee)
            var employees = _repository.GetAvailableEmployeesForUserCreation(user.UserId);
            user.EmployeeOptions = employees.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = e.EmpID.ToString(),
                Text = e.EmpName,
                Selected = e.EmpID == user.EmpID
            }).ToList();
            user.EmployeeOptions.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "-- Select Employee --", Value = "" });
            
            return View("Edit", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(UserModel user)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
            if (result != null) return Json("unauthorized");

            if (!ModelState.IsValid)
            {
                return Json("invalid");
            }

            user.CreateBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            bool isSaved = _repository.UpdateUser(user);

            if (isSaved)
            {
                TempData["ResultType"] = "success";
                TempData["ResultMessage"] = "<strong>Success!</strong> User updated successfully.";
            }

            return Json(isSaved ? "success" : "fail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmUpdate()
        {
            // Optional: log confirmation, trigger workflow, etc.
            return Json("OK"); // Or "done", "ok", etc.
        }

        [HttpGet]
        public IActionResult GetEmployeeDetails(int empId)
        {
            try
            {
                var employee = _repository.GetEmployeeById(empId);
                if (employee != null)
                {
                    return Json(new
                    {
                        success = true,
                        empName = employee.EmpName,
                        email = employee.Email ?? "",
                        mobileNo = employee.MobileNo ?? ""
                    });
                }
                return Json(new { success = false, message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        /// <summary>
        /// TEST ENDPOINT - Check password directly (uses BCrypt verification)
        /// </summary>
        [HttpGet]
        public IActionResult TestPassword(int userId, string password)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                
                using var cmd = new SqlCommand(@"
                    SELECT UserID, LoginID, LoginName, LoginPassword, MustChangePassword 
                    FROM LoginMaster 
                    WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var dbPassword = reader["LoginPassword"]?.ToString() ?? "";
                    var isBCryptHash = _passwordHasher.IsBCryptHash(dbPassword);
                    var match = _passwordHasher.VerifyPassword(password, dbPassword);
                    
                    return Json(new { 
                        success = true,
                        userId = reader["UserID"],
                        loginId = reader["LoginID"],
                        loginName = reader["LoginName"],
                        passwordIsBCryptHash = isBCryptHash,
                        passwordMatch = match,
                        mustChangePassword = reader["MustChangePassword"]
                    });
                }
                
                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Change password for the currently logged in user
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ChangePassword called - currentPassword: {(string.IsNullOrEmpty(currentPassword) ? "EMPTY" : "***")}, newPassword: {(string.IsNullOrEmpty(newPassword) ? "EMPTY" : "***")}");
                
                var userIdStr = HttpContext.Session.GetString("UserID");
                System.Diagnostics.Debug.WriteLine($"UserID from session: {userIdStr}");
                
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "User not logged in. Please login again." });
                }
                
                int userId = Convert.ToInt32(userIdStr);
                System.Diagnostics.Debug.WriteLine($"Parsed UserID: {userId}");

                if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
                {
                    return Json(new { success = false, message = "Password fields cannot be empty" });
                }

                if (newPassword.Length < 6)
                {
                    return Json(new { success = false, message = "New password must be at least 6 characters long" });
                }

                System.Diagnostics.Debug.WriteLine($"Calling _repository.ChangePassword for userId: {userId}");
                bool result = _repository.ChangePassword(userId, currentPassword, newPassword);
                System.Diagnostics.Debug.WriteLine($"ChangePassword result: {result}");

                if (result)
                {
                    return Json(new { success = true, message = "Password changed successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Current password is incorrect" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePassword error: {ex.ToString()}");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        /// <summary>
        /// Reset password for a user (admin function) - sets temporary password with MustChangePassword flag
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, string temporaryPassword)
        {
            try
            {
                int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
                var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
                if (result != null) return Json(new { success = false, message = "Unauthorized" });

                if (string.IsNullOrEmpty(temporaryPassword))
                {
                    return Json(new { success = false, message = "Temporary password cannot be empty" });
                }

                if (temporaryPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Temporary password must be at least 6 characters long" });
                }

                bool resetResult = _repository.ResetPasswordWithFlag(userId, temporaryPassword);

                if (resetResult)
                {
                    // Send email notification with temporary password
                    var targetUser = _repository.GetUserById(userId);
                    var resetByName = HttpContext.Session.GetString("UserName") ?? "Administrator";
                    
                    if (targetUser != null && !string.IsNullOrEmpty(targetUser.EmailID))
                    {
                        bool emailSent = await _emailService.SendPasswordResetNotificationAsync(
                            targetUser.LoginName,
                            targetUser.EmailID,
                            targetUser.LoginId,
                            temporaryPassword,
                            resetByName
                        );

                        if (!emailSent)
                        {
                            return Json(new { success = true, message = "Password reset successfully, but notification email could not be sent. Please check SMTP settings." });
                        }
                    }

                    return Json(new { success = true, message = "Password reset successfully. User will be prompted to change password on next login." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to reset password" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        /// <summary>
        /// Sync employees from EmpMaster to LoginMaster with temporary passwords (admin function)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SyncEmployeesToLogin(string defaultPassword, int defaultRoleId = 102)
        {
            try
            {
                int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
                var authResult = _util.CheckAuthorizationAll(this, rightsId, null, null, "Insert");
                if (authResult != null) return Json(new { success = false, message = "Unauthorized" });

                if (string.IsNullOrEmpty(defaultPassword))
                {
                    return Json(new { success = false, message = "Default password cannot be empty" });
                }

                if (defaultPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Default password must be at least 6 characters long" });
                }

                var userIdStr = HttpContext.Session.GetString("UserID");
                int createdBy = string.IsNullOrEmpty(userIdStr) ? 0 : Convert.ToInt32(userIdStr);

                var (synced, skipped, errors) = _repository.SyncEmployeesToLoginMaster(defaultPassword, defaultRoleId, createdBy);

                if (synced > 0)
                {
                    string message = $"Successfully created {synced} login account(s).";
                    if (skipped > 0)
                    {
                        message += $" {skipped} employee(s) were skipped.";
                    }
                    
                    return Json(new { 
                        success = true, 
                        message = message,
                        synced = synced,
                        skipped = skipped,
                        errors = errors
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = skipped > 0 
                            ? $"No accounts created. {skipped} employee(s) were skipped." 
                            : "No employees found without login accounts.",
                        synced = 0,
                        skipped = skipped,
                        errors = errors
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SyncEmployeesToLogin error: {ex.ToString()}");
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
