using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;

namespace SurveyApp.Controllers
{
    public class SurveyCamRemarksController : Controller
    {
        private readonly ISurveyCamRemarks _camRemarksRepo;

        public SurveyCamRemarksController(ISurveyCamRemarks camRemarksRepo)
        {
            _camRemarksRepo = camRemarksRepo;
        }

        /// <summary>
        /// Save camera remarks (called via AJAX)
        /// </summary>
        [HttpPost]
        public IActionResult SaveCameraRemarks([FromBody] SurveyCamRemarksModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                model.CreatedBy = int.Parse(userId);
                model.CreatedOn = DateTime.Now;

                bool result = _camRemarksRepo.SaveCameraRemarks(model);

                if (result)
                {
                    return Json(new { success = true, message = "Camera remarks saved successfully" });
                }

                return Json(new { success = false, message = "Failed to save camera remarks" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get camera remarks for display
        /// </summary>
        [HttpGet]
        public IActionResult GetCameraRemarks(Int64 surveyId, int locId, int itemId)
        {
            try
            {
                var remarks = _camRemarksRepo.GetCameraRemarks(surveyId, locId, itemId);
                return Json(new { success = true, data = remarks });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get formatted camera remarks
        /// </summary>
        [HttpGet]
        public IActionResult GetFormattedRemarks(Int64 surveyId, int locId, int itemId)
        {
            try
            {
                var formatted = _camRemarksRepo.GetFormattedCameraRemarks(surveyId, locId, itemId);
                return Json(new { success = true, remarks = formatted });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete camera remark
        /// </summary>
        [HttpPost]
        public IActionResult DeleteCameraRemark(int transId)
        {
            try
            {
                bool result = _camRemarksRepo.DeleteCameraRemark(transId);

                if (result)
                {
                    return Json(new { success = true, message = "Camera remark deleted successfully" });
                }

                return Json(new { success = false, message = "Failed to delete camera remark" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Save multiple camera remarks at once
        /// </summary>
        [HttpPost]
        public IActionResult SaveMultipleCameraRemarks([FromBody] List<SurveyCamRemarksModel> remarks)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                int userIdInt = int.Parse(userId);
                bool allSuccess = true;

                foreach (var remark in remarks)
                {
                    remark.CreatedBy = userIdInt;
                    remark.CreatedOn = DateTime.Now;
                    
                    bool result = _camRemarksRepo.SaveCameraRemarks(remark);
                    if (!result)
                    {
                        allSuccess = false;
                    }
                }

                if (allSuccess)
                {
                    return Json(new { success = true, message = "All camera remarks saved successfully" });
                }

                return Json(new { success = false, message = "Some camera remarks failed to save" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
