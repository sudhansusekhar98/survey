-- =====================================================
-- Fix SpSurvey SpType=5: Add "Control Room" location type
-- with preselected device modules
-- =====================================================
-- Control Room preselected module IDs:
--   110  - PoE Switch
--   101  - UPS
--   104  - Patch Panel
--   111  - Non PoE Switch
--   2104 - Workstation
--   2105 - Display
--   2106 - Digital Audio-Video Cable
--   2107 - Server Infrastructure
--   2108 - Storage Infrastructure
--   2109 - Network Security
--   112  - Patch Cord
-- =====================================================

-- Get current SP definition
DECLARE @spDef NVARCHAR(MAX)
SET @spDef = OBJECT_DEFINITION(OBJECT_ID('dbo.SpSurvey'))

-- Replace the old IF/ELSE block in SpType=5 with IF/ELSE IF/ELSE
-- Old: two branches (Traffic / else-Surveillance)
-- New: three branches (Traffic / Control Room / else-Surveillance)

-- The key section to replace is:
-- OLD:
--   if @LocationType = 'Traffic'
--     Begin
--     INSERT ... case when ID = 100 OR ID = 103 OR ID = 109 OR ID = 1104 then 1 ...
--     end
--   else
--     begin
--     INSERT ... case when ID = 100 OR ID = 103 OR ID = 109 then 1 ...
--     end
--
-- NEW:
--   if @LocationType = 'Traffic'
--     Begin
--     INSERT ... case when ID = 100 OR ID = 103 OR ID = 109 OR ID = 1104 then 1 ...
--     end
--   else if @LocationType = 'Control Room'
--     begin
--     INSERT ... case when ID IN (110,101,104,111,2104,2105,2106,2107,2108,2109,112) then 1 ...
--     end
--   else
--     begin
--     INSERT ... case when ID = 100 OR ID = 103 OR ID = 109 then 1 ...
--     end

-- Find and replace the specific text
SET @spDef = REPLACE(@spDef,
    'else
        begin
         INSERT INTO AssignedItems (SurveyId, LocID, ItemTypeID, TypeName, IsAssigned, CreatedOn, CreatedBy)
        SELECT @SurveyID, @LocID, Id, TypeName
            , case when ID = 100 OR ID = 103 OR ID = 109 then 1 Else 0 End Assigend
            , SYSDATETIME(), @CreatedBy FROM ItemTypeMaster WHERE  IsActive=1;
        end',
    'else if @LocationType = ''Control Room''
        begin
         INSERT INTO AssignedItems (SurveyId, LocID, ItemTypeID, TypeName, IsAssigned, CreatedOn, CreatedBy)
        SELECT @SurveyID, @LocID, Id, TypeName
            , case when ID IN (110, 101, 104, 111, 2104, 2105, 2106, 2107, 2108, 2109, 112) then 1 Else 0 End Assigend
            , SYSDATETIME(), @CreatedBy FROM ItemTypeMaster WHERE  IsActive=1;
        end
    else
        begin
         INSERT INTO AssignedItems (SurveyId, LocID, ItemTypeID, TypeName, IsAssigned, CreatedOn, CreatedBy)
        SELECT @SurveyID, @LocID, Id, TypeName
            , case when ID = 100 OR ID = 103 OR ID = 109 then 1 Else 0 End Assigend
            , SYSDATETIME(), @CreatedBy FROM ItemTypeMaster WHERE  IsActive=1;
        end'
)

-- Change CREATE to ALTER
SET @spDef = REPLACE(@spDef, 'CREATE PROCEDURE', 'ALTER PROCEDURE')

-- Execute the ALTER
EXEC sp_executesql @spDef

PRINT 'SpSurvey updated successfully - Control Room location type now has preselected device modules.'
GO

-- Verify the change was applied
DECLARE @verify NVARCHAR(MAX)
SET @verify = OBJECT_DEFINITION(OBJECT_ID('dbo.SpSurvey'))
IF CHARINDEX('Control Room', @verify) > 0
    PRINT 'VERIFICATION PASSED: Control Room branch found in SpSurvey.'
ELSE
    PRINT 'VERIFICATION FAILED: Control Room branch NOT found in SpSurvey!'
GO
