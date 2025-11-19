using AnalyticaDocs.Models;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace AnalyticaDocs.Controllers
{
    public class UsersController : Controller
    {
        private readonly IAdmin _repository;
        private readonly ICommonUtil _util;

        public UsersController(IAdmin repository, ICommonUtil util)
        {
            _repository = repository;
            _util = util;
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
            var employees = _repository.GetEmpMaster();
            model.EmployeeOptions = employees.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = e.EmpID.ToString(),
                Text = e.EmpName
            }).ToList();
            model.EmployeeOptions.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "-- Select Employee --", Value = "" });
            
            return View("Create", model); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserModel user)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Create");
            if (result != null) return result;

            if (!ModelState.IsValid)
            {
                var employees = _repository.GetEmpMaster();
                user.EmployeeOptions = employees.Select(e => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = e.EmpID.ToString(),
                    Text = e.EmpName
                }).ToList();
                user.EmployeeOptions.Insert(0, new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Text = "-- Select Employee --", Value = "" });
                return View("Create", user);
            }

            user.CreateBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            bool isSaved = _repository.AddUser(user);

            if (isSaved)
            {
                TempData["ResultType"] = "success";
                TempData["ResultMessage"] = "<strong>Success!</strong> Record Save successfully.";
                return RedirectToAction("Create");
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
            
            var employees = _repository.GetEmpMaster();
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

            return Json(isSaved ? "success" : "fail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmUpdate()
        {
            // Optional: log confirmation, trigger workflow, etc.
            return Json("OK"); // Or "done", "ok", etc.
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
