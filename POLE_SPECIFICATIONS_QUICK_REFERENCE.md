# Pole Specifications - Quick Reference

## What Was Fixed

### ❌ The Problem

1. Specifications not saving to database
2. SaveSpecificationDetails method missing InstanceNumber in MERGE statement
3. JavaScript not collecting instanceNumber from form inputs
4. Primary key constraint violations on SpecificationDetailsMaster table

### ✅ The Solution

#### 1. Created Stored Procedures

- `SpSaveSpecificationDetails` - Handles UPSERT with InstanceNumber
- `SpDeleteSpecificationDetails` - Delete by item/spec/instance
- `SpGetSpecificationDetails` - Retrieve specs with metadata
- `SpBulkSaveSpecificationDetails` - Batch save from JSON

#### 2. Updated Repository (SurveyRepo.cs)

**Before:**

```csharp
// MERGE without InstanceNumber - BROKEN
ON (target.SurveyID = source.SurveyID
    AND target.LocID = source.LocID
    AND target.ItemID = source.ItemID
    AND target.SpecificationID = source.SpecificationID)
```

**After:**

```csharp
// Using stored procedure with InstanceNumber - FIXED
int instanceNumber = spec.InstanceNumber > 0 ? spec.InstanceNumber : 1;
cmd.CommandText = "SpSaveSpecificationDetails";
cmd.CommandType = CommandType.StoredProcedure;
cmd.Parameters.AddWithValue("@InstanceNumber", instanceNumber);
```

#### 3. Updated Model (ItemSpecificationModel.cs)

```csharp
public class SpecificationDetailItem
{
    [JsonPropertyName("specificationID")]
    public int SpecificationID { get; set; }

    [JsonPropertyName("specificationDetails")]
    public string? SpecificationDetails { get; set; }

    [JsonPropertyName("instanceNumber")]  // ADDED
    public int InstanceNumber { get; set; } = 1;
}
```

#### 4. Updated JavaScript (item-specifications.js)

```javascript
// BEFORE - Missing instanceNumber
specs.push({
  specificationID: specId,
  specificationDetails: value,
});

// AFTER - Includes instanceNumber
specs.push({
  specificationID: specId,
  specificationDetails: value,
  instanceNumber: parseInt(input.dataset.instance) || 1, // ADDED
});
```

## Current Configuration

### Database Tables

#### ItemSpecificationMaster

| SpecificationID | SpecificationName | InputType | ConditionalDisplay | AllowMultipleInstances |
| --------------- | ----------------- | --------- | ------------------ | ---------------------- |
| 101             | Pole Owner        | dropdown  | ExistingQtyOnly    | 1                      |
| 102             | Height            | dropdown  | RequiredQtyOnly    | 1                      |

#### ItemSpecificationOptionsMaster

| OptionID | SpecificationID | OptionValue  | OptionText   | DisplayOrder |
| -------- | --------------- | ------------ | ------------ | ------------ |
| 1        | 101             | Telecom      | Telecom      | 1            |
| 2        | 101             | Electrical   | Electrical   | 2            |
| 3        | 101             | Municipality | Municipality | 3            |
| 4        | 102             | 4m           | 4 meters     | 1            |
| 5        | 102             | 5m           | 5 meters     | 2            |
| 6        | 102             | 6.5m         | 6.5 meters   | 3            |
| 7        | 102             | 8m           | 8 meters     | 4            |
| 8        | 102             | 10m          | 10 meters    | 5            |
| 9        | 102             | 12m          | 12 meters    | 6            |

## How It Works

### Example Scenario

User selects a pole item:

- **Existing Qty**: 2 (two existing poles to inspect)
- **Required Qty**: 3 (need to install 3 new poles)

### What Happens

1. JavaScript detects quantity change
2. Loads specifications from `/SurveyDetails/GetItemSpecifications?itemId=67`
3. Filters specs by ConditionalDisplay:
   - Pole Owner (ExistingQtyOnly): Shows 2 instances (for existing poles)
   - Pole Height (RequiredQtyOnly): Shows 3 instances (for new poles)
4. Renders form:

   ```
   Pole Owner #1: [Dropdown: Telecom/Electrical/Municipality]
   Pole Owner #2: [Dropdown: Telecom/Electrical/Municipality]

   Pole Height #1: [Dropdown: 4m/5m/6.5m/8m/10m/12m]
   Pole Height #2: [Dropdown: 4m/5m/6.5m/8m/10m/12m]
   Pole Height #3: [Dropdown: 4m/5m/6.5m/8m/10m/12m]
   ```

