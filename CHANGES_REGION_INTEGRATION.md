# Survey Application - Region Integration Changes

## Date: November 17, 2025

## Summary
Fixed the "Invalid column name 'ZoneSectorWardNumber'" error and integrated RegionMaster table to display region information instead of the removed ZoneSectorWardNumber column.

---

## Changes Made

### 1. **Model Updates** ✅ (Already Done)
- `SurveyModel.cs` already has:
  - `RegionID` property (int?)
  - `RegionName` property (string)
- These properties will store and display region information

### 2. **Controller Updates** ✅ (Already Done)
- `SurveyCreationController.cs` has been updated:
  - **Create() GET**: Populates `ViewBag.Regions` with region dropdown
  - **Create() POST**: Includes `ViewBag.Regions` in error handling
  - **Edit() GET**: Populates `ViewBag.Regions` with region dropdown and preselects current region
  - **Edit() POST**: Includes `ViewBag.Regions` in error handling
  - All actions use: `new SelectList(_adminRepository.GetRegionMaster(), "RegionID", "RegionDesc")`

### 3. **View Updates** ✅ (Completed)
- **Index.cshtml**: Added Region display in survey cards
  ```html
  <div class="mb-2">
      <small class="text-muted d-block mb-1">
          <i class="bi bi-map me-1"></i><strong>Region:</strong>
      </small>
      <span class="text-dark">@(survey.RegionName ?? "-")</span>
  </div>
  ```
- **SurveyCreation.cshtml**: Already has Region dropdown
- **SurveyEdit.cshtml**: Already has Region dropdown

### 4. **Repository Updates** ✅ (Already Done)
- `AdminRepo.cs` includes:
  - `GetRegionMaster()` method to fetch regions from `RegionMaster` table
  - Uses stored procedure: `SpCommonOptions` with `@SpType = 3`

---

## Required Database Changes ⚠️

### **CRITICAL**: You must update the `SpSurvey` stored procedure

The stored procedure `dbo.SpSurvey` needs to be modified to:
1. **Remove** any references to the deleted column `ZoneSectorWardNumber`
2. **Add** JOINs with `RegionMaster` table to retrieve `RegionName`
3. **Add** JOINs with `EmpMaster` table to retrieve `EmpName` for assignments

### Update Instructions:

#### Step 1: Open the SQL file
- Open the file: `UpdateStoredProcedure_SpSurvey.sql` (in this folder)
- This file contains all the SQL code you need

#### Step 2: Update SpType = 2 (Get All Surveys)
Replace the SELECT statement in your `SpSurvey` procedure with:
```sql
WHEN @SpType = 2 -- Get All Surveys
BEGIN
    SELECT 
        s.SurveyId,
        s.SurveyName,
        s.ImplementationType,
        s.SurveyDate,
        s.SurveyTeamName,
        s.SurveyTeamContact,
        s.AgencyName,
        s.LocationSiteName,
        s.CityDistrict,
        s.ScopeOfWork,
        s.Latitude,
        s.Longitude,
        s.MapMarking,
        s.SurveyStatus,
        s.RegionID,
        ISNULL(r.RegionName, '') AS RegionName,
        s.CreatedBy,
        s.CreatedOn
    FROM dbo.Survey s
    LEFT JOIN dbo.RegionMaster r ON s.RegionID = r.RegionID
    ORDER BY s.SurveyId DESC
END
```

#### Step 3: Update SpType = 7 (Get Survey By Id)
Replace the SELECT statement with:
```sql
WHEN @SpType = 7 -- Get Survey By Id
BEGIN
    SELECT 
        s.SurveyId,
        s.SurveyName,
        s.ImplementationType,
        s.SurveyDate,
        s.SurveyTeamName,
        s.SurveyTeamContact,
        s.AgencyName,
        s.LocationSiteName,
        s.CityDistrict,
        s.ScopeOfWork,
        s.Latitude,
        s.Longitude,
        s.MapMarking,
        s.SurveyStatus,
        s.RegionID,
        ISNULL(r.RegionName, '') AS RegionName,
        s.CreatedBy,
        s.CreatedOn
    FROM dbo.Survey s
    LEFT JOIN dbo.RegionMaster r ON s.RegionID = r.RegionID
    WHERE s.SurveyId = @SurveyId
END
```

