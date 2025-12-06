# Camera Remarks Feature - Implementation Summary

## Overview

This document describes the camera remarks feature implementation that captures camera direction/deployment information when users select cameras (ItemID = 100) during survey data entry.

## Feature Requirements

- Display a "Manage Camera Remarks" button when ItemID = 100 (Camera)
- Modal popup to capture remarks for each camera
- Number of remark fields = Existing Quantity + New Quantity
- Format: "#Cam1 Remarks: Towards temple, #Cam2 Remarks: Towards townhall"
- Store remarks in dedicated `SurveyCamRemarks` table
- Display formatted remarks on the form
- Load existing remarks when editing

## Database Configuration

**Connection String**: `Server=10.0.32.135;Database=VLDev;UID=adminrole;Password=@dminr0le`

**Table**: `SurveyCamRemarks`

- `TransID` (PK, Identity) - Auto-incrementing primary key
- `SurveyID` (Int64) - Foreign key to survey
- `LocID` (Int) - Location ID
- `ItemID` (Int) - Device type ID (100 for cameras)
- `Remarks` (NVarChar 500) - Camera direction/deployment text
- `CreatedBy` (Int) - User ID who created the remark
- `CreatedOn` (DateTime) - Timestamp (auto-populated by database)

## Implementation Files

### 1. Models/SurveyCamRemarksModel.cs

Entity model representing a camera remark record with validation attributes.

**Properties**:

- All table columns mapped as properties
- `[Required]` attributes on mandatory fields
- `[StringLength(500)]` on Remarks
- `CameraNumber` (int) - Helper property for UI display

### 2. Repo/ISurveyCamRemarks.cs

Repository interface defining camera remarks operations.

**Methods**:

```csharp
int SaveCameraRemarks(SurveyCamRemarksModel model);
List<SurveyCamRemarksModel> GetCameraRemarks(Int64 surveyId, int locId, int itemId);
SurveyCamRemarksModel GetCameraRemarkById(int transId);
int DeleteCameraRemark(int transId);
int DeleteAllCameraRemarks(Int64 surveyId, int locId, int itemId);
string GetFormattedCameraRemarks(Int64 surveyId, int locId, int itemId);
```

### 3. Repo/SurveyCamRemarksRepo.cs

Repository implementation with direct SQL connection.

**Key Features**:

- Uses specified connection string (Server=10.0.32.135)
- `SaveCameraRemarks`: Insert new remark, returns TransID
- `GetCameraRemarks`: Retrieve all remarks for survey/location/item
- `GetFormattedCameraRemarks`: Returns formatted string "#Cam1 Remarks: {text}\n#Cam2 Remarks: {text}"
- `DeleteAllCameraRemarks`: Clear all remarks before saving updated set
- Error handling with console logging

### 4. Controllers/SurveyCamRemarksController.cs

API controller for AJAX operations.

**Action Methods**:

- `SaveCameraRemarks(SurveyCamRemarksModel model)` [POST] - Save single remark
- `GetCameraRemarks(long surveyId, int locId, int itemId)` [GET] - Get remarks list
- `GetFormattedRemarks(long surveyId, int locId, int itemId)` [GET] - Get display format
- `DeleteCameraRemark(int transId)` [POST] - Delete single remark
- `SaveMultipleCameraRemarks(List<SurveyCamRemarksModel> models)` [POST] - Batch save

All methods validate user session and authorization.

### 5. Controllers/SurveyDetailsController.cs

Updated to process camera remarks during survey updates.

**Changes**:

- Added `using System.Text.Json` for JSON parsing
- Injected `ISurveyCamRemarks _camRemarksRepo` in constructor
- **UpdateItem GET**: Load existing camera remarks and populate `CameraRemarksJson` property
- **UpdateItem POST**:
  - Parse `CameraRemarksJson` from each item where ItemID = 100
  - Delete existing remarks via `DeleteAllCameraRemarks()`
  - Save new remarks via `SaveCameraRemarks()` for each camera
  - JSON parsing wrapped in try-catch to prevent update failure

### 6. Models/SurveyDetailsUpdate.cs

Added property to support camera remarks JSON storage.

**New Property**:

```csharp
public string? CameraRemarksJson { get; set; }
```

### 7. Views/SurveyDetails/ItemMasterSelection.cshtml

Updated view to display camera remarks UI elements.

**Additions**:

1. **Camera Remarks Section** (only for ItemID = 100):

   ```html
   @if (item.ItemID == 100) {
   <div class="camera-remarks-section" data-item-index="@i">
     <button type="button" class="manage-camera-remarks-btn">
       Manage Camera Remarks
     </button>
     <div class="camera-remarks-display">
       <div class="remarks-list"></div>
     </div>
     <input
       type="hidden"
       name="ItemLists[@i].CameraRemarksJson"
       class="camera-remarks-json"
       value="@item.CameraRemarksJson"
     />
   </div>
   }
   ```

2. **Camera Remarks Modal**:

   - Bootstrap 5 modal with dynamic field container
   - `cameraRemarksContainer` div populated by JavaScript
   - "Save Camera Remarks" button

3. **Script Reference**:
   ```html
   <script src="~/js/camera-remarks.js"></script>
   ```

