# Pole Specifications - Complete Implementation Summary

## Overview

Implemented database-driven pole specifications with conditional display and multi-instance support.

## Database Changes

### 1. Schema Updates

```sql
-- Added columns to ItemSpecificationMaster
ALTER TABLE ItemSpecificationMaster ADD InputType VARCHAR(50) NULL;
ALTER TABLE ItemSpecificationMaster ADD ConditionalDisplay VARCHAR(50) NULL;
ALTER TABLE ItemSpecificationMaster ADD AllowMultipleInstances BIT NULL DEFAULT 0;

-- Added InstanceNumber to SpecificationDetailsMaster
ALTER TABLE SpecificationDetailsMaster ADD InstanceNumber INT NULL DEFAULT 1;

-- Updated Primary Key to include InstanceNumber
ALTER TABLE SpecificationDetailsMaster DROP CONSTRAINT PK_SpecificationDetailsMaster;
ALTER TABLE SpecificationDetailsMaster
ADD CONSTRAINT PK_SpecificationDetailsMaster
PRIMARY KEY (SurveyID, LocID, ItemID, SpecificationID, InstanceNumber);

-- Created ItemSpecificationOptionsMaster table
CREATE TABLE ItemSpecificationOptionsMaster (
    OptionID INT PRIMARY KEY IDENTITY(1,1),
    SpecificationID INT NOT NULL,
    OptionValue VARCHAR(50) NOT NULL,
    OptionText VARCHAR(100) NOT NULL,
    DisplayOrder INT NULL DEFAULT 0,
    IsActive BIT NULL DEFAULT 1,
    FOREIGN KEY (SpecificationID) REFERENCES ItemSpecificationMaster(SpecificationID)
);
```

### 2. Sample Data Inserted

- **Pole Owner** (SpecificationID 101): Telecom, Electrical, Municipality
- **Pole Height** (SpecificationID 102): 4m, 5m, 6.5m, 8m, 10m, 12m

### 3. Stored Procedures Created

#### SpSaveSpecificationDetails

- Purpose: Insert or update specification details with InstanceNumber support
- Returns: Success status and message
- Logic: MERGE with composite key including InstanceNumber

#### SpDeleteSpecificationDetails

- Purpose: Delete specifications by ItemID, SpecificationID, or Instance
- Supports cascading deletes

#### SpGetSpecificationDetails

- Purpose: Retrieve all specifications for an item with metadata
- Joins with ItemSpecificationMaster for complete data

#### SpBulkSaveSpecificationDetails

- Purpose: Save multiple specifications from JSON payload
- Useful for batch operations

## Code Changes

### 1. Models Updated

#### ItemSpecificationModel.cs

- Added `ConditionalDisplay` property (ExistingQtyOnly, RequiredQtyOnly, Always, BothQty)
- Added `AllowMultipleInstances` property
- Created `SpecificationOptionModel` class
- Added `InstanceNumber` to `SpecificationDetailsModel`
- Added `InstanceNumber` to `SpecificationDetailItem`

```csharp
public class SpecificationDetailItem
{
    [JsonPropertyName("specificationID")]
    public int SpecificationID { get; set; }

    [JsonPropertyName("specificationDetails")]
    public string? SpecificationDetails { get; set; }

    [JsonPropertyName("instanceNumber")]
    public int InstanceNumber { get; set; } = 1;
}
```

### 2. Repository Updated

#### ISurvey.cs

- Added `GetSpecificationOptions()` method signature

#### SurveyRepo.cs

- Updated `GetItemSpecifications()` to include new columns (line 1270)
- Added `GetSpecificationOptions()` implementation (line 1424+)
- **CRITICAL FIX**: Updated `SaveSpecificationDetails()` to use stored procedure
  - Changed from inline MERGE to stored procedure call
  - Added InstanceNumber parameter extraction
  - Fixed composite key matching issue

