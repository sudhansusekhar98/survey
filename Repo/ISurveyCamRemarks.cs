using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public interface ISurveyCamRemarks
    {
        /// <summary>
        /// Save or update camera remarks for a specific camera
        /// </summary>
        bool SaveCameraRemarks(SurveyCamRemarksModel model);
        
        /// <summary>
        /// Get all camera remarks for a survey location and item
        /// </summary>
        List<SurveyCamRemarksModel> GetCameraRemarks(Int64 surveyId, int locId, int itemId);
        
        /// <summary>
        /// Get camera remarks by TransID
        /// </summary>
        SurveyCamRemarksModel? GetCameraRemarkById(int transId);
        
        /// <summary>
        /// Delete camera remark
        /// </summary>
        bool DeleteCameraRemark(int transId);
        
        /// <summary>
        /// Delete all camera remarks for a survey location
        /// </summary>
        bool DeleteAllCameraRemarks(Int64 surveyId, int locId, int itemId);
        
        /// <summary>
        /// Get formatted camera remarks for display
        /// </summary>
        string GetFormattedCameraRemarks(Int64 surveyId, int locId, int itemId);
        
        /// <summary>
        /// Get all camera remarks for a survey location (all items)
        /// </summary>
        List<SurveyCamRemarksModel> GetCameraRemarksByLocation(Int64 surveyId, int locId);
    }
}
