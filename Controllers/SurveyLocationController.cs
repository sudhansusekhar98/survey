using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;

namespace SurveyApp.Controllers
{
    public class SurveyLocationController : Controller
    {
        private readonly ISurveyLocation _locationRepo;
        private readonly ICommonUtil _util;

        public SurveyLocationController(ISurveyLocation locationRepo, ICommonUtil util)
        {
            _locationRepo = locationRepo;
            _util = util;
        }

        public IActionResult Index()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "View");
            if (result != null) return result;

            var locations = new List<SurveyLocationModel>();
            return View(locations);
        }

        public IActionResult Edit(int id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Update");
            if (result != null) return result;

            var location = _locationRepo.GetLocationById(id);
            if (location == null)
                return RedirectToAction("Index");
            return View("SurveyLocationEdit", location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SurveyLocationModel model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, model.SurveyID, "Update");
            if (result != null) return result;

            if (!ModelState.IsValid)
                return View("SurveyLocationEdit", model);

            _locationRepo.UpdateLocation(model);
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Create");
            if (result != null) return result;

            return View("SurveyLocationCreate", new SurveyLocationModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SurveyLocationModel model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, model.SurveyID, "Create");
            if (result != null) return result;

            if (!ModelState.IsValid)
                return View("SurveyLocationCreate", model);

            _locationRepo.AddLocation(model);
            return RedirectToAction("Index");       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, rightsId, null, null, "Delete");
            if (result != null) return Json(new { success = false, message = "Unauthorized" });

            _locationRepo.DeleteLocation(id);
            return RedirectToAction("Index");
        }
    }
}
