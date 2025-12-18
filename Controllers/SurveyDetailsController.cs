using AnalyticaDocs.Models;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurveyApp.Models;
using SurveyApp.Repo;
using System.Diagnostics;
using System.Text.Json;

namespace SurveyApp.Controllers
{
    public class SurveyDetailsController : Controller
    {
        private readonly ISurvey _repository;
        private readonly ICommonUtil _util;
        private readonly ISurveyLocationStatus _statusRepo;
        private readonly ISurveySubmission _submissionRepo;
        private readonly ISurveyCamRemarks _camRemarksRepo;

        public SurveyDetailsController(ISurvey repository, ICommonUtil util, ISurveyLocationStatus statusRepo, ISurveySubmission submissionRepo, ISurveyCamRemarks camRemarksRepo)
        {
            _repository = repository;
            _util = util;
            _statusRepo = statusRepo;
            _submissionRepo = submissionRepo;
            _camRemarksRepo = camRemarksRepo;
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

        public IActionResult GetItemSelectionPartial(long surveyId, int locId, int itemTypeID)
        {
            // Authorization check
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return Unauthorized();

            try
            {
                var locationStatus = _statusRepo.GetLocationStatus(surveyId, locId);
                string currentStatus = locationStatus?.Status ?? "Pending";

                if (currentStatus == "Completed" || currentStatus == "Verified")
                {
                    return Content($"<div class='alert alert-warning m-3'><i class='bi bi-exclamation-triangle-fill me-2'></i><strong>Location is {currentStatus}!</strong> Items cannot be modified. Click 'Unlock for Editing' to make changes.</div>");
                }

                var userId = HttpContext.Session.GetString("UserID");
                if (!string.IsNullOrEmpty(userId))
                {
                    if (currentStatus == "Pending")
                    {
                        _statusRepo.MarkLocationAsInProgress(surveyId, locId, Convert.ToInt32(userId), "Auto-marked when item selection started");
                    }
                    
                    var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);
                    if (submission != null && submission.SubmissionStatus == "Rejected")
                    {
                        _submissionRepo.SubmitSurvey(surveyId, Convert.ToInt32(userId), "In Progress");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetItemSelectionPartial status check: {ex.Message}");
            }


            var formModel = new SurveyDetailsUpdate
            {
                SurveyID = surveyId,
                LocID = locId,
                ItemTypeID = itemTypeID,
                ItemLists = _repository.GetSurveyUpdateItemList(surveyId, locId, itemTypeID) ?? new List<SurveyDetailsUpdatelist>()
            };

            // Load camera remarks and set IsCamera flag
            foreach (var item in formModel.ItemLists)
            {
                // Set IsCamera flag based on ItemCode
                item.IsCamera = item.ItemCode?.StartsWith("CAM", StringComparison.OrdinalIgnoreCase) == true;
                
                if (item.IsCamera)
                {
                    var remarks = _camRemarksRepo.GetCameraRemarks(surveyId, locId, item.ItemID);
                    if (remarks != null && remarks.Count > 0)
                    {
                        item.CameraRemarksJson = JsonSerializer.Serialize(remarks.Select(r => r.Remarks).ToList());
                    }
                }
            }

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

            return PartialView("_ItemSelection", formModel);
        }

        public IActionResult UpdateItem(Int64 surveyId, int locId, int itemTypeID, int itemId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Execute");
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

            // Load camera remarks for camera items (ItemCode starts with "CAM")
            foreach (var item in formModel.ItemLists)
            {
                bool isCamera = item.ItemCode?.StartsWith("CAM", StringComparison.OrdinalIgnoreCase) == true;
                if (isCamera)
                {
                    var remarks = _camRemarksRepo.GetCameraRemarks(surveyId, locId, item.ItemID);
                    if (remarks != null && remarks.Count > 0)
                    {
                        // Convert remarks to JSON for the view
                        item.CameraRemarksJson = JsonSerializer.Serialize(remarks.Select(r => r.Remarks).ToList());
                    }
                }
            }

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

            // Note: Pole Owner and Height options are now loaded from database via specifications system
            // They will be dynamically rendered based on ItemSpecificationMaster and ItemSpecificationOptionsMaster tables

            // Use single dynamic view for all item types
            return View("ItemMasterSelection", formModel);
        }

                [HttpPost]
                [ValidateAntiForgeryToken]
                public IActionResult UpdateItem(SurveyDetailsUpdate model)
                {
                    int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
                    var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyID, "Execute");
                    if (result != null) return Json(new { success = false, message = "Unauthorized" });

                    try
                    {
                        var locationStatus = _statusRepo.GetLocationStatus(model.SurveyID, model.LocID);
                        string currentStatus = locationStatus?.Status ?? "Pending";

                        if (currentStatus == "Completed" || currentStatus == "Verified")
                        {
                            return Json(new { success = false, message = $"Location is {currentStatus} and locked. Cannot save changes." });
                        }

                        var userId = HttpContext.Session.GetString("UserID");
                        if (!string.IsNullOrEmpty(userId))
                        {
                            if (currentStatus == "Pending")
                            {
                                _statusRepo.MarkLocationAsInProgress(model.SurveyID, model.LocID, Convert.ToInt32(userId), "Auto-marked on item save");
                            }

                            var submission = _submissionRepo.GetSubmissionBySurveyId(model.SurveyID);
                            if (submission != null && submission.SubmissionStatus == "Rejected")
                            {
                                _submissionRepo.SubmitSurvey(model.SurveyID, Convert.ToInt32(userId), "In Progress");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in UpdateItem [POST] status check: {ex.Message}");
                        // Do not abort the save, but log the error.
                    }
        
                    var userIdForCreate = HttpContext.Session.GetString("UserID");
                    if (string.IsNullOrEmpty(userIdForCreate))
                    {
                        return Json(new { success = false, message = "User not logged in" });
                    }
        
                    model.CreateBy = Convert.ToInt32(userIdForCreate);
        
                    if (!ModelState.IsValid)
                    {
                        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                        return Json(new { success = false, message = "Validation failed", errors });
                    }
        
                    try
                    {
                        // Process camera remarks (ItemCode starts with "CAM")
                        for (int i = 0; i < model.ItemLists.Count; i++)
                        {
                            var item = model.ItemLists[i];
                            // Check if it's a camera item by ItemCode prefix
                            bool isCamera = item.ItemCode?.StartsWith("CAM", StringComparison.OrdinalIgnoreCase) == true;
                            
                            if (isCamera && !string.IsNullOrEmpty(item.CameraRemarksJson))
                            {
                                try
                                {
                                    var remarks = JsonSerializer.Deserialize<List<string>>(item.CameraRemarksJson);
                                    
                                    if (remarks != null && remarks.Count > 0)
                                    {
                                        // Delete existing remarks for this camera item
                                        _camRemarksRepo.DeleteAllCameraRemarks(model.SurveyID, model.LocID, item.ItemID);
                                        
                                        // Save new remarks with sequence number
                                        int remarkNo = 1;
                                        foreach (var remark in remarks)
                                        {
                                            if (!string.IsNullOrWhiteSpace(remark))
                                            {
                                                var camRemark = new SurveyCamRemarksModel
                                                {
                                                    SurveyID = model.SurveyID,
                                                    LocID = model.LocID,
                                                    ItemID = item.ItemID,
                                                    RemarkNo = remarkNo,
                                                    Remarks = remark.Trim(),
                                                    CreatedBy = model.CreateBy
                                                };
                                                _camRemarksRepo.SaveCameraRemarks(camRemark);
                                                remarkNo++;
                                            }
                                        }
                                    }
                                }
                                catch (JsonException jsonEx)
                                {
                                    Console.WriteLine($"JSON parsing error for camera remarks: {jsonEx.Message}");
                                    // Continue processing, don't fail the entire update
                                }
                            }
                        }
        
                        bool isSaved = _repository.UpdateSurveyDetails(model);
        
                        if (isSaved)
                        {
                            // Save item specifications from form data
                            // Field names are: ItemSpecs_{itemIndex}_{specificationId}_{instance}
                            try
                            {
                                var formData = Request.Form;
                                var userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                                
                                // Process all ItemSpecs_ form fields
                                var processedSpecs = new HashSet<string>(); // Avoid duplicates
                                
                                foreach (var key in formData.Keys.Where(k => k.StartsWith("ItemSpecs_")))
                                {
                                    if (processedSpecs.Contains(key)) continue;
                                    processedSpecs.Add(key);
                                    
                                    // Parse: ItemSpecs_{itemIndex}_{specificationId}_{instance}
                                    var parts = key.Split('_');
                                    if (parts.Length >= 4)
                                    {
                                        if (int.TryParse(parts[1], out int itemIndex) &&
                                            int.TryParse(parts[2], out int specId) &&
                                            int.TryParse(parts[3], out int instance))
                                        {
                                            // Get item ID from the form using the item index
                                            var itemIdKey = $"ItemLists[{itemIndex}].ItemID";
                                            if (formData.ContainsKey(itemIdKey) && 
                                                int.TryParse(formData[itemIdKey], out int itemIdFromForm))
                                            {
                                                var specValue = formData[key].ToString();
                                                
                                                Console.WriteLine($"[Controller] Saving spec: Key={key}, ItemID={itemIdFromForm}, SpecID={specId}, Instance={instance}, Value={specValue}");
                                                
                                                if (!string.IsNullOrEmpty(specValue))
                                                {
                                                    var specModel = new SpecificationDetailsSubmitModel
                                                    {
                                                        SurveyID = model.SurveyID,
                                                        LocID = model.LocID,
                                                        ItemID = itemIdFromForm,
                                                        Specifications = new List<SpecificationDetailItem>
                                                        {
                                                            new SpecificationDetailItem
                                                            {
                                                                SpecificationID = specId,
                                                                SpecificationDetails = specValue,
                                                                InstanceNumber = instance
                                                            }
                                                        }
                                                    };
                                                    
                                                    _repository.SaveSpecificationDetails(specModel, userId);
                                                }
                                            }
                                            else
                                            {
                                                Console.WriteLine($"[Controller] Could not find ItemID for key: {itemIdKey}");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception specEx)
                            {
                                Console.WriteLine($"Error saving specifications: {specEx.Message}");
                                Console.WriteLine($"Stack trace: {specEx.StackTrace}");
                                // Don't fail the entire save, just log the error
                            }
                            
                            return Json(new { success = true, message = "Survey details updated successfully." });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Failed to update survey details." });
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Json(new { success = false, message = $"Validation Error! {ex.Message}" });
                    }
                                    catch (Exception ex)
                                    {
                                        return Json(new { success = false, message = $"Error! {ex.Message}" });
                                    }
                                }
                    
                                public IActionResult GetAccordionBody(long surveyId, int locId, int itemTypeID)
                                {
                                    // Authorization check
                                    var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
                                    if (result != null) return Unauthorized();
                    
                                    // Get the specific device type details
                                    var deviceType = (_repository.GetAssignedTypeList(surveyId, locId) ?? new List<SurveyDetailsLocationModel>())
                                                     .FirstOrDefault(dt => dt.ItemTypeID == itemTypeID);
                    
                                    if (deviceType == null)
                                    {
                                        return Content("<div class='alert alert-danger'>Could not reload item data.</div>");
                                    }
                    
                                    // Load the item list for this type
                                    deviceType.ItemLists = _repository.GetAssignedItemList(surveyId, locId, itemTypeID) ?? new List<SurveyDetailsModel>();
                    
                                    // Return the partial view with the model
                                    return PartialView("_SurveyDetailsGrid", deviceType);
                                }

        public IActionResult GetSurveyAccordionItem(long surveyId, int locId, int itemTypeID)
        {
            // Authorization check
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return Unauthorized();

            // Get the specific device type details
            var deviceType = (_repository.GetAssignedTypeList(surveyId, locId) ?? new List<SurveyDetailsLocationModel>())
                             .FirstOrDefault(dt => dt.ItemTypeID == itemTypeID);

            if (deviceType == null)
            {
                // Return a specific empty content if the item type is somehow gone
                return Content("");
            }

            // Load the item list for this type
            deviceType.ItemLists = _repository.GetAssignedItemList(surveyId, locId, itemTypeID) ?? new List<SurveyDetailsModel>();

            // Return the new partial view with the model
            return PartialView("_SurveyAccordionItem", deviceType);
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
        public IActionResult SubmitLocationCompletion(long surveyId, int locId, string? globalCableCount = null, string? globalCableRemarks = null)
        {
            try
            {
                int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
                var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Execute");
                if (result != null) 
                    return Json(new { success = false, message = "Unauthorized access" });

                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                // Save global cable count if provided
                if (!string.IsNullOrEmpty(globalCableCount))
                {
                    try
                    {
                        // Save cable count to database
                        _repository.SaveGlobalCableCount(surveyId, locId, globalCableCount, globalCableRemarks ?? "", Convert.ToInt32(userId));
                    }
                    catch (Exception cableEx)
                    {
                        Console.WriteLine($"Error saving global cable count: {cableEx.Message}");
                        // Continue with submission even if cable count save fails
                    }
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
                // User must have Delete rights to unlock a location
                var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Delete");
                if (result != null)
                    return Json(new { success = false, message = "You do not have permission to unlock locations. Delete rights are required." });

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

        [HttpGet]
        public IActionResult GetLocationStatus(long surveyId, int locId)
        {
            try
            {
                var locationStatus = _statusRepo.GetLocationStatus(surveyId, locId);
                string currentStatus = locationStatus?.Status ?? "Pending";
                return Json(new { success = true, status = currentStatus });
            }
            catch (Exception ex)
            {
                // Log the exception details for debugging
                Console.WriteLine($"Error fetching location status: {ex.Message}");
                return StatusCode(500, Json(new { success = false, message = "An error occurred while fetching the status." }));
            }
        }

        /// <summary>
        /// Get item specifications for a given item ID
        /// </summary>
        [HttpGet]
        public IActionResult GetItemSpecifications(int itemId)
        {
            try
            {
                var specifications = _repository.GetItemSpecifications(itemId);
                Console.WriteLine($"GetItemSpecifications for ItemID {itemId}: Found {specifications.Count} specifications");
                foreach (var spec in specifications)
                {
                    Console.WriteLine($"  - {spec.SpecificationName} (ID: {spec.SpecificationID}, Type: {spec.InputType}, Options: {spec.OptionsList?.Count ?? 0})");
                }
                return Json(new { success = true, specifications = specifications });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching item specifications: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get dropdown options for a specific specification ID
        /// </summary>
        [HttpGet]
        public IActionResult GetSpecificationOptions(int specificationId)
        {
            try
            {
                var options = _repository.GetSpecificationOptions(specificationId);
                Console.WriteLine($"GetSpecificationOptions for SpecID {specificationId}: Found {options.Count} options");
                return Json(new { success = true, options = options });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching specification options: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get saved specification details for a survey/location/item
        /// </summary>
        [HttpGet]
        public IActionResult GetSpecificationDetails(long surveyId, int locId, int itemId)
        {
            try
            {
                // Get saved values directly - return all instances
                var savedDetails = _repository.GetSpecificationDetails(surveyId, locId, itemId);

                // Return all saved details with their instance numbers
                var result = savedDetails.Select(sd => new {
                    sd.SurveyID,
                    sd.LocID,
                    sd.ItemID,
                    sd.SpecificationID,
                    sd.SpecificationName,
                    sd.InstanceNumber,
                    sd.SpecificationDetails
                }).ToList();

                return Json(new { success = true, specifications = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching specification details: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Save specification details for a survey item
        /// </summary>
        [HttpPost]
        public IActionResult SaveSpecificationDetails([FromBody] SpecificationDetailsSubmitModel model)
        {
            try
            {
                Console.WriteLine($"SaveSpecificationDetails called - SurveyID: {model?.SurveyID}, LocID: {model?.LocID}, ItemID: {model?.ItemID}");
                Console.WriteLine($"Specifications count: {model?.Specifications?.Count ?? 0}");
                
                if (model == null)
                {
                    Console.WriteLine("Model is null!");
                    return Json(new { success = false, message = "Invalid request - model is null" });
                }

                // Authorization check
                var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyID, "Execute");
                if (result != null) return Json(new { success = false, message = "Unauthorized" });

                // Check location status
                var locationStatus = _statusRepo.GetLocationStatus(model.SurveyID, model.LocID);
                string currentStatus = locationStatus?.Status ?? "Pending";

                if (currentStatus == "Completed" || currentStatus == "Verified")
                {
                    return Json(new { success = false, message = $"Location is {currentStatus} and locked. Cannot save changes." });
                }

                var userIdStr = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                int userId = Convert.ToInt32(userIdStr);

                // Validate input
                if (model.Specifications == null || model.Specifications.Count == 0)
                {
                    Console.WriteLine("No specifications to save");
                    return Json(new { success = true, message = "No specifications to save." });
                }

                // Log each specification
                foreach (var spec in model.Specifications)
                {
                    Console.WriteLine($"  Spec: ID={spec.SpecificationID}, Value={spec.SpecificationDetails}");
                }

                // Save specifications
                bool saved = _repository.SaveSpecificationDetails(model, userId);
                Console.WriteLine($"SaveSpecificationDetails result: {saved}");

                if (saved)
                {
                    return Json(new { success = true, message = "Specifications saved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save specifications." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving specification details: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
