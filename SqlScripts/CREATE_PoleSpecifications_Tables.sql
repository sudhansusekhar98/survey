-- =============================================
-- Script: CREATE_PoleSpecifications_Tables.sql
-- Description: Create tables and data for pole specifications
-- Date: December 12, 2024
-- =============================================

USE [VLDev]
GO

-- =============================================
-- 1. CREATE SPECIFICATION OPTIONS TABLE
-- =============================================

-- This table stores dropdown options for specifications
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationOptionsMaster]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ItemSpecificationOptionsMaster](
        [OptionID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SpecificationID] INT NOT NULL,
        [OptionValue] NVARCHAR(100) NOT NULL,
        [OptionText] NVARCHAR(100) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_SpecOptions_SpecMaster FOREIGN KEY (SpecificationID) 
            REFERENCES ItemSpecificationMaster(SpecificationID)
    );

    PRINT 'Table ItemSpecificationOptionsMaster created successfully';
END
ELSE
BEGIN
    PRINT 'Table ItemSpecificationOptionsMaster already exists';
END
GO

-- =============================================
-- 2. ADD COLUMNS TO ItemSpecificationMaster IF NEEDED
-- =============================================

-- Add ConditionalDisplay column to specify when to show the field
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationMaster]') AND name = 'ConditionalDisplay')
BEGIN
    ALTER TABLE [dbo].[ItemSpecificationMaster]
    ADD [ConditionalDisplay] NVARCHAR(50) NULL; -- Values: 'Always', 'ExistingQtyOnly', 'RequiredQtyOnly', 'BothQty'
    
    PRINT 'Column ConditionalDisplay added to ItemSpecificationMaster';
END
GO

-- Add AllowMultipleInstances column (for handling multiple poles with different values)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationMaster]') AND name = 'AllowMultipleInstances')
BEGIN
    ALTER TABLE [dbo].[ItemSpecificationMaster]
    ADD [AllowMultipleInstances] BIT NOT NULL DEFAULT 0;
    
    PRINT 'Column AllowMultipleInstances added to ItemSpecificationMaster';
END
GO

-- =============================================
-- 3. UPDATE SpecificationDetailsMaster FOR MULTIPLE INSTANCES
-- =============================================

-- Add InstanceNumber column to support multiple pole instances
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SpecificationDetailsMaster]') AND name = 'InstanceNumber')
BEGIN
    ALTER TABLE [dbo].[SpecificationDetailsMaster]
    ADD [InstanceNumber] INT NOT NULL DEFAULT 1;
    
    PRINT 'Column InstanceNumber added to SpecificationDetailsMaster';
    
    -- Drop existing PK if exists
    DECLARE @pkName NVARCHAR(128);
    SELECT @pkName = name 
    FROM sys.key_constraints 
    WHERE type = 'PK' AND parent_object_id = OBJECT_ID('SpecificationDetailsMaster');
    
    IF @pkName IS NOT NULL
    BEGIN
        DECLARE @sql NVARCHAR(MAX) = 'ALTER TABLE SpecificationDetailsMaster DROP CONSTRAINT ' + @pkName;
        EXEC sp_executesql @sql;
        PRINT 'Dropped existing primary key';
    END
    
    -- Create new composite PK including InstanceNumber
    ALTER TABLE [dbo].[SpecificationDetailsMaster]
    ADD CONSTRAINT PK_SpecificationDetailsMaster 
        PRIMARY KEY (SurveyID, LocID, ItemID, SpecificationID, InstanceNumber);
    
    PRINT 'New composite primary key created';
END
GO

-- =============================================
-- 4. INSERT POLE SPECIFICATIONS
-- =============================================

-- First, find the ItemID for POLE items (assuming ItemCode contains 'POLE')
DECLARE @PoleItemID INT;
SELECT TOP 1 @PoleItemID = ItemID 
FROM ItemMaster 
WHERE ItemCode LIKE '%POLE%' OR ItemName LIKE '%POLE%'
ORDER BY ItemID;

