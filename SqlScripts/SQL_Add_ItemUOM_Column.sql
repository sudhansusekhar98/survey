-- =============================================
-- Add ItemUOM column to ItemMaster table if it doesn't exist
-- =============================================
USE [VLDev]
GO

-- Check if column exists and add if not
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ItemMaster' 
    AND COLUMN_NAME = 'ItemUOM'
)
BEGIN
    PRINT 'Adding ItemUOM column to ItemMaster table...'
    
    ALTER TABLE [dbo].[ItemMaster]
    ADD [ItemUOM] NVARCHAR(50) NULL;
    
    PRINT 'ItemUOM column added successfully!'
END
ELSE
BEGIN
    PRINT 'ItemUOM column already exists in ItemMaster table.'
END
GO