### 8. wwwroot/js/camera-remarks.js

Client-side logic for camera remarks management.

**Key Functions**:

- **initCameraRemarks()**:

  - Initializes event listeners on page load
  - Handles "Manage Camera Remarks" button clicks
  - Listens to quantity changes to update display
  - Calls `loadExistingCameraRemarks()` on page load

- **generateCameraRemarkFields(totalQty, itemIndex)**:

  - Creates input fields based on total camera quantity
  - Pre-fills existing remarks if available
  - Each field labeled "Camera {N} Direction/Location"

- **saveCameraRemarks()**:

  - Validates all fields are filled
  - Stores remarks in `cameraRemarksData[itemIndex]` object
  - Updates hidden field with `JSON.stringify(remarks)`
  - Updates display via `updateCameraRemarksDisplay()`
  - Shows SweetAlert2 success message

- **updateCameraRemarksDisplay(itemIndex)**:

  - Formats remarks as "#Cam1: {remark}" in UI
  - Shows/hides display based on remark availability

- **loadExistingCameraRemarks()**:

  - Parses JSON from hidden fields on page load
  - Populates `cameraRemarksData` object
  - Updates display for each item with existing remarks

- **getFormattedCameraRemarks(itemIndex)**:
  - Returns formatted string for submission
  - Format: "#Cam1 Remarks: {text}\n#Cam2 Remarks: {text}"

**Exports**: `window.cameraRemarksModule` for external access

### 9. Program.cs

Registered camera remarks repository in dependency injection.

**Addition**:

```csharp
builder.Services.AddScoped<ISurveyCamRemarks, SurveyCamRemarksRepo>();
```

## User Workflow

1. **Survey Item Selection**:

   - User navigates to survey details item selection
   - Selects "Camera" (ItemID = 100) from device list
   - Enters quantity (Existing: 2, New: 1 = Total 3 cameras)

2. **Manage Remarks**:

   - "Manage Camera Remarks" button appears below quantity fields
   - User clicks button
   - Modal opens with 3 input fields (one per camera)

3. **Enter Remarks**:

   - User enters direction for each camera:
     - Camera 1: "Towards temple"
     - Camera 2: "Main entrance"
     - Camera 3: "Parking area"
   - Clicks "Save Camera Remarks"

4. **Display**:

   - Remarks displayed in formatted view:
     ```
     #Cam1: Towards temple
     #Cam2: Main entrance
     #Cam3: Parking area
     ```

5. **Submit Survey**:

   - User completes other fields and submits form
   - Controller processes camera remarks JSON
   - Each remark saved to `SurveyCamRemarks` table

6. **Edit Survey**:
   - User returns to edit survey
   - Existing remarks automatically loaded
   - Displayed in formatted view
   - User can click "Manage Camera Remarks" to update

## Technical Notes

### JSON Storage Format

Hidden field stores remarks as JSON array:

```json
["Towards temple", "Main entrance", "Parking area"]
```

### Database Storage

Each remark stored as individual record:

```
TransID | SurveyID | LocID | ItemID | Remarks           | CreatedBy
----------------------------------------------------------------------
1       | 1001     | 5     | 100    | Towards temple    | 123
2       | 1001     | 5     | 100    | Main entrance     | 123
3       | 1001     | 5     | 100    | Parking area      | 123
```

### Update Strategy

- Delete all existing remarks for survey/location/item
- Insert new set of remarks
- Prevents orphaned records and duplicate entries

### Error Handling

- JSON parsing wrapped in try-catch
- Invalid JSON won't block survey update
- Console logging for debugging
- SweetAlert2 warnings for incomplete data

## Testing Checklist

- [ ] Camera remarks button only appears for ItemID = 100
- [ ] Modal shows correct number of fields based on quantity
- [ ] Validation prevents saving with empty fields
- [ ] Remarks display correctly after saving
- [ ] Hidden field populated with JSON array
- [ ] Form submission processes camera remarks
- [ ] Database records created correctly
- [ ] Existing remarks load when editing
- [ ] Quantity changes update remark count
- [ ] Multiple cameras per survey handled correctly
- [ ] Authorization checks work for all endpoints
- [ ] Non-camera items unaffected by changes

## Dependencies

- **Bootstrap 5**: Modal UI component
- **SweetAlert2**: User notifications and alerts
- **System.Text.Json**: JSON serialization/deserialization
- **SQL Server**: Database storage

## Security Considerations

- Session validation on all controller actions
- Authorization checks via `CommonUtil.CheckAuthorizationAll()`
- SQL parameters to prevent injection
- Input validation on model properties
- Max length enforcement (500 characters per remark)

## Performance Notes

- Minimal database queries (batch delete + batch insert)
- Client-side storage in memory until save
- Only loads remarks for ItemID = 100
- JSON parsing occurs only on form submission
- Efficient display updates without page refresh

## Future Enhancements

- [ ] Add image upload for camera views
- [ ] Support GPS coordinates for camera locations
- [ ] Export camera deployment map
- [ ] Validate against duplicate camera directions
- [ ] Add camera installation date/time tracking
- [ ] Support camera angle/field of view parameters