IF @PoleItemID IS NOT NULL
BEGIN
    PRINT 'Found Pole ItemID: ' + CAST(@PoleItemID AS NVARCHAR(10));
    
    -- Insert Pole Owner specification (101)
    IF NOT EXISTS (SELECT 1 FROM ItemSpecificationMaster WHERE ItemId = @PoleItemID AND SpecificationID = 101)
    BEGIN
        INSERT INTO ItemSpecificationMaster (ItemId, SpecificationID, SpecificationName, InputType, Options, ConditionalDisplay, AllowMultipleInstances)
        VALUES (@PoleItemID, 101, 'Pole Owner', 'dropdown', NULL, 'ExistingQtyOnly', 1);
        
        PRINT 'Inserted Pole Owner specification';
    END
    ELSE
    BEGIN
        UPDATE ItemSpecificationMaster 
        SET InputType = 'dropdown', 
            ConditionalDisplay = 'ExistingQtyOnly',
            AllowMultipleInstances = 1
        WHERE ItemId = @PoleItemID AND SpecificationID = 101;
        
        PRINT 'Updated Pole Owner specification';
    END
    
    -- Insert Pole Height specification (102)
    IF NOT EXISTS (SELECT 1 FROM ItemSpecificationMaster WHERE ItemId = @PoleItemID AND SpecificationID = 102)
    BEGIN
        INSERT INTO ItemSpecificationMaster (ItemId, SpecificationID, SpecificationName, InputType, Options, ConditionalDisplay, AllowMultipleInstances)
        VALUES (@PoleItemID, 102, 'Pole Height', 'dropdown', NULL, 'RequiredQtyOnly', 1);
        
        PRINT 'Inserted Pole Height specification';
    END
    ELSE
    BEGIN
        UPDATE ItemSpecificationMaster 
        SET InputType = 'dropdown',
            ConditionalDisplay = 'RequiredQtyOnly',
            AllowMultipleInstances = 1
        WHERE ItemId = @PoleItemID AND SpecificationID = 102;
        
        PRINT 'Updated Pole Height specification';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: No POLE item found in ItemMaster. Please insert specifications manually.';
END
GO

-- =============================================
-- 5. INSERT POLE OWNER OPTIONS
-- =============================================

-- Clear existing options for Pole Owner (SpecificationID = 101)
DELETE FROM ItemSpecificationOptionsMaster WHERE SpecificationID = 101;

INSERT INTO ItemSpecificationOptionsMaster (SpecificationID, OptionValue, OptionText, DisplayOrder)
VALUES 
    (101, 'Telecom', 'Telecom', 1),
    (101, 'Electrical', 'Electrical', 2),
    (101, 'Municipality', 'Municipality', 3);

PRINT 'Inserted Pole Owner options';
GO

-- =============================================
-- 6. INSERT POLE HEIGHT OPTIONS
-- =============================================

-- Clear existing options for Pole Height (SpecificationID = 102)
DELETE FROM ItemSpecificationOptionsMaster WHERE SpecificationID = 102;

INSERT INTO ItemSpecificationOptionsMaster (SpecificationID, OptionValue, OptionText, DisplayOrder)
VALUES 
    (102, '4m', '4 meters', 1),
    (102, '5m', '5 meters', 2),
    (102, '6.5m', '6.5 meters', 3),
    (102, '8m', '8 meters', 4),
    (102, '10m', '10 meters', 5),
    (102, '12m', '12 meters', 6);

PRINT 'Inserted Pole Height options';
GO

-- =============================================
-- 7. VERIFY DATA
-- =============================================

PRINT '';
PRINT '=== VERIFICATION ===';
PRINT '';

PRINT '--- ItemSpecificationMaster for POLE items ---';
SELECT 
    sm.ItemId,
    im.ItemName,
    sm.SpecificationID,
    sm.SpecificationName,
    sm.InputType,
    sm.ConditionalDisplay,
    sm.AllowMultipleInstances
FROM ItemSpecificationMaster sm
INNER JOIN ItemMaster im ON sm.ItemId = im.ItemID
WHERE im.ItemCode LIKE '%POLE%' OR im.ItemName LIKE '%POLE%'
ORDER BY sm.ItemId, sm.SpecificationID;

PRINT '';
PRINT '--- Specification Options ---';
SELECT 
    so.OptionID,
    so.SpecificationID,
    sm.SpecificationName,
    so.OptionValue,
    so.OptionText,
    so.DisplayOrder,
    so.IsActive
FROM ItemSpecificationOptionsMaster so
INNER JOIN ItemSpecificationMaster sm ON so.SpecificationID = sm.SpecificationID
WHERE so.SpecificationID IN (101, 102)
ORDER BY so.SpecificationID, so.DisplayOrder;

PRINT '';
PRINT '=== Script Completed Successfully ===';
GO
