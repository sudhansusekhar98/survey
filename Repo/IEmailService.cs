namespace AnalyticaDocs.Repository
{
    public interface IEmailService
    {
        Task<bool> SendCanteenEmailAsync(string employeeName, string epin, string recipients);
        
        // Send survey submission notification to supervisor
        Task<bool> SendSurveySubmissionNotificationAsync(string supervisorName, string supervisorEmail, 
            string surveyName, string submittedBy, DateTime submissionDate);
        
        // Send survey approval notification to submitter
        Task<bool> SendSurveyApprovalNotificationAsync(string submitterName, string submitterEmail, 
            string surveyName, string reviewedBy, string reviewComments);
        
        // Send survey rejection notification to submitter
        Task<bool> SendSurveyRejectionNotificationAsync(string submitterName, string submitterEmail, 
            string surveyName, string reviewedBy, string reviewComments);

        // Send survey assignment notification to employee
        Task<bool> SendSurveyAssignmentNotificationAsync(string employeeName, string employeeEmail, 
            string surveyName, DateTime? dueDate);

        // Send new user account creation notification with temporary password
        Task<bool> SendNewUserAccountNotificationAsync(string userName, string userEmail, 
            string loginId, string temporaryPassword);

        // Send password reset notification with temporary password
        Task<bool> SendPasswordResetNotificationAsync(string userName, string userEmail, 
            string loginId, string temporaryPassword, string resetByName);
    }
}
