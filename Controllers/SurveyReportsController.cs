using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using OfficeOpenXml;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SurveyApp.Controllers
{
    public class SurveyReportsController : Controller
    {
        private readonly ISurvey _surveyRepo;
        private readonly IAdmin _adminRepo;

        public SurveyReportsController(ISurvey surveyRepo, IAdmin adminRepo)
        {
            _surveyRepo = surveyRepo;
            _adminRepo = adminRepo;
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
        public IActionResult DetailedReport(long surveyId)
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

                // Fetch CreatedBy user name
                var createdByUser = _adminRepo.GetUserById(survey.CreatedBy);
                ViewBag.CreatedByName = createdByUser?.LoginName ?? "Unknown";

                var locations = _surveyRepo.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
                var assignments = _surveyRepo.GetSurveyAssignments(surveyId) ?? new List<SurveyAssignmentModel>();
                var submission = _surveyRepo.GetSubmissionBySurveyId(surveyId);
                var submissions = submission != null ? new List<SurveySubmissionModel> { submission } : new List<SurveySubmissionModel>();

                // Get survey details (devices/items) for all locations
                var surveyDetails = new List<SurveyDetailsLocationModel>();
                foreach (var location in locations)
                {
                    var details = _surveyRepo.GetAssignedTypeList(surveyId, location.LocID);
                    if (details != null && details.Any())
                    {
                        foreach (var detail in details)
                        {
                            // Get items for each type
                            var items = _surveyRepo.GetAssignedItemList(surveyId, location.LocID, detail.ItemTypeID);
                            detail.ItemLists = items ?? new List<SurveyDetailsModel>();
                        }
                        surveyDetails.AddRange(details);
                    }
                }

                var report = new DetailedSurveyReportModel
                {
                    ReportTitle = $"Detailed Report - {survey.SurveyName}",
                    GeneratedBy = HttpContext.Session.GetString("UserName") ?? "System",
                    Survey = survey,
                    Locations = locations,
                    Assignments = assignments,
                    Submissions = submissions,
                    SurveyDetails = surveyDetails,
                    TotalLocations = locations.Count,
                    CompletedLocations = locations.Count(l => l.Isactive),
                    TotalAssignments = assignments.Count
                };

                // Calculate location completion rate
                report.LocationCompletionRate = report.TotalLocations > 0
                    ? Math.Round((decimal)report.CompletedLocations / report.TotalLocations * 100, 1)
                    : 0;

                // Calculate time to complete
                if (submissions.Any() && survey.SurveyDate.HasValue)
                {
                    var firstSubmission = submissions.OrderBy(s => s.SubmissionDate).FirstOrDefault();
                    if (firstSubmission?.SubmissionDate.HasValue == true)
                    {
                        report.TimeToComplete = firstSubmission.SubmissionDate.Value - survey.SurveyDate.Value;
                    }
                }

                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ResultMessage"] = $"<strong>Error!</strong> {ex.Message}";
                TempData["ResultType"] = "danger";
                return RedirectToAction("SummaryReport");
            }
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
    }
}
