using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SurveyApp.Models;
using SurveyApp.Repo;
using System;
using System.Linq;

namespace SurveyApp.Controllers
{
    public class SurveyCreationController : Controller
    {
        private readonly ISurvey _surveyRepository;
        private readonly ICommonUtil _util;
        private readonly IAdmin _adminRepository;
        private readonly ISurveyLocationStatus _statusRepo;
        private readonly IClientMaster _clientRepository;
        private readonly SurveyApp.Services.ILocationApiService _locationService;

        public SurveyCreationController(ISurvey surveyRepository, ICommonUtil util, IAdmin adminRepository, ISurveyLocationStatus statusRepo, IClientMaster clientRepository, SurveyApp.Services.ILocationApiService locationService)
        {
            _surveyRepository = surveyRepository;
            _util = util;
            _adminRepository = adminRepository;
            _statusRepo = statusRepo;
            _clientRepository = clientRepository;
            _locationService = locationService;
        }
    // ...existing code...
    // ...existing code...

    // ...existing code...

            // GET: SurveyCreation/CameraDevices 
            public IActionResult CameraDevices()
            {
                return View();
            }
            

        // GET: SurveyCreation/Index - List all surveys with filtering
        public IActionResult Index(string? status = null, string? region = null, string? type = null, string? search = null, bool? missed = null)
        {
            var result = _util.CheckAuthorizationAll(this, 103, null,null,"View");
            //if (result != null) return result;
            int UserID = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
            try
            {
                var surveys = _surveyRepository.GetAllSurveys(UserID) ?? new List<SurveyModel>();
                var today = DateTime.Now.Date;
                
                // Handle "Missed Deadline" as a special status value
                if (status == "Missed Deadline")
                {
                    missed = true;
                    status = null;
                }
                
                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    surveys = surveys.Where(s => s.SurveyStatus == status).ToList();
                }
                
                if (!string.IsNullOrEmpty(region))
                {
                    surveys = surveys.Where(s => s.RegionName == region).ToList();
                }
                
                if (!string.IsNullOrEmpty(type))
                {
                    surveys = surveys.Where(s => s.ImplementationType == type).ToList();
                }
                
                if (missed == true)
                {
                    surveys = surveys.Where(s => 
                        s.DueDate.HasValue && 
                        s.DueDate.Value.Date < today && 
                        s.SurveyStatus != "Completed"
                    ).ToList();
                }
                
                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    surveys = surveys.Where(s => 
                        (s.SurveyName != null && s.SurveyName.ToLower().Contains(searchLower)) ||
                        (s.LocationSiteName != null && s.LocationSiteName.ToLower().Contains(searchLower)) ||
                        (s.CityDistrict != null && s.CityDistrict.ToLower().Contains(searchLower)) ||
                        (s.SurveyTeamName != null && s.SurveyTeamName.ToLower().Contains(searchLower))
                    ).ToList();
                }
                
                // Get submission status and assignment status for each survey
                foreach (var survey in surveys)
                {
                    var submission = _surveyRepository.GetSubmissionBySurveyId(survey.SurveyId);
                    if (submission != null)
                    {
                        ViewData[$"IsSubmitted_{survey.SurveyId}"] = submission.SubmissionStatus == "Submitted" || submission.SubmissionStatus == "Approved";
                        ViewData[$"IsLocked_{survey.SurveyId}"] = submission.IsLockedForEditing;
                    }
                    
                    // Check if survey has team assignments
                    var assignments = _surveyRepository.GetSurveyAssignments(survey.SurveyId);
                    ViewData[$"HasAssignments_{survey.SurveyId}"] = assignments != null && assignments.Count > 0;
                }
                
                // Get all surveys for filter options
                var allSurveys = _surveyRepository.GetAllSurveys(UserID) ?? new List<SurveyModel>();
                
                // Pass filter options to view
                ViewBag.StatusOptions = allSurveys
                    .Where(s => !string.IsNullOrEmpty(s.SurveyStatus))
                    .Select(s => s.SurveyStatus)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
                    