### Form Submission

User fills in:

- Pole Owner #1 = Telecom
- Pole Owner #2 = Electrical
- Pole Height #1 = 8m
- Pole Height #2 = 10m
- Pole Height #3 = 12m

JavaScript collects:

```json
{
  "surveyID": 123,
  "locID": 45,
  "itemID": 67,
  "specifications": [
    {
      "specificationID": 101,
      "instanceNumber": 1,
      "specificationDetails": "Telecom"
    },
    {
      "specificationID": 101,
      "instanceNumber": 2,
      "specificationDetails": "Electrical"
    },
    {
      "specificationID": 102,
      "instanceNumber": 1,
      "specificationDetails": "8m"
    },
    {
      "specificationID": 102,
      "instanceNumber": 2,
      "specificationDetails": "10m"
    },
    {
      "specificationID": 102,
      "instanceNumber": 3,
      "specificationDetails": "12m"
    }
  ]
}
```

Controller receives → Repository calls SP → Database saves:
| SurveyID | LocID | ItemID | SpecificationID | InstanceNumber | SpecificationDetails |
|----------|-------|--------|-----------------|----------------|---------------------|
| 123 | 45 | 67 | 101 | 1 | Telecom |
| 123 | 45 | 67 | 101 | 2 | Electrical |
| 123 | 45 | 67 | 102 | 1 | 8m |
| 123 | 45 | 67 | 102 | 2 | 10m |
| 123 | 45 | 67 | 102 | 3 | 12m |

## Testing Checklist

- [x] Database schema updated with InstanceNumber column
- [x] Primary key updated to include InstanceNumber
- [x] Stored procedures created and tested
- [x] Model classes updated with InstanceNumber property
- [x] Repository updated to use stored procedure
- [x] JavaScript updated to collect instanceNumber
- [x] Build successful (0 errors)
- [x] Database configuration verified (9 options for 2 specs)

## Test the Feature

1. **Start Application**

   ```bash
   cd "d:\VL Access\Survey\CODES\Survey\survey"
   dotnet run
   ```

2. **Navigate to Survey Details**

   - Login → Dashboard → Select Survey → Edit Location → Item Selection

3. **Add Pole Item**

   - Search for "pole" item
   - Set Existing Qty = 2
   - Set Required Qty = 3

4. **Verify Specifications Render**

   - Should see "Pole Owner #1" and "Pole Owner #2" dropdowns
   - Should see "Pole Height #1", "#2", "#3" dropdowns

5. **Fill and Submit**

   - Select values for all instances
   - Click Save
   - Check browser console for "Saving specifications:" log
   - Check server console for "Specification saved successfully" logs

6. **Verify Database**
   ```sql
   SELECT * FROM SpecificationDetailsMaster
   WHERE SurveyID = [your_survey_id]
     AND ItemID = [pole_item_id]
   ORDER BY SpecificationID, InstanceNumber;
   ```

## Files Changed

**Database:**

- `SqlScripts/UPDATE_PoleSpecifications_Schema.sql`
- `SqlScripts/CREATE_SpecificationStoredProcedures.sql`

**Backend:**

- `Models/ItemSpecificationModel.cs`
- `Repo/SurveyRepo.cs`
- `Controllers/SurveyDetailsController.cs`

**Frontend:**

- `Views/SurveyDetails/ItemMasterSelection.cshtml`
- `wwwroot/js/item-specifications.js`
- `wwwroot/js/item-specifications-conditional.js` (NEW)

**Documentation:**

- `POLE_SPECIFICATIONS_COMPLETE_IMPLEMENTATION.md`
- `POLE_SPECIFICATIONS_QUICK_REFERENCE.md` (this file)

## Common Issues

### Issue: Specifications not showing up

**Solution:** Check browser console for JavaScript errors, verify ItemID has specifications in database

### Issue: Specifications saving with InstanceNumber = 1 for all

**Solution:** Verify JavaScript is setting `data-instance` attribute on inputs

### Issue: Primary key violation

**Solution:** Should be fixed now - stored procedure handles MERGE correctly with InstanceNumber

### Issue: Values not loading on edit

**Solution:** Check GetSpecificationDetails includes InstanceNumber in WHERE clause

## Support

For issues, check:

1. Browser console (F12) for JavaScript errors
2. Server console for repository/SP errors
3. SQL Server Profiler to see actual SP calls
4. Database directly with test queries
