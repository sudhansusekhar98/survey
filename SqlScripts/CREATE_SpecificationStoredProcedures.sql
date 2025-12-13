-- =============================================
-- Stored Procedures for Item Specifications
-- Database: VLDev
-- Date: December 12, 2024
-- =============================================

USE [VLDev]
GO

PRINT '=== Creating/Updating Specification Stored Procedures ==='
PRINT ''

-- =============================================
-- 1. SP: Save/Update Specification Details (UPSERT)
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpSaveSpecificationDetails]') AND type = N'P')
BEGIN
    DROP PROCEDURE [dbo].[SpSaveSpecificationDetails];
    PRINT '→ Dropped existing SpSaveSpecificationDetails';
END
GO

CREATE PROCEDURE [dbo].[SpSaveSpecificationDetails]
    @SurveyID BIGINT,
    @LocID INT,
    @ItemID INT,
    @SpecificationID INT,
    @InstanceNumber INT = 1,
    @SpecificationDetails VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Use MERGE to handle insert or update
        MERGE INTO SpecificationDetailsMaster AS target
        USING (
            SELECT 
                @SurveyID AS SurveyID, 
                @LocID AS LocID, 
                @ItemID AS ItemID, 
                @SpecificationID AS SpecificationID,
                @InstanceNumber AS InstanceNumber
        ) AS source
        ON (
            target.SurveyID = source.SurveyID 
            AND target.LocID = source.LocID 
            AND target.ItemID = source.ItemID 
            AND target.SpecificationID = source.SpecificationID
            AND target.InstanceNumber = source.InstanceNumber
        )
        WHEN MATCHED THEN
            UPDATE SET SpecificationDetails = @SpecificationDetails
        WHEN NOT MATCHED THEN
            INSERT (SurveyID, LocID, ItemID, SpecificationID, InstanceNumber, SpecificationDetails)
            VALUES (@SurveyID, @LocID, @ItemID, @SpecificationID, @InstanceNumber, @SpecificationDetails);
        
        -- Return success
        SELECT 1 AS Success, 'Specification saved successfully' AS Message;
    END TRY
    BEGIN CATCH
        -- Return error
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

PRINT '✓ Created SpSaveSpecificationDetails';
GO

-- =============================================
-- 2. SP: Delete Specification Details
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpDeleteSpecificationDetails]') AND type = N'P')
BEGIN
    DROP PROCEDURE [dbo].[SpDeleteSpecificationDetails];
    PRINT '→ Dropped existing SpDeleteSpecificationDetails';
END
GO

CREATE PROCEDURE [dbo].[SpDeleteSpecificationDetails]
    @SurveyID BIGINT,
    @LocID INT,
    @ItemID INT,
    @SpecificationID INT = NULL,
    @InstanceNumber INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF @SpecificationID IS NULL
        BEGIN
            -- Delete all specifications for the item
            DELETE FROM SpecificationDetailsMaster
            WHERE SurveyID = @SurveyID
              AND LocID = @LocID
              AND ItemID = @ItemID;
        END
        ELSE IF @InstanceNumber IS NULL
        BEGIN
            -- Delete all instances of a specific specification
            DELETE FROM SpecificationDetailsMaster
            WHERE SurveyID = @SurveyID
              AND LocID = @LocID
              AND ItemID = @ItemID
              AND SpecificationID = @SpecificationID;
        END
        ELSE
        BEGIN
            -- Delete specific instance
            DELETE FROM SpecificationDetailsMaster
            WHERE SurveyID = @SurveyID
              AND LocID = @LocID
              AND ItemID = @ItemID
              AND SpecificationID = @SpecificationID
              AND InstanceNumber = @InstanceNumber;
        END
        
        SELECT 1 AS Success, 'Specification(s) deleted successfully' AS Message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

PRINT '✓ Created SpDeleteSpecificationDetails';
GO

-- =============================================
-- 3. SP: Get Specification Details for Item
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpGetSpecificationDetails]') AND type = N'P')
BEGIN
    DROP PROCEDURE [dbo].[SpGetSpecificationDetails];
    PRINT '→ Dropped existing SpGetSpecificationDetails';
END
GO

