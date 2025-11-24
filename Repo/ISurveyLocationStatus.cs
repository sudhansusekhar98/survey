using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public interface ISurveyLocationStatus
    {
        /// <summary>
        /// Get status for a specific location in a survey
        /// </summary>
        SurveyLocationStatusModel? GetLocationStatus(long surveyId, int locId);
        
        /// <summary>
        /// Get all location statuses for a survey
        /// </summary>
        List<SurveyLocationStatusModel> GetSurveyLocationStatuses(long surveyId);
        
        /// <summary>
        /// Update or insert location status
        /// </summary>
        bool UpsertLocationStatus(long surveyId, int locId, string status, string? remarks, int userId);
        
        /// <summary>
        /// Mark location as completed
        /// </summary>
        bool MarkLocationAsCompleted(long surveyId, int locId, int userId, string? remarks = null);
        
        /// <summary>
        /// Mark location as in progress
        /// </summary>
        bool MarkLocationAsInProgress(long surveyId, int locId, int userId, string? remarks = null);
        
        /// <summary>
        /// Mark location as verified
        /// </summary>
        bool MarkLocationAsVerified(long surveyId, int locId, int userId, string? remarks = null);
        
        /// <summary>
        /// Delete location status
        /// </summary>
        bool DeleteLocationStatus(long surveyId, int locId);
    }
}
