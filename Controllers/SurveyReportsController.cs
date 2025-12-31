using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Util;
using OfficeOpenXml;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Data;
using Microsoft.Data.SqlClient;
using OfficeOpenXml.Style;
using System.Drawing;

namespace SurveyApp.Controllers
{
    public class SurveyReportsController : Controller
    {
        private readonly ISurvey _surveyRepo;
        private readonly IAdmin _adminRepo;
        private readonly ISurveySubmission _submissionRepo;
        private readonly ISurveyCamRemarks _camRemarksRepo;
        private readonly IReportOTP _otpRepo;
        private const int SUPER_ADMIN_ROLE_ID = 101;

        public SurveyReportsController(ISurvey surveyRepo, IAdmin adminRepo, ISurveySubmission submissionRepo, ISurveyCamRemarks camRemarksRepo, IReportOTP otpRepo)
        {
            _surveyRepo = surveyRepo;
            _adminRepo = adminRepo;
            _submissionRepo = submissionRepo;
            _camRemarksRepo = camRemarksRepo;
            _otpRepo = otpRepo;
        }

        /// <summary>
        /// Check if the current user is authorized to download reports (Super Admin or has valid OTP)
        /// </summary>
        private bool IsAuthorizedForDownload()
        {
            int roleId = Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "102");
            
            // Super Admins are always authorized
            if (roleId == SUPER_ADMIN_ROLE_ID)
                return true;
            
            // Non-super admins need a validated OTP
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
            return _otpRepo.HasValidOTP(userId);
        }

        /// <summary>
        /// Get authorization denied result with message
        /// </summary>
        private IActionResult GetUnauthorizedResult(string reportType = "report")
        {
            TempData["ResultMessage"] = $"<strong>Authorization Required!</strong> You need OTP verification to download this {reportType}. Please request an OTP and get it validated by a Super Admin.";
            TempData["ResultType"] = "warning";
            TempData["RequireOTP"] = true;
            return RedirectToAction("SummaryReport");
        }

