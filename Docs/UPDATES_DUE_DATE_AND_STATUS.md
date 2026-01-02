# Survey Submission and Due Date Enhancement

## Changes Summary

### 1. Survey Status Update on Submission ✅

**File: `Repo/SurveyRepo.cs`**
- Modified `SubmitSurvey()` method to update survey status to "Completed" after successful submission
- Now performs two operations:
  1. Inserts submission record via `SpSurveySubmission` stored procedure
  2. Updates `Survey.SurveyStatus` to "Completed"

**Behavior:**
- When user submits a survey, the status automatically changes to "Completed"
- This reflects in survey cards, dashboard, and all listing views

---

### 2. Due Date Display on Survey Cards ✅

**Files Modified:**
- `Models/SurveyModel.cs` - Added `DueDate` property
- `Repo/SurveyRepo.cs` - Updated `GetAllSurveys()` to fetch minimum DueDate from SurveyAssignment table
- `Views/SurveyCreation/Index.cshtml` - Added due date display with overdue indicator

**Features:**
- **DueDate Source**: Fetches the earliest (MIN) due date from `SurveyAssignment` table for each survey
- **Visual Indicators**:
  - Shows due date with calendar icon
  - **Blue badge** for surveys with future due dates
  - **Red badge with warning icon** for overdue surveys (past due date and status ≠ "Completed")
  
**Display Logic:**
```csharp
var isOverdue = survey.DueDate.Value < DateTime.Now && survey.SurveyStatus != "Completed";
```

---

### 3. Missed Deadline Parameter in Dashboard ✅

**File: `Controllers/DashboardController.cs`**
- Updated missed deadline logic to use `DueDate` from survey model
- Simplified calculation by removing dependency on separate assignment queries

**Dashboard Features:**
- **Missed Deadline Count**: Shows surveys with `DueDate < Today` and status ≠ "Completed"
- **Clickable Filter**: Users can click "Missed Deadline" parameter to see all overdue surveys
- **Real-time Updates**: Dashboard automatically reflects survey status changes

**Query Logic:**
```csharp
var missedDeadlineSurveys = allSurveys
    .Where(s => s.DueDate.HasValue && s.DueDate.Value.Date < today && s.SurveyStatus != "Completed")
    .ToList();
```

---

## Database Schema Updates

### SurveyModel - New Property
```csharp
[Display(Name = "Due Date")]
public DateTime? DueDate { get; set; }
```

### SQL Query Enhancement
The `GetAllSurveys()` method now uses:
```sql
SELECT s.*, r.RegionName,
       (SELECT MIN(DueDate) FROM SurveyAssignment WHERE SurveyID = s.SurveyId) AS DueDate
FROM Survey s
LEFT JOIN RegionMaster r ON s.RegionID = r.RegionID
WHERE (@CreatedBy IS NULL OR s.CreatedBy = @CreatedBy)
ORDER BY s.SurveyId DESC
```

This automatically populates the `DueDate` field from the earliest assignment.

---

## User Experience Improvements

### Survey Card Display
Each survey card now shows:
- Survey ID and Status badge
- Type, Date, Team, Location, Region
- **NEW: Due Date** (if assigned)
  - Format: `dd-MMM-yyyy`
  - Color-coded badge
  - Warning icon for overdue items

### Dashboard Enhancements
- Missed deadline count in metrics
- Clickable filter to view all overdue surveys
- Visual distinction between on-time and overdue surveys

### Status Flow
```
Survey Created → Assigned (DueDate set) → In Progress → Submit → Status: "Completed"
                                                              ↓
                                                    Removes from "Missed" list
```

---

## Testing Checklist

- [ ] Create a survey and assign with due date in the past
- [ ] Verify survey card shows red badge with warning icon
- [ ] Check dashboard shows correct count in "Missed Deadline" parameter
- [ ] Click "Missed Deadline" parameter - should filter to show only overdue surveys
- [ ] Submit the overdue survey (complete all locations first)
- [ ] Verify survey status changes to "Completed"
- [ ] Confirm survey no longer appears in "Missed Deadline" filter
- [ ] Check dashboard count decreases by 1
- [ ] Test with survey that has future due date - should show blue badge
- [ ] Verify survey without assignment shows no due date

---

## Benefits

### For Users:
✅ Clear visibility of survey deadlines on cards
✅ Easy identification of overdue surveys
✅ Automatic status updates on submission
✅ Dashboard filtering by missed deadlines

### For Administrators:
✅ Quick overview of overdue surveys
✅ Tracking completion vs. deadlines
✅ Better project management
✅ Reduced manual status updates

### For System:
✅ Consistent status tracking
✅ Simplified deadline logic
✅ Reduced database queries (using subquery)
✅ Better data integrity

---

## Configuration

No additional configuration required. The feature automatically works when:
1. Survey has records in `SurveyAssignment` table with `DueDate`
2. Survey status is tracked in `Survey.SurveyStatus` column
3. User submits survey via the Submit button

---

## Notes

- **DueDate is optional**: Surveys without assignments show no due date
- **Multiple assignments**: System uses the earliest (MIN) due date
- **Completed surveys**: Never marked as "Missed" regardless of due date
- **Dashboard filter**: "Missed" parameter dynamically filters based on current date
- **Status change**: Happens automatically on successful submission
- **Time component**: Comparison uses date only (ignores time portion)

---

## Future Enhancements

- Email notifications for approaching due dates
- Reminder alerts 2-3 days before deadline
- Due date modification history
- Bulk extension of due dates
- Custom deadline colors (yellow for 3 days before)
- Export overdue survey reports

---

## Support

For any issues:
1. Check if `SurveyAssignment` table has `DueDate` values
2. Verify Survey table has `SurveyStatus` column
3. Ensure `SpSurveySubmission` stored procedure exists
4. Check browser console for JavaScript errors
5. Review server logs for SQL exceptions
