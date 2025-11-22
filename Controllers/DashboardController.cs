using Microsoft.AspNetCore.Mvc;
using SurveyApp.Repo;

namespace SurveyApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ISurvey _surveyRepo;

        public DashboardController(ISurvey surveyRepo)
        {
            _surveyRepo = surveyRepo;
        }

        public IActionResult Index()
        {
            // You can use session or a default userId for demo
            int userId = 0;
            if (HttpContext.Session.GetString("UserID") != null)
                int.TryParse(HttpContext.Session.GetString("UserID"), out userId);

            var surveys = _surveyRepo.GetAllSurveys(userId);
            var statusSummary = surveys
                .GroupBy(s => s.SurveyStatus ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate completion metrics
            var totalSurveys = surveys.Count;
            var completedSurveys = statusSummary.GetValueOrDefault("Completed", 0);
            var inProgressSurveys = statusSummary.GetValueOrDefault("In Progress", 0);
            var pendingSurveys = statusSummary.GetValueOrDefault("Pending", 0);
            var onHoldSurveys = statusSummary.GetValueOrDefault("On Hold", 0);
            
            var completionRate = totalSurveys > 0 ? (int)Math.Round((double)completedSurveys / totalSurveys * 100) : 0;
            
            // Calculate on-time vs delayed (assuming surveys with "Completed" status are on time, "On Hold" are delayed)
            var onTimeSurveys = completedSurveys;
            var delayedSurveys = onHoldSurveys;

            ViewBag.SurveyStatusSummary = statusSummary;
            ViewBag.TotalSurveys = totalSurveys;
            ViewBag.CompletedSurveys = completedSurveys;
            ViewBag.InProgressSurveys = inProgressSurveys;
            ViewBag.PendingSurveys = pendingSurveys;
            ViewBag.OnHoldSurveys = onHoldSurveys;
            ViewBag.CompletionRate = completionRate;
            ViewBag.OnTimeSurveys = onTimeSurveys;
            ViewBag.DelayedSurveys = delayedSurveys;
            
            return View();
        }
    }
}
