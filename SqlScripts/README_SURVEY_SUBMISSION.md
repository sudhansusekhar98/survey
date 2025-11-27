# Survey Submission and Status Tracking Feature

## Overview
This implementation adds a complete survey submission and approval workflow with edit restrictions.

## Database Setup

### 1. Run the SQL Script
Execute the `SQL_CreateSurveySubmissionTable.sql` file in your database to create:
- **SurveySubmission** table
- **SpSurveySubmission** stored procedure

```sql
-- Run this script in SQL Server Management Studio or Azure Data Studio
-- File: SQL_CreateSurveySubmissionTable.sql
```

## Features Implemented

### 1. Survey Submission
- **Submit Button**: Added "Submit" button on survey cards in Index view
- **Confirmation Dialog**: Users must confirm before submitting
- **Status Tracking**: Survey status tracked in `SurveySubmission` table
- **Lock Mechanism**: Once submitted, survey is locked for editing

### 2. Edit Restrictions
- **Edit Check**: Before editing, system checks if survey is locked
- **Smart Edit Button**: Edit button now checks submission status
- **User Feedback**: Clear messages when edit is not allowed

### 3. Submission Statuses
- **Draft**: Initial state, can be edited
- **Submitted**: Survey submitted, locked for editing
- **Approved**: Reviewed and approved by manager
- **Rejected**: Rejected with comments, unlocked for re-editing

### 4. Submissions List Page
**URL**: `/SurveyCreation/SubmissionsList`

Features:
- View all submissions (admins) or your submissions (users)
- Filter by status
- Approve/Reject submissions
- Add review comments
- Withdraw submissions
- View survey details

## User Roles

### Regular Users (RoleId > 2)
- Can submit their own surveys
- Can view their own submissions
- Can withdraw draft submissions
- Cannot edit submitted surveys

### Admins/Managers (RoleId <= 2)
- Can view all submissions
- Can approve/reject submissions
- Can add review comments
- Full access to all surveys

## Usage Flow

### For Survey Creators:
1. Create and complete survey data
2. Click "Submit" button on survey card
3. Confirm submission
4. Survey is now locked for editing
5. Wait for review

### For Reviewers:
1. Navigate to `/SurveyCreation/SubmissionsList`
2. Review submitted surveys
3. Click View to see survey details
4. Click Approve (✓) or Reject (✗)
5. Enter review comments
6. Status is updated and creator is notified

### Withdrawing Submissions:
1. Go to Submissions List
2. Find your submission
3. Click Withdraw button (↶)
4. Survey returns to Draft status
5. Can now edit the survey

## Database Schema

### SurveySubmission Table
```sql
- SubmissionId (bigint, PK, Identity)
- SurveyId (bigint, FK)
- SubmissionStatus (nvarchar(50)) - Draft, Submitted, Approved, Rejected
- SubmittedBy (int)
- SubmissionDate (datetime)
- ReviewedBy (int)
- ReviewDate (datetime)
- ReviewComments (nvarchar(max))
- IsLockedForEditing (bit)
- CreatedOn (datetime)
- ModifiedOn (datetime)
```

## API Endpoints

### POST `/SurveyCreation/SubmitSurvey`
Submit a survey for review
- Parameters: `surveyId`
- Returns: JSON success/error

### POST `/SurveyCreation/WithdrawSubmission`
Withdraw a submission
- Parameters: `surveyId`
- Returns: JSON success/error

### GET `/SurveyCreation/GetSubmissionStatus`
Get submission status for a survey
- Parameters: `surveyId`
- Returns: JSON with submission data and canEdit flag

### GET `/SurveyCreation/SubmissionsList`
View all submissions
- Returns: View with submissions table

### POST `/SurveyCreation/UpdateSubmissionStatus`
Update submission status (approve/reject)
- Parameters: `submissionId`, `status`, `comments`
- Returns: JSON success/error

## Files Modified/Created

### Models
- ✅ `Models/SurveySubmissionModel.cs` (NEW)

### Database
- ✅ `SQL_CreateSurveySubmissionTable.sql` (NEW)

### Repository
- ✅ `Repo/ISurvey.cs` (UPDATED - added 6 new methods)
- ✅ `Repo/SurveyRepo.cs` (UPDATED - implemented 6 new methods)

### Controllers
- ✅ `Controllers/SurveyCreationController.cs` (UPDATED - added 5 new actions)

### Views
- ✅ `Views/SurveyCreation/Index.cshtml` (UPDATED - added submit button and edit check)
- ✅ `Views/SurveyCreation/SubmissionsList.cshtml` (NEW)

## Navigation

Add link to navigation menu (in `_Layout.cshtml` or sidebar):
```html
<a asp-controller="SurveyCreation" asp-action="SubmissionsList" class="nav-link">
    <i class="bi bi-file-earmark-check me-2"></i>Submissions
</a>
```

## Testing Checklist

- [ ] Run SQL script successfully
- [ ] Submit a survey
- [ ] Verify survey is locked after submission
- [ ] Try editing locked survey (should show message)
- [ ] View submissions list
- [ ] Approve a submission as admin
- [ ] Reject a submission with comments
- [ ] Withdraw a submission
- [ ] Edit survey after withdrawal

## Troubleshooting

### Error: "Stored procedure not found"
- Run the SQL script in the correct database
- Verify connection string points to correct database

### Error: "Cannot edit survey"
- Check `SurveySubmission` table for entry
- Verify `IsLockedForEditing` column value
- Run `SpSurveySubmission` with SpType=6 to check

### Submissions not showing
- Check if user has correct RoleId in session
- Verify data exists in `SurveySubmission` table
- Check stored procedure SpType=4 returns data

## Future Enhancements
- Email notifications on status change
- Multi-level approval workflow
- Submission history log
- Dashboard widgets for pending approvals
- Export submission reports

## Support
For issues or questions, check:
1. Browser console for JavaScript errors
2. Application logs for server errors
3. SQL Server Profiler for database issues
