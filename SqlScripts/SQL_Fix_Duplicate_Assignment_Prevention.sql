-- Fix for Duplicate Assignment Prevention
-- This script adds duplicate checking to the SpSurvey stored procedure for SpType = 17 (Add Survey Assignment)
-- Run this script against your VLDev database

USE [VLDev]
GO

PRINT 'Adding duplicate prevention for survey assignments...'
GO

-- First, check if there's a unique constraint on the SurveyAssignment table
-- If not, we'll add it at the database level for extra safety

-- Check for existing duplicates first
SELECT 
    SurveyID, 
    EmpID, 
    COUNT(*) as DuplicateCount
FROM SurveyAssignment
GROUP BY SurveyID, EmpID
HAVING COUNT(*) > 1;

-- If duplicates exist, you'll need to clean them up first:
-- Keep the first occurrence and delete the rest
;WITH CTE AS (
    SELECT 
        TransID,
        ROW_NUMBER() OVER (PARTITION BY SurveyID, EmpID ORDER BY TransID) as RowNum
    FROM SurveyAssignment
)
DELETE FROM CTE WHERE RowNum > 1;

PRINT 'Existing duplicates cleaned up (if any).'
GO

-- Add unique constraint to prevent future duplicates at database level
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'UQ_SurveyAssignment_SurveyID_EmpID' 
    AND object_id = OBJECT_ID('SurveyAssignment')
)
BEGIN
    ALTER TABLE SurveyAssignment
    ADD CONSTRAINT UQ_SurveyAssignment_SurveyID_EmpID 
    UNIQUE (SurveyID, EmpID);
    
    PRINT 'Unique constraint added successfully.';
END
ELSE
BEGIN
    PRINT 'Unique constraint already exists.';
END
GO

-- Update the SpSurvey stored procedure to check for duplicates before inserting
-- You need to modify the SpType = 17 section in your existing SpSurvey procedure

PRINT '
--------------------------------------------------------------------
MANUAL UPDATE REQUIRED:
--------------------------------------------------------------------
Update the SpType = 17 section in your dbo.SpSurvey stored procedure
with the following code:

ELSE IF @SpType = 17 -- Add Survey Assignment with duplicate check
BEGIN
    -- Check if assignment already exists
    IF EXISTS (SELECT 1 FROM SurveyAssignment WHERE SurveyID = @SurveyID AND EmpID = @EmpID)
    BEGIN
        -- Return 0 to indicate duplicate (no insertion)
        SELECT 0 as Result, ''Employee already assigned to this survey'' as Message;
        RETURN;
    END
    
    INSERT INTO SurveyAssignment (SurveyID, EmpID, DueDate, CreatedBy, CreatedDate)
    VALUES (@SurveyID, @EmpID, @DueDate, @CreatedBy, GETDATE());
    
    SELECT 1 as Result, ''Assignment created successfully'' as Message;
END

--------------------------------------------------------------------
'
GO

PRINT 'Script completed. Please update the SpSurvey stored procedure manually.'
GO
