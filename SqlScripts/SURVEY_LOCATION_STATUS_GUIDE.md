# Survey Location Status - Implementation Guide

## Overview
The Survey Location Status feature allows you to track the progress of survey locations with the following statuses:
- **Pending** - Initial state
- **In Progress** - Survey started
- **Completed** - Survey finished
- **Verified** - Survey verified by supervisor

## Components Created

### 1. Database
- **Table**: `SurveyLocationStatus`
- **Stored Procedure**: `SpSurveyLocationStatus`

### 2. Models
- **File**: `Models/SurveyLocationStatusModel.cs`
- Contains all properties for status tracking

### 3. Repository Layer
- **Interface**: `Repo/ISurveyLocationStatus.cs`
- **Implementation**: `Repo/SurveyLocationStatusRepo.cs`
- Methods:
  - `GetLocationStatus(surveyId, locId)` - Get status for a specific location
  - `GetSurveyLocationStatuses(surveyId)` - Get all statuses for a survey
  - `MarkLocationAsCompleted(surveyId, locId, userId, remarks)` - Mark as completed
  - `MarkLocationAsInProgress(surveyId, locId, userId, remarks)` - Mark as in progress
  - `MarkLocationAsVerified(surveyId, locId, userId, remarks)` - Mark as verified
  - `UpsertLocationStatus(surveyId, locId, status, remarks, userId)` - General update
  - `DeleteLocationStatus(surveyId, locId)` - Delete status

### 4. Controller
- **Updated**: `Controllers/SurveyLocationController.cs`
- New Actions:
  - `MarkAsCompleted` - POST action to mark location as completed
  - `MarkAsInProgress` - POST action to mark location as in progress
  - `MarkAsVerified` - POST action to mark location as verified
  - `GetLocationStatus` - GET action to retrieve status
  - `GetSurveyLocationStatuses` - GET action to retrieve all statuses for a survey

### 5. JavaScript Helper
- **File**: `wwwroot/js/survey-location-status.js`
- Provides easy-to-use JavaScript functions for status updates

### 6. Dependency Injection
- **Updated**: `Program.cs`
- Registered `ISurveyLocationStatus` and `SurveyLocationStatusRepo`

## Usage Examples

### A. Using Controller Actions Directly

#### 1. Mark Location as Completed (AJAX)
```javascript
$.ajax({
    url: '/SurveyCreation/MarkLocationAsCompleted',
    type: 'POST',
    data: {
        surveyId: 123,
        locId: 456,
        remarks: 'All items surveyed'
    },
    success: function(response) {
        if (response.success) {
            alert(response.message);
        } else {
            alert('Error: ' + response.message);
        }
    }
});
```

#### 2. Get Location Status (AJAX)
```javascript
$.ajax({
    url: '/SurveyCreation/GetLocationStatus',
    type: 'GET',
    data: {
        surveyId: 123,
        locId: 456
    },
    success: function(response) {
        if (response.success) {
            console.log('Status:', response.data.Status);
            console.log('Completed Date:', response.data.CompletedDate);
        }
    }
});
```

### B. Using JavaScript Helper (Recommended)

Include the helper in your view:
```html
<script src="~/js/survey-location-status.js"></script>
```

#### 1. Mark Location as Completed
```javascript
SurveyLocationStatus.markAsCompleted(123, 456, 'Survey completed successfully', function(success, message) {
    if (success) {
        alert(message);
        location.reload(); // Refresh page
    } else {
        alert('Error: ' + message);
    }
});
```

#### 2. Mark Location as In Progress
```javascript
SurveyLocationStatus.markAsInProgress(123, 456, null, function(success, message) {
    if (success) {
        console.log('Status updated:', message);
    }
});
```

#### 3. Get Status and Display Badge
```javascript
SurveyLocationStatus.getLocationStatus(123, 456, function(success, data) {
    if (success && data) {
        var badgeHtml = SurveyLocationStatus.getStatusBadge(data.Status);
        $('#statusContainer').html(badgeHtml);
    }
});
```

#### 4. Display Status Buttons
```javascript
var currentStatus = 'In Progress';
var buttons = SurveyLocationStatus.getStatusButtons(123, 456, currentStatus);
$('#buttonsContainer').html(buttons);
```

### C. Using Repository in C# Code

