# Survey Revision Feature - Implementation Plan

## Overview
Enable supervisors to assign revisions to previously submitted and approved surveys, creating new copies tagged as "Revised" while maintaining audit trails.

## Current System Analysis
- **SurveyModel**: Contains survey metadata (SurveyId, SurveyName, Status, Team, Client, etc.)
- **SurveySubmission**: Tracks submission status (Draft, Submitted, Approved, Rejected)
- **SurveyAssignment**: Links employees to surveys
- **SurveyDetails**: Contains device quantities per location
- **SurveyLocation**: Contains location information

## Database Changes Required

### 1. Add Revision Tracking Columns to Survey Table
```sql
ALTER TABLE Survey ADD
    IsRevised BIT DEFAULT 0,                    -- Flag to mark as revised survey
    OriginalSurveyId BIGINT NULL,               -- Link to original survey
    RevisionNumber INT DEFAULT 0,               -- Revision count (0 = original)
    RevisedFromSubmissionId BIGINT NULL,        -- Link to parent submission that triggered revision
    RevisionReason NVARCHAR(500) NULL,          -- Reason for revision
    RevisionAssignedBy INT NULL,                -- Supervisor who assigned revision
    RevisionAssignedDate DATETIME NULL          -- When revision was assigned
```

### 2. Create Revision Log Table
```sql
CREATE TABLE SurveyRevisionLog (
    RevisionLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    OriginalSurveyId BIGINT NOT NULL,           -- Original survey
    RevisedSurveyId BIGINT NOT NULL,            -- New revised survey
    RevisionNumber INT NOT NULL,
    RevisionReason NVARCHAR(500) NULL,
    AssignedBy INT NOT NULL,                    -- Supervisor UserID
    AssignedTo INT NULL,                        -- Primary team member assigned
    AssignedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CompletedDate DATETIME NULL,
    Status NVARCHAR(50) DEFAULT 'Assigned',     -- Assigned, InProgress, Completed
    Notes NVARCHAR(MAX) NULL,
    CreatedOn DATETIME DEFAULT GETDATE(),
    ModifiedOn DATETIME DEFAULT GETDATE()
)
```

