namespace AnalyticaDocs.Repository
{
    public interface IEmailService
    {
        Task<bool> SendCanteenEmailAsync(string employeeName, string epin, string recipients);
        
        /// <summary>
        /// Send survey submission notification to supervisor
        /// </summary>
        Task<bool> SendSurveySubmissionNotificationAsync(string supervisorName, string supervisorEmail, 
            string surveyName, string submittedBy, DateTime submissionDate);
        
        /// <summary>
        /// Send survey approval notification to submitter
        /// </summary>
        Task<bool> SendSurveyApprovalNotificationAsync(string submitterName, string submitterEmail, 
            string surveyName, string reviewedBy, string reviewComments);
        
        /// <summary>
        /// Send survey rejection notification to submitter
        /// </summary>
        Task<bool> SendSurveyRejectionNotificationAsync(string submitterName, string submitterEmail, 
            string surveyName, string reviewedBy, string reviewComments);
    }
}