        // GET: SurveyReports/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: SurveyReports/SummaryReport
        public IActionResult SummaryReport(DateTime? fromDate = null, DateTime? toDate = null,
            string? status = null, string? region = null, string? type = null, long? selectedSurveyId = null)
        {
            try
            {
                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                string userName = HttpContext.Session.GetString("UserName") ?? "Guest";

                var allSurveys = _surveyRepo.GetAllSurveys(userId) ?? new List<SurveyModel>();
                var today = DateTime.Now.Date;

                // Fetch submission dates for all surveys
                var allSubmissions = _surveyRepo.GetAllSubmissions();
                foreach (var survey in allSurveys)
                {
                    var submission = allSubmissions.FirstOrDefault(s => s.SurveyId == survey.SurveyId);
                    if (submission != null)
                    {
                        survey.SubmittedDate = submission.SubmissionDate;
                    }
                }

                // Apply date filters
                if (fromDate.HasValue)
                {
                    allSurveys = allSurveys.Where(s => s.SurveyDate >= fromDate.Value).ToList();
                }
                if (toDate.HasValue)
                {
                    allSurveys = allSurveys.Where(s => s.SurveyDate <= toDate.Value).ToList();
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(status))
                {
                    allSurveys = allSurveys.Where(s => s.SurveyStatus == status).ToList();
                }

                // Apply region filter
                if (!string.IsNullOrEmpty(region))
                {
                    allSurveys = allSurveys.Where(s => s.RegionName == region).ToList();
                }

                // Apply type filter
                if (!string.IsNullOrEmpty(type))
                {
                    allSurveys = allSurveys.Where(s => s.ImplementationType == type).ToList();
                }

                var report = new SurveyReportViewModel
                {
                    ReportTitle = "Survey Summary Report",
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName,
                    FromDate = fromDate,
                    ToDate = toDate,
                    Status = status,
                    Region = region,
                    ImplementationType = type,

                    TotalSurveys = allSurveys.Count,
                    CompletedSurveys = allSurveys.Count(s => s.SurveyStatus == "Completed"),
                    InProgressSurveys = allSurveys.Count(s => s.SurveyStatus == "In Progress"),
                    PendingSurveys = allSurveys.Count(s => s.SurveyStatus == "Pending"),
                    OnHoldSurveys = allSurveys.Count(s => s.SurveyStatus == "On Hold"),
                    MissedDeadlineSurveys = allSurveys.Count(s =>
                        s.DueDate.HasValue && s.DueDate.Value.Date < today && s.SurveyStatus != "Completed"),

                    Surveys = allSurveys.OrderByDescending(s => s.SurveyDate).ToList(),

                    SurveysByStatus = allSurveys
                        .GroupBy(s => s.SurveyStatus ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),

                    SurveysByRegion = allSurveys
                        .Where(s => !string.IsNullOrEmpty(s.RegionName))
                        .GroupBy(s => s.RegionName!)
                        .ToDictionary(g => g.Key, g => g.Count()),

                    SurveysByImplementationType = allSurveys
                        .Where(s => !string.IsNullOrEmpty(s.ImplementationType))
                        .GroupBy(s => s.ImplementationType!)
                        .ToDictionary(g => g.Key, g => g.Count()),

                    MonthlySurveyCount = allSurveys
                        .Where(s => s.SurveyDate.HasValue)
                        .GroupBy(s => s.SurveyDate!.Value.ToString("MMM yyyy"))
                        .ToDictionary(g => g.Key, g => g.Count()),

                    MonthlyCompletionCount = allSurveys
                        .Where(s => s.SurveyDate.HasValue && s.SurveyStatus == "Completed")
                        .GroupBy(s => s.SurveyDate!.Value.ToString("MMM yyyy"))
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                // Calculate completion rate
                report.CompletionRate = report.TotalSurveys > 0
                    ? Math.Round((decimal)report.CompletedSurveys / report.TotalSurveys * 100, 1)
                    : 0;

                // Pass filter options
                ViewBag.StatusOptions = _surveyRepo.GetAllSurveys(userId)
                    .Where(s => !string.IsNullOrEmpty(s.SurveyStatus))
                    .Select(s => s.SurveyStatus)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                ViewBag.RegionOptions = _surveyRepo.GetAllSurveys(userId)
                    .Where(s => !string.IsNullOrEmpty(s.RegionName))
                    .Select(s => s.RegionName)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToList();

                ViewBag.TypeOptions = _surveyRepo.GetAllSurveys(userId)
                    .Where(s => !string.IsNullOrEmpty(s.ImplementationType))
                    .Select(s => s.ImplementationType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

                // Fetch Requirement Summary data if a survey is selected
                if (selectedSurveyId.HasValue && selectedSurveyId.Value > 0)
                {
                    var requirementSummary = GetRequirementSummaryData(selectedSurveyId.Value);
                    ViewBag.RequirementSummary = requirementSummary;
                    ViewBag.SelectedSurveyId = selectedSurveyId.Value;
                    
                    // Get selected survey details
                    var selectedSurvey = allSurveys.FirstOrDefault(s => s.SurveyId == selectedSurveyId.Value);
                    ViewBag.SelectedSurvey = selectedSurvey;
                }
                else
                {
                    ViewBag.RequirementSummary = null;
                    ViewBag.SelectedSurveyId = null;
                    ViewBag.SelectedSurvey = null;
                }

                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(new SurveyReportViewModel());
            }
        }

        /// <summary>
        /// Get Requirement Summary data for a survey with locations as rows and device types as columns.
        /// Excludes locations without selected devices.
        /// </summary>
        private RequirementSummaryModel GetRequirementSummaryData(long surveyId)
        {
            var result = new RequirementSummaryModel();
            
            try
            {
                // Get survey details
                DataTable dtSurveyDetails = _surveyRepo.GetSurveyDetails(surveyId, 1);
                DataTable dtSurveyLocEmp = _surveyRepo.GetSurveyDetails(surveyId, 2);
                DataTable dtSurveyItems = _surveyRepo.GetSurveyDetails(surveyId, 3);
                
                // CRITICAL: Add image URL and Remarks columns to dtSurveyItems for Summary Report
                DataTable dtSurveyRemarks = _surveyRepo.GetSurveyDetails(surveyId, 4);
                dtSurveyItems = EnrichItemsTableWithImagesAndRemarks(dtSurveyItems, surveyId, dtSurveyRemarks);
                
                if (dtSurveyDetails == null || dtSurveyDetails.Rows.Count == 0)
                    return result;
                
                var surveyRow = dtSurveyDetails.Rows[0];
                result.SurveyId = surveyId;
                result.SurveyName = surveyRow["SurveyName"]?.ToString() ?? "";
                result.ClientName = surveyRow["ClientName"]?.ToString() ?? "";
                result.StartDate = surveyRow["SurveyDate"] as DateTime?;
                result.CompletionDate = surveyRow["SubmissionDate"] as DateTime?;
                
                // Get device types from the pivot table
                // Pivot table structure: ItemCode(0), Type(1), Item(2), UOM(3), [Location Existing/Required pairs...], TotalExisting, TotalRequired, Remarks
                if (dtSurveyItems != null && dtSurveyItems.Rows.Count > 0)
                {
                    // Get column names for debugging
                    var allColumns = dtSurveyItems.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                    System.Diagnostics.Debug.WriteLine($"Pivot Table Columns: {string.Join(", ", allColumns)}");
                    
                    // Find location columns - those ending with "Existing" or "Required"
                    var locationColumnPairs = new Dictionary<string, (string ExistingCol, string RequiredCol)>();
                    foreach (var col in allColumns)
                    {
                        if (col.EndsWith("Existing"))
                        {
                            var locName = col.Replace("Existing", "").Trim();
                            if (!locationColumnPairs.ContainsKey(locName))
                            {
                                var requiredCol = allColumns.FirstOrDefault(c => c.Equals(locName + "Required", StringComparison.OrdinalIgnoreCase)) ?? "";
                                locationColumnPairs[locName] = (col, requiredCol);
                            }
                        }
                    }
                    
                    // Get unique item names from column index 2 (Item column - e.g., "Dome", "Bullet", "PTZ")
                    // and build category mapping from column index 1 (Type column - e.g., "Camera")
                    var deviceTypes = new List<string>();
                    var deviceCategories = new Dictionary<string, List<string>>();
                    
                    foreach (DataRow itemRow in dtSurveyItems.Rows)
                    {
                        if (itemRow.ItemArray.Length > 2)
                        {
                            string typeName = itemRow[1]?.ToString()?.Trim() ?? "";  // Category (e.g., "Camera")
                            string itemName = itemRow[2]?.ToString()?.Trim() ?? "";  // Item (e.g., "Dome")
                            
                            if (!string.IsNullOrEmpty(itemName))
                            {
                                // CRITICAL: Use composite key (TypeName||ItemName) to distinguish items with same name in different categories
                                string compositeKey = string.IsNullOrEmpty(typeName) ? itemName : $"{typeName}||{itemName}";
                                
                                if (!deviceTypes.Contains(compositeKey))
                                {
                                    deviceTypes.Add(compositeKey);
                                }
                                
                                // Build category -> items mapping (using composite keys)
                                if (!string.IsNullOrEmpty(typeName))
                                {
                                    if (!deviceCategories.ContainsKey(typeName))
                                    {
                                        deviceCategories[typeName] = new List<string>();
                                    }
                                    if (!deviceCategories[typeName].Contains(compositeKey))
                                    {
                                        deviceCategories[typeName].Add(compositeKey);
                                    }
                                }
                            }
                        }
                    }
                    
                    result.DeviceTypes = deviceTypes;
                    result.DeviceCategories = deviceCategories;
                    var devColVis = deviceTypes.ToDictionary(dt => dt, dt => new bool[] { false, false, false, false }, StringComparer.OrdinalIgnoreCase);
                    
                    System.Diagnostics.Debug.WriteLine($"Device Types Found: {string.Join(", ", deviceTypes)}");
                    System.Diagnostics.Debug.WriteLine($"Device Categories Found: {string.Join(", ", deviceCategories.Select(c => $"{c.Key}:[{string.Join(",", c.Value)}]"))}");
                    System.Diagnostics.Debug.WriteLine($"Location Columns Found: {string.Join(", ", locationColumnPairs.Keys)}");
                    
                    // Process each location from dtSurveyLocEmp
                    if (dtSurveyLocEmp != null && dtSurveyLocEmp.Rows.Count > 0)
                    {
                        int slNo = 1;
                        foreach (DataRow locRow in dtSurveyLocEmp.Rows)
                        {
                            var locationData = new LocationRequirementData
                            {
                                SlNo = slNo,
                                LocId = Convert.ToInt32(locRow["LocID"]),
                                LocationName = locRow["LocName"]?.ToString()?.Trim() ?? "",
                                LocationType = locRow["LocationType"]?.ToString() ?? "",
                                Coordinates = locRow["Cordinate"]?.ToString() ?? "",
                                EmployeeName = locRow["EmpName"]?.ToString() ?? "",
                                EmployeeId = locRow["EmpID"]?.ToString() ?? "",
                                DeviceData = new Dictionary<string, DeviceRequirementData>()
                            };
                            
                            // Parse coordinates for latitude/longitude
                            if (!string.IsNullOrEmpty(locationData.Coordinates))
                            {
                                var coords = locationData.Coordinates.Split(',');
                                if (coords.Length >= 2)
                                {
                                    locationData.Latitude = coords[0].Trim();
                                    locationData.Longitude = coords[1].Trim();
                                }
                            }
                            
                            // Find matching location columns (case-insensitive, trimmed)
                            string matchedLocName = locationColumnPairs.Keys
                                .FirstOrDefault(k => k.Trim().Equals(locationData.LocationName.Trim(), StringComparison.OrdinalIgnoreCase)) ?? "";
                            
                            bool hasDevices = false;
                            
                                if (!string.IsNullOrEmpty(matchedLocName) && locationColumnPairs.ContainsKey(matchedLocName))
                                {
                                    var (existingCol, requiredCol) = locationColumnPairs[matchedLocName];
                                    string photoCol = matchedLocName + "_Photos";
                                    string remarkCol = matchedLocName + "_Remarks"; // Standardized remark column name if available
                                    
                                    // For each device type (Item), get quantities from this location's columns
                                    foreach (var deviceType in deviceTypes)
                                    {
                                        var deviceData = new DeviceRequirementData { DeviceType = deviceType };
                                        
                                        // Find rows for this specific category and item
                                        foreach (DataRow itemRow in dtSurveyItems.Rows)
                                        {
                                            string rowType = itemRow[1]?.ToString()?.Trim() ?? "";
                                            string rowItem = itemRow[2]?.ToString()?.Trim() ?? "";
                                            string rowKey = string.IsNullOrEmpty(rowType) ? rowItem : $"{rowType}||{rowItem}";
                                            
                                            if (rowKey.Equals(deviceType, StringComparison.OrdinalIgnoreCase))
                                            {
                                                // 1. Existing Quantity
                                                if (!string.IsNullOrEmpty(existingCol) && dtSurveyItems.Columns.Contains(existingCol))
                                                {
                                                    int.TryParse(itemRow[existingCol]?.ToString(), out int existing);
                                                    deviceData.ExistingQty += existing;
                                                    if (existing > 0) devColVis[deviceType][0] = true;
                                                }
                                                // 2. Required Quantity
                                                if (!string.IsNullOrEmpty(requiredCol) && dtSurveyItems.Columns.Contains(requiredCol))
                                                {
                                                    int.TryParse(itemRow[requiredCol]?.ToString(), out int required);
                                                    deviceData.RequiredQty += required;
                                                    if (required > 0) devColVis[deviceType][1] = true;
                                                }
                                                
                                                // 3. Images
                                                if (dtSurveyItems.Columns.Contains(photoCol))
                                                {
                                                    string photoUrls = itemRow[photoCol]?.ToString() ?? "";
                                                    if (!string.IsNullOrEmpty(photoUrls))
                                                    {
                                                        var imgList = photoUrls.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                            .Select(u => u.Trim()).ToList();
                                                        deviceData.ImageUrls.AddRange(imgList.Where(i => !deviceData.ImageUrls.Contains(i)));
                                                        devColVis[deviceType][2] = true;
                                                    }
                                                }
                                                
                                                // 4. Remarks
                                                string localRemark = "";
                                                if (dtSurveyItems.Columns.Contains(remarkCol)) localRemark = itemRow[remarkCol]?.ToString() ?? "";
                                                else if (dtSurveyItems.Columns.Contains("Remarks")) localRemark = itemRow["Remarks"]?.ToString() ?? "";
                                                
                                                if (!string.IsNullOrEmpty(localRemark))
                                                {
                                                    if (string.IsNullOrEmpty(deviceData.Remarks)) deviceData.Remarks = localRemark;
                                                    else if (!deviceData.Remarks.Contains(localRemark)) deviceData.Remarks += "; " + localRemark;
                                                    devColVis[deviceType][3] = true;
                                                }

                                                // 5. UOM
                                                if (dtSurveyItems.Columns.Contains("ItemUOM")) deviceData.UOM = itemRow["ItemUOM"]?.ToString() ?? "";
                                                else if (itemRow.Table.Columns.Count > 3) deviceData.UOM = itemRow[3]?.ToString() ?? "";
                                            }
                                        }
                                        
                                        // Only add to DeviceData if there are quantities > 0 OR images OR remarks
                                        if (deviceData.ExistingQty > 0 || deviceData.RequiredQty > 0 || deviceData.ImageUrls.Any() || !string.IsNullOrEmpty(deviceData.Remarks))
                                        {
                                            hasDevices = true;
                                            locationData.DeviceData[deviceType] = deviceData;
                                        }
                                    }
                                }
                            
                            // Only add locations that have at least one device with quantity
                            if (hasDevices)
                            {
                                result.Locations.Add(locationData);
                                slNo++;
                            }
                        }
                    }
                    
                    // Filter device types to only show those with at least one location having quantity > 0
                    // Use case-insensitive comparison for reliability
                    var deviceTypesWithData = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    
                    foreach (var location in result.Locations)
                    {
                        foreach (var deviceData in location.DeviceData)
                        {
                            if (deviceData.Value.ExistingQty > 0 || deviceData.Value.RequiredQty > 0)
                            {
                                deviceTypesWithData.Add(deviceData.Key);
                            }
                        }
                    }
                    
                    // Keep only device types that have at least one location with data
                    result.DeviceTypes = result.DeviceTypes
                        .Where(dt => deviceTypesWithData.Contains(dt))
                        .ToList();

                    // Assign granular visibility to result
                    result.DeviceColumnVisibility = devColVis;
                    
                    System.Diagnostics.Debug.WriteLine($"Device types with data: {string.Join(", ", deviceTypesWithData)}");
                    System.Diagnostics.Debug.WriteLine($"Filtered DeviceTypes: {string.Join(", ", result.DeviceTypes)}");
                    
                    // Check if any location has existing quantities > 0
                    result.HasAnyExisting = result.Locations.Any(l => 
                        l.DeviceData.Any(d => d.Value.ExistingQty > 0));
                    
                    // Update DeviceCategories to only include device types that have data
                    var filteredCategories = new Dictionary<string, List<string>>();
                    foreach (var category in result.DeviceCategories)
                    {
                        var filteredItems = category.Value
                            .Where(item => deviceTypesWithData.Contains(item))
                            .ToList();
                        if (filteredItems.Any())
                        {
                            filteredCategories[category.Key] = filteredItems;
                        }
                    }
                    result.DeviceCategories = filteredCategories;
                    
                    System.Diagnostics.Debug.WriteLine($"Filtered DeviceCategories: {string.Join(", ", result.DeviceCategories.Select(c => $"{c.Key}:[{string.Join(",", c.Value)}]"))}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetRequirementSummaryData: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            return result;
        }

        // GET: SurveyReports/DetailedReport
        // public IActionResult DetailedReport(long surveyId)
        // {
        //     try
        //     {
        //         var survey = _surveyRepo.GetSurveyById(surveyId);
        //         if (survey == null)
        //         {
        //             TempData["ResultMessage"] = "<strong>Error!</strong> Survey not found.";
        //             TempData["ResultType"] = "danger";
        //             return RedirectToAction("SummaryReport");
        //         }

        //         // Fetch CreatedBy user name
        //         var createdByUser = _adminRepo.GetUserById(survey.CreatedBy);
        //         ViewBag.CreatedByName = createdByUser?.LoginName ?? "Unknown";

        //         var locations = _surveyRepo.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
        //         var assignments = _surveyRepo.GetSurveyAssignments(surveyId) ?? new List<SurveyAssignmentModel>();
        //         var submission = _surveyRepo.GetSubmissionBySurveyId(surveyId);
        //         var submissions = submission != null ? new List<SurveySubmissionModel> { submission } : new List<SurveySubmissionModel>();

        //         // Get survey details (devices/items) for all locations
        //         var surveyDetails = new List<SurveyDetailsLocationModel>();
        //         foreach (var location in locations)
        //         {
        //             var details = _surveyRepo.GetAssignedTypeList(surveyId, location.LocID);
        //             if (details != null && details.Any())
        //             {
        //                 foreach (var detail in details)
        //                 {
        //                     // Get items for each type
        //                     var items = _surveyRepo.GetAssignedItemList(surveyId, location.LocID, detail.ItemTypeID);
        //                     detail.ItemLists = items ?? new List<SurveyDetailsModel>();
        //                 }
        //                 surveyDetails.AddRange(details);
        //             }
        //         }

        //         var report = new DetailedSurveyReportModel
        //         {
        //             ReportTitle = $"Detailed Report - {survey.SurveyName}",
        //             GeneratedBy = HttpContext.Session.GetString("UserName") ?? "System",
        //             Survey = survey,
        //             Locations = locations,
        //             Assignments = assignments,
        //             Submissions = submissions,
        //             SurveyDetails = surveyDetails,
        //             TotalLocations = locations.Count,
        //             CompletedLocations = locations.Count(l => l.Isactive),
        //             TotalAssignments = assignments.Count
        //         };

        //         // Calculate location completion rate
        //         report.LocationCompletionRate = report.TotalLocations > 0
        //             ? Math.Round((decimal)report.CompletedLocations / report.TotalLocations * 100, 1)
        //             : 0;

        //         // Calculate time to complete
        //         if (submissions.Any() && survey.SurveyDate.HasValue)
        //         {
        //             var firstSubmission = submissions.OrderBy(s => s.SubmissionDate).FirstOrDefault();
        //             if (firstSubmission?.SubmissionDate.HasValue == true)
        //             {
        //                 report.TimeToComplete = firstSubmission.SubmissionDate.Value - survey.SurveyDate.Value;
        //             }
        //         }

        //         return View("DetailedReportNew",report);
        //     }
        //     catch (Exception ex)
        //     {
        //         TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
        //         TempData["ResultType"] = "danger";
        //         return RedirectToAction("SummaryReport");
        //     }
        // }

        public IActionResult DetailedReport(long surveyId)
        {
            DataTable dtSurveyDetails = _surveyRepo.GetSurveyDetails(surveyId, 1);
            DataTable dtSurveyLocEmp = _surveyRepo.GetSurveyDetails(surveyId, 2);
            DataTable dtSurveyItems = _surveyRepo.GetSurveyDetails(surveyId, 3);
            DataTable dtGlobalItems = _surveyRepo.GetSurveyDetails(surveyId, 5); // Global cable counts
            DataTable dtSurveyRemarks = _surveyRepo.GetSurveyDetails(surveyId, 4);

            // Add image URL and Remarks columns to dtSurveyItems
            dtSurveyItems = EnrichItemsTableWithImagesAndRemarks(dtSurveyItems, surveyId, dtSurveyRemarks);

            // Get submission information
            var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);

            // Get all camera remarks for this survey directly from the database
            var allCameraRemarks = _camRemarksRepo.GetAllCameraRemarksBySurvey(surveyId);
            bool hasCameraRemarks = allCameraRemarks != null && allCameraRemarks.Count > 0;

            // Group remarks by location, then by ItemID
            var cameraRemarks = new Dictionary<string, List<SurveyCamRemarksModel>>();
            var cameraItemNames = new Dictionary<int, string>();

            if (hasCameraRemarks)
            {
                var groupedByLocation = allCameraRemarks.GroupBy(r => r.LocID);
                foreach (var group in groupedByLocation)
                {
                    cameraRemarks[$"{surveyId}_{group.Key}"] = group.ToList();
                }

                // Get unique ItemIDs and fetch their names from the database
                var uniqueItemIds = allCameraRemarks.Select(r => r.ItemID).Distinct().ToList();
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                foreach (var itemId in uniqueItemIds)
                {
                    using var cmd = new SqlCommand("SELECT ItemName FROM ItemMaster WHERE ItemID = @ItemID", con);
                    cmd.Parameters.AddWithValue("@ItemID", itemId);
                    var itemName = cmd.ExecuteScalar()?.ToString();
                    if (!string.IsNullOrEmpty(itemName))
                    {
                        cameraItemNames[itemId] = itemName;
                    }
                    else
                    {
                        cameraItemNames[itemId] = $"Camera Item #{itemId}";
                    }
                }
            }

            ViewBag.SurveyDetails = dtSurveyDetails;
            ViewBag.SurveyLocEmp = dtSurveyLocEmp;
            ViewBag.SurveyItems = dtSurveyItems;
            ViewBag.GlobalItems = dtGlobalItems; // Global cable counts
            ViewBag.SurveyId = surveyId;
            ViewBag.Submission = submission;
            ViewBag.CameraRemarks = cameraRemarks;
            ViewBag.CameraItemNames = cameraItemNames;
            ViewBag.HasCameraDevices = hasCameraRemarks;

            return View("DetailedReport");
        }

        // Debug endpoint to check image data
        [HttpGet]
        public IActionResult DebugImageData(long surveyId = 0)
        {
            var debugInfo = new Dictionary<string, object>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();

                // First, get all surveys with their status
                if (surveyId == 0)
                {
                    string surveyQuery = @"SELECT TOP 10 SurveyId, SurveyName, SurveyStatus FROM Survey ORDER BY SurveyId DESC";
                    using var surveyCmd = new SqlCommand(surveyQuery, con);
                    var surveys = new List<object>();
                    using var surveyReader = surveyCmd.ExecuteReader();
                    while (surveyReader.Read())
                    {
                        surveys.Add(new
                        {
                            SurveyId = surveyReader["SurveyId"],
                            SurveyName = surveyReader["SurveyName"]?.ToString(),
                            SurveyStatus = surveyReader["SurveyStatus"]?.ToString()
                        });
                    }
                    debugInfo["AvailableSurveys"] = surveys;
                    debugInfo["Message"] = "No surveyId provided. Pass ?surveyId=X to check specific survey.";
                    return Json(debugInfo);
                }

                // Check if any SurveyDetails records exist with images (globally)
                string globalImageQuery = @"SELECT TOP 5 sd.SurveyID, sl.LocName, sd.ItemID, sd.ItemTypeID, 
                                            LEFT(sd.ImgPath, 100) as ImgPathPreview
                                            FROM SurveyDetails sd
                                            LEFT JOIN SurveyLocation sl ON sd.LocID = sl.LocID AND sd.SurveyID = sl.SurveyID
                                            WHERE sd.ImgPath IS NOT NULL AND sd.ImgPath != '' AND LEN(sd.ImgPath) > 0
                                            ORDER BY sd.SurveyID DESC";
                using var globalCmd = new SqlCommand(globalImageQuery, con);
                var globalImages = new List<object>();
                using var globalReader = globalCmd.ExecuteReader();
                while (globalReader.Read())
                {
                    globalImages.Add(new
                    {
                        SurveyID = globalReader["SurveyID"],
                        LocName = globalReader["LocName"]?.ToString(),
                        ItemID = globalReader["ItemID"]?.ToString(),
                        ItemTypeID = globalReader["ItemTypeID"]?.ToString(),
                        ImgPathPreview = globalReader["ImgPathPreview"]?.ToString()
                    });
                }
                globalReader.Close();
                debugInfo["GlobalImagesInDatabase"] = globalImages;
                debugInfo["HasAnyImagesInDatabase"] = globalImages.Count > 0;

                // Get raw image data for specific survey
                var imageList = GetSurveyItemImages(surveyId);
                debugInfo["ImageCount"] = imageList.Count;
                debugInfo["Images"] = imageList.Select(i => new { i.LocationName, i.ItemCode, UrlLength = i.ImageUrls?.Length ?? 0, Preview = i.ImageUrls?.Length > 100 ? i.ImageUrls.Substring(0, 100) + "..." : i.ImageUrls }).ToList();

                // Get pivot table columns
                DataTable dtItems = _surveyRepo.GetSurveyDetails(surveyId, 3);
                debugInfo["PivotColumns"] = dtItems.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                debugInfo["PivotRowCount"] = dtItems.Rows.Count;

                if (dtItems.Rows.Count > 0)
                {
                    debugInfo["FirstRowData"] = new Dictionary<string, string>();
                    foreach (DataColumn col in dtItems.Columns)
                    {
                        ((Dictionary<string, string>)debugInfo["FirstRowData"])[col.ColumnName] = dtItems.Rows[0][col]?.ToString() ?? "null";
                    }
                }

                // Get raw SurveyDetails records for this survey
                string query = @"SELECT TOP 10 sd.SurveyID, sd.LocID, sl.LocName, sd.ItemTypeID, sd.ItemID, 
                                LEFT(sd.ImgPath, 100) as ImgPath, sd.ImgID 
                                FROM SurveyDetails sd
                                LEFT JOIN SurveyLocation sl ON sd.LocID = sl.LocID AND sd.SurveyID = sl.SurveyID
                                WHERE sd.SurveyID = @SurveyID";
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);

                var rawRecords = new List<object>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    rawRecords.Add(new
                    {
                        SurveyID = reader["SurveyID"],
                        LocID = reader["LocID"],
                        LocName = reader["LocName"]?.ToString(),
                        ItemTypeID = reader["ItemTypeID"],
                        ItemID = reader["ItemID"]?.ToString(),
                        ImgPath = reader["ImgPath"]?.ToString()
                    });
                }
                debugInfo["RawSurveyDetailsForSurvey"] = rawRecords;
            }
            catch (Exception ex)
            {
                debugInfo["Error"] = ex.Message;
                debugInfo["StackTrace"] = ex.StackTrace ?? "No stack trace";
            }

            return Json(debugInfo);
        }

        private DataTable EnrichItemsTableWithImagesAndRemarks(DataTable dtItems, long surveyId, DataTable dtRemarks)
        {
            System.Diagnostics.Debug.WriteLine($"=== EnrichItemsTableWithImagesAndRemarks START for Survey {surveyId} ===");
            
            if (dtItems == null || dtItems.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("dtItems is null or empty");
                return dtItems;
            }

            try
            {
                // Get all location columns (those with "Existing" or "Required" in the name)
                var locationColumns = dtItems.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName.Contains("Existing") || c.ColumnName.Contains("Required"))
                    .Select(c => c.ColumnName.Replace("Existing", "").Replace("Required", "").Trim())
                    .Distinct()
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Found {locationColumns.Count} location columns: {string.Join(", ", locationColumns)}");

                // 1. IMAGES
                var itemImages = GetSurveyItemImages(surveyId);
                System.Diagnostics.Debug.WriteLine($"Retrieved {itemImages.Count} image records from database");
                
                // Add image columns for each location
                foreach (var locationName in locationColumns)
                {
                    string imageColumnName = $"{locationName}Photos";
                    if (!dtItems.Columns.Contains(imageColumnName))
                    {
                        dtItems.Columns.Add(imageColumnName, typeof(string));
                        System.Diagnostics.Debug.WriteLine($"Added image column: {imageColumnName}");
                    }
                }

                // 2. REMARKS - Fetch from SurveyDetails.Remarks (not camera installation remarks)
                // Pre-process remarks into a dictionary for faster and accurate lookup
                // Map: LocationName -> Dictionary<ItemName, JoinedRemarks>
                var remarksLookup = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                
                System.Diagnostics.Debug.WriteLine("Fetching general remarks from SurveyDetails table...");

                try
                {
                    using var con = new SqlConnection(DBConnection.ConnectionString);
                    // Query to get general remarks from SurveyDetails (not camera installation remarks)
                    string remarksQuery = @"
                        SELECT 
                            LTRIM(RTRIM(sl.LocName)) as LocationName,
                            im.ItemName,
                            sd.Remarks
                        FROM SurveyDetails sd
                        INNER JOIN SurveyLocation sl ON sd.LocID = sl.LocID AND sd.SurveyID = sl.SurveyID
                        LEFT JOIN ItemMaster im ON sd.ItemID = im.ItemID
                        WHERE sd.SurveyID = @SurveyID
                            AND sd.Remarks IS NOT NULL 
                            AND sd.Remarks != ''
                            AND LEN(LTRIM(RTRIM(sd.Remarks))) > 0";

                    using var cmd = new SqlCommand(remarksQuery, con);
                    cmd.Parameters.AddWithValue("@SurveyID", surveyId);

                    con.Open();
                    using var reader = cmd.ExecuteReader();

                    int remarksCount = 0;
                    while (reader.Read())
                    {
                        string loc = reader["LocationName"]?.ToString()?.Trim() ?? "";
                        string itemName = reader["ItemName"]?.ToString()?.Trim() ?? "";
                        string msg = reader["Remarks"]?.ToString()?.Trim() ?? "";

                        System.Diagnostics.Debug.WriteLine($"Remark from DB: Loc='{loc}', Item='{itemName}', Msg='{msg}'");

                        if (!string.IsNullOrEmpty(loc) && !string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(msg))
                        {
                            if (!remarksLookup.ContainsKey(loc))
                                remarksLookup[loc] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                            if (remarksLookup[loc].ContainsKey(itemName))
                                remarksLookup[loc][itemName] += "; " + msg;
                            else
                                remarksLookup[loc][itemName] = msg;

                            remarksCount++;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Built remarks lookup with {remarksCount} entries from SurveyDetails across {remarksLookup.Count} locations");

                    // Add remark columns for each location
                    foreach (var locationName in locationColumns)
                    {
                        string remarkColumnName = $"{locationName}Remarks";
                        if (!dtItems.Columns.Contains(remarkColumnName))
                            dtItems.Columns.Add(remarkColumnName, typeof(string));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching remarks from SurveyDetails: {ex.Message}");
                }

                // 3. SPECIFICATIONS - Fetch from SpecificationDetailsMaster
                // Map: LocationName -> Dictionary<ItemName, JoinedSpecifications>
                var specificationsLookup = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
                
                System.Diagnostics.Debug.WriteLine("Fetching specifications from SpecificationDetailsMaster table...");

                try
                {
                    using var con = new SqlConnection(DBConnection.ConnectionString);
                    // Query to get specifications with their names
                    string specificationsQuery = @"
                        SELECT 
                            LTRIM(RTRIM(sl.LocName)) as LocationName,
                            im.ItemName,
                            ism.SpecificationName,
                            sd.SpecificationDetails,
                            sd.InstanceNumber
                        FROM SpecificationDetailsMaster sd
                        INNER JOIN SurveyLocation sl ON sd.LocID = sl.LocID AND sd.SurveyID = sl.SurveyID
                        LEFT JOIN ItemMaster im ON sd.ItemID = im.ItemID
                        LEFT JOIN ItemSpecificationMaster ism ON sd.ItemID = ism.ItemId AND sd.SpecificationID = ism.SpecificationID
                        WHERE sd.SurveyID = @SurveyID
                            AND sd.SpecificationDetails IS NOT NULL 
                            AND sd.SpecificationDetails != ''
                            AND LEN(LTRIM(RTRIM(sd.SpecificationDetails))) > 0
                        ORDER BY sd.LocID, sd.ItemID, sd.SpecificationID, sd.InstanceNumber";

                    using var cmd = new SqlCommand(specificationsQuery, con);
                    cmd.Parameters.AddWithValue("@SurveyID", surveyId);

                    con.Open();
                    using var reader = cmd.ExecuteReader();

                    int specificationsCount = 0;
                    while (reader.Read())
                    {
                        string loc = reader["LocationName"]?.ToString()?.Trim() ?? "";
                        string itemName = reader["ItemName"]?.ToString()?.Trim() ?? "";
                        string specName = reader["SpecificationName"]?.ToString()?.Trim() ?? "";
                        string specDetails = reader["SpecificationDetails"]?.ToString()?.Trim() ?? "";
                        int instanceNum = reader["InstanceNumber"] != DBNull.Value ? Convert.ToInt32(reader["InstanceNumber"]) : 1;

                        System.Diagnostics.Debug.WriteLine($"Spec from DB: Loc='{loc}', Item='{itemName}', Spec='{specName}', Value='{specDetails}', Instance={instanceNum}");

                        if (!string.IsNullOrEmpty(loc) && !string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(specDetails))
                        {
                            if (!specificationsLookup.ContainsKey(loc))
                                specificationsLookup[loc] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                            // Format: "SpecName: Value" or just "Value" if no spec name
                            string formattedSpec = !string.IsNullOrEmpty(specName) 
                                ? $"{specName}: {specDetails}" 
                                : specDetails;

                            if (specificationsLookup[loc].ContainsKey(itemName))
                                specificationsLookup[loc][itemName] += "; " + formattedSpec;
                            else
                                specificationsLookup[loc][itemName] = formattedSpec;

                            specificationsCount++;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Built specifications lookup with {specificationsCount} entries from SpecificationDetailsMaster across {specificationsLookup.Count} locations");

                    // Add specification columns for each location
                    foreach (var locationName in locationColumns)
                    {
                        string specColumnName = $"{locationName}Specification";
                        if (!dtItems.Columns.Contains(specColumnName))
                            dtItems.Columns.Add(specColumnName, typeof(string));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching specifications from SpecificationDetailsMaster: {ex.Message}");
                }

                // Determine the identifiers
                string itemCodeColumnName = dtItems.Columns.Contains("Item Code") ? "Item Code"
                    : dtItems.Columns.Contains("ItemCode") ? "ItemCode"
                    : dtItems.Columns.Count > 0 ? dtItems.Columns[0].ColumnName : "";

                // Populate Rows
                foreach (DataRow row in dtItems.Rows)
                {
                    string itemCode = !string.IsNullOrEmpty(itemCodeColumnName)
                        ? row[itemCodeColumnName]?.ToString()?.Trim() ?? ""
                        : row[0]?.ToString()?.Trim() ?? "";

                    // Attempt to get ItemName from column index 2 (Common Pivot Structure: ItemCode, Type, ItemName...)
                    string itemName = dtItems.Columns.Count > 2 ? row[2]?.ToString()?.Trim() ?? "" : "";

                    foreach (var locationName in locationColumns)
                    {
                        // 1. POPULATE IMAGES (Multiple + Clean paths)
                        string imageColumnName = $"{locationName}Photos";
                        
                        // Get all matching image records for this location + item
                        var matchingRecords = itemImages
                            .Where(img =>
                                img.LocationName.Trim().Equals(locationName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                                (
                                    (!string.IsNullOrEmpty(itemName) && img.ItemName.Trim().Equals(itemName, StringComparison.OrdinalIgnoreCase)) ||
                                    (!string.IsNullOrEmpty(itemCode) && img.ItemCode.Trim().Equals(itemCode, StringComparison.OrdinalIgnoreCase))
                                ))
                            .ToList();

                        // Each record's ImageUrls could be comma-separated (multiple images per record)
                        // Split them, clean each URL, then join ALL with pipe separator for the frontend
                        var allImageUrls = new List<string>();
                        foreach (var record in matchingRecords)
                        {
                            if (!string.IsNullOrEmpty(record.ImageUrls))
                            {
                                // Split by comma (original separator in DB)
                                var urls = record.ImageUrls.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var url in urls)
                                {
                                    var cleanedUrl = url.Trim().Replace("~", "");
                                    if (!string.IsNullOrEmpty(cleanedUrl))
                                    {
                                        allImageUrls.Add(cleanedUrl);
                                    }
                                }
                            }
                        }

                        row[imageColumnName] = allImageUrls.Any() ? string.Join("|", allImageUrls) : "";
                        
                        if (allImageUrls.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"Populated {allImageUrls.Count} image(s) for {locationName} / {itemName}: {string.Join("|", allImageUrls.Take(2))}...");
                        }

                        // 2. POPULATE REMARKS (From Lookup)
                        string remarkColumnName = $"{locationName}Remarks";
                        string remarkVal = "";
                        
                        if (remarksLookup.ContainsKey(locationName))
                        {
                            var locRem = remarksLookup[locationName];
                            // Match by Name
                            if (!string.IsNullOrEmpty(itemName) && locRem.ContainsKey(itemName))
                                remarkVal = locRem[itemName];
                            // Fallback: Match by Code (if Code matches Name logic, or we expand map keys)
                            else if (!string.IsNullOrEmpty(itemCode) && locRem.ContainsKey(itemCode))
                                 remarkVal = locRem[itemCode];
                        }
                        
                        row[remarkColumnName] = remarkVal;
                        
                        if (!string.IsNullOrEmpty(remarkVal))
                        {
                            System.Diagnostics.Debug.WriteLine($"Populated remark for {locationName} / {itemName}: {remarkVal}");
                        }

                        // 3. POPULATE SPECIFICATIONS (From Lookup)
                        string specificationColumnName = $"{locationName}Specification";
                        string specificationVal = "";
                        
                        if (specificationsLookup.ContainsKey(locationName))
                        {
                            var locSpec = specificationsLookup[locationName];
                            // Match by Name
                            if (!string.IsNullOrEmpty(itemName) && locSpec.ContainsKey(itemName))
                                specificationVal = locSpec[itemName];
                            // Fallback: Match by Code
                            else if (!string.IsNullOrEmpty(itemCode) && locSpec.ContainsKey(itemCode))
                                 specificationVal = locSpec[itemCode];
                        }
                        
                        row[specificationColumnName] = specificationVal;
                        
                        if (!string.IsNullOrEmpty(specificationVal))
                        {
                            System.Diagnostics.Debug.WriteLine($"Populated specification for {locationName} / {itemName}: {specificationVal}");
                        }
                    }
                }

                return dtItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EnrichItemsTableWithImagesAndRemarks: {ex.Message}");
                return dtItems;
            }
        }

        private List<SurveyItemImageInfo> GetSurveyItemImages(long surveyId)
        {
            var imageList = new List<SurveyItemImageInfo>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                // Query to get images per location and item
                // Added ItemName fetch from ItemMaster to match with Pivot Table's Item Name column
                string query = @"
                    SELECT 
                        LTRIM(RTRIM(sl.LocName)) as LocationName,
                        CAST(sd.ItemID AS VARCHAR(20)) as ItemCode,
                        im.ItemName,
                        sd.ImgPath
                    FROM SurveyDetails sd
                    INNER JOIN SurveyLocation sl ON sd.LocID = sl.LocID AND sd.SurveyID = sl.SurveyID
                    LEFT JOIN ItemMaster im ON sd.ItemID = im.ItemID
                    WHERE sd.SurveyID = @SurveyID
                        AND sd.ImgPath IS NOT NULL 
                        AND sd.ImgPath != ''
                        AND LEN(LTRIM(RTRIM(sd.ImgPath))) > 0";

                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);

                con.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    imageList.Add(new SurveyItemImageInfo
                    {
                        LocationName = reader["LocationName"]?.ToString() ?? "",
                        ItemCode = reader["ItemCode"]?.ToString() ?? "",
                        ItemName = reader["ItemName"]?.ToString() ?? "", 
                        ImageUrls = reader["ImgPath"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting survey item images: {ex.Message}");
            }
            return imageList;
        }

        // GET: SurveyReports/ExportToExcel
        public IActionResult ExportToExcel(DateTime? fromDate = null, DateTime? toDate = null,
            string? status = null, string? region = null, string? type = null)
        {
            try
            {
                // OTP Authorization Check - Non-super admins need validated OTP
                if (!IsAuthorizedForDownload())
                {
                    return GetUnauthorizedResult("Excel report");
                }

                int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
                var surveys = _surveyRepo.GetAllSurveys(userId) ?? new List<SurveyModel>();

                // Apply filters
                if (fromDate.HasValue)
                    surveys = surveys.Where(s => s.SurveyDate >= fromDate.Value).ToList();
                if (toDate.HasValue)
                    surveys = surveys.Where(s => s.SurveyDate <= toDate.Value).ToList();
                if (!string.IsNullOrEmpty(status))
                    surveys = surveys.Where(s => s.SurveyStatus == status).ToList();
                if (!string.IsNullOrEmpty(region))
                    surveys = surveys.Where(s => s.RegionName == region).ToList();
                if (!string.IsNullOrEmpty(type))
                    surveys = surveys.Where(s => s.ImplementationType == type).ToList();

                ExcelPackage.License.SetNonCommercialOrganization("ABTMS");

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Survey Report");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Survey ID";
                    worksheet.Cells[1, 2].Value = "Survey Name";
                    worksheet.Cells[1, 3].Value = "Status";
                    worksheet.Cells[1, 4].Value = "Region";
                    worksheet.Cells[1, 5].Value = "Implementation Type";
                    worksheet.Cells[1, 6].Value = "Survey Date";
                    worksheet.Cells[1, 7].Value = "Due Date";
                    worksheet.Cells[1, 8].Value = "Location";
                    worksheet.Cells[1, 9].Value = "City";
                    worksheet.Cells[1, 10].Value = "Team";

                    // Style header
                    using (var range = worksheet.Cells[1, 1, 1, 10])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Data
                    int row = 2;
                    foreach (var survey in surveys.OrderByDescending(s => s.SurveyDate))
                    {
                        worksheet.Cells[row, 1].Value = survey.SurveyId;
                        worksheet.Cells[row, 2].Value = survey.SurveyName;
                        worksheet.Cells[row, 3].Value = survey.SurveyStatus;
                        worksheet.Cells[row, 4].Value = survey.RegionName;
                        worksheet.Cells[row, 5].Value = survey.ImplementationType;
                        worksheet.Cells[row, 6].Value = survey.SurveyDate?.ToString("dd-MMM-yyyy");
                        worksheet.Cells[row, 7].Value = survey.DueDate?.ToString("dd-Mmm-yyyy");
                        worksheet.Cells[row, 8].Value = survey.LocationSiteName;
                        worksheet.Cells[row, 9].Value = survey.CityDistrict;
                        worksheet.Cells[row, 10].Value = survey.SurveyTeamName;
                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    var stream = new System.IO.MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"SurveyReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("SummaryReport");
            }
        }

        // GET: SurveyReports/ExportDetailedReport
        public IActionResult ExportDetailedReport(Int64 surveyId)
        {
            try
            {
                // OTP Authorization Check - Non-super admins need validated OTP
                if (!IsAuthorizedForDownload())
                {
                    return GetUnauthorizedResult("detailed report");
                }

                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Survey not found.";
                    TempData["ResultType"] = "danger";
                    return RedirectToAction("SummaryReport");
                }
                
                var locations = _surveyRepo.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
                var assignments = _surveyRepo.GetSurveyAssignments(surveyId) ?? new List<SurveyAssignmentModel>();

                // Get survey details (devices/items) for all locations
                var surveyDetails = new List<SurveyDetailsLocationModel>();
                foreach (var location in locations)
                {
                    var details = _surveyRepo.GetAssignedTypeList(surveyId, location.LocID);
                    if (details != null && details.Any())
                    {
                        foreach (var detail in details)
                        {
                            var items = _surveyRepo.GetAssignedItemList(surveyId, location.LocID, detail.ItemTypeID);
                            detail.ItemLists = items ?? new List<SurveyDetailsModel>();
                        }
                        surveyDetails.AddRange(details);
                    }
                }

                OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("ABTMS");

                using (var package = new ExcelPackage())
                {
                    // Survey Overview Sheet
                    var overviewSheet = package.Workbook.Worksheets.Add("Survey Overview");
                    overviewSheet.Cells["A1"].Value = "Survey Information";
                    overviewSheet.Cells["A1:B1"].Merge = true;
                    overviewSheet.Cells["A1:B1"].Style.Font.Bold = true;
                    overviewSheet.Cells["A1:B1"].Style.Font.Size = 14;

                    int row = 2;
                    overviewSheet.Cells[row, 1].Value = "Survey ID:";
                    overviewSheet.Cells[row++, 2].Value = survey.SurveyId;
                    overviewSheet.Cells[row, 1].Value = "Survey Name:";
                    overviewSheet.Cells[row++, 2].Value = survey.SurveyName;
                    overviewSheet.Cells[row, 1].Value = "Status:";
                    overviewSheet.Cells[row++, 2].Value = survey.SurveyStatus;
                    overviewSheet.Cells[row, 1].Value = "Region:";
                    overviewSheet.Cells[row++, 2].Value = survey.RegionName;
                    overviewSheet.Cells[row, 1].Value = "Implementation Type:";
                    overviewSheet.Cells[row++, 2].Value = survey.ImplementationType;
                    overviewSheet.Cells[row, 1].Value = "Survey Date:";
                    overviewSheet.Cells[row++, 2].Value = survey.SurveyDate?.ToString("dd-MMM-yyyy");
                    overviewSheet.Cells[row, 1].Value = "Due Date:";
                    overviewSheet.Cells[row++, 2].Value = survey.DueDate?.ToString("dd-MMM-yyyy");
                    overviewSheet.Cells["A2:A" + (row - 1)].Style.Font.Bold = true;
                    overviewSheet.Cells.AutoFitColumns();

                    // Locations Sheet
                    if (locations.Any())
                    {
                        var locSheet = package.Workbook.Worksheets.Add("Locations");
                        locSheet.Cells["A1"].Value = "Location ID";
                        locSheet.Cells["B1"].Value = "Location Name";
                        locSheet.Cells["C1"].Value = "Location Type";
                        locSheet.Cells["D1"].Value = "Latitude";
                        locSheet.Cells["E1"].Value = "Longitude";
                        locSheet.Cells["A1:E1"].Style.Font.Bold = true;

                        row = 2;
                        foreach (var loc in locations)
                        {
                            locSheet.Cells[row, 1].Value = loc.LocID;
                            locSheet.Cells[row, 2].Value = loc.LocName;
                            locSheet.Cells[row, 3].Value = loc.LocationType;
                            locSheet.Cells[row, 4].Value = loc.LocLat;
                            locSheet.Cells[row, 5].Value = loc.LocLog;
                            row++;
                        }
                        locSheet.Cells.AutoFitColumns();
                    }

                    // Assignments Sheet
                    if (assignments.Any())
                    {
                        var assignSheet = package.Workbook.Worksheets.Add("Assignments");
                        assignSheet.Cells["A1"].Value = "Transaction ID";
                        assignSheet.Cells["B1"].Value = "Survey ID";
                        assignSheet.Cells["C1"].Value = "Employee ID";
                        assignSheet.Cells["D1"].Value = "Employee Name";
                        assignSheet.Cells["E1"].Value = "Due Date";
                        assignSheet.Cells["A1:E1"].Style.Font.Bold = true;

                        row = 2;
                        foreach (var assign in assignments)
                        {
                            assignSheet.Cells[row, 1].Value = assign.TransID;
                            assignSheet.Cells[row, 2].Value = assign.SurveyID;
                            assignSheet.Cells[row, 3].Value = assign.EmpID;
                            assignSheet.Cells[row, 4].Value = assign.EmpName;
                            assignSheet.Cells[row, 5].Value = assign.DueDate.HasValue ? assign.DueDate.Value.ToString("dd-MM-yyyy") : "";
                            row++;
                        }
                        assignSheet.Cells.AutoFitColumns();
                    }

                    // Survey Details/Devices Sheet
                    if (surveyDetails.Any())
                    {
                        var deviceSheet = package.Workbook.Worksheets.Add("Devices & Items");
                        deviceSheet.Cells["A1"].Value = "Location";
                        deviceSheet.Cells["B1"].Value = "Type";
                        deviceSheet.Cells["C1"].Value = "Item ID";
                        deviceSheet.Cells["D1"].Value = "Item Name";
                        deviceSheet.Cells["E1"].Value = "Description";
                        deviceSheet.Cells["F1"].Value = "Qty Existing";
                        deviceSheet.Cells["G1"].Value = "Qty Required";
                        deviceSheet.Cells["H1"].Value = "Remarks";
                        deviceSheet.Cells["A1:H1"].Style.Font.Bold = true;

                        row = 2;
                        foreach (var detail in surveyDetails)
                        {
                            foreach (var item in detail.ItemLists)
                            {
                                deviceSheet.Cells[row, 1].Value = detail.LocName;
                                deviceSheet.Cells[row, 2].Value = detail.TypeName;
                                deviceSheet.Cells[row, 3].Value = item.ItemID;
                                deviceSheet.Cells[row, 4].Value = item.ItemName;
                                deviceSheet.Cells[row, 5].Value = item.ItemDesc;
                                deviceSheet.Cells[row, 6].Value = item.ItemQtyExist;
                                deviceSheet.Cells[row, 7].Value = item.ItemQtyReq;
                                deviceSheet.Cells[row, 8].Value = item.Remarks;
                                row++;
                            }
                        }
                        deviceSheet.Cells.AutoFitColumns();
                    }

                    var stream = new System.IO.MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"DetailedReport_{survey.SurveyName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("DetailedReport", new { surveyId });
            }
        }

        public IActionResult ExportDetailedReportNew(long surveyId)
        {
            try
            {
                // OTP Authorization Check - Non-super admins need validated OTP
                if (!IsAuthorizedForDownload())
                {
                    return GetUnauthorizedResult("detailed report");
                }

                // 1) Get data (same as your original)
                DataTable dtSurveyDetails = _surveyRepo.GetSurveyDetails(surveyId, 1);
                DataTable dtSurveyLocEmp = _surveyRepo.GetSurveyDetails(surveyId, 2);
                DataTable dtSurveyItems = _surveyRepo.GetSurveyDetails(surveyId, 3);
                DataTable dtSurveyRemarks = _surveyRepo.GetSurveyDetails(surveyId, 4);

                // Enrich items with images and remarks
                dtSurveyItems = EnrichItemsTableWithImagesAndRemarks(dtSurveyItems, surveyId, dtSurveyRemarks);

                if (dtSurveyDetails == null || dtSurveyDetails.Rows.Count == 0)
                {
                    TempData["ResultMessage"] = "<strong>Error!</strong> Survey not found.";
                    TempData["ResultType"] = "danger";
                    return RedirectToAction("SummaryReport");
                }

                var sRow = dtSurveyDetails.Rows[0];
                string surveyName = sRow["SurveyName"]?.ToString() ?? string.Empty;
                string clientId = sRow["ClientId"]?.ToString() ?? string.Empty;
                string clientName = sRow["ClientName"]?.ToString() ?? string.Empty;
                string clientAddr = sRow["ClintAddress"]?.ToString() ?? string.Empty;
                string contactPers = sRow["ContactPerson"]?.ToString() ?? string.Empty;
                string status = sRow["SurveyStatus"]?.ToString() ?? string.Empty;
                string region = sRow["RegionID"]?.ToString() ?? string.Empty;
                string implType = sRow["ImplementationType"]?.ToString() ?? string.Empty;
                string scopeOfWork = sRow["ScopeOfWork"]?.ToString() ?? string.Empty;
                DateTime? startDate = sRow["SurveyDate"] as DateTime?;
                DateTime? complDate = sRow["SubmissionDate"] as DateTime?;
                string locationSite = sRow["LocationSiteName"]?.ToString() ?? string.Empty;

                // EPPlus license context (modern API)
                OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("ABTMS");

                using (var package = new ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Survey Report");
                    int row = 1;
                    int maxCols = Math.Max(dtSurveyItems?.Columns.Count ?? 8, 8);

                    // ---------- helpers ----------
                    Action<int, int, int, int, Color> applyBorder = (r1, c1, r2, c2, color) =>
                    {
                        using (var rng = ws.Cells[r1, c1, r2, c2])
                        {
                            rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }
                    };

                    Func<int, int, string, int, Color, ExcelRange> sectionHeader = (r, c, text, colspan, bg) =>
                    {
                        var target = ws.Cells[r, c, r, c + colspan - 1];
                        target.Merge = true;
                        target.Value = text;
                        target.Style.Font.Bold = true;
                        target.Style.Font.Size = 14;
                        target.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        target.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        target.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        target.Style.Fill.BackgroundColor.SetColor(bg);
                        return target;
                    };

                    Action<int, int, int, int, bool> setBoldRange = (r1, c1, r2, c2, bold) =>
                    {
                        ws.Cells[r1, c1, r2, c2].Style.Font.Bold = bold;
                    };

                    // subtle palette
                    Color headerBg = Color.FromArgb(210, 225, 240); // light steel-like
                    Color sectionBg = Color.FromArgb(230, 230, 230); // light gray
                    Color locBg = Color.FromArgb(224, 243, 250); // light blue-ish
                    Color totalReqBg = Color.FromArgb(217, 234, 211); // light green
                    Color totalExBg = Color.FromArgb(255, 249, 196); // light yellow

                    // ---------- TITLE ----------
                    var titleRange = ws.Cells[row, 1, row, 8];
                    titleRange.Merge = true;
                    titleRange.Value = $"Survey Report: {surveyId} — {surveyName}";
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.Size = 16;
                    titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    titleRange.Style.Fill.BackgroundColor.SetColor(headerBg);
                    applyBorder(row, 1, row, 8, Color.Black);
                    row += 1;

                    // ---------- CLIENT INFO / STATUS BLOCK ----------
                    int blockStart = row;

                    // headers
                    sectionHeader(row, 1, "Client Info", 4, sectionBg);
                    sectionHeader(row, 5, "Survey Status", 4, sectionBg);
                    row++;

                    // client left side
                    ws.Cells[row, 1].Value = "ID:";
                    ws.Cells[row, 1].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 2].Merge = true;
                    ws.Cells[row, 3].Value = clientId;
                    ws.Cells[row, 3, row, 4].Merge = true;

                    ws.Cells[row + 1, 1].Value = "Name:";
                    ws.Cells[row + 1, 1].Style.Font.Bold = true;
                    ws.Cells[row + 1, 1, row + 1, 2].Merge = true;
                    ws.Cells[row + 1, 3].Value = clientName;
                    ws.Cells[row + 1, 3, row + 1, 4].Merge = true;

                    ws.Cells[row + 2, 1].Value = "Address:";
                    ws.Cells[row + 2, 1].Style.Font.Bold = true;
                    ws.Cells[row + 2, 1, row + 2, 2].Merge = true;
                    ws.Cells[row + 2, 3].Value = clientAddr;
                    ws.Cells[row + 2, 3, row + 2, 4].Merge = true;

                    ws.Cells[row + 3, 1].Value = "Contact:";
                    ws.Cells[row + 3, 1].Style.Font.Bold = true;
                    ws.Cells[row + 3, 1, row + 3, 2].Merge = true;
                    ws.Cells[row + 3, 3].Value = contactPers;
                    ws.Cells[row + 3, 3, row + 3, 4].Merge = true;

                    // right side (status)
                    ws.Cells[row, 5].Value = "Status:";
                    ws.Cells[row, 5].Style.Font.Bold = true;
                    ws.Cells[row, 5, row, 6].Merge = true;
                    ws.Cells[row, 7].Value = status;
                    ws.Cells[row, 7, row, 8].Merge = true;

                    ws.Cells[row + 1, 5].Value = "Implementation Type:";
                    ws.Cells[row + 1, 5].Style.Font.Bold = true;
                    ws.Cells[row + 1, 5, row + 1, 6].Merge = true;
                    ws.Cells[row + 1, 7].Value = implType;
                    ws.Cells[row + 1, 7, row + 1, 8].Merge = true;

                    ws.Cells[row + 2, 5].Value = "Start Date:";
                    ws.Cells[row + 2, 5].Style.Font.Bold = true;
                    ws.Cells[row + 2, 5, row + 2, 6].Merge = true;
                    ws.Cells[row + 2, 7].Value = startDate?.ToString("dd-MMM-yyyy") ?? string.Empty;
                    ws.Cells[row + 2, 7, row + 2, 8].Merge = true;

                    ws.Cells[row + 3, 5].Value = "Completion Date:";
                    ws.Cells[row + 3, 5].Style.Font.Bold = true;
                    ws.Cells[row + 3, 5, row + 3, 6].Merge = true;
                    ws.Cells[row + 3, 7].Value = complDate?.ToString("dd-MMM-yyyy") ?? string.Empty;
                    ws.Cells[row + 3, 7, row + 3, 8].Merge = true;

                    int blockEnd = row + 3;
                    applyBorder(blockStart, 1, blockEnd, 8, Color.Black);
                    row = blockEnd + 2;

                    // ---------- SCOPE OF WORK ----------
                    sectionHeader(row, 1, "Scope Of Work", 8, sectionBg);
                    row++;
                    ws.Cells[row, 1].Value = scopeOfWork;
                    ws.Cells[row, 1, row, 8].Merge = true;
                    ws.Cells[row, 1, row, 8].Style.WrapText = true;
                    ws.Row(row).CustomHeight = true;
                    ws.Row(row).Height = 60; // reasonable default; tweak if needed
                    applyBorder(row - 1, 1, row, 8, Color.Black);
                    row += 2;

                    // ---------- LOCATIONS + TEAM ----------
                    int locHeaderRow = row;
                    sectionHeader(row, 1, "Locations", 4, locBg);
                    sectionHeader(row, 5, "Team", 4, locBg);
                    row++;

                    // column headers
                    var locCols = new[] { "ID", "Location Name", "Location Type", "Coordinates", "Emp ID", "Name", "", "Contact No" };
                    for (int c = 0; c < locCols.Length; c++)
                    {
                        ws.Cells[row, c + 1].Value = locCols[c];
                    }
                    using (var hdr = ws.Cells[row, 1, row, 8])
                    {
                        hdr.Style.Font.Bold = true;
                        hdr.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        hdr.Style.Fill.BackgroundColor.SetColor(sectionBg);
                        hdr.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    row++;

                    if (dtSurveyLocEmp != null && dtSurveyLocEmp.Rows.Count > 0)
                    {
                        foreach (DataRow lr in dtSurveyLocEmp.Rows)
                        {
                            ws.Cells[row, 1].Value = lr["LocID"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 2].Value = lr["LocName"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 3].Value = lr["LocationType"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 4].Value = lr["Cordinate"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 5].Value = lr["EmpID"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 6].Value = lr["EmpName"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 6, row, 7].Merge = true;
                            ws.Cells[row, 8].Value = lr["MobileNo"]?.ToString() ?? string.Empty;
                            row++;
                        }
                    }
                    applyBorder(locHeaderRow, 1, row - 1, 8, Color.Black);
                    row += 2;

                    // ---------- REQUIREMENT SUMMARY (Locations as rows, Device Types as columns) ----------
                    if (dtSurveyItems != null && dtSurveyItems.Rows.Count > 0 && dtSurveyLocEmp != null && dtSurveyLocEmp.Rows.Count > 0)
                    {
                        // STEP 1: Extract device categories and types
                        var deviceCategories = new Dictionary<string, List<string>>();
                        var allDeviceTypes = new List<string>();
                        // Visibility: [0]=Ex, [1]=Req, [2]=Img, [3]=Rem, [4]=Spec
                        var devColVis = new Dictionary<string, bool[]>();
                        var deviceUOMs = new Dictionary<string, string>();
                        
                        foreach (DataRow itemRow in dtSurveyItems.Rows)
                        {
                            string typeName = itemRow[1]?.ToString()?.Trim() ?? "";
                            string itemName = itemRow[2]?.ToString()?.Trim() ?? "";
                            
                            if (!string.IsNullOrEmpty(itemName))
                            {
                                // CRITICAL: Use composite key (TypeName||ItemName)
                                string compositeKey = string.IsNullOrEmpty(typeName) ? itemName : $"{typeName}||{itemName}";
                                
                                if (!allDeviceTypes.Contains(compositeKey))
                                {
                                    allDeviceTypes.Add(compositeKey);
                                    devColVis[compositeKey] = new bool[] { false, false, false, false, false };
                                }
                                
                                if (!deviceUOMs.ContainsKey(compositeKey))
                                {
                                    if (dtSurveyItems.Columns.Contains("ItemUOM"))
                                        deviceUOMs[compositeKey] = itemRow["ItemUOM"]?.ToString()?.Trim() ?? "";
                                    else if (dtSurveyItems.Columns.Count > 3)
                                        deviceUOMs[compositeKey] = itemRow[3]?.ToString()?.Trim() ?? "";
                                    else
                                        deviceUOMs[compositeKey] = "";
                                }
                                
                                if (!string.IsNullOrEmpty(typeName))
                                {
                                    if (!deviceCategories.ContainsKey(typeName)) deviceCategories[typeName] = new List<string>();
                                    if (!deviceCategories[typeName].Contains(compositeKey)) deviceCategories[typeName].Add(compositeKey);
                                }
                            }
                        }
                        
                        // Find location columns
                        var allColumns = dtSurveyItems.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                        var locationColumnPairs = new Dictionary<string, (string, string, string, string, string)>(StringComparer.OrdinalIgnoreCase); // Ex, Req, Pho, Rem, Spec
                        foreach (var col in allColumns)
                        {
                            if (col.EndsWith("Existing"))
                            {
                                var locName = col.Replace("Existing", "").Trim();
                                if (!locationColumnPairs.ContainsKey(locName))
                                {
                                    string rCol = allColumns.FirstOrDefault(c => c.Equals(locName + "Required", StringComparison.OrdinalIgnoreCase)) ?? "";
                                    string pCol = allColumns.FirstOrDefault(c => c.Equals(locName + "Photo", StringComparison.OrdinalIgnoreCase)) ?? 
                                                  allColumns.FirstOrDefault(c => c.Equals(locName + "Photos", StringComparison.OrdinalIgnoreCase)) ?? "";
                                    string remCol = allColumns.FirstOrDefault(c => c.Equals(locName + "Remarks", StringComparison.OrdinalIgnoreCase)) ?? "";
                                    string specCol = allColumns.FirstOrDefault(c => c.Equals(locName + "Specification", StringComparison.OrdinalIgnoreCase)) ?? "";
                                    locationColumnPairs[locName] = (col, rCol, pCol, remCol, specCol);
                                }
                            }
                        }
                        
                        // STEP 2: Calculate totals and build location data
                        var existingTotals = allDeviceTypes.ToDictionary(dt => dt, dt => 0);
                        var requiredTotals = allDeviceTypes.ToDictionary(dt => dt, dt => 0);
                        var locationDataList = new List<(int, string, string, string, Dictionary<string, (int, int, string, string, string)>)>(); // slNo, locName, locType, coords, deviceData
                        
                        int slNo = 1;
                        foreach (DataRow locRow in dtSurveyLocEmp.Rows)
                        {
                            string locName = locRow["LocName"]?.ToString()?.Trim() ?? "";
                            string locType = locRow["LocationType"]?.ToString()?.Trim() ?? "";
                            string coords = locRow["Cordinate"]?.ToString()?.Trim() ?? "";
                            
                            var deviceData = new Dictionary<string, (int, int, string, string, string)>(); // exQty, reqQty, pho, rem, spec
                            bool hasData = false;
                            
                            string mLoc = locationColumnPairs.Keys.FirstOrDefault(l => l.Trim().Equals(locName, StringComparison.OrdinalIgnoreCase)) ?? "";
                            if (!string.IsNullOrEmpty(mLoc))
                            {
                                var cols = locationColumnPairs[mLoc];
                                foreach (var deviceType in allDeviceTypes)
                                {
                                    int exQty = 0; int reqQty = 0; string pho = ""; string rem = ""; string spec = "";
                                    foreach (DataRow itemRow in dtSurveyItems.Rows)
                                    {
                                        string rowType = itemRow[1]?.ToString()?.Trim() ?? "";
                                        string rowItem = itemRow[2]?.ToString()?.Trim() ?? "";
                                        string rowKey = string.IsNullOrEmpty(rowType) ? rowItem : $"{rowType}||{rowItem}";

                                        if (rowKey.Equals(deviceType, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (!string.IsNullOrEmpty(cols.Item1)) { int val = 0; int.TryParse(itemRow[cols.Item1]?.ToString(), out val); exQty += val; }
                                            if (!string.IsNullOrEmpty(cols.Item2)) { int val = 0; int.TryParse(itemRow[cols.Item2]?.ToString(), out val); reqQty += val; }
                                            if (!string.IsNullOrEmpty(cols.Item3)) pho = itemRow[cols.Item3]?.ToString() ?? "";
                                            if (!string.IsNullOrEmpty(cols.Item4)) rem = itemRow[cols.Item4]?.ToString() ?? "";
                                            else if (dtSurveyItems.Columns.Contains("Remarks")) rem = itemRow["Remarks"]?.ToString() ?? "";
                                            if (!string.IsNullOrEmpty(cols.Item5)) spec = itemRow[cols.Item5]?.ToString() ?? "";
                                        }
                                    }
                                    
                                    if (exQty > 0 || reqQty > 0 || !string.IsNullOrEmpty(pho) || !string.IsNullOrEmpty(rem) || !string.IsNullOrEmpty(spec))
                                    {
                                        hasData = true;
                                        deviceData[deviceType] = (exQty, reqQty, pho, rem, spec);
                                        if (exQty > 0) { existingTotals[deviceType] += exQty; devColVis[deviceType][0] = true; }
                                        if (reqQty > 0) { requiredTotals[deviceType] += reqQty; devColVis[deviceType][1] = true; }
                                        if (!string.IsNullOrEmpty(pho)) devColVis[deviceType][2] = true;
                                        if (!string.IsNullOrEmpty(rem)) devColVis[deviceType][3] = true;
                                        if (!string.IsNullOrEmpty(spec)) devColVis[deviceType][4] = true;
                                    }
                                }
                            }
                            if (hasData) { locationDataList.Add((slNo, locName, locType, coords, deviceData)); slNo++; }
                        }

                        // Force hide Images column for Excel Export (as per requirement)
                        // "In the Excel report, the Image column must not be included"
                        foreach (var key in devColVis.Keys.ToList())
                        {
                            devColVis[key][2] = false;
                        }
                        
                        // STEP 3: Filter visible items
                        // STEP 3: Filter visible items
                        var rawVisibleTypes = allDeviceTypes.Where(dt => devColVis[dt].Any(v => v)).ToHashSet();
                        var filteredCategories = new Dictionary<string, List<string>>();
                        foreach (var cat in deviceCategories)
                        {
                            var vItems = cat.Value.Where(i => rawVisibleTypes.Contains(i)).ToList();
                            if (vItems.Any()) filteredCategories[cat.Key] = vItems;
                        }

                        // CRITICAL: Ensure visibleDeviceTypes follows the exact order of filteredCategories keys and values
                        // This prevents header mismatch where Category Header spans over wrong devices
                        var visibleDeviceTypes = filteredCategories.SelectMany(c => c.Value).ToList();
                        
                        if (visibleDeviceTypes.Any() && locationDataList.Any())
                        {
                            Color greenHeader = Color.FromArgb(112, 173, 71); 
                            Color blueHeader = Color.FromArgb(91, 155, 213);
                            Color yellowHeader = Color.FromArgb(255, 192, 0);
                            Color yellowLight = Color.FromArgb(255, 235, 156);
                            Color blueSubHeader = Color.FromArgb(47, 117, 181);
                            Color existingBg = Color.FromArgb(255, 249, 230);
                            Color requiredBg = Color.FromArgb(198, 239, 206);
                            Color remarksBg = Color.FromArgb(255, 242, 204);
                            Color rowBg = Color.FromArgb(226, 239, 218);
                            Color coordBg = Color.FromArgb(221, 235, 247);
                            Color totalOrangeBg = Color.FromArgb(244, 176, 132);
                            Color totalGreenBg = Color.FromArgb(146, 208, 80);
                            
                            // Title row
                            int reqTitleRow = row;
                            int totalCols = 5 + visibleDeviceTypes.Sum(dt => devColVis[dt].Count(v => v));
                            var reqTitle = ws.Cells[reqTitleRow, 1, reqTitleRow, totalCols];
                            reqTitle.Merge = true; reqTitle.Value = "Requirement Summary"; reqTitle.Style.Font.Bold = true; reqTitle.Style.Font.Size = 12;
                            reqTitle.Style.Fill.PatternType = ExcelFillStyle.Solid; reqTitle.Style.Fill.BackgroundColor.SetColor(greenHeader);
                            reqTitle.Style.Font.Color.SetColor(Color.White);
                            row++;
                            
                            int row1 = row; int row2 = row + 1; int row3 = row + 2;
                            ws.Cells[row1, 1, row3, 1].Merge = true; ws.Cells[row1, 1].Value = "SL.No";
                            ws.Cells[row1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row1, 1].Style.Fill.BackgroundColor.SetColor(greenHeader);
                            ws.Cells[row1, 1].Style.Font.Color.SetColor(Color.White);
                            
                            ws.Cells[row1, 2, row3, 2].Merge = true; ws.Cells[row1, 2].Value = "Location Name";
                            ws.Cells[row1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row1, 2].Style.Fill.BackgroundColor.SetColor(greenHeader);
                            ws.Cells[row1, 2].Style.Font.Color.SetColor(Color.White);
                            
                            ws.Cells[row1, 3, row2, 5].Merge = true; ws.Cells[row1, 3].Value = "Geo coordinates";
                            ws.Cells[row1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row1, 3].Style.Fill.BackgroundColor.SetColor(blueHeader);
                            ws.Cells[row1, 3].Style.Font.Color.SetColor(Color.White);
                            
                            ws.Cells[row3, 3].Value = "Latitude"; ws.Cells[row3, 4].Value = "Longitude"; ws.Cells[row3, 5].Value = "Link";
                            using (var rng = ws.Cells[row3, 3, row3, 5]) { rng.Style.Fill.PatternType = ExcelFillStyle.Solid; rng.Style.Fill.BackgroundColor.SetColor(blueSubHeader); rng.Style.Font.Color.SetColor(Color.White); rng.Style.Font.Size = 9; }
                            
                            // Category headers
                            int col = 6;
                            foreach (var cat in filteredCategories)
                            {
                                int catColspan = cat.Value.Sum(dt => devColVis[dt].Count(v => v));
                                ws.Cells[row1, col, row1, col + catColspan - 1].Merge = true; ws.Cells[row1, col].Value = cat.Key;
                                ws.Cells[row1, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row1, col].Style.Fill.BackgroundColor.SetColor(yellowHeader);
                                col += catColspan;
                            }
                            
                            // Device type headers
                            col = 6;
                            foreach (var dt in visibleDeviceTypes)
                            {
                                int devColspan = devColVis[dt].Count(v => v);
                                ws.Cells[row2, col, row2, col + devColspan - 1].Merge = true; 
                                // Split composite key (Category||Item) for display
                                ws.Cells[row2, col].Value = dt.Contains("||") ? dt.Split(new[] { "||" }, StringSplitOptions.None).Last() : dt;
                                ws.Cells[row2, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row2, col].Style.Fill.BackgroundColor.SetColor(yellowLight);
                                col += devColspan;
                            }
                            
                            // Sub-headers
                            col = 6;
                            foreach (var dt in visibleDeviceTypes)
                            {
                                var vis = devColVis[dt];
                                if (vis[0]) { ws.Cells[row3, col].Value = "Existing"; ws.Cells[row3, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row3, col].Style.Fill.BackgroundColor.SetColor(existingBg); col++; }
                                if (vis[1]) { ws.Cells[row3, col].Value = "Required"; ws.Cells[row3, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row3, col].Style.Fill.BackgroundColor.SetColor(requiredBg); col++; }
                                if (vis[2]) { ws.Cells[row3, col].Value = "Images"; ws.Cells[row3, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row3, col].Style.Fill.BackgroundColor.SetColor(remarksBg); col++; }
                                if (vis[3]) { ws.Cells[row3, col].Value = "Remarks"; ws.Cells[row3, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row3, col].Style.Fill.BackgroundColor.SetColor(remarksBg); col++; }
                                if (vis[4]) { ws.Cells[row3, col].Value = "Specification"; ws.Cells[row3, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row3, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(231, 230, 255)); col++; }
                            }
                            
                            using (var hdr = ws.Cells[row1, 1, row3, totalCols]) { hdr.Style.Font.Bold = true; hdr.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; hdr.Style.VerticalAlignment = ExcelVerticalAlignment.Center; hdr.Style.Font.Size = 9; }
                            row = row3 + 1;
                            
                            // Data rows
                            foreach (var loc in locationDataList)
                            {
                                var parts = (loc.Item4 ?? "").Split(','); string lat = parts.Length > 0 ? parts[0].Trim() : ""; string lng = parts.Length > 1 ? parts[1].Trim() : "";
                                string link = (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng)) ? $"https://www.google.com/maps?q={lat},{lng}" : "";
                                
                                ws.Cells[row, 1].Value = loc.Item1; ws.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(rowBg);
                                ws.Cells[row, 2].Value = string.IsNullOrEmpty(loc.Item3) ? loc.Item2 : $"{loc.Item2} ({loc.Item3})";
                                ws.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(rowBg);
                                ws.Cells[row, 3].Value = lat; ws.Cells[row, 4].Value = lng; ws.Cells[row, 5].Value = !string.IsNullOrEmpty(link) ? "maps.google.com/" : "";
                                if (!string.IsNullOrEmpty(link)) { ws.Cells[row, 5].Hyperlink = new Uri(link); ws.Cells[row, 5].Style.Font.Color.SetColor(Color.Blue); ws.Cells[row, 5].Style.Font.UnderLine = true; }
                                using (var rng = ws.Cells[row, 3, row, 5]) { rng.Style.Fill.PatternType = ExcelFillStyle.Solid; rng.Style.Fill.BackgroundColor.SetColor(coordBg); rng.Style.Font.Size = 9; }
                                
                                int dCol = 6;
                                foreach (var dt in visibleDeviceTypes)
                                {
                                    var vis = devColVis[dt];
                                    var data = loc.Item5.ContainsKey(dt) ? loc.Item5[dt] : (0, 0, "", "", "");
                                    string uomSuffix = (deviceUOMs.ContainsKey(dt) && deviceUOMs[dt].Equals("MTR", StringComparison.OrdinalIgnoreCase)) ? " mtr" : "";
                                    
                                    if (vis[0]) { if (data.Item1 > 0) { ws.Cells[row, dCol].Value = data.Item1 > 0 ? $"{data.Item1}{uomSuffix}" : (object)data.Item1; ws.Cells[row, dCol].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row, dCol].Style.Fill.BackgroundColor.SetColor(existingBg); ws.Cells[row, dCol].Style.Font.Size = 9; } dCol++; }
                                    if (vis[1]) { if (data.Item2 > 0) { ws.Cells[row, dCol].Value = data.Item2 > 0 ? $"{data.Item2}{uomSuffix}" : (object)data.Item2; ws.Cells[row, dCol].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[row, dCol].Style.Fill.BackgroundColor.SetColor(requiredBg); ws.Cells[row, dCol].Style.Font.Bold = true; ws.Cells[row, dCol].Style.Font.Size = 9; } dCol++; }
                                    if (vis[2]) { if (!string.IsNullOrEmpty(data.Item3)) { ws.Cells[row, dCol].Value = "View Photos"; ws.Cells[row, dCol].Style.Font.Color.SetColor(Color.Blue); ws.Cells[row, dCol].Style.Font.UnderLine = true; } dCol++; }
                                    if (vis[3]) { ws.Cells[row, dCol].Value = data.Item4; ws.Cells[row, dCol].Style.Font.Size = 8; dCol++; }
                                    if (vis[4]) { ws.Cells[row, dCol].Value = data.Item5; ws.Cells[row, dCol].Style.Font.Size = 8; dCol++; }
                                }
                                row++;
                            }
                            
                            // Total row
                            int totRow = row;
                            ws.Cells[totRow, 1, totRow, 2].Merge = true; ws.Cells[totRow, 1].Value = "TOTAL"; ws.Cells[totRow, 1].Style.Font.Bold = true;
                            ws.Cells[totRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[totRow, 1].Style.Fill.BackgroundColor.SetColor(totalOrangeBg);
                            using (var rng = ws.Cells[totRow, 3, totRow, 5]) { rng.Style.Fill.PatternType = ExcelFillStyle.Solid; rng.Style.Fill.BackgroundColor.SetColor(totalOrangeBg); }
                            
                            col = 6;
                            foreach (var dt in visibleDeviceTypes)
                            {
                                var vis = devColVis[dt];
                                string uomSuffix = (deviceUOMs.ContainsKey(dt) && deviceUOMs[dt].Equals("MTR", StringComparison.OrdinalIgnoreCase)) ? " mtr" : "";
                                
                                if (vis[0]) { ws.Cells[totRow, col].Value = existingTotals[dt] > 0 ? $"{existingTotals[dt]}{uomSuffix}" : (object)"0"; ws.Cells[totRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[totRow, col].Style.Fill.BackgroundColor.SetColor(totalOrangeBg); ws.Cells[totRow, col].Style.Font.Bold = true; col++; }
                                if (vis[1]) { ws.Cells[totRow, col].Value = requiredTotals[dt] > 0 ? $"{requiredTotals[dt]}{uomSuffix}" : (object)"0"; ws.Cells[totRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[totRow, col].Style.Fill.BackgroundColor.SetColor(totalGreenBg); ws.Cells[totRow, col].Style.Font.Bold = true; col++; }
                                if (vis[2]) { ws.Cells[totRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[totRow, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(252, 228, 214)); col++; }
                                if (vis[3]) { ws.Cells[totRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[totRow, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(252, 228, 214)); col++; }
                                if (vis[4]) { ws.Cells[totRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid; ws.Cells[totRow, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(252, 228, 214)); col++; }
                            }
                            
                            applyBorder(reqTitleRow, 1, totRow, totalCols, Color.Black);
                            row = totRow + 2;
                        }
                    }

                    // ---------- Camera Installation Remarks ----------
                    if (dtSurveyRemarks != null && dtSurveyRemarks.Rows.Count > 0)
                    {
                        int remarksTitleRow = row;
                        int remarksCols = 8; // Fixed column count for remarks section
                        var titleRange1 = ws.Cells[remarksTitleRow, 1, remarksTitleRow, remarksCols];
                        titleRange1.Merge = true;
                        titleRange1.Value = "Camera Installation Remarks";
                        titleRange1.Style.Font.Bold = true;
                        titleRange1.Style.Font.Size = 14;
                        titleRange1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        titleRange1.Style.Fill.BackgroundColor.SetColor(headerBg);
                        row++;

                        // header for remarks
                        ws.Cells[row, 1].Value = "Location";
                        ws.Cells[row, 2].Value = "Item Code";
                        ws.Cells[row, 3].Value = "Items";
                        ws.Cells[row, 4].Value = "Remarks";
                        ws.Cells[row, 4, row, remarksCols].Merge = true;

                        using (var rng = ws.Cells[row, 1, row, remarksCols])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(sectionBg);
                        }
                        row++;

                        // populate remarks (with merging of same locations)
                        int startMergeRow = row;
                        string prevLoc = null;
                        
                        for (int i = 0; i < dtSurveyRemarks.Rows.Count; i++)
                        {
                            var dr = dtSurveyRemarks.Rows[i];
                            string currentLoc = dr["LocName"]?.ToString() ?? string.Empty;

                            ws.Cells[row, 1].Value = currentLoc;
                            ws.Cells[row, 2].Value = dr["ItemID"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 3].Value = dr["Cameras"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 4].Value = dr["Remarks"]?.ToString() ?? string.Empty;
                            ws.Cells[row, 4, row, remarksCols].Merge = true;

                            if (prevLoc != null && prevLoc != currentLoc)
                            {
                                if (row - 1 > startMergeRow)
                                    ws.Cells[startMergeRow, 1, row - 1, 1].Merge = true;
                                startMergeRow = row;
                            }

                            prevLoc = currentLoc;
                            row++;
                        }

                        if (row - 1 > startMergeRow)
                            ws.Cells[startMergeRow, 1, row - 1, 1].Merge = true;

                        applyBorder(remarksTitleRow, 1, row - 1, remarksCols, Color.Black);
                    }

                    // Autosize and small polish
                    ws.Cells[1, 1, ws.Dimension?.End.Row ?? 100, ws.Dimension?.End.Column ?? maxCols].AutoFitColumns();

                    // Freeze top rows and first column for easy reading
                    // ws.View.FreezePanes(4, 2);

                    // prepare file
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"SurveyReport_{surveyId}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("DetailedReport", new { surveyId });
            }
        }

    }

    // Helper class for storing survey item image information
    internal class SurveyItemImageInfo
    {
        public string LocationName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ImageUrls { get; set; } = string.Empty;
    }
}