```csharp
// Extract instance number from spec if available, default to 1
int instanceNumber = spec.InstanceNumber > 0 ? spec.InstanceNumber : 1;

// Use stored procedure for save/update
using var cmd = new SqlCommand("SpSaveSpecificationDetails", con, transaction);
cmd.CommandType = System.Data.CommandType.StoredProcedure;

cmd.Parameters.AddWithValue("@SurveyID", model.SurveyID);
cmd.Parameters.AddWithValue("@LocID", model.LocID);
cmd.Parameters.AddWithValue("@ItemID", model.ItemID);
cmd.Parameters.AddWithValue("@SpecificationID", spec.SpecificationID);
cmd.Parameters.AddWithValue("@InstanceNumber", instanceNumber);
cmd.Parameters.AddWithValue("@SpecificationDetails",
    string.IsNullOrEmpty(spec.SpecificationDetails) ? (object)DBNull.Value : spec.SpecificationDetails);
```

### 3. Controller Updated

#### SurveyDetailsController.cs

- Removed hardcoded `PoleOwnerOptions` and `PoleHeightOptions` SelectList
- Specifications now loaded via AJAX from database

### 4. View Updated

#### ItemMasterSelection.cshtml

- Removed hardcoded dropdown HTML
- Added dynamic specification container:

```html
<div
  id="item-specifications-@i"
  class="item-specifications-container mb-3"
  data-item-id="@item.ItemID"
  data-item-index="@i"
  data-existing-qty="@item.ItemQtyExist"
  data-required-qty="@item.ItemQtyReq"
>
  <!-- Specifications will be dynamically loaded here via JavaScript -->
</div>
```

### 5. JavaScript Updated

#### item-specifications-conditional.js (NEW FILE)