                ViewBag.RegionOptions = allSurveys
                    .Where(s => !string.IsNullOrEmpty(s.RegionName))
                    .Select(s => s.RegionName)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();
                    
                ViewBag.TypeOptions = allSurveys
                    .Where(s => !string.IsNullOrEmpty(s.ImplementationType))
                    .Select(s => s.ImplementationType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
                
                // Pass current filter values
                ViewBag.CurrentStatus = status;
                ViewBag.CurrentRegion = region;
                ViewBag.CurrentType = type;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentMissed = missed;
                ViewBag.IsFiltered = !string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(region) || 
                                     !string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(search) || missed == true;
                ViewBag.TotalCount = allSurveys.Count;
                ViewBag.FilteredCount = surveys.Count;
                
                // Calculate missed deadline count for display
                ViewBag.MissedDeadlineCount = allSurveys.Count(s => 
                    s.DueDate.HasValue && 
                    s.DueDate.Value.Date < today && 
                    s.SurveyStatus != "Completed"
                );
                
                return View(surveys);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View("Index", new List<SurveyModel>());
            }
        }

        // GET: SurveyCreation/Create
        public IActionResult Create()
        {
            var result = _util.CheckAuthorizationAll(this, 103, null, null, "Create");
            if (result != null) return result;

            ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc");
            ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName");
            return View("SurveyCreation", new SurveyModel());
        }

        // POST: SurveyCreation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SurveyModel model)
        {

           
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", model.RegionID);
                    ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", model.ClientID);
                    return View("SurveyCreation", model);
                }
                
                // Set CreatedBy from session
                model.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");

                bool result = _surveyRepository.AddSurvey(model);
                
