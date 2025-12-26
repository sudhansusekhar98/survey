using SurveyApp.Models;

namespace SurveyApp.Repo
{
    /// <summary>
    /// Interface for Survey Revision operations
    /// </summary>
    public interface ISurveyRevision
    {
        /// <summary>
        /// Create a new revision of an approved survey
        /// </summary>
        /// <param name="surveyId">Original survey ID to revise</param>
        /// <param name="assignedBy">User ID of the supervisor assigning the revision</param>
        /// <param name="reason">Reason for the revision</param>
        /// <param name="teamMembers">List of employee IDs to assign</param>
        /// <param name="dueDate">Optional new due date</param>
        /// <returns>Tuple containing success flag, new survey ID, and message</returns>
        Task<RevisionResultModel> CreateRevisionAsync(
            long surveyId, 
            int assignedBy, 
            string? reason, 
            List<int> teamMembers, 
            DateTime? dueDate);

        /// <summary>
        /// Get the revision history for a survey (including all revisions of the original)
        /// </summary>
        /// <param name="surveyId">Any survey ID in the revision chain</param>
        /// <returns>List of revision log entries</returns>
        List<SurveyRevisionModel> GetRevisionHistory(long surveyId);

        /// <summary>
        /// Get all pending revisions (for supervisor dashboard)
        /// </summary>
        /// <returns>List of pending revision entries</returns>
        List<SurveyRevisionModel> GetAllPendingRevisions();

        /// <summary>
        /// Update the status of a revision
        /// </summary>
        /// <param name="revisionLogId">Revision log ID</param>
        /// <param name="status">New status (Assigned, InProgress, Completed, Cancelled)</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>True if successful</returns>
        bool UpdateRevisionStatus(long revisionLogId, string status, string? notes);

        /// <summary>
        /// Check if a survey can be revised
        /// </summary>
        /// <param name="surveyId">Survey ID to check</param>
        /// <returns>Check result with reason if not allowed</returns>
        CanReviseCheckModel CanRevise(long surveyId);

        /// <summary>
        /// Get the original (root) survey of a revision chain
        /// </summary>
        /// <param name="surveyId">Any survey ID in the chain</param>
        /// <returns>Root survey or null</returns>
        SurveyModel? GetOriginalSurvey(long surveyId);
    }
}