CREATE PROCEDURE [dbo].[SpGetSpecificationDetails]
    @SurveyID BIGINT,
    @LocID INT,
    @ItemID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        sd.SurveyID,
        sd.LocID,
        sd.ItemID,
        sd.SpecificationID,
        sd.InstanceNumber,
        sd.SpecificationDetails,
        sm.SpecificationName,
        sm.InputType,
        sm.ConditionalDisplay,
        sm.AllowMultipleInstances
    FROM SpecificationDetailsMaster sd
    INNER JOIN ItemSpecificationMaster sm 
        ON sd.ItemID = sm.ItemId 
        AND sd.SpecificationID = sm.SpecificationID
    WHERE sd.SurveyID = @SurveyID
      AND sd.LocID = @LocID
      AND sd.ItemID = @ItemID
    ORDER BY sd.SpecificationID, sd.InstanceNumber;
END
GO

PRINT '✓ Created SpGetSpecificationDetails';
GO

-- =============================================
-- 4. SP: Bulk Save Specification Details
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpBulkSaveSpecificationDetails]') AND type = N'P')
BEGIN
    DROP PROCEDURE [dbo].[SpBulkSaveSpecificationDetails];
    PRINT '→ Dropped existing SpBulkSaveSpecificationDetails';
END
GO

CREATE PROCEDURE [dbo].[SpBulkSaveSpecificationDetails]
    @SurveyID BIGINT,
    @LocID INT,
    @ItemID INT,
    @SpecificationsJSON NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Parse JSON and save each specification
        -- Expected JSON format: [{"specificationID":101,"instanceNumber":1,"specificationDetails":"Telecom"},...]
        
        DECLARE @SpecID INT, @Instance INT, @Details VARCHAR(100);
        
        -- Use OPENJSON to parse the JSON array
        DECLARE spec_cursor CURSOR FOR
        SELECT 
            JSON_VALUE(value, '$.specificationID') AS SpecificationID,
            ISNULL(JSON_VALUE(value, '$.instanceNumber'), 1) AS InstanceNumber,
            JSON_VALUE(value, '$.specificationDetails') AS SpecificationDetails
        FROM OPENJSON(@SpecificationsJSON);
        
        OPEN spec_cursor;
        FETCH NEXT FROM spec_cursor INTO @SpecID, @Instance, @Details;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Execute the save SP for each specification
            EXEC SpSaveSpecificationDetails 
                @SurveyID = @SurveyID,
                @LocID = @LocID,
                @ItemID = @ItemID,
                @SpecificationID = @SpecID,
                @InstanceNumber = @Instance,
                @SpecificationDetails = @Details;
            
            FETCH NEXT FROM spec_cursor INTO @SpecID, @Instance, @Details;
        END
        
        CLOSE spec_cursor;
        DEALLOCATE spec_cursor;
        
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 'All specifications saved successfully' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO

PRINT '✓ Created SpBulkSaveSpecificationDetails';
GO

-- =============================================
-- TESTING
-- =============================================

PRINT ''
PRINT '=== Testing Stored Procedures ==='
PRINT ''

-- Test 1: Save a specification detail
PRINT '--- Test 1: Save Pole Owner ---'
EXEC SpSaveSpecificationDetails 
    @SurveyID = 1, 
    @LocID = 1, 
    @ItemID = 1000037, 
    @SpecificationID = 101, 
    @InstanceNumber = 1,
    @SpecificationDetails = 'Telecom';
GO

-- Test 2: Save another instance
PRINT ''
PRINT '--- Test 2: Save Pole Height ---'
EXEC SpSaveSpecificationDetails 
    @SurveyID = 1, 
    @LocID = 1, 
    @ItemID = 1000037, 
    @SpecificationID = 102, 
    @InstanceNumber = 1,
    @SpecificationDetails = '4m';
GO

-- Test 3: Get saved specifications
PRINT ''
PRINT '--- Test 3: Get Specifications ---'
EXEC SpGetSpecificationDetails 
    @SurveyID = 1, 
    @LocID = 1, 
    @ItemID = 1000037;
GO

-- Test 4: Update existing specification
PRINT ''
PRINT '--- Test 4: Update Specification ---'
EXEC SpSaveSpecificationDetails 
    @SurveyID = 1, 
    @LocID = 1, 
    @ItemID = 1000037, 
    @SpecificationID = 101, 
    @InstanceNumber = 1,
    @SpecificationDetails = 'Electrical';
GO

-- Verify update
EXEC SpGetSpecificationDetails 
    @SurveyID = 1, 
    @LocID = 1, 
    @ItemID = 1000037;
GO

PRINT ''
PRINT '=== All Stored Procedures Created Successfully ==='
GO
