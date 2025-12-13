-- =============================================
-- Script: UPDATE_PoleSpecifications_Schema.sql
-- Description: Update schema for pole specifications with database-driven options
-- Date: December 12, 2024
-- =============================================

USE [VLDev]
GO

PRINT '=== Starting Schema Update ==='
PRINT ''

-- =============================================
-- 1. ADD COLUMNS TO ItemSpecificationMaster
-- =============================================

-- Add InputType column
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationMaster]') 
               AND name = 'InputType')
BEGIN
    ALTER TABLE [dbo].[ItemSpecificationMaster]
    ADD [InputType] VARCHAR(50) NULL;
    
    PRINT '✓ Added InputType column to ItemSpecificationMaster';
END
ELSE
BEGIN
    PRINT '→ InputType column already exists';
END
GO

-- Add ConditionalDisplay column
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationMaster]') 
               AND name = 'ConditionalDisplay')
BEGIN
    ALTER TABLE [dbo].[ItemSpecificationMaster]
    ADD [ConditionalDisplay] VARCHAR(50) NULL;
    
    PRINT '✓ Added ConditionalDisplay column to ItemSpecificationMaster';
END
ELSE
BEGIN
    PRINT '→ ConditionalDisplay column already exists';
END
GO

-- Add AllowMultipleInstances column
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationMaster]') 
               AND name = 'AllowMultipleInstances')
BEGIN
    ALTER TABLE [dbo].[ItemSpecificationMaster]
    ADD [AllowMultipleInstances] BIT NOT NULL DEFAULT 0;
    
    PRINT '✓ Added AllowMultipleInstances column to ItemSpecificationMaster';
END
ELSE
BEGIN
    PRINT '→ AllowMultipleInstances column already exists';
END
GO

-- =============================================
-- 2. CREATE OPTIONS TABLE
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.objects 
               WHERE object_id = OBJECT_ID(N'[dbo].[ItemSpecificationOptionsMaster]') 
               AND type = N'U')
BEGIN
    CREATE TABLE [dbo].[ItemSpecificationOptionsMaster](
        [OptionID] INT IDENTITY(1,1) NOT NULL,
        [SpecificationID] INT NOT NULL,
        [OptionValue] VARCHAR(100) NOT NULL,
        [OptionText] VARCHAR(100) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        CONSTRAINT [PK_ItemSpecificationOptionsMaster] PRIMARY KEY CLUSTERED ([OptionID] ASC)
    );

    PRINT '✓ Created ItemSpecificationOptionsMaster table';
END
ELSE
BEGIN
    PRINT '→ ItemSpecificationOptionsMaster table already exists';
END
GO

-- =============================================
-- 3. ADD InstanceNumber TO SpecificationDetailsMaster
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[SpecificationDetailsMaster]') 
               AND name = 'InstanceNumber')
BEGIN
    -- Add column with default value
    ALTER TABLE [dbo].[SpecificationDetailsMaster]
    ADD [InstanceNumber] INT NOT NULL DEFAULT 1;
    
    PRINT '✓ Added InstanceNumber column to SpecificationDetailsMaster';
    
    -- Drop existing PK if exists
    DECLARE @pkName NVARCHAR(128);
    SELECT @pkName = name 
    FROM sys.key_constraints 
    WHERE type = 'PK' 
    AND parent_object_id = OBJECT_ID('SpecificationDetailsMaster');
    
    IF @pkName IS NOT NULL
    BEGIN
        DECLARE @dropSql NVARCHAR(MAX) = 'ALTER TABLE SpecificationDetailsMaster DROP CONSTRAINT ' + QUOTENAME(@pkName);
        EXEC sp_executesql @dropSql;
        PRINT '✓ Dropped existing primary key: ' + @pkName;
    END
    
    -- Create new composite PK including InstanceNumber
    ALTER TABLE [dbo].[SpecificationDetailsMaster]
    ADD CONSTRAINT [PK_SpecificationDetailsMaster] 
        PRIMARY KEY CLUSTERED (SurveyID, LocID, ItemID, SpecificationID, InstanceNumber);
    
    PRINT '✓ Created new composite primary key with InstanceNumber';
END
ELSE
BEGIN
    PRINT '→ InstanceNumber column already exists';
END
GO

-- =============================================
-- 4. UPDATE EXISTING POLE SPECIFICATIONS
-- =============================================

PRINT ''
PRINT '=== Updating Pole Specifications ==='

-- Update Pole Owner (SpecificationID = 101)
UPDATE ItemSpecificationMaster
SET InputType = 'dropdown',
    ConditionalDisplay = 'ExistingQtyOnly',
    AllowMultipleInstances = 1
WHERE SpecificationID = 101;

PRINT '✓ Updated Pole Owner specification (ID: 101)';

-- Update Pole Height (SpecificationID = 102)  
UPDATE ItemSpecificationMaster
SET InputType = 'dropdown',
    ConditionalDisplay = 'RequiredQtyOnly',
    AllowMultipleInstances = 1
WHERE SpecificationID = 102;

PRINT '✓ Updated Pole Height specification (ID: 102)';

-- Update Road Width specifications (keep existing behavior)
UPDATE ItemSpecificationMaster
SET InputType = 'text',
    ConditionalDisplay = 'Always',
    AllowMultipleInstances = 0
