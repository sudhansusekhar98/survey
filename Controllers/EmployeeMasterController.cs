using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;

namespace SurveyApp.Controllers
{
    public class EmployeeMasterController : Controller
    {
        private readonly IEmpMaster _empRepository;
        private readonly ICommonUtil _util;

        public EmployeeMasterController(IEmpMaster empRepository, ICommonUtil util)
        {
            _empRepository = empRepository;
            _util = util;
        }

        // GET: EmployeeMaster/Index
        public IActionResult Index()
        {
            // Authorization check - adjust rights ID as needed
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "View");
            if (result != null) return result;

            try
            {
                var employees = _empRepository.GetAllEmployees();
                return View(employees);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(new List<EmpMasterModel>());
            }
        }

        // GET: EmployeeMaster/Create
        public IActionResult Create()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Create");
            if (result != null) return result;

            return View(new EmpMasterModel());
        }

        // POST: EmployeeMaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EmpMasterModel employee)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Create");
            if (result != null) return result;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View(employee);
                }

                // Set CreatedBy from session
                employee.CreatedBy = HttpContext.Session.GetString("UserName") ?? "System";

                bool isSaved = _empRepository.AddEmployee(employee);

                if (isSaved)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Employee created successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to create employee.";
                    TempData["ResultType"] = "danger";
                    return View(employee);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(employee);
            }
        }

        // GET: EmployeeMaster/Edit/5
        public IActionResult Edit(int id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
            if (result != null) return result;

            try
            {
                var employee = _empRepository.GetEmployeeById(id);
                if (employee == null || employee.EmpID == 0)
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Employee not found.";
                    TempData["ResultType"] = "danger";
                    return RedirectToAction("Index");
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // POST: EmployeeMaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EmpMasterModel employee)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
            if (result != null) return result;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View(employee);
                }

                // Set ModifiedBy from session
                employee.ModifiedBy = HttpContext.Session.GetString("UserName") ?? "System";

                bool isUpdated = _empRepository.UpdateEmployee(employee);

                if (isUpdated)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Employee updated successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update employee.";
                    TempData["ResultType"] = "danger";
                    return View(employee);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(employee);
            }
        }

        // POST: EmployeeMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Delete");
            if (result != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool isDeleted = _empRepository.DeleteEmployee(id);

                if (isDeleted)
                {
                    return Json(new { success = true, message = "Employee deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete employee." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: EmployeeMaster/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                var employee = _empRepository.GetEmployeeById(id);
                if (employee == null || employee.EmpID == 0)
                {
                    return NotFound();
                }

                return PartialView("_EmployeeDetails", employee);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index");
            }
        }
    }
}
