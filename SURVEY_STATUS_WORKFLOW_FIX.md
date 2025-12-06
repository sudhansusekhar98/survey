# Survey Status Workflow Fix

## Issue Summary

The survey submission and approval workflow had incorrect status transitions:

1. ❌ Survey status was set to "Completed" immediately upon submission (should be "Submitted")
2. ❌ Approval didn't update the survey status to "Completed"
3. ❌ Rejection didn't update the survey status to "In Progress"
4. ❌ UnlockSurvey action method was missing, causing errors when users tried to unlock surveys

## Correct Workflow (After Fix)

### 1. Survey Submission Flow

```
Created → Assigned → In Progress → Submitted → (Approved) Completed
                                            ↓
                                       (Rejected) In Progress
```

### 2. Status Transitions

| Action                 | Old Status → New Status                   | Description                                    |
| ---------------------- | ----------------------------------------- | ---------------------------------------------- |
| **Submit Survey**      | In Progress → ~~Completed~~ **Submitted** | Survey submitted for review (not yet approved) |
| **Approve Submission** | Submitted → **Completed**                 | Admin/Creator approves survey                  |
| **Reject Submission**  | Submitted → **In Progress**               | Admin/Creator rejects, allows re-editing       |
| **Unlock Survey**      | Submitted → **In Progress**               | User withdraws submission to make changes      |

## Changes Made

### 1. File: `Repo/SurveyRepo.cs` (Line 978)

**What Changed:**

- Changed survey status update from "Completed" to "Submitted" on submission

**Before:**

```csharp
// Update survey status to Completed
using var updateCmd = new SqlCommand("UPDATE Survey SET SurveyStatus = 'Completed' WHERE SurveyId = @SurveyId", con);
```

**After:**

```csharp
// Update survey status to Submitted (not Completed - completion happens on approval)
using var updateCmd = new SqlCommand("UPDATE Survey SET SurveyStatus = 'Submitted' WHERE SurveyId = @SurveyId", con);
```

**Impact:**

- ✅ Survey status correctly reflects "Submitted" state awaiting approval
- ✅ Users can see pending submissions in correct state
- ✅ Dashboard/reports show accurate survey status

---

### 2. File: `Repo/SurveySubmissionRepo.cs` (Lines 34-78)

**What Changed:**

- Enhanced `UpdateReviewStatus()` to update Survey table status based on approval/rejection
- Added transaction support to ensure both SurveySubmission and Survey tables update together

**Before:**

```csharp
public bool UpdateReviewStatus(Int64 submissionId, string status, int reviewedBy, string? reviewComments)
{
    // Only updated SurveySubmission table
    // Did NOT update Survey table status
}
```

**After:**

```csharp
public bool UpdateReviewStatus(Int64 submissionId, string status, int reviewedBy, string? reviewComments)
{
    // Uses transaction to update both tables
    // 1. Updates SurveySubmission table (submission status)
    // 2. Updates Survey table:
    //    - Status = "Completed" when Approved
    //    - Status = "In Progress" when Rejected
}
```

**Implementation Details:**

```csharp
// Get SurveyId from submission
using var getSurveyCmd = new SqlCommand("SELECT SurveyId FROM SurveySubmission WHERE SubmissionId = @SubmissionId", con, transaction);
var surveyId = getSurveyCmd.ExecuteScalar();

// Update Survey table status based on approval/rejection
string surveyStatus = status == "Approved" ? "Completed" : "In Progress";
using var updateSurveyCmd = new SqlCommand("UPDATE Survey SET SurveyStatus = @SurveyStatus WHERE SurveyId = @SurveyId", con, transaction);
```

**Impact:**

- ✅ Approved surveys correctly marked as "Completed"
- ✅ Rejected surveys reverted to "In Progress" for re-editing
- ✅ Atomic transaction ensures data consistency
- ✅ Survey list shows correct status after review

---

### 3. File: `Controllers/SurveyCreationController.cs` (Lines 1292-1336)

**What Changed:**

- Added new `UnlockSurvey()` action method to handle survey unlocking

**New Method Added:**

```csharp
/// <summary>
/// Unlock a survey to allow editing after submission
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult UnlockSurvey(long surveyId)
{
    // 1. Check user is logged in
    // 2. Verify survey has submission record
    // 3. Prevent unlocking approved surveys
    // 4. Call WithdrawSubmission() to unlock and set status to "In Progress"
    // 5. Return JSON response with success/error message
}
```

**Functionality:**

- ✅ Validates user session
- ✅ Checks survey submission exists
- ✅ Prevents unlocking approved surveys (maintains data integrity)
- ✅ Calls existing `WithdrawSubmission()` method to handle unlock logic
- ✅ Provides user-friendly success/error messages
- ✅ Sets TempData for UI notifications

**Impact:**

