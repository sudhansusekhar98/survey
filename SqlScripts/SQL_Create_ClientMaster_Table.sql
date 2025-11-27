-- =============================================
-- Script: Create ClientMaster Table
-- Run this FIRST if ClientMaster table doesn't exist
-- =============================================

USE [VLDev]
GO

-- Check if table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClientMaster')
BEGIN
    PRINT 'Creating ClientMaster table...'
    
    CREATE TABLE [dbo].[ClientMaster](
        [ClientID] [int] IDENTITY(1,1) NOT NULL,
        [ClientName] [nvarchar](200) NOT NULL,
        [ClientType] [nvarchar](100) NOT NULL,
        [Address1] [nvarchar](500) NULL,
        [Address3] [nvarchar](500) NULL,
        [State] [nvarchar](100) NULL,
        [City] [nvarchar](100) NULL,
        [ContactPerson] [nvarchar](200) NULL,
        [ContactNumber] [nvarchar](20) NULL,
        [Isactive] [bit] NOT NULL DEFAULT 1,
        [CreatedOn] [datetime] NULL DEFAULT GETDATE(),
        [CreatedBy] [int] NULL,
        CONSTRAINT [PK_ClientMaster] PRIMARY KEY CLUSTERED ([ClientID] ASC)
    )
    
    PRINT 'ClientMaster table created successfully!'
END
ELSE
BEGIN
    PRINT 'ClientMaster table already exists.'
END
GO

-- Verify table structure
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ClientMaster'
ORDER BY ORDINAL_POSITION
GO

PRINT 'ClientMaster table is ready!'
GO
