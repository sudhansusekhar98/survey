using AnalyticaDocs.Models;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using System.Diagnostics;

namespace SurveyApp.Controllers
{
    public class SurveyDetailsController : Controller
    {
        private readonly ISurvey _repository;
        private readonly ICommonUtil _util;

        public SurveyDetailsController(ISurvey repository, ICommonUtil util)
        {
            _repository = repository;
            _util = util;
        }

        public IActionResult Index(long? surveyId, int? locId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return result;

            // If no parameters provided, redirect to survey list
            if (!surveyId.HasValue || !locId.HasValue)
            {
                TempData["ResultMessage"] = "Please select a survey and location.";
                TempData["ResultType"] = "warning";
                return RedirectToAction("Index", "SurveyCreation");
            }

            // Get the list of types/locations assigned
            var deviceTypes = _repository.GetAssignedTypeList(surveyId.Value, locId.Value)
                              ?? new List<SurveyDetailsLocationModel>();

            var modelList = new List<SurveyDetailsLocationModel>();

            foreach (var dt in deviceTypes)
            {
                // Load item list for this type/location
                var items = _repository.GetAssignedItemList(dt.SurveyID, dt.LocID, dt.ItemTypeID)
                            ?? new List<SurveyDetailsModel>();

                // Create a new instance so we keep any extra properties from dt
                modelList.Add(new SurveyDetailsLocationModel
                {
                    SurveyID = dt.SurveyID,
                    LocID = dt.LocID,
                    ItemTypeID = dt.ItemTypeID,
                    LocName = dt.LocName,
                    SurveyName = dt.SurveyName,
                    TypeName = dt.TypeName,
                    TypeDesc = dt.TypeDesc,
                    GroupName = dt.GroupName,
                    CreatedBy = dt.CreatedBy,
                    ItemLists = items
                });
            }

            ViewBag.SelectedSurveyId = surveyId.Value;
            ViewBag.SelectedLocId = locId.Value;

            // Pass the list as the model to the view
            return View("SurveyDetails", modelList);
        }

        public IActionResult UpdateItem(Int64 surveyId, int locId, int itemTypeID, int itemId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Update");
            if (result != null) return result;

            if (surveyId <= 0 || itemTypeID <= 0 || locId <= 0)
            {
                TempData["ResultMessage"] = "Invalid survey, location, or item type.";
                TempData["ResultType"] = "error";
                return RedirectToAction("Index", new { surveyId, locId });
            }

            var formModel = new SurveyDetailsUpdate
            {
                SurveyID = surveyId,
                LocID = locId,
                ItemTypeID = itemTypeID,
                ItemLists = _repository.GetSurveyUpdateItemList(surveyId, locId, itemTypeID) ?? new List<SurveyDetailsUpdatelist>()
            };

            // Get survey and location names for display
            var surveyInfo = _repository.GetAssignedTypeList(surveyId, locId)?.FirstOrDefault();
            if (surveyInfo != null)
            {
                ViewBag.SelectedSurveyName = surveyInfo.SurveyName;
                ViewBag.SelectedLocName = surveyInfo.LocName;
            }

            ViewBag.SelectedSurveyId = surveyId;
            ViewBag.SelectedLocId = locId;
            ViewBag.ItemTypeID = itemTypeID;

            // Use single dynamic view for all item types
            return View("ItemMasterSelection", formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(SurveyDetailsUpdate model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyID, "Update");
            if (result != null) return Json(new { success = false, message = "Unauthorized" });

            var userId = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not logged in" });
            }

            model.CreateBy = Convert.ToInt32(userId);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Validation failed", errors });
            }

            bool isSaved = _repository.UpdateSurveyDetails(model);

            if (isSaved)
            {
                TempData["ResultMessage"] = "Survey details updated successfully.";
                TempData["ResultType"] = "success";
                return RedirectToAction("Index", new { surveyId = model.SurveyID, locId = model.LocID });
            }
            else
            {
                TempData["ResultMessage"] = "Failed to update survey details.";
                TempData["ResultType"] = "error";
                return RedirectToAction("UpdateItem", new { surveyId = model.SurveyID, locId = model.LocID, itemTypeID = model.ItemTypeID, itemId = 0 });
            }
        }

        // GET: SurveyDetails/GetLocationPreview
        public IActionResult GetLocationPreview(long surveyId, int locId)
        {
            try
            {
                // Get the list of types/locations assigned
                var deviceTypes = _repository.GetAssignedTypeList(surveyId, locId)
                                  ?? new List<SurveyDetailsLocationModel>();

                var modelList = new List<SurveyDetailsLocationModel>();

                foreach (var dt in deviceTypes)
                {
                    // Load item list for this type/location
                    var items = _repository.GetAssignedItemList(dt.SurveyID, dt.LocID, dt.ItemTypeID)
                                ?? new List<SurveyDetailsModel>();

                    // Create a new instance so we keep any extra properties from dt
                    modelList.Add(new SurveyDetailsLocationModel
                    {
                        SurveyID = dt.SurveyID,
                        LocID = dt.LocID,
                        ItemTypeID = dt.ItemTypeID,
                        LocName = dt.LocName,
                        SurveyName = dt.SurveyName,
                        TypeName = dt.TypeName,
                        TypeDesc = dt.TypeDesc,
                        GroupName = dt.GroupName,
                        CreatedBy = dt.CreatedBy,
                        ItemLists = items
                    });
                }

                return PartialView("_LocationPreview", modelList);
            }
            catch (Exception ex)
            {
                return Content($"<div class='alert alert-danger'><i class='bi bi-exclamation-triangle me-2'></i>Error loading preview: {ex.Message}</div>");
            }
        }

        // POST: SurveyDetails/SubmitLocationCompletion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitLocationCompletion(long surveyId, int locId)
        {
            try
            {
                int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
                var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Update");
                if (result != null) 
                    return Json(new { success = false, message = "Unauthorized access" });

                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Mark location as completed
                bool isCompleted = _repository.MarkLocationAsCompleted(surveyId, locId, Convert.ToInt32(userId));

                if (isCompleted)
                {
                    return Json(new { success = true, message = "Location marked as completed successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to mark location as completed. Please try again." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