### 3. Stored Procedure: SpSurveyRevision
Operations needed:
- **SpType 1**: Create Revision (complete workflow)
  1. Generate new SurveyId (YYYYMMDD### format)
  2. Copy Survey record with revision fields
  3. Copy Sub-Locations from SurveyLocation
  4. Copy Item Quantities from SurveyDetails (with LocID mapping)
  5. Create Revision Log Entry in SurveyRevisionLog
  6. Add Team Leader to SurveyAssignment
  7. Create SurveyLocationStatus entries (set to "In Progress")
  8. Return new SurveyId and RevisionNumber
- **SpType 2**: Get Revision History for a Survey
- **SpType 3**: Get All Revisions (for dashboard)
- **SpType 4**: Update Revision Status
- **SpType 5**: Get Original Survey Chain (trace back to root)
- **SpType 6**: Check if survey can be revised

---

## Model Changes

### 1. Update SurveyModel.cs
Add revision-related properties:
```csharp
public bool IsRevised { get; set; }
public long? OriginalSurveyId { get; set; }
public int RevisionNumber { get; set; }
public string? RevisionReason { get; set; }
public int? RevisionAssignedBy { get; set; }
public DateTime? RevisionAssignedDate { get; set; }
```

### 2. Create SurveyRevisionModel.cs
```csharp
public class SurveyRevisionModel
{
    public long RevisionLogId { get; set; }
    public long OriginalSurveyId { get; set; }
    public string? OriginalSurveyName { get; set; }
    public long RevisedSurveyId { get; set; }
    public string? RevisedSurveyName { get; set; }
    public int RevisionNumber { get; set; }
    public string? RevisionReason { get; set; }
    public int AssignedBy { get; set; }
    public string? AssignedByName { get; set; }
    public int? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Assigned";
    public string? Notes { get; set; }
}

public class CreateRevisionModel
{
    public long SurveyId { get; set; }
    public string? RevisionReason { get; set; }
    public List<int> AssignedTeamMembers { get; set; } = new();
    public DateTime? NewDueDate { get; set; }
}
```

---

## Repository Changes

### 1. Create ISurveyRevision.cs Interface
```csharp
public interface ISurveyRevision
{
    Task<(bool Success, long NewSurveyId, string Message)> CreateRevisionAsync(
        long surveyId, int assignedBy, string? reason, List<int> teamMembers, DateTime? dueDate);
    List<SurveyRevisionModel> GetRevisionHistory(long surveyId);
    List<SurveyRevisionModel> GetAllPendingRevisions();
    bool UpdateRevisionStatus(long revisionLogId, string status, string? notes);
    SurveyModel? GetOriginalSurvey(long surveyId);
}
```

### 2. Create SurveyRevisionRepo.cs
Implement all revision operations.

---

## Controller Changes

### 1. Add to SurveyCreationController
- `[HttpGet] AssignRevision(long surveyId)` - Show revision assignment form
- `[HttpPost] AssignRevision(CreateRevisionModel model)` - Create revision
- `[HttpGet] RevisionHistory(long surveyId)` - Show revision history

### 2. Add SupervisorController (or extend existing)
- `[HttpGet] PendingRevisions()` - Dashboard for pending revisions
- `[HttpGet] AllRevisions()` - Full revision audit log

---

## UI Changes

### 1. Survey Cards (Index.cshtml, MySubmissions.cshtml)
- Add "Revised" badge for revised surveys
- Show revision number (e.g., "Rev 2")
- Link to original survey

### 2. Approved Survey View
- Add "Assign Revision" button (visible to supervisors only)

### 3. New Views
- `Views/SurveyCreation/AssignRevision.cshtml` - Form to assign revision
- `Views/SurveyCreation/RevisionHistory.cshtml` - View revision chain
- `Views/SurveySubmission/PendingRevisions.cshtml` - Supervisor dashboard

### 4. Survey Detail View
- Show revision info banner if survey is a revision
- Link to view original and all revisions

---

## Workflow

### A. Supervisor Assigns Revision
1. Supervisor views approved survey
2. Clicks "Assign Revision"
3. Fills form:
   - Reason for revision
   - Select team members
   - Set new due date (optional)
4. System creates new survey copy:
   - Copies all locations, items, specifications
   - Sets `IsRevised = true`, `OriginalSurveyId`, `RevisionNumber`
   - Creates assignments for selected team members
   - Status set to "Assigned"
5. Creates entry in `SurveyRevisionLog`
6. Notification sent to assigned team members

### B. Team Member Works on Revision
1. Team member sees revision in "My Submissions" with "Revised" tag
2. Can edit device quantities, add/remove items
3. Submits for review (follows normal submission flow)

### C. Supervisor Reviews Revision
1. Reviews changes (normal approval flow)
2. Approves → Revision marked complete
3. Rejects → Can be re-revised if needed

---

## Implementation Order

### Phase 1: Database & Backend ✅ COMPLETED
1. [x] Create SQL script for table alterations (`SQL_Create_SurveyRevision.sql`)
2. [x] Create SurveyRevisionLog table
3. [x] Create stored procedure SpSurveyRevision
4. [x] Create SurveyRevisionModel.cs (includes CreateRevisionModel, RevisionResultModel, CanReviseCheckModel)
5. [x] Update SurveyModel.cs with revision fields
6. [x] Create ISurveyRevision interface
7. [x] Implement SurveyRevisionRepo.cs

### Phase 2: Controllers & Logic ✅ COMPLETED
1. [x] Add revision endpoints to SurveyCreationController
   - CanRevise(surveyId) - Check if survey can be revised
   - AssignRevision(surveyId) - GET: Show form / POST: Create revision
   - RevisionHistory(surveyId) - View revision chain
   - PendingRevisions() - Supervisor dashboard
   - UpdateRevisionStatus() - Update status
2. [x] Register services in Program.cs
3. [x] Implement survey copying logic in stored procedure

### Phase 3: UI ✅ COMPLETED
1. [x] Create AssignRevision.cshtml view
2. [x] Create RevisionHistory.cshtml view
3. [x] Create PendingRevisions.cshtml view
4. [x] Create CompletedSurveys.cshtml view (new dedicated view for completed/approved surveys)
5. [x] Update Index.cshtml - show "Revised" badge, filter out completed surveys
6. [x] Update navigation menu - add Completed Surveys and Pending Revisions (admin only)
7. [x] Add "Assign Revision" button to completed surveys view (admin only)

### Phase 4: Database Deployment ✅ COMPLETED
1. [x] Run SQL script on the database
2. [x] Create SurveyRevisionLog table with correct column types
3. [x] Create SpSurveyRevision stored procedure

### Phase 5: Testing 🔲 PENDING
1. [ ] Test revision creation
2. [ ] Test multi-level revisions
3. [ ] Test audit trail display
4. [ ] Add email notifications for revision assignments

---

## Acceptance Criteria
- [x] Supervisors can assign revisions to approved surveys
- [ ] New survey created as copy with "Revised" tag
- [ ] Team members can be assigned to revisions
- [ ] Multiple revisions of same survey are supported
- [ ] Clear audit trail maintained
- [ ] Revision history viewable for any survey
- [ ] UI clearly identifies revised surveys

---

## Notes
- Original survey remains locked after revision is created
- Each revision gets its own submission workflow
- Revision chain can be traced back to original
- Only supervisors (RoleId 101 or with appropriate rights) can assign revisions