                if (result)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Survey created successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to create survey.";
                    TempData["ResultType"] = "danger";
                    ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", model.RegionID);
                    ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", model.ClientID);
                    return View("SurveyCreation", model);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", model.RegionID);
                ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", model.ClientID);
                return View("SurveyCreation", model);
            }
        }

        // GET: SurveyCreation/Edit/5
        public IActionResult Edit(Int64? id)
        {
            //int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, id, "Update");
            if (result != null) return result;

            if (!id.HasValue || id.Value == 0)
            {
                TempData["ResultMessage"] = "<strong>Info!</strong> Record Not Found.";
                TempData["ResultType"] = "warning";
                return RedirectToAction("Index");
            }
            var survey = _surveyRepository.GetSurveyById(id.Value);
            if (survey == null) 
            {
                TempData["ResultMessage"] = "<strong>Not Found!</strong> Survey not found.";
                TempData["ResultType"] = "warning";
                return RedirectToAction("Index"); // Render full page for normal navigation
            }
            ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", survey.RegionID);
            ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", survey.ClientID);
            return View("SurveyEdit", survey); // Render full page for normal navigation
        }

        // POST: SurveyCreation/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SurveyModel model)
        {
            //int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, model.RegionID, model.SurveyId, "Update");
            if (result != null) return result;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", model.RegionID);
                    ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", model.ClientID);
                    return View("SurveyEdit", model);
                }

                // Set CreatedBy from session
                model.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");

                // Preserve original status
                var originalSurvey = _surveyRepository.GetSurveyById(model.SurveyId);
                if (originalSurvey != null)
                {
                    model.SurveyStatus = originalSurvey.SurveyStatus;
                }

                bool isSaved = _surveyRepository.UpdateSurvey(model);

                if (isSaved)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Survey updated successfully.";
                    TempData["ResultType"] = "success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update survey.";
                    TempData["ResultType"] = "danger";
                    ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", model.RegionID);
                    ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", model.ClientID);
                    return View("SurveyEdit", model);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                ViewBag.Regions = new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc", model.RegionID);
                ViewBag.Clients = new SelectList(_clientRepository.GetAllClients(), "ClientID", "ClientName", model.ClientID);
                return View("SurveyEdit", model);
            }
        }

        // POST: SurveyCreation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Int64 id)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, id, "Delete");
            if (result != null) return Json(new { success = false, message = "Unauthorized" });

            try
            {
                bool isDeleted = _surveyRepository.DeleteSurvey(id);

                if (isDeleted)
                {
                    return Json(new { success = true, message = "Survey deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete survey." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: SurveyCreation/ViewDetails/5
        public IActionResult ViewDetails(Int64 id)
        {
            try
            {
                var survey = _surveyRepository.GetSurveyById(id);
                if (survey == null)
                {
                    return NotFound();
                }
                
                // Check if survey is submitted
                var submission = _surveyRepository.GetSubmissionBySurveyId(id);
                ViewBag.IsSubmitted = submission != null;
                
                return PartialView("_SurveyDetailModal", survey);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //GET: SurveyCreation/SurveyAssignment
        public IActionResult SurveyAssignment(Int64 surveyId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return result;

            var assignments = _surveyRepository.GetSurveyAssignments(surveyId) ?? new List<SurveyAssignmentModel>();
            var survey = _surveyRepository.GetSurveyById(surveyId); 
            ViewBag.SurveyName = survey?.SurveyName;
            
            if (assignments == null || assignments.Count == 0)
            {
                TempData["ResultMessage"] = $"<strong>Info!</strong> No assignments found for this survey {surveyId}.";
                TempData["ResultType"] = "danger";
            }
            
            ViewBag.SurveyID = surveyId;
            return View("SurveyAssignment", assignments);
        }

        // GET: SurveyCreation/CreateSurveyAssignment
        public IActionResult CreateSurveyAssignment(Int64 surveyId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Create");
            if (result != null) return result;

            var model = new SurveyAssignmentModel { SurveyID = surveyId };
            
            // Pre-populate DueDate from existing assignments
            var existingAssignments = _surveyRepository.GetSurveyAssignments(surveyId);
            if (existingAssignments != null && existingAssignments.Count > 0)
            {
                model.DueDate = existingAssignments[0].DueDate;
            }
            
            ViewBag.SurveyID = surveyId;
            
            // TODO: Populate employee dropdown
            ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName");            
            return View(model);        
        }

        //POST: SurveyCreation/SurveyAssignment/Create
                [HttpPost]
                [ValidateAntiForgeryToken]
                public IActionResult CreateSurveyAssignment(SurveyAssignmentModel model)
                {
                    int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
                    var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyID, "Create");
                    if (result != null) return result;

                    try
                    {
                        if (!ModelState.IsValid)
                        {
                            ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName");
                            foreach (var key in ModelState.Keys)
                            {
                                var modelStateEntry = ModelState[key];
                                if (modelStateEntry != null)
                                {
                                    var errors = modelStateEntry.Errors;
                                    foreach (var error in errors)
                                    {
                                        Console.WriteLine($"{key}: {error.ErrorMessage}");
                                    }
                                }
                            }
                            TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                            TempData["ResultType"] = "warning";
                            return View("CreateSurveyAssignment", model);
                        }
                        int createdBy = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "101");
                        if (model.SelectedEmpIDs != null && model.SelectedEmpIDs.Count > 0)
                        {
                            foreach (var empId in model.SelectedEmpIDs)
                            {
                                var assignment = new SurveyAssignmentModel
                                {
                                    SurveyID = model.SurveyID,
                                    EmpID = empId,
                                    DueDate = model.DueDate,
                                    CreateBy = createdBy,
                                };
                                _surveyRepository.AddSurveyAssignment(assignment);
                            }
                            TempData["ResultMessage"] = "<strong>Success!</strong> Assignments created successfully.";
                            TempData["ResultType"] = "success";
                            return RedirectToAction("SurveyAssignment", new { surveyId = model.SurveyID });
                        }
                        else
                        {
                            ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName");
                            TempData["ResultMessage"] = "<strong>Error!</strong> No employees selected.";
                            TempData["ResultType"] = "danger";
                            return View("CreateSurveyAssignment", model); // <-- Fix here
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName");
                        TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                        TempData["ResultType"] = "danger";
                        return View("CreateSurveyAssignment", model); // <-- Fix here
                    }
        }

        // GET: SurveyCreation/EditSurveyAssignment
        public IActionResult EditSurveyAssignment(int transId, Int64 surveyId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Update");
            if (result != null) return result;

            try
            {
                var assignments = _surveyRepository.GetSurveyAssignments(surveyId);
                var assignment = assignments?.FirstOrDefault(a => a.TransID == transId);

                if (assignment == null)
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Assignment not found.";
                    TempData["ResultType"] = "danger";
                    return RedirectToAction("SurveyAssignment", new { surveyId });
                }

                ViewBag.SurveyID = surveyId;
                ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName", assignment.EmpID);
                return View("CreateSurveyAssignment", assignment);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("SurveyAssignment", new { surveyId });
            }
        }

        // POST: SurveyCreation/EditSurveyAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSurveyAssignment(SurveyAssignmentModel model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyID, "Update");
            if (result != null) return result;

            try
            {
                // Remove SelectedEmpIDs from ModelState for edit mode
                ModelState.Remove("SelectedEmpIDs");
                
                if (!ModelState.IsValid)
                {
                    ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName", model.EmpID);
                    TempData["ResultMessage"] = "<strong>Validation Error!</strong> Please check all required fields.";
                    TempData["ResultType"] = "warning";
                    return View("CreateSurveyAssignment", model);
                }

                // Update due date for all assignments of this survey
                bool isUpdated = _surveyRepository.UpdateAllSurveyAssignmentsDueDate(model.SurveyID, model.DueDate);

                if (isUpdated)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Due date updated for all assignments.";
                    TempData["ResultType"] = "success";
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update due date for assignments.";
                    TempData["ResultType"] = "danger";
                }
                return RedirectToAction("SurveyAssignment", new { surveyId = model.SurveyID });
            }
            catch (Exception ex)
            {
                ViewBag.Employees = new SelectList(_adminRepository.GetEmpMaster(), "EmpID", "EmpName", model.EmpID);
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View("CreateSurveyAssignment", model);
            }
        }

        // POST: SurveyCreation/DeleteSurveyAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSurveyAssignment(int transId, Int64 surveyId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Delete");
            if (result != null) return result;

            try
            {
                bool isDeleted = _surveyRepository.DeleteSurveyAssignment(transId);

                if (isDeleted)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Assignment deleted successfully.";
                    TempData["ResultType"] = "success";
                }
                else
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Failed to delete assignment.";
                    TempData["ResultType"] = "danger";
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
            }

            return RedirectToAction("SurveyAssignment", new { surveyId });
        }

        // GET: SurveyCreation/SurveyLocation
        public IActionResult SurveyLocation(Int64 surveyId, string SurveyName, int? editId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return result;

            // Fetch locations for the selected survey
            var locations = _surveyRepository.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
            
            // Get all location statuses for this survey (with error handling)
            try
            {
                var statuses = _statusRepo.GetSurveyLocationStatuses(surveyId);
                ViewBag.LocationStatuses = statuses.ToDictionary(s => s.LocID, s => s.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading location statuses: {ex.Message}");
                ViewBag.LocationStatuses = new Dictionary<int, string>();
            }
            
            ViewBag.SelectedSurveyId = surveyId;
            ViewBag.SelectedSurveyName = SurveyName;
            ViewBag.LocationTypeOptions = SurveyLocationModel.LocationTypeOptions;
            ViewBag.WayTypeOptions = SurveyLocationModel.WayTypeOptions;
            return View("SurveyLocation", locations);
        }

        // POST: SurveyCreation/SurveyLocation - Handle inline create/update form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SurveyLocation(SurveyLocationModel model, bool Isactive = false)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            string actionType = model.LocID > 0 ? "Update" : "Create";
            var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyID, actionType);
            if (result != null) return result;

            try
            {
                // Explicitly set Isactive from the parameter
                model.Isactive = Isactive;
                
                // Get current user from session
                int createdBy = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                model.CreateBy = createdBy;

                bool isSaved;
        
                // Check if this is an update or create operation
                if (model.LocID > 0)
                {
                    // Update existing location
                    isSaved = _surveyRepository.UpdateSurveyLocation(model);
            
                    if (isSaved)
                    {
                        TempData["ResultMessage"] = "<strong>Success!</strong> Location updated successfully.";
                        TempData["ResultType"] = "success";
                    }
                    else
                    {
                        TempData["ResultMessage"] = "<strong>Error!</strong> Failed to update location.";
                        TempData["ResultType"] = "danger";
                    }
                }
                else
                {
                    // Create new location
                    isSaved = _surveyRepository.AddSurveyLocation(model);
                    if (isSaved)
                    {
                        TempData["ResultMessage"] = "<strong>Success!</strong> Location added successfully.";
                        TempData["ResultType"] = "success";
                    }
                    else
                    {
                        TempData["ResultMessage"] = "<strong>Error!</strong> Failed to add location.";
                        TempData["ResultType"] = "danger";
                    }
                }
                return RedirectToAction("SurveyLocation", new { surveyId = model.SurveyID, SurveyName = ViewBag.SelectedSurveyName });
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("SurveyLocation", new { surveyId = model.SurveyID });
            }
        }

        // Delete Survey sub-locations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSurveyLocation(int locId, int surveyId, string surveyName = "")
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Delete");
            if (result != null) return result;

            try
            {
                bool isDeleted = _surveyRepository.DeleteSurveyLocation(locId);
                TempData["ResultMessage"] = isDeleted
                    ? "<strong>Success!</strong> Location deleted successfully."
                    : "<strong>Error!</strong> Failed to delete location.";
                TempData["ResultType"] = isDeleted ? "success" : "danger";
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
            }

            // Try to retain surveyName
            surveyName = !string.IsNullOrWhiteSpace(surveyName)
                ? surveyName
                : Request.Form["surveyName"].ToString()
                ?? Request.Query["surveyName"].ToString()
                ?? TempData["SelectedSurveyName"] as string
                ?? "";

            TempData["SelectedSurveyName"] = surveyName;

            return RedirectToAction("SurveyLocation", new { surveyId, surveyName });
        }


        // POST: SurveyCreation/AddSurveyLocations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSurveyLocations(Int64 surveyId, List<SurveyLocationModel> locations)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "Create");
            if (result != null) return result;

            int createdBy = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
            if (locations == null || locations.Count == 0)
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> No locations provided.";
                TempData["ResultType"] = "danger";
                return RedirectToAction("SurveyLocation", new { surveyId });
            }

            bool isSuccess = _surveyRepository.CreateLocationsBySurveyId(surveyId, locations, createdBy);

            if (isSuccess)
            {
                TempData["ResultMessage"] = "<strong>Success!</strong> Locations added successfully.";
                TempData["ResultType"] = "success";
            }
            else
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> Failed to add locations.";
                TempData["ResultType"] = "danger";
            }
            return RedirectToAction("SurveyLocation", new { surveyId });
        }

        // GET: SurveyCreation/ItemTypeMaster
        public IActionResult ItemTypeMaster(int locId, string SurveyName, Int64 surveyId)
        {
            //int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return result;

            try
            {
                var itemTypes = _surveyRepository.GetItemTypeMaster(locId) ?? new List<ItemTypeMasterModel>();
                var selectedItemTypes = _surveyRepository.GetSelectedItemTypesForLocation(locId) ?? new List<ItemTypeMasterModel>();

                // Mark items that are already assigned
                var selectedIds = selectedItemTypes.Select(x => x.Id).ToList();
                foreach (var item in itemTypes)
                {
                    item.IsAssigned = selectedIds.Contains(item.Id);
                }
                ViewBag.LocId = locId; // Pass locId to the view if needed
                ViewBag.SelectedSurveyId = surveyId;
                ViewBag.SelectedSurveyName = SurveyName;
                return View("ItemTypeMaster", itemTypes);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View("ItemTypeMaster", new List<ItemTypeMasterModel>());
            }
        }

        // GET: SurveyCreation/SaveItemType
        [HttpGet]
        public IActionResult ItemTypeMaster(int locId, Int64 surveyId)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return result;

            var formModel = new AssignedItemsModel
            {
                SurveyId = surveyId,
                LocID = locId,

                AssignItemList = _surveyRepository.GetItemTypebySurveyLoc(locId, surveyId) ?? new List<AssignedItemsListModel>()
            };
            return View("ItemTypeMaster", formModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveItemType(AssignedItemsModel model)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, model.SurveyId, "Update");
            if (result != null) return Json("unauthorized");

            // Get action value from form
            var action = Request.Form["action"].ToString();

            if (!ModelState.IsValid)
            {
                return Json("invalid");
            }

            model.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            bool isSaved = _surveyRepository.UpdateAssignedItems(model);

            TempData["ResultMessage"] = "Device Updated for Survey";
            TempData["ResultType"] = "success";

            if (action == "start")
            {
                // Redirect to SurveyDetails accordion page
                return RedirectToAction("Index", "SurveyDetails", new { surveyId = model.SurveyId, locId = model.LocID });
            }
            else
            {
                var locations = _surveyRepository.GetSurveyLocationById(model.SurveyId) ?? new List<SurveyLocationModel>();
                ViewBag.SelectedSurveyId = model.SurveyId;
                //ViewBag.SelectedSurveyName = SurveyName;
                return View("SurveyLocation", locations);
            }
        }

        //GET: SurveyCreation/ViewSelectedItemTypes 
        public IActionResult ViewSelectedItemTypes(int locId, Int64 surveyId, string surveyName)
        {
            int rightsId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "101");
            var result = _util.CheckAuthorizationAll(this, 103, null, surveyId, "View");
            if (result != null) return result;

            try
            {
                var selectedItemTypes = _surveyRepository.GetSelectedItemTypesForLocation(locId);
                var location = _surveyRepository.GetSurveyLocationByLocId(locId);

                var viewModel = new AssignedItemsModel
                {
                    LocID = locId,
                    SurveyId = surveyId,
                    SurveyName = surveyName ?? string.Empty,
                    AssignItemList = selectedItemTypes.Select
                    (
                        item => new AssignedItemsListModel
                        {
                            ItemTypeID = item.Id,
                            TypeName = item.TypeName,
                            TypeDesc = item.TypeDesc,
                            GroupName = item.GroupName,
                            IsAssigned = true,
                        }
                    ).ToList()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // Survey Location Status Management Actions
        
        [HttpPost]
        public IActionResult MarkLocationAsCompleted(long surveyId, int locId, string? remarks)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                
                // Log the parameters being sent
                Console.WriteLine($"MarkLocationAsCompleted called: SurveyID={surveyId}, LocID={locId}, UserID={userId}, Remarks={remarks}");
                
                bool result = _statusRepo.MarkLocationAsCompleted(surveyId, locId, userId, remarks);
                
                if (result)
                {
                    return Json(new { success = true, message = "Location marked as completed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to mark location as completed. Check server logs for details." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkLocationAsCompleted Exception: {ex.ToString()}");
                return Json(new { success = false, message = $"Error: {ex.Message}\n{ex.StackTrace}" });
            }
        }

        [HttpPost]
        public IActionResult MarkLocationAsInProgress(long surveyId, int locId, string? remarks)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                
                bool result = _statusRepo.MarkLocationAsInProgress(surveyId, locId, userId, remarks);
                
                if (result)
                {
                    return Json(new { success = true, message = "Location marked as in progress successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to mark location as in progress." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult MarkLocationAsVerified(long surveyId, int locId, string? remarks)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                
                bool result = _statusRepo.MarkLocationAsVerified(surveyId, locId, userId, remarks);
                
                if (result)
                {
                    return Json(new { success = true, message = "Location marked as verified successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to mark location as verified." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetLocationStatus(long surveyId, int locId)
        {
            try
            {
                var status = _statusRepo.GetLocationStatus(surveyId, locId);
                
                if (status != null)
                {
                    return Json(new { success = true, data = status });
                }
                else
                {
                    return Json(new { success = true, data = new { Status = "Pending" } });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetSurveyLocationStatuses(long surveyId)
        {
            try
            {
                var statuses = _statusRepo.GetSurveyLocationStatuses(surveyId);
                return Json(new { success = true, data = statuses });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClientDetails(int clientId)
        {
            try
            {
                var client = _clientRepository.GetClientById(clientId);
                if (client == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }

                int? stateId = null;
                int? cityId = null;

                // Map state name to ID
                if (!string.IsNullOrWhiteSpace(client.State))
                {
                    var states = await _locationService.GetStatesAsync();
                    var matchedState = states.FirstOrDefault(s => 
                        string.Equals(s.name, client.State, StringComparison.OrdinalIgnoreCase));
                    
                    if (matchedState != null)
                    {
                        stateId = matchedState.id;

                        // Map city name to ID
                        if (!string.IsNullOrWhiteSpace(client.City))
                        {
                            var cities = await _locationService.GetCitiesByStateAsync(matchedState.id);
                            var matchedCity = cities.FirstOrDefault(c => 
                                string.Equals(c.name, client.City, StringComparison.OrdinalIgnoreCase));
                            cityId = matchedCity?.id;
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        clientName = client.ClientName,
                        address = client.Address1,
                        contactPerson = client.ContactPerson,
                        contactNumber = client.ContactNumber,
                        stateId,
                        stateName = client.State,
                        cityId,
                        cityName = client.City
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        // Survey Submission Actions
        
        [HttpGet]
        public IActionResult CheckSurveyCompletion(long surveyId)
        {
            try
            {
                var completionStatus = _surveyRepository.CheckSurveyCompletionStatus(surveyId);
                return Json(new { success = true, data = completionStatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        
        [HttpPost]
        public IActionResult SubmitSurvey(long surveyId)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                
                if (userId == 0)
                {
                    return Json(new { success = false, message = "User not logged in." });
                }
                
                // Check if all locations are completed
                var completionStatus = _surveyRepository.CheckSurveyCompletionStatus(surveyId);
                
                if (!completionStatus.IsComplete)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = completionStatus.Message,
                        incompleteLocations = completionStatus.IncompleteLocationNames,
                        totalLocations = completionStatus.TotalLocations,
                        completedLocations = completionStatus.CompletedLocations,
                        pendingLocations = completionStatus.PendingLocations
                    });
                }
                
                bool result = _surveyRepository.SubmitSurvey(surveyId, userId);
                
                if (result)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Survey submitted successfully.";
                    TempData["ResultType"] = "success";
                    return Json(new { success = true, message = "Survey submitted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to submit survey." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WithdrawSubmission(long surveyId)
        {
            try
            {
                bool result = _surveyRepository.WithdrawSubmission(surveyId);
                
                if (result)
                {
                    TempData["ResultMessage"] = "<strong>Success!</strong> Submission withdrawn successfully.";
                    TempData["ResultType"] = "info";
                    return Json(new { success = true, message = "Submission withdrawn successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to withdraw submission." });
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> " + ex.Message;
                TempData["ResultType"] = "danger";
                return Json(new { success = false, message = "An error occurred while withdrawing submission." });
            }
        }

}
}
