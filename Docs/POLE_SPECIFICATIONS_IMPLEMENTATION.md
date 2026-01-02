# Pole Specifications - Database-Driven Implementation

## Summary

Successfully implemented a database-driven pole specifications system with conditional display logic and support for multiple pole instances.

## What Was Implemented

### 1. Database Schema ✅

**New Table: ItemSpecificationOptionsMaster**

- Stores dropdown options (Pole Owner, Pole Height) in the database
- Columns: OptionID, SpecificationID, OptionValue, OptionText, DisplayOrder, IsActive
- Allows dynamic management of options without code changes

**Updated Table: ItemSpecificationMaster**

- Added columns:
  - `InputType` (VARCHAR(50)): 'dropdown', 'text', 'number'
  - `ConditionalDisplay` (VARCHAR(50)): 'Always', 'ExistingQtyOnly', 'RequiredQtyOnly', 'BothQty'
  - `AllowMultipleInstances` (BIT): Enables multiple poles with different values

**Updated Table: SpecificationDetailsMaster**

- Added `InstanceNumber` (INT): Supports multiple pole instances (pole #1, pole #2, etc.)
- Updated Primary Key: (SurveyID, LocID, ItemID, SpecificationID, InstanceNumber)

### 2. Data Inserted ✅

**Pole Owner Options (SpecificationID = 101)**:

- Telecom
- Electrical
- Municipality

**Pole Height Options (SpecificationID = 102)**:

- 4m (4 meters)
- 5m (5 meters)
- 6.5m (6.5 meters)
- 8m (8 meters)
- 10m (10 meters)
- 12m (12 meters)

**Specification Rules**:

- Pole Owner: ConditionalDisplay='ExistingQtyOnly', AllowMultipleInstances=1
- Pole Height: ConditionalDisplay='RequiredQtyOnly', AllowMultipleInstances=1

### 3. Stored Procedures ✅

**SpGetSpecificationOptions**

```sql
EXEC SpGetSpecificationOptions @SpecificationID = 101
-- Returns all active options for Pole Owner
```

**SpGetItemSpecificationsWithOptions**

```sql
EXEC SpGetItemSpecificationsWithOptions @ItemID = 1000037
-- Returns specifications with comma-separated options
```

### 4. Backend Code ✅

**Models** ([ItemSpecificationModel.cs](d:\VL Access\Survey\CODES\Survey\survey\Models\ItemSpecificationModel.cs)):

- Added `ConditionalDisplay`, `AllowMultipleInstances` properties
- Added `InstanceNumber` to SpecificationDetailsModel
- Created `SpecificationOptionModel` class

**Repository** ([SurveyRepo.cs](d:\VL Access\Survey\CODES\Survey\survey\Repo\SurveyRepo.cs)):

- `GetItemSpecifications()`: Updated query to include new columns
- `GetSpecificationOptions()`: New method to fetch options from database

**Controller** ([SurveyDetailsController.cs](d:\VL Access\Survey\CODES\Survey\survey\Controllers\SurveyDetailsController.cs)):

- Removed hardcoded `PoleOwnerOptions` and `PoleHeightOptions` SelectList
- Specifications now loaded dynamically via JavaScript

### 5. Frontend Code ✅

**View** ([ItemMasterSelection.cshtml](d:\VL Access\Survey\CODES\Survey\survey\Views\SurveyDetails\ItemMasterSelection.cshtml)):

- Removed hardcoded pole owner/height dropdowns
- Added dynamic specification container:
  ```html
  <div
    id="item-specifications-@i"
    data-item-id="@item.ItemID"
    data-existing-qty="@item.ItemQtyExist"
    data-required-qty="@item.ItemQtyReq"
  ></div>
  ```

**JavaScript** ([item-specifications-conditional.js](d:\VL Access\Survey\CODES\Survey\survey\wwwroot\js\item-specifications-conditional.js)):

- Loads specifications from database via API
- Implements conditional display logic:
  - **ExistingQtyOnly**: Shows only if ExistingQty > 0
  - **RequiredQtyOnly**: Shows only if RequiredQty > 0
  - **Always**: Shows always (like Road Width)
- Supports multiple instances (renders Pole #1, Pole #2, etc.)
- Updates dynamically when quantities change

## How It Works

### Conditional Display Logic

1. **Pole Owner** (SpecificationID = 101):

   - Appears ONLY when `ItemQtyExist > 0`
   - Allows multiple instances (one dropdown per existing pole)
   - Example: If 3 poles exist, shows "Pole Owner #1", "Pole Owner #2", "Pole Owner #3"

2. **Pole Height** (SpecificationID = 102):

   - Appears ONLY when `ItemQtyReq > 0`
   - Allows multiple instances (one dropdown per new pole)
   - Example: If deploying 2 new poles, shows "Pole Height #1", "Pole Height #2"

3. **Road Width** (SpecificationID = 100, 103):
   - Always visible regardless of quantities
   - Single instance only

### User Workflow

1. User opens survey item selection for a POLE item
2. Enters quantities:
   - Existing Qty: 2 → Shows 2 "Pole Owner" dropdowns
   - Required Qty: 3 → Shows 3 "Pole Height" dropdowns
3. Selects owner for each existing pole (Telecom, Electrical, Municipality)
4. Selects height for each new pole to deploy (4m, 5m, 6.5m, etc.)
5. Data saved to `SpecificationDetailsMaster` with instance numbers

### Data Storage Example

```
SurveyID | LocID | ItemID  | SpecificationID | InstanceNumber | SpecificationDetails
---------|-------|---------|-----------------|----------------|--------------------
1        | 5     | 1000037 | 101             | 1              | Telecom
1        | 5     | 1000037 | 101             | 2              | Electrical
1        | 5     | 1000037 | 102             | 1              | 4m
1        | 5     | 1000037 | 102             | 2              | 6.5m
1        | 5     | 1000037 | 102             | 3              | 8m
```

## Files Created/Modified

### Created:

- [SqlScripts/UPDATE_PoleSpecifications_Schema.sql](d:\VL Access\Survey\CODES\Survey\survey\SqlScripts\UPDATE_PoleSpecifications_Schema.sql)
- [wwwroot/js/item-specifications-conditional.js](d:\VL Access\Survey\CODES\Survey\survey\wwwroot\js\item-specifications-conditional.js)

### Modified:

- [Models/ItemSpecificationModel.cs](d:\VL Access\Survey\CODES\Survey\survey\Models\ItemSpecificationModel.cs)
- [Models/SurveyDetailsUpdate.cs](d:\VL Access\Survey\CODES\Survey\survey\Models\SurveyDetailsUpdate.cs)
- [Repo/ISurvey.cs](d:\VL Access\Survey\CODES\Survey\survey\Repo\ISurvey.cs)
- [Repo/SurveyRepo.cs](d:\VL Access\Survey\CODES\Survey\survey\Repo\SurveyRepo.cs)
- [Controllers/SurveyDetailsController.cs](d:\VL Access\Survey\CODES\Survey\survey\Controllers\SurveyDetailsController.cs)
- [Views/SurveyDetails/ItemMasterSelection.cshtml](d:\VL Access\Survey\CODES\Survey\survey\Views\SurveyDetails\ItemMasterSelection.cshtml)

## Database Connection

```
Server: 10.0.32.135
Database: VLDev
User: adminrole
Password: @dminr0le
```

## Testing

Build Status: ✅ **SUCCESS** (88 warnings, 0 errors)

To test:

1. Navigate to a survey with pole items
2. Adjust existing/required quantities
3. Verify:
   - Pole Owner appears when ExistingQty > 0
   - Pole Height appears when RequiredQty > 0
   - Multiple dropdowns render for multiple poles
   - Options loaded from database
   - Values save correctly with instance numbers

## Benefits

✅ **Database-Driven**: Options managed in database, no code changes needed  
✅ **Conditional Display**: Fields appear only when relevant  
✅ **Multiple Instances**: Supports different values per pole  
✅ **Scalable**: Easy to add new specifications or options  
✅ **Maintainable**: Centralized specification system  
✅ **User-Friendly**: Dynamic UI adapts to quantities

## Next Steps (Optional Enhancements)

1. Add UI for managing specification options (CRUD)
2. Add validation rules for specifications
3. Add specification templates for different item types
4. Export/import specification data
5. Add audit trail for specification changes
