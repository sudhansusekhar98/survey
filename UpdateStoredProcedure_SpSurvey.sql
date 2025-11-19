-- =============================================
-- Update Stored Procedure: dbo.SpSurvey
-- Purpose: Fix 'ZoneSectorWardNumber' error and add RegionName from RegionMaster
-- Date: November 17, 2025
-- =============================================

-- IMPORTANT: This script updates the SpSurvey stored procedure to:
-- 1. Remove references to the deleted column 'ZoneSectorWardNumber'
-- 2. Add JOIN with RegionMaster table to get RegionName for surveys
-- 3. Add JOIN with EmpMaster table to get EmpName for survey assignments
-- 4. Return RegionID and RegionName in survey result sets
-- 5. Return EmpID and EmpName in survey assignment result sets

-- =============================================
-- For SpType = 2 (GetAllSurveys)
-- =============================================
-- UPDATE THIS SECTION IN YOUR SpSurvey STORED PROCEDURE:
-- Replace the SELECT statement for @SpType = 2 with:

/*
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
*/

-- =============================================
-- For SpType = 7 (GetSurveyById)
-- =============================================
-- UPDATE THIS SECTION IN YOUR SpSurvey STORED PROCEDURE:
-- Replace the SELECT statement for @SpType = 7 with:

/*
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
*/

-- =============================================
-- For SpType = 16 (GetSurveyAssignments)
-- =============================================
-- UPDATE THIS SECTION IN YOUR SpSurvey STORED PROCEDURE:
-- Replace the SELECT statement for @SpType = 16 with:

/*
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
*/

-- =============================================
-- INSTRUCTIONS:
-- =============================================
-- 1. Open SQL Server Management Studio (SSMS)
-- 2. Connect to your VLDev database
-- 3. Find and open the stored procedure: dbo.SpSurvey
-- 4. Locate the sections with @SpType = 2, @SpType = 7, and @SpType = 16
-- 5. Replace the SELECT statements with the ones provided above
-- 6. Make sure to remove any references to 'ZoneSectorWardNumber' column
-- 7. Execute the ALTER PROCEDURE statement to save changes
-- 8. Test by running: EXEC dbo.SpSurvey @SpType = 2
-- =============================================
