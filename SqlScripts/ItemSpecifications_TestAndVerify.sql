-- =============================================
-- Script: ItemSpecifications_TestAndVerify.sql
-- Description: Test scripts for Item Specifications feature
-- =============================================

-- =============================================
-- 1. VIEW EXISTING DATA IN SPECIFICATION TABLES
-- =============================================

-- View ItemSpecificationMaster data
PRINT '=== ItemSpecificationMaster Contents ==='
SELECT [ItemId], [SpecificationID], [SpecificationName]
FROM [VLDev].[dbo].[ItemSpecificationMaster]
ORDER BY ItemId, SpecificationID;
GO

-- View SpecificationDetailsMaster data
PRINT ''
PRINT '=== SpecificationDetailsMaster Contents ==='
SELECT [SurveyID], [LocID], [ItemID], [SpecificationID], [SpecificationDetails]
FROM [VLDev].[dbo].[SpecificationDetailsMaster]
ORDER BY SurveyID, LocID, ItemID, SpecificationID;
GO

-- =============================================
-- 2. TEST LOADING SPECIFICATIONS FOR AN ITEM
-- =============================================

-- Example: Load specifications for ItemId = 1000037 (should show Pole Owner and Height)
PRINT ''
PRINT '=== Test: Load Specifications for ItemId 1000037 ==='
SELECT ItemId, SpecificationID, SpecificationName
FROM ItemSpecificationMaster
WHERE ItemId = 1000037
ORDER BY SpecificationID;
GO

-- Example: Load specifications for ItemId = 1000004 (should show Road Width)
PRINT ''
PRINT '=== Test: Load Specifications for ItemId 1000004 ==='
SELECT ItemId, SpecificationID, SpecificationName
FROM ItemSpecificationMaster
WHERE ItemId = 1000004
ORDER BY SpecificationID;
GO

-- =============================================
-- 3. TEST SAVING SPECIFICATION DETAILS
-- =============================================

-- Test insert/update (MERGE) for specification details
-- Replace @TestSurveyID, @TestLocID with actual values from your test data

DECLARE @TestSurveyID BIGINT = 1;  -- Replace with a valid SurveyID
DECLARE @TestLocID INT = 1;         -- Replace with a valid LocID
DECLARE @TestItemID INT = 1000037;  -- Item with specifications (Pole)
DECLARE @TestSpecID INT = 101;      -- Pole Owner specification
DECLARE @TestValue NVARCHAR(500) = 'Telecom';

-- Upsert specification detail
MERGE INTO SpecificationDetailsMaster AS target
USING (SELECT @TestSurveyID AS SurveyID, @TestLocID AS LocID, @TestItemID AS ItemID, @TestSpecID AS SpecificationID) AS source
ON (target.SurveyID = source.SurveyID 
    AND target.LocID = source.LocID 
    AND target.ItemID = source.ItemID 
    AND target.SpecificationID = source.SpecificationID)
WHEN MATCHED THEN
    UPDATE SET SpecificationDetails = @TestValue
WHEN NOT MATCHED THEN
    INSERT (SurveyID, LocID, ItemID, SpecificationID, SpecificationDetails)
    VALUES (@TestSurveyID, @TestLocID, @TestItemID, @TestSpecID, @TestValue);

PRINT 'Test specification saved successfully.';
GO

-- =============================================
-- 4. VERIFY SAVED DATA
-- =============================================

-- Check the saved specification
PRINT ''
PRINT '=== Verify: Saved Specification Details ==='
SELECT 
    sd.SurveyID, 
    sd.LocID, 
    sd.ItemID, 
    sd.SpecificationID, 
    sd.SpecificationDetails,
    sm.SpecificationName
FROM SpecificationDetailsMaster sd
LEFT JOIN ItemSpecificationMaster sm 
    ON sd.ItemID = sm.ItemId AND sd.SpecificationID = sm.SpecificationID
ORDER BY sd.SurveyID, sd.LocID, sd.ItemID, sd.SpecificationID;
GO

-- =============================================
-- 5. HELPER: LIST ALL ITEMS WITH THEIR SPECIFICATIONS
-- =============================================

PRINT ''
PRINT '=== All Items with Specifications ==='
SELECT 
    im.ItemID,
    im.ItemName,
    im.ItemCode,
    sm.SpecificationID,
    sm.SpecificationName
FROM ItemMaster im
INNER JOIN ItemSpecificationMaster sm ON im.ItemID = sm.ItemId
ORDER BY im.ItemID, sm.SpecificationID;
GO

-- =============================================
-- 6. SAMPLE: ADD NEW SPECIFICATION FOR AN ITEM
-- (Run if you need to add specifications for testing)
-- =============================================

/*
-- Uncomment and modify as needed:

-- Add "Cable Type" specification for an item
INSERT INTO ItemSpecificationMaster (ItemId, SpecificationID, SpecificationName)
VALUES (1000050, 200, 'Cable Type');

-- Add "Installation Height" specification for an item
INSERT INTO ItemSpecificationMaster (ItemId, SpecificationID, SpecificationName)
VALUES (1000050, 201, 'Installation Height');

*/

PRINT ''
PRINT '=== Script Completed Successfully ==='
