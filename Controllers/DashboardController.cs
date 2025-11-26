using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;

namespace SurveyApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ISurvey _surveyRepo;
        private readonly IAdmin _adminRepo;

        public DashboardController(ISurvey surveyRepo, IAdmin adminRepo)
        {
            _surveyRepo = surveyRepo;
            _adminRepo = adminRepo;
        }

        public IActionResult Index(string? filter = null, string? value = null)
        {
            // Check if user is logged in - redirect to login if not
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Index", "UserLogin");
            }
            
            // Get user information from session
            int userId = 0;
            string userName = "Guest";
            int roleId = 0;
            
            if (HttpContext.Session.GetString("UserID") != null)
                int.TryParse(HttpContext.Session.GetString("UserID"), out userId);
            
            if (HttpContext.Session.GetString("UserName") != null)
                userName = HttpContext.Session.GetString("UserName") ?? "Guest";
            
            if (HttpContext.Session.GetString("RoleId") != null)
                int.TryParse(HttpContext.Session.GetString("RoleId"), out roleId);

            // Get user rights
            var userRights = _adminRepo.GetUserRights(userId);
            var surveyRights = userRights.FirstOrDefault(r => r.RightsName?.Contains("Survey") == true);
            
            // Get all surveys for the user
            var allSurveys = _surveyRepo.GetAllSurveys(userId);
            
            var today = DateTime.Now.Date;
            
            // Find surveys with missed deadlines (overdue and not completed)
            var missedDeadlineSurveys = allSurveys
                .Where(s => s.DueDate.HasValue && s.DueDate.Value.Date < today && s.SurveyStatus != "Completed")
                .ToList();
            
            // Apply filters if provided
            var surveys = allSurveys;
            if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(value))
            {
                surveys = filter.ToLower() switch
                {
                    "status" => allSurveys.Where(s => s.SurveyStatus == value).ToList(),
                    "region" => allSurveys.Where(s => s.RegionName == value).ToList(),
                    "type" => allSurveys.Where(s => s.ImplementationType == value).ToList(),
                    "missed" => missedDeadlineSurveys,
                    _ => allSurveys
                };
            }
            
            // Build dashboard view model
            var model = new DashboardViewModel
            {
                UserName = userName,
                UserRoleId = roleId,
                CanCreate = surveyRights?.IsCreate ?? false,
                CanUpdate = surveyRights?.IsUpdate ?? false,
                CanView = surveyRights?.IsView ?? false,
                
                // Overall statistics
                TotalSurveys = allSurveys.Count,
                CompletedSurveys = allSurveys.Count(s => s.SurveyStatus == "Completed"),
                InProgressSurveys = allSurveys.Count(s => s.SurveyStatus == "In Progress"),
                PendingSurveys = allSurveys.Count(s => s.SurveyStatus == "Pending"),
                OnHoldSurveys = allSurveys.Count(s => s.SurveyStatus == "On Hold"),
                AssignedSurveys = allSurveys.Count(s => s.SurveyStatus == "Assigned"),
                MissedDeadlineSurveys = missedDeadlineSurveys.Count,
                
                // Status breakdown
                SurveysByStatus = allSurveys
                    .GroupBy(s => s.SurveyStatus ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Region breakdown
                SurveysByRegion = allSurveys
                    .Where(s => !string.IsNullOrEmpty(s.RegionName))
                    .GroupBy(s => s.RegionName!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Implementation type breakdown
                SurveysByImplementationType = allSurveys
                    .Where(s => !string.IsNullOrEmpty(s.ImplementationType))
                    .GroupBy(s => s.ImplementationType!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Recent surveys (filtered list or last 10)
                RecentSurveys = surveys
                    .OrderByDescending(s => s.SurveyId)
                    .Take(100)
                    .ToList(),
                
                // Monthly trends (last 6 months)
                MonthlySurveyCount = allSurveys
                    .Where(s => s.SurveyDate.HasValue && s.SurveyDate.Value >= DateTime.Now.AddMonths(-6))
                    .GroupBy(s => s.SurveyDate!.Value.ToString("MMM yyyy"))
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                MonthlyCompletionCount = allSurveys
                    .Where(s => s.SurveyDate.HasValue && s.SurveyDate.Value >= DateTime.Now.AddMonths(-6) && s.SurveyStatus == "Completed")
                    .GroupBy(s => s.SurveyDate!.Value.ToString("MMM yyyy"))
                    .ToDictionary(g => g.Key, g => g.Count()),
                
                // Performance metrics
                TotalLocations = allSurveys.Count * 3,
                CompletedLocations = allSurveys.Count(s => s.SurveyStatus == "Completed") * 3,
                PendingLocations = allSurveys.Count(s => s.SurveyStatus != "Completed") * 3,
                
                // Alerts
                OverdueSurveys = missedDeadlineSurveys.Count,
                DueSoon = allSurveys.Count(s => s.SurveyStatus == "In Progress"),
                
                // Team statistics
                ActiveTeamMembers = _adminRepo.GetEmpMaster().Count,
                TotalAssignments = allSurveys.Count(s => s.DueDate.HasValue)
            };
            
            // Calculate completion rate
            model.CompletionRate = model.TotalSurveys > 0 
                ? Math.Round((decimal)model.CompletedSurveys / model.TotalSurveys * 100, 1) 
                : 0;
            
            // Pass filter info to view
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentValue = value;
            ViewBag.IsFiltered = !string.IsNullOrEmpty(filter);
            
            return View(model);
        }
    }
}