- Handles conditional specification rendering based on quantity
- Implements multi-instance logic (Pole #1, Pole #2, etc.)
- Conditional display rules:
  - `ExistingQtyOnly`: Shows only if ExistingQty > 0 (Pole Owner)
  - `RequiredQtyOnly`: Shows only if RequiredQty > 0 (Pole Height)
  - `BothQty`: Shows if ExistingQty > 0 OR RequiredQty > 0
  - `Always`: Always shows

Key functions:

- `loadAndRenderSpecifications()` - Fetches specs and options from server
- `checkConditionalDisplay()` - Determines if spec should be shown
- `renderSpecificationInput()` - Creates HTML for dropdown/text input
- `renderConditionalSpecifications()` - Handles multi-instance rendering

#### item-specifications.js

- **CRITICAL FIX**: Updated `collectSpecificationValues()` to include `instanceNumber`

```javascript
specs.push({
  specificationID: specId,
  specificationDetails: value,
  instanceNumber: instanceNum, // Include instance number in payload
});
```

## Conditional Display Logic

| Condition                        | Pole Owner (101) | Pole Height (102) |
| -------------------------------- | ---------------- | ----------------- |
| ExistingQty = 0, RequiredQty = 0 | Hidden           | Hidden            |
| ExistingQty = 2, RequiredQty = 0 | 2 instances      | Hidden            |
| ExistingQty = 0, RequiredQty = 3 | Hidden           | 3 instances       |
| ExistingQty = 2, RequiredQty = 3 | 2 instances      | 3 instances       |

## Multi-Instance Example

If user selects:

- **Existing Qty**: 2 (two existing poles)
- **Required Qty**: 3 (need 3 total poles)

The form will show:

- **Pole Owner #1** (dropdown: Telecom/Electrical/Municipality)
- **Pole Owner #2** (dropdown: Telecom/Electrical/Municipality)
- **Pole Height #1** (dropdown: 4m/5m/6.5m/8m/10m/12m)
- **Pole Height #2** (dropdown: 4m/5m/6.5m/8m/10m/12m)
- **Pole Height #3** (dropdown: 4m/5m/6.5m/8m/10m/12m)

Database records created:

```
SurveyID | LocID | ItemID | SpecificationID | InstanceNumber | SpecificationDetails
---------|-------|--------|-----------------|----------------|---------------------
123      | 45    | 67     | 101            | 1              | Telecom
123      | 45    | 67     | 101            | 2              | Electrical
123      | 45    | 67     | 102            | 1              | 8m
123      | 45    | 67     | 102            | 2              | 10m
123      | 45    | 67     | 102            | 3              | 12m
```

## Testing Performed

### 1. Database Tests

```sql
-- Test 1: Save Telecom pole owner (instance 1)
EXEC SpSaveSpecificationDetails
    @SurveyID=123, @LocID=45, @ItemID=67,
    @SpecificationID=101, @InstanceNumber=1,
    @SpecificationDetails='Telecom';
-- Result: ✅ "1 Specification saved successfully"

-- Test 2: Save 4m pole height (instance 1)
EXEC SpSaveSpecificationDetails
    @SurveyID=123, @LocID=45, @ItemID=67,
    @SpecificationID=102, @InstanceNumber=1,
    @SpecificationDetails='4m';
-- Result: ✅ "1 Specification saved successfully"

-- Test 3: Update Telecom to Electrical
EXEC SpSaveSpecificationDetails
    @SurveyID=123, @LocID=45, @ItemID=67,
    @SpecificationID=101, @InstanceNumber=1,
    @SpecificationDetails='Electrical';
-- Result: ✅ "1 Specification saved successfully"

-- Test 4: Retrieve specifications
EXEC SpGetSpecificationDetails
    @SurveyID=123, @LocID=45, @ItemID=67;
-- Result: ✅ Returns both rows with updated Electrical value
```

### 2. Build Test

```bash
dotnet build
# Result: ✅ Build succeeded with 0 errors (88 warnings - pre-existing)
```

## Files Modified

1. **SqlScripts/UPDATE_PoleSpecifications_Schema.sql** - Schema updates
2. **SqlScripts/CREATE_SpecificationStoredProcedures.sql** - Stored procedures
3. **Models/ItemSpecificationModel.cs** - Added properties and classes
4. **Repo/ISurvey.cs** - Added method signature
5. **Repo/SurveyRepo.cs** - Updated save method to use SP
6. **Controllers/SurveyDetailsController.cs** - Removed hardcoded data
7. **Views/SurveyDetails/ItemMasterSelection.cshtml** - Dynamic container
8. **wwwroot/js/item-specifications-conditional.js** - NEW conditional logic
9. **wwwroot/js/item-specifications.js** - Fixed data collection

## Key Fixes Applied

### ❌ Original Problem

- SaveSpecificationDetails used inline MERGE without InstanceNumber
- Primary key constraint violation on SpecificationDetailsMaster
- JavaScript not sending InstanceNumber to server

### ✅ Solution Implemented

1. Created stored procedures with proper InstanceNumber handling
2. Updated repository to use SpSaveSpecificationDetails
3. Added InstanceNumber property to SpecificationDetailItem model
4. Updated JavaScript to collect and send instanceNumber from data-instance attribute

## Usage Instructions

### For Administrators

1. Add new specifications in ItemSpecificationMaster table
2. Set `InputType` (dropdown/text/number/date)
3. Set `ConditionalDisplay` (ExistingQtyOnly/RequiredQtyOnly/Always/BothQty)
4. Set `AllowMultipleInstances` (0 or 1)
5. Add options in ItemSpecificationOptionsMaster if InputType=dropdown

### For Users

1. Select item and enter quantities
2. Specifications appear automatically based on conditional rules
3. Fill in values for each instance
4. Submit form - data saved to SpecificationDetailsMaster with instance numbers
5. Values persist and reload on edit

## Database Connection Info

- **Server**: 10.0.32.135
- **Database**: VLDev
- **User**: adminrole
- **Password**: @dminr0le

## Next Steps (Optional Enhancements)

1. Add validation rules to specifications (min/max values, regex patterns)
2. Implement specification grouping/categories
3. Add specification history/audit trail
4. Support file attachments for specifications
5. Add specification templates for common item types