- ✅ Fixes "UnlockSurvey not found" error
- ✅ Users can unlock submitted surveys to make changes
- ✅ Survey status correctly changes to "In Progress" when unlocked
- ✅ Prevents data corruption (can't unlock approved surveys)

---

## Testing Checklist

### Test Case 1: Survey Submission

- [ ] Create and complete a survey
- [ ] Submit the survey
- [ ] **Verify:** Survey status = "Submitted" (NOT "Completed")
- [ ] **Verify:** Survey appears in "Pending Reviews" for creator/admin

### Test Case 2: Survey Approval

- [ ] Navigate to pending submissions (as creator or super admin)
- [ ] Approve a submitted survey
- [ ] **Verify:** Survey status changes from "Submitted" → "Completed"
- [ ] **Verify:** Submitter receives approval email
- [ ] **Verify:** Survey no longer editable

### Test Case 3: Survey Rejection

- [ ] Navigate to pending submissions (as creator or super admin)
- [ ] Reject a submitted survey with reason
- [ ] **Verify:** Survey status changes from "Submitted" → "In Progress"
- [ ] **Verify:** Submitter receives rejection email with reason
- [ ] **Verify:** Survey is unlocked and editable
- [ ] **Verify:** User can make changes and resubmit

### Test Case 4: Survey Unlock

- [ ] Submit a survey
- [ ] Click "Locations" button on submitted survey
- [ ] Confirm unlock action
- [ ] **Verify:** No error occurs (UnlockSurvey action exists)
- [ ] **Verify:** Survey status changes to "In Progress"
- [ ] **Verify:** Survey is editable
- [ ] **Verify:** User can add/edit locations

### Test Case 5: Prevent Unlocking Approved Surveys

- [ ] Approve a survey
- [ ] Try to unlock the approved survey
- [ ] **Verify:** Error message: "Cannot unlock an approved survey"
- [ ] **Verify:** Survey remains locked

## Database Impact

### Tables Affected

1. **SurveySubmission** (existing behavior maintained)
   - SubmissionStatus values: Draft, Submitted, Approved, Rejected
2. **Survey** (NEW: status now updated by review actions)
   - SurveyStatus values: Created, Assigned, In Progress, Submitted, Completed

### Data Consistency

- Both tables updated in same transaction (atomic operation)
- If submission update fails, survey status update rolls back
- Ensures no orphaned states

## User Experience Improvements

### Before Fix

- ❌ Confusion: Survey marked "Completed" before approval
- ❌ Wrong counts in dashboard (completed vs pending)
- ❌ Error when clicking "Locations" on submitted survey
- ❌ Rejected surveys remained locked

### After Fix

- ✅ Clear status progression: Submitted → Approved → Completed
- ✅ Accurate dashboard metrics
- ✅ Unlock button works correctly
- ✅ Rejected surveys automatically unlock for editing
- ✅ Proper workflow enforcement

## Authorization & Security

### Unchanged Security Rules

- Only survey creator and super admin (RoleId 100) can approve/reject
- User must be logged in to unlock surveys
- Cannot unlock approved surveys (prevents tampering with completed work)

### No Security Issues Introduced

- All existing authorization checks remain in place
- Transaction rollback prevents partial updates
- Validation prevents invalid state transitions

## Backwards Compatibility

### Existing Surveys

- No migration needed for existing data
- Workflow applies to all future submissions
- Existing "Completed" surveys remain unchanged

### API Compatibility

- All existing endpoints work as before
- No breaking changes to method signatures
- Enhanced functionality only (additive changes)

## Related Files (Not Modified)

- `Controllers/SurveySubmissionController.cs` - Approval/rejection logic (unchanged)
- `Views/SurveyCreation/Index.cshtml` - UI for unlock button (already exists)
- `wwwroot/js/survey-index.js` - Frontend unlock functionality (already exists)

## Deployment Notes

### Prerequisites

- None (code-only changes)

### Deployment Steps

1. Deploy updated DLL files
2. Restart application pool
3. Test workflow with test survey
4. Monitor for any errors

### Rollback Plan

If issues occur, restore previous versions of:

- `Repo/SurveyRepo.cs`
- `Repo/SurveySubmissionRepo.cs`
- `Controllers/SurveyCreationController.cs`

## Support & Troubleshooting

### Common Issues

**Issue:** Survey still showing as "Completed" after submission

- **Cause:** Old code cached
- **Fix:** Restart application, clear browser cache

**Issue:** Unlock button not working

- **Cause:** CSRF token missing
- **Fix:** Ensure `@Html.AntiForgeryToken()` in view

**Issue:** Transaction timeout

- **Cause:** Database connection issue
- **Fix:** Check connection string, database availability

---

## Summary

All three issues have been resolved:

1. ✅ **Survey Submission:** Status correctly set to "Submitted" (not "Completed")
2. ✅ **Survey Approval:** Status updated to "Completed" when approved
3. ✅ **Survey Rejection:** Status updated to "In Progress" when rejected, allowing re-editing
4. ✅ **Survey Unlock:** UnlockSurvey action method added, prevents errors

The workflow now correctly reflects the approval process and allows users to edit rejected or unlocked surveys.
