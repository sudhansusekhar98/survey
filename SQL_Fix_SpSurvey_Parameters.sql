-- Fix for dbo.SpSurvey - Ensure @SurveyID parameter is declared
-- Run this script to verify and fix the stored procedure parameter declarations

-- First, check the current procedure definition
-- You can run: sp_helptext 'dbo.SpSurvey' to see the full procedure

-- The stored procedure should have these parameters declared at the top:
-- Make sure these parameters are included in your SP:

/*
ALTER PROCEDURE dbo.SpSurvey
    @SpType INT,
    @SurveyID BIGINT = NULL,          -- REQUIRED for SpType 10
    @SurveyName VARCHAR(200) = NULL,
    @LocID INT = NULL,                -- REQUIRED for SpType 10
    @ItemTypeID INT = NULL,           -- REQUIRED for SpType 10
    @IsAssigned INT = NULL,           -- REQUIRED for SpType 10
    @CreatedBy INT = NULL,            -- REQUIRED for SpType 10
    @CreatedOn DATETIME = NULL,
    @ImplementationType VARCHAR(100) = NULL,
    @SurveyDate VARCHAR(100) = NULL,
    @SurveyTeamName VARCHAR(200) = NULL,
    @SurveyTeamContact VARCHAR(50) = NULL,
    @AgencyName VARCHAR(200) = NULL,
    @LocationSiteName VARCHAR(200) = NULL,
    @CityDistrict VARCHAR(100) = NULL,
    @ZoneSectorWardNumber VARCHAR(100) = NULL,
    @ScopeOfWork VARCHAR(MAX) = NULL,
    @Latitude VARCHAR(50) = NULL,
    @Longitude VARCHAR(50) = NULL,
    @MapMarking VARCHAR(MAX) = NULL,
    @SurveyStatus VARCHAR(50) = NULL,
    @LocName VARCHAR(200) = NULL,
    @LocLat DECIMAL(18, 6) = NULL,
    @LocLog DECIMAL(18, 6) = NULL,
    @Isactive CHAR(1) = NULL
AS
BEGIN
    -- Your SpType logic here
    
    if @SpType = 10
    begin
        if exists (select * from AssignedItems where SurveyId=@SurveyID and LocID=@LocID and ItemTypeID=@ItemTypeID)
        begin
            update AssignedItems set IsAssigned = @IsAssigned where SurveyId=@SurveyID and LocID=@LocID and ItemTypeID=@ItemTypeID
        end
        else 
        begin
            INSERT INTO AssignedItems (SurveyId, LocID, ItemTypeID,TypeName,IsAssigned, CreatedOn, CreatedBy)
            VALUES (@SurveyID, @LocID, @ItemTypeID,(Select TypeName from ItemTypeMaster where Id=@ItemTypeID),@IsAssigned, SYSDATETIME(), @CreatedBy)
        end
    end
    
    -- Rest of your SpType logic...
END
*/

-- IMPORTANT: 
-- 1. Open SQL Server Management Studio
-- 2. Run: sp_helptext 'dbo.SpSurvey'
-- 3. Check if @SurveyID is declared in the parameter list
-- 4. If missing, add it: @SurveyID BIGINT = NULL
-- 5. Make sure the parameter name matches exactly (case-sensitive in parameter binding)
