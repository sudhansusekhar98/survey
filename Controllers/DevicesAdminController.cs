using AnalyticaDocs.Repo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurveyApp.Models;

namespace SurveyApp.Controllers
{
    public class DevicesAdminController : Controller
    {
        private readonly IAdmin _adminRepository;

        public DevicesAdminController(IAdmin adminRepository)
        {
            _adminRepository = adminRepository;
        }

        private int GetCurrentUserId()
        {
            return Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
        }

        // GET: DevicesAdmin/Manage
        public IActionResult Manage()
        {
            try
            {
                ViewBag.DeviceModules = _adminRepository.GetAllDeviceModules(activeOnly: false);
                return View();
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View();
            }
        }

        // GET: DevicesAdmin/ModuleDevices/5
        public IActionResult ModuleDevices(int id)
        {
            try
            {
                var module = _adminRepository.GetDeviceModuleById(id);
                if (module == null)
                {
                    TempData["ResultMessage"] = "<strong>Not Found!</strong> Device Module not found.";
                    TempData["ResultType"] = "warning";
                    return RedirectToAction("Manage");
                }

                ViewBag.Module = module;
                ViewBag.Devices = _adminRepository.GetAllDevices(moduleId: id, activeOnly: false);
                return View();
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Manage");
            }
        }

        #region Device Modules Actions

        // GET: DevicesAdmin/CreateModule
        public IActionResult CreateModule()
        {
            return View(new DeviceModuleViewModel());
        }

        // POST: DevicesAdmin/CreateModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateModule(DeviceModuleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View(model);
                }

                int userId = GetCurrentUserId();
                bool isCreated = _adminRepository.CreateDeviceModule(model, userId);

                if (isCreated)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Device Module created successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Manage");
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to create Device Module.";
                    TempData["ResultType"] = "danger";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(model);
            }
        }

        // GET: DevicesAdmin/EditModule/5
        public IActionResult EditModule(int id)
        {
            try
            {
                var module = _adminRepository.GetDeviceModuleById(id);
                if (module == null)
                {
                    TempData["ResultMessage"] = "<strong>Not Found!</strong> Device Module not found.";
                    TempData["ResultType"] = "warning";
                    return RedirectToAction("Manage");
                }
                return View(module);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Manage");
            }
        }

        // POST: DevicesAdmin/EditModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditModule(DeviceModuleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View(model);
                }

                int userId = GetCurrentUserId();
                bool isUpdated = _adminRepository.UpdateDeviceModule(model, userId);

                if (isUpdated)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Device Module updated successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Manage");
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update Device Module.";
                    TempData["ResultType"] = "danger";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(model);
            }
        }

        // POST: DevicesAdmin/DeleteModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteModule(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool isDeleted = _adminRepository.DeleteDeviceModule(id, userId);

                if (isDeleted)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Device Module deleted successfully.";
                    TempData["ResultType"] = "success";
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to delete Device Module.";
                    TempData["ResultType"] = "danger";
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
            }
            return RedirectToAction("Manage");
        }

        #endregion

        #region Devices Actions

        // GET: DevicesAdmin/CreateDevice
        public IActionResult CreateDevice(int? moduleId = null)
        {
            ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", moduleId);
            var model = new DeviceViewModel();
            if (moduleId.HasValue)
            {
                model.ModuleId = moduleId.Value;
            }
            return View(model);
        }

        // POST: DevicesAdmin/CreateDevice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDevice(DeviceViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", model.ModuleId);
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View(model);
                }

                int userId = GetCurrentUserId();
                bool isCreated = _adminRepository.CreateDevice(model, userId);

                if (isCreated)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Device created successfully.";
                    TempData["ResultType"] = "success";
                    if (model.ModuleId > 0)
                    {
                        return RedirectToAction("ModuleDevices", new { id = model.ModuleId });
                    }
                    return RedirectToAction("Manage");
                }
                else
                {
                    ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", model.ModuleId);
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to create Device.";
                    TempData["ResultType"] = "danger";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", model.ModuleId);
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(model);
            }
        }

        // GET: DevicesAdmin/EditDevice/5
        public IActionResult EditDevice(int id)
        {
            try
            {
                var device = _adminRepository.GetDeviceById(id);
                if (device == null)
                {
                    TempData["ResultMessage"] = "<strong>Not Found!</strong> Device not found.";
                    TempData["ResultType"] = "warning";
                    return RedirectToAction("Manage");
                }
                ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", device.ModuleId);
                ViewBag.ReturnModuleId = device.ModuleId;
                return View(device);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Manage");
            }
        }

        // POST: DevicesAdmin/EditDevice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDevice(DeviceViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", model.ModuleId);
                    ViewBag.ReturnModuleId = model.ModuleId;
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View(model);
                }

                int userId = GetCurrentUserId();
                bool isUpdated = _adminRepository.UpdateDevice(model, userId);

                if (isUpdated)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Device updated successfully.";
                    TempData["ResultType"] = "success";
                    if (model.ModuleId > 0)
                    {
                        return RedirectToAction("ModuleDevices", new { id = model.ModuleId });
                    }
                    return RedirectToAction("Manage");
                }
                else
                {
                    ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", model.ModuleId);
                    ViewBag.ReturnModuleId = model.ModuleId;
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update Device.";
                    TempData["ResultType"] = "danger";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.DeviceModules = new SelectList(_adminRepository.GetAllDeviceModules(activeOnly: true), "Id", "Name", model.ModuleId);
                ViewBag.ReturnModuleId = model.ModuleId;
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(model);
            }
        }

        // POST: DevicesAdmin/DeleteDevice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDevice(int id, int? moduleId = null)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool isDeleted = _adminRepository.DeleteDevice(id, userId);

                if (isDeleted)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Device deleted successfully.";
                    TempData["ResultType"] = "success";
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to delete Device.";
                    TempData["ResultType"] = "danger";
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
            }
            
            if (moduleId.HasValue && moduleId.Value > 0)
            {
                return RedirectToAction("ModuleDevices", new { id = moduleId.Value });
            }
            return RedirectToAction("Manage");
        }

        // API: Get devices by module
        [HttpGet]
        public JsonResult GetDevicesByModule(int moduleId)
        {
            try
            {
                var devices = _adminRepository.GetAllDevices(moduleId, activeOnly: false);
                return Json(new { success = true, data = devices });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