```csharp
// In your controller or service
public class YourController : Controller
{
    private readonly ISurveyLocationStatus _statusRepo;
    
    public YourController(ISurveyLocationStatus statusRepo)
    {
        _statusRepo = statusRepo;
    }
    
    public IActionResult UpdateStatus()
    {
        long surveyId = 123;
        int locId = 456;
        int userId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
        
        // Mark as completed
        bool success = _statusRepo.MarkLocationAsCompleted(surveyId, locId, userId, "Done");
        
        // Get current status
        var status = _statusRepo.GetLocationStatus(surveyId, locId);
        
        // Get all statuses for a survey
        var allStatuses = _statusRepo.GetSurveyLocationStatuses(surveyId);
        
        return View();
    }
}
```

## Integration with Existing Views

### Example: Add Status to SurveyLocation List

```html
@model List<SurveyApp.Models.SurveyLocationModel>

@section Scripts {
    <script src="~/js/survey-location-status.js"></script>
    <script>
        $(document).ready(function() {
            // Load statuses for all locations
            var surveyId = @ViewBag.SelectedSurveyId;
            
            SurveyLocationStatus.getSurveyLocationStatuses(surveyId, function(success, statuses) {
                if (success && statuses) {
                    statuses.forEach(function(status) {
                        var badge = SurveyLocationStatus.getStatusBadge(status.Status);
                        $('#status-' + status.LocID).html(badge);
                        
                        var buttons = SurveyLocationStatus.getStatusButtons(
                            status.SurveyID, 
                            status.LocID, 
                            status.Status
                        );
                        $('#actions-' + status.LocID).html(buttons);
                    });
                }
            });
        });
    </script>
}

<table class="table">
    <thead>
        <tr>
            <th>Location Name</th>
            <th>Status</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach(var location in Model)
        {
            <tr>
                <td>@location.LocName</td>
                <td id="status-@location.LocID">
                    <span class="badge bg-secondary">Loading...</span>
                </td>
                <td id="actions-@location.LocID"></td>
            </tr>
        }
    </tbody>
</table>
```

## API Endpoints

| Method | Endpoint | Parameters | Description |
|--------|----------|------------|-------------|
| POST | `/SurveyCreation/MarkLocationAsCompleted` | surveyId, locId, remarks | Mark location as completed |
| POST | `/SurveyCreation/MarkLocationAsInProgress` | surveyId, locId, remarks | Mark location as in progress |
| POST | `/SurveyCreation/MarkLocationAsVerified` | surveyId, locId, remarks | Mark location as verified |
| GET | `/SurveyCreation/GetLocationStatus` | surveyId, locId | Get status for a location |
| GET | `/SurveyCreation/GetSurveyLocationStatuses` | surveyId | Get all statuses for a survey |

## Status Workflow

```
Pending → In Progress → Completed → Verified
   ↓           ↓            ↓
  Can         Can          Can
 Start      Complete     Verify
```

## Troubleshooting

### Issue: "Repository not found"
**Solution**: Make sure you've registered the services in `Program.cs`:
```csharp
builder.Services.AddScoped<ISurveyLocationStatus, SurveyLocationStatusRepo>();
```

### Issue: "Stored procedure not found"
**Solution**: Run the SQL script `SQL_Add_LocationCompletion_SpType.sql` in your database

### Issue: AJAX calls return 404
**Solution**: Verify the controller routes are correct and the controller is properly instantiated with dependency injection

### Issue: Status not updating
**Solution**: 
1. Check browser console for JavaScript errors
2. Verify surveyId and locId parameters are correct
3. Check that the stored procedure is working correctly:
```sql
EXEC SpSurveyLocationStatus @SpType=3, @SurveyID=123, @LocID=456, @UserID=1
```

## Database Queries for Testing

```sql
-- Check if status exists
SELECT * FROM SurveyLocationStatus WHERE SurveyID = 123 AND LocID = 456

-- Get all statuses for a survey
EXEC SpSurveyLocationStatus @SpType=4, @SurveyID=123

-- Manually mark as completed
EXEC SpSurveyLocationStatus @SpType=3, @SurveyID=123, @LocID=456, @UserID=1, @Remarks='Test'

-- Delete a status
EXEC SpSurveyLocationStatus @SpType=2, @SurveyID=123, @LocID=456
```

## Next Steps

1. ✅ Database table and stored procedure created
2. ✅ Models created
3. ✅ Repository layer implemented
4. ✅ Controller actions added
5. ✅ JavaScript helper created
6. ✅ Services registered in Program.cs
7. 🔲 Add UI elements to your views
8. 🔲 Test functionality
9. 🔲 Add validation and error handling as needed
10. 🔲 Consider adding notifications (toast messages) instead of alerts

## Support

For issues or questions, refer to:
- Repository interfaces in `Repo/ISurveyLocationStatus.cs`
- Controller implementation in `Controllers/SurveyLocationController.cs`
- JavaScript helper in `wwwroot/js/survey-location-status.js`
