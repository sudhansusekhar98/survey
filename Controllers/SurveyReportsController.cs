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

        public SurveyReportsController(ISurvey surveyRepo, IAdmin adminRepo, ISurveySubmission submissionRepo, ISurveyCamRemarks camRemarksRepo)
        {
            _surveyRepo = surveyRepo;
            _adminRepo = adminRepo;
            _submissionRepo = submissionRepo;
            _camRemarksRepo = camRemarksRepo;
        }

        // GET: SurveyReports/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: SurveyReports/SummaryReport
        public IActionResult SummaryReport(DateTime? fromDate = null, DateTime? toDate = null, 
            string? status = null, string? region = null, string? type = null)
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

                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return View(new SurveyReportViewModel());
            }
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

            // Add image URL columns to dtSurveyItems
            dtSurveyItems = AddImageColumnsToItemsTable(dtSurveyItems, surveyId);

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
                        surveys.Add(new {
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
                    globalImages.Add(new {
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
                    rawRecords.Add(new {
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

        private DataTable AddImageColumnsToItemsTable(DataTable dtItems, long surveyId)
        {
            if (dtItems == null || dtItems.Rows.Count == 0)
                return dtItems;

            try
            {
                // Get all location columns (those with "Existing" or "Required" in the name)
                // Trim the location names to handle trailing spaces
                var locationColumns = dtItems.Columns.Cast<DataColumn>()
                    .Where(c => c.ColumnName.Contains("Existing") || c.ColumnName.Contains("Required"))
                    .Select(c => c.ColumnName.Replace("Existing", "").Replace("Required", "").Trim())
                    .Distinct()
                    .ToList();

                // Get survey item details with images from database
                var itemImages = GetSurveyItemImages(surveyId);

                // Debug: Log image count
                System.Diagnostics.Debug.WriteLine($"Found {itemImages.Count} images for survey {surveyId}");

                // Add image columns for each location
                foreach (var locationName in locationColumns)
                {
                    string imageColumnName = $"{locationName}_Photos";
                    if (!dtItems.Columns.Contains(imageColumnName))
                    {
                        dtItems.Columns.Add(imageColumnName, typeof(string));
                    }
                }

                // Determine the ItemCode column name - the pivot uses "Item Code" with space
                string itemCodeColumnName = dtItems.Columns.Contains("Item Code") ? "Item Code" 
                    : dtItems.Columns.Contains("ItemCode") ? "ItemCode"
                    : dtItems.Columns.Count > 0 ? dtItems.Columns[0].ColumnName : "";

                // Populate image URLs for each row
                foreach (DataRow row in dtItems.Rows)
                {
                    string itemCode = !string.IsNullOrEmpty(itemCodeColumnName) 
                        ? row[itemCodeColumnName]?.ToString()?.Trim() ?? ""
                        : row[0]?.ToString()?.Trim() ?? "";  // Fallback to first column
                    
                    foreach (var locationName in locationColumns)
                    {
                        string imageColumnName = $"{locationName}_Photos";
                        // Use case-insensitive and trimmed comparison for matching
                        // The location names from database are trimmed, the pivot column names need trimming too
                        var imageUrl = itemImages
                            .FirstOrDefault(img => 
                                img.LocationName.Trim().Equals(locationName.Trim(), StringComparison.OrdinalIgnoreCase) && 
                                img.ItemCode.Trim().Equals(itemCode.Trim(), StringComparison.OrdinalIgnoreCase))?.ImageUrls ?? "";
                        
                        row[imageColumnName] = imageUrl;
                    }
                }

                return dtItems;
            }
            catch (Exception ex)
            {
                // Log error and return original table
                System.Diagnostics.Debug.WriteLine($"Error in AddImageColumnsToItemsTable: {ex.Message}");
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
                // The pivot table uses ItemID (stored as "Item Code"), not the actual ItemCode from ItemMaster
                // Also need to trim location names to handle trailing spaces
                string query = @"
                    SELECT 
                        LTRIM(RTRIM(sl.LocName)) as LocationName,
                        CAST(sd.ItemID AS VARCHAR(20)) as ItemCode,
                        sd.ImgPath
                    FROM SurveyDetails sd
                    INNER JOIN SurveyLocation sl ON sd.LocID = sl.LocID AND sd.SurveyID = sl.SurveyID
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
                        ImageUrls = reader["ImgPath"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error - could add logging here for debugging
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
                            assignSheet.Cells[row, 5].Value = assign.DueDate.ToString("dd-MMM-yyyy");
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
        // 1: Get data (same as for the view)
        DataTable dtSurveyDetails = _surveyRepo.GetSurveyDetails(surveyId, 1);
        DataTable dtSurveyLocEmp = _surveyRepo.GetSurveyDetails(surveyId, 2);
        DataTable dtSurveyItems = _surveyRepo.GetSurveyDetails(surveyId, 3);
        DataTable dtSurveyRemarks = _surveyRepo.GetSurveyDetails(surveyId, 4);

        if (dtSurveyDetails == null || dtSurveyDetails.Rows.Count == 0)
        {
            TempData["ResultMessage"] = "<strong>Error!</strong> Survey not found.";
            TempData["ResultType"] = "danger";
            return RedirectToAction("SummaryReport");
        }

        var sRow = dtSurveyDetails.Rows[0];
        string? surveyName = sRow["SurveyName"]?.ToString();
        string? clientId = sRow["ClientId"]?.ToString();
        string? clientName = sRow["ClientName"]?.ToString();
        string? clientAddr = sRow["ClintAddress"]?.ToString();
        string? contactPers = sRow["ContactPerson"]?.ToString();
        string? status = sRow["SurveyStatus"]?.ToString();
        string? region = sRow["RegionID"]?.ToString();
        string? implType = sRow["ImplementationType"]?.ToString();
        string? scopeOfWork = sRow["ScopeOfWork"]?.ToString();
        DateTime? startDate = sRow["SurveyDate"] as DateTime?;
        DateTime? complDate = sRow["SubmissionDate"] as DateTime?;
        string? locationSite = sRow["LocationSiteName"]?.ToString();

        OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("ABTMS");

        using (var package = new ExcelPackage())
        {
            var ws = package.Workbook.Worksheets.Add("Survey Report");

            // will use max column width across sections
            int maxCols = Math.Max(
                dtSurveyItems?.Columns.Count ?? 10,
                12);

            int row = 1;

            // ========== TITLE ==========
            ws.Cells[row, 1].Value = $"Survey Report: {surveyId} ({surveyName})";
            ws.Cells[row, 1, row, 8].Merge = true;
            ws.Cells[row, 1, row, 8].Style.Font.Bold = true;
            ws.Cells[row, 1, row, 8].Style.Font.Size = 16;
            ws.Cells[row, 1, row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[row, 1, row, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

            row += 1;

            // ========== CLIENT INFO + STATUS BLOCK ==========
            int blockHeaderRow = row;
            // headers
            ws.Cells[blockHeaderRow, 1].Value = "Client Info";
            ws.Cells[blockHeaderRow, 1, blockHeaderRow, 4].Merge = true;
            ws.Cells[blockHeaderRow, 1, blockHeaderRow, 4].Style.Font.Bold = true;
            ws.Cells[blockHeaderRow, 1, blockHeaderRow, 4].Style.Font.Size = 14;
            ws.Cells[blockHeaderRow, 1, blockHeaderRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[blockHeaderRow, 1, blockHeaderRow, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[blockHeaderRow, 1, blockHeaderRow, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            ws.Cells[blockHeaderRow, 5].Value = "Survey Status";
            ws.Cells[blockHeaderRow, 5, blockHeaderRow, 8].Merge = true;
            ws.Cells[blockHeaderRow, 5, blockHeaderRow, 8].Style.Font.Bold = true;
            ws.Cells[blockHeaderRow, 5, blockHeaderRow, 8].Style.Font.Size = 14;
            ws.Cells[blockHeaderRow, 5, blockHeaderRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[blockHeaderRow, 5, blockHeaderRow, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[blockHeaderRow, 5, blockHeaderRow, 8].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            row++;

            // left side (Client)
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


            ws.Cells[row + 2, 1].Value = "Address :";
            ws.Cells[row + 2, 1].Style.Font.Bold = true;
            ws.Cells[row + 2, 1, row + 2, 2].Merge = true;
            ws.Cells[row + 2, 3].Value = clientAddr;
            ws.Cells[row + 2, 3, row + 2, 4].Merge = true;

            ws.Cells[row + 3, 1].Value = "Contact :";
            ws.Cells[row + 3, 1].Style.Font.Bold = true;
            ws.Cells[row + 3, 1, row + 3, 2].Merge = true;
            ws.Cells[row + 3, 3].Value = contactPers;
            ws.Cells[row + 3, 3, row + 3, 4].Merge = true;

            //ws.Cells[row, 1, row + 3, 1].Style.Font.Bold = true;

            // right side (Status / Region / Impl / Dates)
            int rs = row;
            ws.Cells[rs, 5].Value = "Status:";
            ws.Cells[rs, 5].Style.Font.Bold = true;
            ws.Cells[rs, 5, row, 6].Merge = true;
            ws.Cells[rs, 7].Value = status;
            ws.Cells[rs, 7, row, 8].Merge = true;

            //ws.Cells[rs + 1, 5].Value = "Region:";
            //ws.Cells[rs + 1, 6].Value = region;

            ws.Cells[rs + 1, 5].Value = "Implementation Type:";
            ws.Cells[rs + 1, 5].Style.Font.Bold = true;
            ws.Cells[rs + 1, 5, row + 1, 6].Merge = true;
            ws.Cells[rs + 1, 7].Value = implType;
            ws.Cells[rs + 1, 7, row + 1, 8].Merge = true;

            ws.Cells[rs + 2, 5].Value = "Start Date:";
            ws.Cells[rs + 2, 5].Style.Font.Bold = true;
            ws.Cells[rs + 2, 5, row + 2, 6].Merge = true;
            ws.Cells[rs + 2, 7].Value = startDate?.ToString("dd-MMM-yyyy");
            ws.Cells[rs + 2, 7, row + 2, 8].Merge = true;

            ws.Cells[rs + 3, 5].Value = "Completion Date";
            ws.Cells[rs + 3, 5].Style.Font.Bold = true;
            ws.Cells[rs + 3, 5, row + 3, 6].Merge = true;
            ws.Cells[rs + 3, 7].Value = complDate?.ToString("dd-MMM-yyyy");
            ws.Cells[rs + 3, 7, row + 3, 8].Merge = true;

            // ws.Cells[rs, 4, rs + 4, 4].Style.Font.Bold = true;

            row = rs + 4;

            // add border for this whole top block
            using (var rng = ws.Cells[blockHeaderRow, 1, row - 1, 8])
            {
                rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            row++; // blank line

            // ========== SCOPE OF WORK ==========
            ws.Cells[row, 1].Value = "Scope Of Work";
            ws.Cells[row, 1, row, 8].Merge = true;
            ws.Cells[row, 1, row, 8].Style.Font.Bold = true;
            ws.Cells[row, 1, row, 8].Style.Font.Size = 14;
            ws.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

            row++;

            ws.Cells[row, 1].Value = scopeOfWork;
            ws.Cells[row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            ws.Cells[row, 1, row, 8].Merge = true;
            ws.Cells[row, 1, row, 8].Style.WrapText = true;

            ws.Row(row).CustomHeight = true;
            ws.Row(row).Height = 60;


            using (var rng = ws.Cells[row - 1, 1, row, 8])
            {
                rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            row += 2;

            // ========== LOCATIONS + TEAM ==========
            int locHeaderRow = row;

            // section header row ("Locations" / "Team")
            ws.Cells[locHeaderRow, 1].Value = "Locations";
            ws.Cells[locHeaderRow, 1, locHeaderRow, 4].Merge = true;
            ws.Cells[locHeaderRow, 1, locHeaderRow, 4].Style.Font.Bold = true;
            ws.Cells[locHeaderRow, 1, locHeaderRow, 4].Style.Font.Size = 14;
            ws.Cells[locHeaderRow, 1, locHeaderRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[locHeaderRow, 1, locHeaderRow, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[locHeaderRow, 1, locHeaderRow, 4].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

            ws.Cells[locHeaderRow, 5].Value = "Team";
            ws.Cells[locHeaderRow, 5, locHeaderRow, 8].Merge = true;
            ws.Cells[locHeaderRow, 5, locHeaderRow, 8].Style.Font.Bold = true;
            ws.Cells[locHeaderRow, 5, locHeaderRow, 8].Style.Font.Size = 14;
            ws.Cells[locHeaderRow, 5, locHeaderRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[locHeaderRow, 5, locHeaderRow, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[locHeaderRow, 5, locHeaderRow, 8].Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

            row++;

            // column headers
            ws.Cells[row, 1].Value = "ID";
            ws.Cells[row, 2].Value = "Location Name";
            ws.Cells[row, 3].Value = "Location Type";
            ws.Cells[row, 4].Value = "Cordinates";
            ws.Cells[row, 5].Value = "Emp ID";
            ws.Cells[row, 6].Value = "Name";
            ws.Cells[row, 6, row, 7].Merge = true;
            ws.Cells[row, 8].Value = "Contact No";
            //  ws.Cells[row, 9].Value = "Due Date";

            using (var rng = ws.Cells[row, 1, row, 8])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            row++;

            if (dtSurveyLocEmp != null && dtSurveyLocEmp.Rows.Count > 0)
            {
                foreach (DataRow lr in dtSurveyLocEmp.Rows)
                {
                    ws.Cells[row, 1].Value = lr["LocID"];
                    ws.Cells[row, 2].Value = lr["LocName"];
                    ws.Cells[row, 3].Value = lr["LocationType"];
                    ws.Cells[row, 4].Value = lr["Cordinate"];
                    ws.Cells[row, 5].Value = lr["EmpID"];
                    ws.Cells[row, 6].Value = lr["EmpName"];
                    ws.Cells[row, 6, row, 7].Merge = true;
                    // ws.Cells[row, 7].Value = lr["Email"];
                    ws.Cells[row, 8].Value = lr["MobileNo"];
                    //ws.Cells[row, 9].Value = complDate?.ToString("dd-MMM-yyyy"); // due date (you can adjust)
                    row++;
                }
            }

            using (var rng = ws.Cells[locHeaderRow, 1, row - 1, 8])
            {
                rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            row += 2;


            // ========== REQUIREMENT SUMMARY (ITEMS PIVOT) ==========
            if (dtSurveyItems != null && dtSurveyItems.Rows.Count > 0)
            {
                int itemsCols = dtSurveyItems.Columns.Count;
                int reqTitleRow = row;

                // Title
                var titleRange = ws.Cells[reqTitleRow, 1, reqTitleRow, itemsCols];
                titleRange.Merge = true;
                titleRange.Value = "Requirement Summary";
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.Size = 14;
                // titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleRange.Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

                row++;

                // 3 Header rows
                int row1 = row;
                int row2 = row + 1;
                int row3 = row + 2;
                int dataStartRow = row + 3;

                int locPairs = (itemsCols - 7) / 2;
                int firstLocCol = 5;
                int lastLocCol = firstLocCol + locPairs * 2 - 1;
                int totalStartCol = lastLocCol + 1;
                int totalEndCol = totalStartCol + 1;
                int remarksCol = totalEndCol + 1;

                // Row1: main headers
                ws.Cells[row1, 1].Value = "Item Code";
                ws.Cells[row1, 1, row3, 1].Merge = true;

                ws.Cells[row1, 2].Value = "Type";
                ws.Cells[row1, 2, row3, 2].Merge = true;

                ws.Cells[row1, 3].Value = "Item";
                ws.Cells[row1, 3, row3, 3].Merge = true;

                ws.Cells[row1, 4].Value = "UOM";
                ws.Cells[row1, 4, row3, 4].Merge = true;

                ws.Cells[row1, firstLocCol].Value = "Locations";
                ws.Cells[row1, firstLocCol, row1, lastLocCol].Merge = true;

                ws.Cells[row1, totalStartCol].Value = "Total";
                ws.Cells[row1, totalStartCol, row2, totalEndCol].Merge = true;

                ws.Cells[row1, remarksCol].Value = "Remarks";
                ws.Cells[row1, remarksCol, row3, remarksCol].Merge = true;

                // Row2: Location names
                int colIndex = firstLocCol;
                for (int i = 0; i < locPairs; i++)
                {
                    string rawName = dtSurveyItems.Columns[colIndex - 1].ColumnName;
                    string locName = rawName.Replace("Existing", "").Replace("Required", "").Trim();

                    ws.Cells[row2, colIndex].Value = locName;
                    ws.Cells[row2, colIndex, row2, colIndex + 1].Merge = true;

                    colIndex += 2;
                }

                // Row3: Existing / Required
                colIndex = firstLocCol;
                for (int i = 0; i < locPairs; i++)
                {
                    ws.Cells[row3, colIndex].Value = "Existing";
                    ws.Cells[row3, colIndex + 1].Value = "Required";
                    colIndex += 2;
                }

                ws.Cells[row3, totalStartCol].Value = "Existing";
                ws.Cells[row3, totalEndCol].Value = "Required";


                // 🔹 Style ALL header rows
                var headerRange = ws.Cells[row1, 1, row3, itemsCols];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);


                // -------- Data Rows (with conditional formatting) --------
                int r = dataStartRow;
                foreach (DataRow dr in dtSurveyItems.Rows)
                {
                    for (int c = 0; c < itemsCols; c++)
                    {
                        var cell = ws.Cells[r, c + 1];
                        object value = dr[c];



                        if (c == 0) // first column = Item Code
                        {
                            if (decimal.TryParse(value?.ToString(), out decimal codeNum))
                            {
                                cell.Value = codeNum;
                                cell.Style.Numberformat.Format = "###0";
                            }
                            else
                                cell.Value = value;

                            continue;
                        }

                        // safe numeric parsing
                        decimal num = 0m;
                        bool isNumeric = value != DBNull.Value && decimal.TryParse(value.ToString(), out num);

                        // Is this column a numeric location/total column? (Existing/Required)
                        bool isLocValueColumn =
                            (c + 1 >= firstLocCol && c + 1 <= lastLocCol) ||   // all location Existing/Required
                            (c + 1 == totalStartCol || c + 1 == totalEndCol);  // Total Existing / Total Required

                        // If numeric and zero → keep blank
                        if (isNumeric && num == 0m)
                        {
                            cell.Value = value;
                            cell.Style.Font.Color.SetColor(Color.LightGray);
                            //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.Numberformat.Format = "#,##0.00";
                        }
                        else
                        {
                            // keep original value
                            cell.Value = value;
                            cell.Style.Numberformat.Format = "#,##0.00";
                            // cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            // Only color numeric > 0 in location/total columns
                            if (isNumeric && num > 0m && isLocValueColumn)
                            {
                                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;

                                // Location Existing/Required → light yellow
                                if (c + 1 >= firstLocCol && c + 1 <= lastLocCol)
                                {
                                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                }
                                // Total Existing → light yellow
                                else if (c + 1 == totalStartCol)
                                {
                                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                }
                                // Total Required → light green
                                else if (c + 1 == totalEndCol)
                                {
                                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                                }
                            }
                        }
                    }


                    r++;
                }

                // Borders
                using (var tableRange = ws.Cells[row1, 1, r - 1, itemsCols])
                {
                    tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // 🔹 Highlight totals
                var exCol = ws.Cells[dataStartRow, totalStartCol, r - 1, totalStartCol];
                exCol.Style.Fill.PatternType = ExcelFillStyle.Solid;
                exCol.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);

                var reqCol = ws.Cells[dataStartRow, totalEndCol, r - 1, totalEndCol];
                reqCol.Style.Fill.PatternType = ExcelFillStyle.Solid;
                reqCol.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);


                r++;
                
                // Survey Camera Remarks ------------------------------------------------
                row = r +1;
               

                int itemsCols1 = dtSurveyItems.Columns.Count;
                int reqTitleRow1 = row;

                // Title
                var titleRange1 = ws.Cells[reqTitleRow1, 1, reqTitleRow1, itemsCols1];
                titleRange1.Merge = true;
                titleRange1.Value = "Camera Installation Remarks";
                titleRange1.Style.Font.Bold = true;
                titleRange1.Style.Font.Size = 14;
                // titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleRange1.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleRange1.Style.Fill.BackgroundColor.SetColor(Color.LightSteelBlue);

                row++;

                // column headers
                ws.Cells[row, 1].Value = "Location";
                ws.Cells[row, 2].Value = "Item Code";
                ws.Cells[row, 3].Value = "Items";
                ws.Cells[row, 4].Value = "Remarks";
                ws.Cells[row, 4, row, itemsCols1].Merge = true;
                //ws.Cells[row, 8].Value = "Contact No";
                //  ws.Cells[row, 9].Value = "Due Date";

                using (var rng = ws.Cells[row, 1, row, 4])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                row++;
                int startMergeRow = row;
                string prevLoc = "";

                if (dtSurveyRemarks != null && dtSurveyRemarks.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSurveyRemarks.Rows.Count; i++)
                    {
                        DataRow lr = dtSurveyRemarks.Rows[i];
                        string currentLoc = lr["LocName"].ToString();

                        // Updated column order
                        ws.Cells[row, 1].Value = currentLoc;           // LocName first column
                        ws.Cells[row, 2].Value = lr["ItemID"];          // ItemID second column
                        ws.Cells[row, 3].Value = lr["Cameras"];
                        ws.Cells[row, 4].Value = lr["Remarks"];

                        // Merge remarks column
                        ws.Cells[row, 4, row, itemsCols1].Merge = true;

                        // Merge LocName if same
                        if (prevLoc != "" && prevLoc != currentLoc)
                        {
                            if (row - 1 > startMergeRow)
                                ws.Cells[startMergeRow, 1, row - 1, 1].Merge = true; // Merge first column now
                            startMergeRow = row;
                        }

                        prevLoc = currentLoc;
                        row++;
                    }

                    // Final merge block
                    if (row - 1 > startMergeRow)
                        ws.Cells[startMergeRow, 1, row - 1, 1].Merge = true;
                }



                using (var rng = ws.Cells[locHeaderRow, 1, row - 1, itemsCols1])
                {
                    rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

               
                                     

              

                row = r + 2;
            }

            ws.Cells.AutoFitColumns();


            // return file
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
        public string ImageUrls { get; set; } = string.Empty;
    }
}