WHERE SpecificationID IN (100, 103);

PRINT '✓ Updated Road Width specifications';
GO

-- =============================================
-- 5. INSERT POLE OWNER OPTIONS
-- =============================================

PRINT ''
PRINT '=== Inserting Pole Owner Options ==='

-- Clear existing options for Pole Owner
DELETE FROM ItemSpecificationOptionsMaster WHERE SpecificationID = 101;

-- Insert Pole Owner options
INSERT INTO ItemSpecificationOptionsMaster (SpecificationID, OptionValue, OptionText, DisplayOrder, IsActive)
VALUES 
    (101, 'Telecom', 'Telecom', 1, 1),
    (101, 'Electrical', 'Electrical', 2, 1),
    (101, 'Municipality', 'Municipality', 3, 1);

PRINT '✓ Inserted 3 Pole Owner options';
GO

-- =============================================
-- 6. INSERT POLE HEIGHT OPTIONS
-- =============================================

PRINT ''
PRINT '=== Inserting Pole Height Options ==='

-- Clear existing options for Pole Height
DELETE FROM ItemSpecificationOptionsMaster WHERE SpecificationID = 102;

-- Insert Pole Height options
INSERT INTO ItemSpecificationOptionsMaster (SpecificationID, OptionValue, OptionText, DisplayOrder, IsActive)
VALUES 
    (102, '4m', '4 meters', 1, 1),
    (102, '5m', '5 meters', 2, 1),
    (102, '6.5m', '6.5 meters', 3, 1),
    (102, '8m', '8 meters', 4, 1),
    (102, '10m', '10 meters', 5, 1),
    (102, '12m', '12 meters', 6, 1);

PRINT '✓ Inserted 6 Pole Height options';
GO

-- =============================================
-- 7. CREATE STORED PROCEDURE - Get Specification Options
-- =============================================

PRINT ''
PRINT '=== Creating Stored Procedures ==='

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpGetSpecificationOptions]') AND type = N'P')
BEGIN
    DROP PROCEDURE [dbo].[SpGetSpecificationOptions];
    PRINT '→ Dropped existing SpGetSpecificationOptions';
END
GO

CREATE PROCEDURE [dbo].[SpGetSpecificationOptions]
    @SpecificationID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        OptionID,
        SpecificationID,
        OptionValue,
        OptionText,
        DisplayOrder,
        IsActive
    FROM ItemSpecificationOptionsMaster
    WHERE SpecificationID = @SpecificationID
      AND IsActive = 1
    ORDER BY DisplayOrder, OptionText;
END
GO

PRINT '✓ Created SpGetSpecificationOptions stored procedure';
GO

-- =============================================
-- 8. CREATE STORED PROCEDURE - Get Item Specifications with Options
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpGetItemSpecificationsWithOptions]') AND type = N'P')
BEGIN
    DROP PROCEDURE [dbo].[SpGetItemSpecificationsWithOptions];
    PRINT '→ Dropped existing SpGetItemSpecificationsWithOptions';
END
GO

CREATE PROCEDURE [dbo].[SpGetItemSpecificationsWithOptions]
    @ItemID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get specifications for the item
    SELECT 
        sm.ItemId,
        sm.SpecificationID,
        sm.SpecificationName,
        sm.InputType,
        sm.ConditionalDisplay,
        sm.AllowMultipleInstances,
        -- Concatenate options for dropdown types
        STUFF((
            SELECT ',' + OptionValue
            FROM ItemSpecificationOptionsMaster opt
            WHERE opt.SpecificationID = sm.SpecificationID
              AND opt.IsActive = 1
            ORDER BY opt.DisplayOrder
            FOR XML PATH(''), TYPE
        ).value('.', 'VARCHAR(MAX)'), 1, 1, '') AS Options
    FROM ItemSpecificationMaster sm
    WHERE sm.ItemId = @ItemID
    ORDER BY sm.SpecificationID;
END
GO

PRINT '✓ Created SpGetItemSpecificationsWithOptions stored procedure';
GO

-- =============================================
-- 9. VERIFICATION
-- =============================================

PRINT ''
PRINT '=== VERIFICATION ==='
PRINT ''

PRINT '--- ItemSpecificationMaster Schema ---'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ItemSpecificationMaster'
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT '--- SpecificationDetailsMaster Schema ---'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SpecificationDetailsMaster'
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT '--- ItemSpecificationMaster Data ---'
SELECT 
    ItemId,
    SpecificationID,
    SpecificationName,
    InputType,
    ConditionalDisplay,
    AllowMultipleInstances
FROM ItemSpecificationMaster
ORDER BY ItemId, SpecificationID;

PRINT ''
PRINT '--- Specification Options ---'
SELECT 
    opt.OptionID,
    opt.SpecificationID,
    sm.SpecificationName,
    opt.OptionValue,
    opt.OptionText,
    opt.DisplayOrder
FROM ItemSpecificationOptionsMaster opt
INNER JOIN ItemSpecificationMaster sm ON opt.SpecificationID = sm.SpecificationID
WHERE opt.IsActive = 1
ORDER BY opt.SpecificationID, opt.DisplayOrder;

PRINT ''
PRINT '=== Schema Update Completed Successfully ==='
GO