#### Step 4: Update SpType = 16 (Get Survey Assignments)
Replace the SELECT statement with:
```sql
WHEN @SpType = 16 -- Get Survey Assignments
BEGIN
    SELECT 
        sa.TransID,
        sa.SurveyID,
        sa.EmpID,
        ISNULL(e.EmpName, '') AS EmpName,
        sa.DueDate,
        sa.CreatedBy,
        sa.CreatedOn
    FROM dbo.SurveyAssignment sa
    LEFT JOIN dbo.EmpMaster e ON sa.EmpID = e.EmpID
    WHERE sa.SurveyID = @SurveyID
    ORDER BY sa.TransID DESC
END
```

#### Step 5: Execute the Changes
1. Open SQL Server Management Studio (SSMS)
2. Connect to your `VLDev` database
3. Find the stored procedure: `dbo.SpSurvey`
4. Right-click → Modify
5. Make the changes from Steps 2-4 above
6. Execute (F5) to save the changes
7. Test by running: `EXEC dbo.SpSurvey @SpType = 2`

---

## Testing Checklist

After updating the stored procedure, test the following:

- [ ] **Survey List Page** (`/SurveyCreation/Index`)
  - Verify surveys display without errors
  - Verify Region Name shows correctly in survey cards
  
- [ ] **Create Survey** (`/SurveyCreation/Create`)
  - Verify Region dropdown appears
  - Verify you can select a region
  - Verify survey is created with the selected region
  
- [ ] **Edit Survey** (`/SurveyCreation/Edit/{id}`)
  - Verify Region dropdown appears
  - Verify current region is pre-selected
  - Verify you can change the region
  
- [ ] **Survey Assignments** (`/SurveyCreation/SurveyAssignment`)
  - Verify Employee Name appears (not null)
  - Verify all assignment details display correctly

---

## Files Modified

### Application Code:
1. ✅ `Views/SurveyCreation/Index.cshtml` - Added Region display
2. ✅ `Controllers/SurveyCreationController.cs` - Already had ViewBag.Regions
3. ✅ `Models/SurveyModel.cs` - Already had RegionID and RegionName properties
4. ✅ `Repo/AdminRepo.cs` - Already had GetRegionMaster() method

### Database:
5. ⚠️ `dbo.SpSurvey` stored procedure - **REQUIRES MANUAL UPDATE**

---

## Important Notes

### Why the Error Occurred
The error "Invalid column name 'ZoneSectorWardNumber'" happened because:
- The column `ZoneSectorWardNumber` was removed from the `Survey` table
- The stored procedure `SpSurvey` was still trying to SELECT this column
- When the app tried to fetch survey data, SQL Server threw this error

### The Solution
- Replace `ZoneSectorWardNumber` with `RegionID` and `RegionName`
- Join with `RegionMaster` table to get the region name
- This provides better data normalization and allows sharing region data

### Region vs ZoneSectorWardNumber
- **Old**: `ZoneSectorWardNumber` was a text field storing zone/sector/ward information
- **New**: `RegionID` references `RegionMaster` table, `RegionName` is displayed from the lookup
- **Benefit**: Consistent region names, easier to maintain, better data integrity

---

## Support

If you encounter any issues:
1. Check that the stored procedure was updated correctly
2. Verify the `RegionMaster` table has data
3. Verify the `EmpMaster` table has employee data
4. Check SQL Server error logs for detailed error messages
5. Ensure connection string points to the correct database

---

## Next Steps

After completing the stored procedure update:
1. Run the application
2. Navigate to the Survey List page
3. Create a new survey and select a region
4. Verify the region appears in the survey list
5. Create a survey assignment and verify employee names appear

---

**Status**: Code changes complete ✅ | Database update required ⚠️
