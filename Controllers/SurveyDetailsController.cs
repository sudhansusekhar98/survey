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
        private readonly ISurveyLocationStatus _statusRepo;
        private readonly ISurveySubmission _submissionRepo;

        public SurveyDetailsController(ISurvey repository, ICommonUtil util, ISurveyLocationStatus statusRepo, ISurveySubmission submissionRepo)
        {
            _repository = repository;
            _util = util;
            _statusRepo = statusRepo;
            _submissionRepo = submissionRepo;
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

            // Check location status (with error handling)
            try
            {
                var locationStatus = _statusRepo.GetLocationStatus(surveyId.Value, locId.Value);
                ViewBag.LocationStatus = locationStatus?.Status ?? "Pending";
                ViewBag.IsCompleted = locationStatus?.Status == "Completed" || locationStatus?.Status == "Verified";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading location status: {ex.Message}");
                ViewBag.LocationStatus = "Pending";
                ViewBag.IsCompleted = false;
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

            // Check location status (with error handling)
            string currentStatus = "Pending";
            try
            {
                var locationStatus = _statusRepo.GetLocationStatus(surveyId, locId);
                currentStatus = locationStatus?.Status ?? "Pending";
                
                // Prevent editing if location is completed or verified
                if (currentStatus == "Completed" || currentStatus == "Verified")
                {
                    TempData["ResultMessage"] = $"<strong>Location is {currentStatus}!</strong> Cannot modify items. Click 'Unlock for Editing' to make changes.";
                    TempData["ResultType"] = "warning";
                    return RedirectToAction("Index", new { surveyId, locId });
                }

                // Check if survey submission was rejected and change it back to In Progress
                var userId = HttpContext.Session.GetString("UserID");
                if (!string.IsNullOrEmpty(userId))
                {
                    var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);
                    
                    // If submission was rejected, change back to In Progress when user starts editing
                    if (submission != null && submission.SubmissionStatus == "Rejected")
                    {
                        _submissionRepo.SubmitSurvey(surveyId, Convert.ToInt32(userId), "In Progress");
                        TempData["ResultMessage"] = "<div class='alert alert-info'><i class='bi bi-info-circle'></i> Survey status changed from <strong>Rejected</strong> to <strong>In Progress</strong>. You can now make changes and resubmit.</div>";
                    }
                    
                    // Auto-mark location as In Progress when user starts selecting items
                    if (currentStatus == "Pending")
                    {
                        _statusRepo.MarkLocationAsInProgress(surveyId, locId, Convert.ToInt32(userId), "Auto-marked when item selection started");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking/updating location status: {ex.Message}");
                // Continue anyway - don't block the user if status tracking fails
            }

            var formModel = new SurveyDetailsUpdate
            {
                SurveyID = surveyId,
                LocID = locId,
                ItemTypeID = itemTypeID,
                ItemLists = _repository.GetSurveyUpdateItemList(surveyId, locId, itemTypeID) ?? new List<SurveyDetailsUpdatelist>()
            };

            // Get survey and location names for display
            var surveyInfo = _repository.GetAssignedTypeList(surveyId, locId)?.FirstOrDefault(x => x.ItemTypeID == itemTypeID);
            if (surveyInfo != null)
            {
                ViewBag.SelectedSurveyName = surveyInfo.SurveyName;
                ViewBag.SelectedLocName = surveyInfo.LocName;
                formModel.TypeName = surveyInfo.TypeName;
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

            try
            {
                bool isSaved = _repository.UpdateSurveyDetails(model);

                if (isSaved)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Survey details updated successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Index", new { surveyId = model.SurveyID, locId = model.LocID });
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update survey details.";
                    TempData["ResultType"] = "danger";
                    return RedirectToAction("UpdateItem", new { surveyId = model.SurveyID, locId = model.LocID, itemTypeID = model.ItemTypeID, itemId = 0 });
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ResultMessage"] = $"<strong>Validation Error!</strong> {ex.Message}";
                TempData["ResultType"] = "warning";
                return RedirectToAction("UpdateItem", new { surveyId = model.SurveyID, locId = model.LocID, itemTypeID = model.ItemTypeID, itemId = 0 });
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
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

                // Validate that all device types have at least one item with quantity
                var deviceTypes = _repository.GetAssignedTypeList(surveyId, locId) ?? new List<SurveyDetailsLocationModel>();
                var deviceTypesWithoutItems = new List<string>();

                foreach (var deviceType in deviceTypes)
                {
                    var items = _repository.GetSurveyUpdateItemList(surveyId, locId, deviceType.ItemTypeID);
                    
                    // Check if device type has no items added at all OR all items have zero quantities
                    if (items == null || !items.Any())
                    {
                        deviceTypesWithoutItems.Add(deviceType.TypeName);
                    }
                    else
                    {
                        // Check if at least one item has quantity > 0
                        bool hasAtLeastOneItemWithQty = items.Any(item => item.ItemQtyExist > 0 || item.ItemQtyReq > 0);
                        
                        if (!hasAtLeastOneItemWithQty)
                        {
                            deviceTypesWithoutItems.Add(deviceType.TypeName);
                        }
                    }
                }

                // Build error message if validation fails
                if (deviceTypesWithoutItems.Any())
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "Cannot submit location. The following device types need at least one item with quantity:",
                        errorDetails = string.Join("<br>", deviceTypesWithoutItems.Select(dt => $"• {dt}")),
                        deviceTypesWithoutItems = deviceTypesWithoutItems
                    });
                }

                // Auto-mark location as completed using status repository
                bool isCompleted = _statusRepo.MarkLocationAsCompleted(surveyId, locId, Convert.ToInt32(userId), "Auto-marked when items were submitted");

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

        // POST: SurveyDetails/UnlockLocationForEditing
        [HttpPost]
        public IActionResult UnlockLocationForEditing(long surveyId, int locId)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Change status back to In Progress to allow editing
                bool unlocked = _statusRepo.MarkLocationAsInProgress(surveyId, locId, Convert.ToInt32(userId), "Unlocked for editing");

                if (unlocked)
                {
                    return Json(new { success = true, message = "Location unlocked for editing" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to unlock location" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Submit survey for approval - checks if survey can be submitted
        /// </summary>
        [HttpPost]
        public IActionResult SubmitSurveyForApproval(Int64 surveyId)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Check if survey can be edited (not already locked)
                bool canEdit = _submissionRepo.CanEditSurvey(surveyId);
                if (!canEdit)
                {
                    return Json(new { success = false, message = "Survey is already submitted and locked" });
                }

                // Redirect to submission controller which handles email notifications
                return Json(new 
                { 
                    success = true, 
                    redirectUrl = Url.Action("SubmitForApproval", "SurveySubmission", new { surveyId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Check if survey can be edited based on submission status
        /// </summary>
        [HttpGet]
        public IActionResult CheckSurveyEditStatus(Int64 surveyId)
        {
            try
            {
                bool canEdit = _submissionRepo.CanEditSurvey(surveyId);
                var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);

                return Json(new 
                { 
                    success = true,
                    canEdit = canEdit,
                    status = submission?.SubmissionStatus ?? "Draft",
                    isLocked = submission?.IsLockedForEditing ?? false,
                    submittedBy = submission?.SubmittedByName,
                    submissionDate = submission?.SubmissionDate?.ToString("dd-MMM-yyyy hh:mm tt")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
