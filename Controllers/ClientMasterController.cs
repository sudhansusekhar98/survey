using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using AnalyticaDocs.Repository;
using AnalyticaDocs.Models;
using System.Diagnostics;
using SurveyApp.Services;

namespace SurveyApp.Controllers
{
    public class ClientMasterController : Controller
    {
        private readonly IClientMaster _repository;
        private readonly ICommonUtil _util;
        private readonly ILocationApiService _locationService;

        public ClientMasterController(IClientMaster repository, ICommonUtil util, ILocationApiService locationService)
        {
            _repository = repository;
            _util = util;
            _locationService = locationService;
        }

        // GET: ClientMaster/Index
        public IActionResult Index()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 104, null, null, "View");
            if (result != null) return result;

            var clients = _repository.GetAllClients();
            return View(clients);
        }

        // GET: ClientMaster/Create
        public async Task<IActionResult> Create()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 104, null, null, "Add");
            if (result != null) return result;

            ViewBag.States = await _locationService.GetStatesAsync();
            return View(new ClientMasterModel());
        }

        // POST: ClientMaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClientMasterModel model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 104, null, null, "Add");
            if (result != null) return result;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ResultMessage"] = "User not logged in";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index", "UserLogin");
            }

            model.CreatedBy = Convert.ToInt32(userId);

            int newClientId = _repository.InsertClient(model);

            if (newClientId > 0)
            {
                TempData["ResultMessage"] = $"<strong>Success!</strong> Client <strong>{model.ClientName}</strong> created successfully.";
                TempData["ResultType"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> Failed to create client. Please try again.";
                TempData["ResultType"] = "danger";
                return View(model);
            }
        }

        // GET: ClientMaster/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 104, null, null, "Update");
            if (result != null) return result;

            var client = _repository.GetClientById(id);
            if (client == null)
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> Client not found.";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index");
            }

            ViewBag.States = await _locationService.GetStatesAsync();
            
            // If client has a state, load cities for that state
            if (!string.IsNullOrEmpty(client.State) && int.TryParse(client.State, out int stateId))
            {
                ViewBag.Cities = await _locationService.GetCitiesByStateAsync(stateId);
            }

            return View(client);
        }

        // POST: ClientMaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ClientMasterModel model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 104, null, null, "Update");
            if (result != null) return result;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isUpdated = _repository.UpdateClient(model);

            if (isUpdated)
            {
                TempData["ResultMessage"] = $"<strong>Success!</strong> Client <strong>{model.ClientName}</strong> updated successfully.";
                TempData["ResultType"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update client. Please try again.";
                TempData["ResultType"] = "danger";
                return View(model);
            }
        }

        // POST: ClientMaster/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 104, null, null, "Delete");
            if (result != null) return Json(new { success = false, message = "Unauthorized" });

            bool isDeleted = _repository.DeleteClient(id);

            if (isDeleted)
            {
                return Json(new { success = true, message = "Client deleted successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete client" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
