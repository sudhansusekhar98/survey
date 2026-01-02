# Camera Remarks Feature - Corrected Implementation

## Overview

Camera remarks feature that **automatically captures deployment direction for NEW cameras only** when users increase the new camera quantity during survey data entry.

## Corrected Requirements

### Key Changes from Original:

1. ✅ **Auto-trigger modal when NEW camera quantity increases** (not manual button click)
2. ✅ **Only for NEW cameras** (ItemQtyReq field), NOT existing cameras
3. ✅ **Camera remarks displayed in Detailed Reports** under new section
4. ✅ **Section only appears if camera devices exist in survey**

## User Workflow

### Automatic Popup Behavior:

```
User Action: Changes "New Devices (To Deploy)" from 0 → 3
System Response: Modal AUTOMATICALLY opens with 3 remark fields
```

### Step-by-Step:

1. User selects Camera device (ItemID = 100)
2. User increases "New Devices" quantity field
3. **Modal auto-opens** showing fields for NEW cameras only
4. User enters deployment direction for each new camera
5. Remarks saved and displayed on form
6. On submit, saved to database
7. **Visible in Detailed Report** under "Camera Deployment Direction Remarks"

## Implementation Changes

### JavaScript Updates (`camera-remarks.js`):

**Before:**

- Manual button click to open modal
- Fields = Existing + New cameras
- No auto-trigger

**After:**

```javascript
// Auto-trigger on NEW camera quantity increase
document.addEventListener("input", function (e) {
  if (
    e.target.classList.contains("cam-qty-req") &&
    e.target.dataset.type === "req"
  ) {
    const newQty = parseInt(e.target.value || 0);
    const previousQty = previousNewCameraQty[itemIndex] || 0;

    // Auto-open modal when NEW camera quantity INCREASED
    if (newQty > previousQty && newQty > 0) {
      setTimeout(() => openCameraRemarksModal(itemIndex, 100), 300);
    }
  }
});
```

**Key Function:**

```javascript
function openCameraRemarksModal(itemIndex, itemId) {
  // Get NEW camera quantity ONLY (not existing)
  const newQty = parseInt(card.querySelector(".cam-qty-req")?.value || 0);

  // Generate fields ONLY for new cameras
  generateCameraRemarkFields(newQty, itemIndex);
}
```

### View Updates (`ItemMasterSelection.cshtml`):

**Labels Changed:**

- "Camera Direction/Remarks" → "New Camera Deployment Direction"
- "Manage Camera Remarks" → "Manage New Camera Remarks"
- "Camera Directions:" → "New Camera Directions:"

**Modal Title:**

- "Camera Direction/Deployment Remarks" → "New Camera Deployment Remarks"

**Instructions:**

- Old: "The number of fields will match total quantity (Existing + New)"
- New: "The number of fields will match the quantity of new cameras to be deployed"

### Report Display (`DetailedReport.cshtml`):

**New Section Added:**

```html
@if (ViewBag.HasCameraDevices == true) {
<!-- Camera Deployment Direction Remarks Section -->
<div class="card shadow-sm">
  <div class="card-header bg-gradient-info">
    <i class="bi bi-camera-video"></i>
    Camera Deployment Direction Remarks
  </div>
  <div class="card-body">
    @foreach (var locationRemarks in cameraRemarks) {
    <h6>@locName</h6>
    @foreach (var remark in locationRemarks.Value) {
    <span class="badge bg-info">Camera @index</span>
    <i class="bi bi-arrow-right-circle"></i>@remark.Remarks } }
  </div>
</div>
}
```

### Controller Updates (`SurveyReportsController.cs`):

**Dependency Injection:**

```csharp
private readonly ISurveyCamRemarks _camRemarksRepo;

public SurveyReportsController(
    ISurvey surveyRepo,
    IAdmin adminRepo,
    ISurveySubmission submissionRepo,
    ISurveyCamRemarks camRemarksRepo)
{
    _camRemarksRepo = camRemarksRepo;
}
```

**DetailedReport Action:**

```csharp
// Check if survey has camera devices
bool hasCameraDevices = dtSurveyItems.Rows
    .Cast<DataRow>()
    .Any(row => row["ItemID"]?.ToString() == "100");

// Fetch camera remarks only if cameras exist
if (hasCameraDevices)
{
    var cameraRemarks = new Dictionary<string, List<SurveyCamRemarksModel>>();
    // ... fetch remarks for each location
    ViewBag.CameraRemarks = cameraRemarks;
}

ViewBag.HasCameraDevices = hasCameraDevices;
```

## Testing Checklist

### Form Behavior:

- [ ] Modal auto-opens when NEW camera quantity is increased
- [ ] Modal does NOT open when existing camera quantity changes
- [ ] Number of fields = NEW camera quantity (not total)
- [ ] Validation prevents saving with empty fields
- [ ] Remarks display correctly after saving
- [ ] Manual button click also opens modal

### Data Persistence:

- [ ] Remarks saved to SurveyCamRemarks table
- [ ] Existing remarks load when editing
- [ ] Updates replace old remarks correctly
- [ ] Multiple cameras per location handled

### Report Display:

- [ ] Camera remarks section appears ONLY if cameras exist
- [ ] Section does NOT appear for surveys without cameras
- [ ] Remarks grouped by location correctly
- [ ] Formatting matches design (badge + arrow + text)

## Files Modified

1. `wwwroot/js/camera-remarks.js` - Auto-trigger logic, NEW cameras only
2. `Views/SurveyDetails/ItemMasterSelection.cshtml` - Updated labels and text
3. `Controllers/SurveyReportsController.cs` - Injected ISurveyCamRemarks, fetch remarks
4. `Views/SurveyReports/DetailedReport.cshtml` - Added camera remarks section

## Database Schema

```sql
TABLE: SurveyCamRemarks
- TransID (PK, Identity)
- SurveyID (Int64)
- LocID (Int)
- ItemID (Int) -- Always 100 for cameras
- Remarks (NVarChar 500) -- Deployment direction
- CreatedBy (Int)
- CreatedOn (DateTime, default GETDATE())
```

## Example Scenario

### Data Entry:

```
Location: Main Building
Camera - New Devices: 3

AUTO-POPUP APPEARS:
┌─────────────────────────────────────┐
│  New Camera Deployment Remarks      │
├─────────────────────────────────────┤
│ New Camera 1: [Towards temple    ] │
│ New Camera 2: [Main entrance     ] │
│ New Camera 3: [Parking area      ] │
│                    [Save] [Cancel]  │
└─────────────────────────────────────┘
```

### Report Display:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 📹 Camera Deployment Direction Remarks
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 📍 Main Building
   [Camera 1] → Towards temple
   [Camera 2] → Main entrance
   [Camera 3] → Parking area
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## Key Differences Summary

| Feature             | Old Behavior          | New Behavior                    |
| ------------------- | --------------------- | ------------------------------- |
| Trigger             | Manual button click   | Auto-opens on qty increase      |
| Cameras             | Existing + New        | NEW cameras only                |
| Field Count         | Total cameras         | New cameras only                |
| Label               | "Camera Remarks"      | "New Camera Deployment"         |
| Report Display      | Not implemented       | Shows in Detailed Report        |
| Conditional Display | Always for ItemID=100 | Only if cameras exist in survey |
