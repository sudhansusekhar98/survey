-- =============================================
-- Create SurveyLocationStatus Table
-- Purpose: Track completion status of survey locations
-- Date: November 24, 2025
-- =============================================

USE [VLDev]
GO

-- Drop table if exists (for clean reinstall - comment out if you want to preserve data)
-- DROP TABLE IF EXISTS [dbo].[SurveyLocationStatus]
-- GO

-- Create the SurveyLocationStatus table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurveyLocationStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SurveyLocationStatus] (
        [StatusID] INT IDENTITY(1,1) PRIMARY KEY,
        [SurveyID] BIGINT NOT NULL,
        [LocID] INT NOT NULL,
        [Status] VARCHAR(50) NOT NULL DEFAULT 'Pending',
        [StartedDate] DATETIME NULL,
        [CompletedDate] DATETIME NULL,
        [CompletedBy] INT NULL,
        [VerifiedDate] DATETIME NULL,
        [VerifiedBy] INT NULL,
        [Remarks] NVARCHAR(500) NULL,
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatedBy] INT NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        
        -- Create unique constraint to prevent duplicate entries
        CONSTRAINT UQ_SurveyLocationStatus UNIQUE (SurveyID, LocID)
    )
    
    PRINT 'SUCCESS: Table [SurveyLocationStatus] created successfully'
END
ELSE
BEGIN
    PRINT 'INFO: Table [SurveyLocationStatus] already exists'
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SurveyLocationStatus_SurveyID' AND object_id = OBJECT_ID('SurveyLocationStatus'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SurveyLocationStatus_SurveyID
    ON [dbo].[SurveyLocationStatus] ([SurveyID])
    INCLUDE ([LocID], [Status], [CompletedDate])
    
    PRINT 'SUCCESS: Index [IX_SurveyLocationStatus_SurveyID] created successfully'
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SurveyLocationStatus_Status' AND object_id = OBJECT_ID('SurveyLocationStatus'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SurveyLocationStatus_Status
    ON [dbo].[SurveyLocationStatus] ([Status])
    INCLUDE ([SurveyID], [LocID], [CompletedDate])
    
    PRINT 'SUCCESS: Index [IX_SurveyLocationStatus_Status] created successfully'
END
GO

PRINT ''
PRINT '================================================'
PRINT 'Table Creation Complete!'
PRINT '================================================'
PRINT 'Table: SurveyLocationStatus'
PRINT 'Columns:'
PRINT '  - StatusID (PK, Identity)'
PRINT '  - SurveyID (BIGINT, NOT NULL)'
PRINT '  - LocID (INT, NOT NULL)'
PRINT '  - Status (VARCHAR(50), Default: Pending)'
PRINT '  - StartedDate (DATETIME, NULL)'
PRINT '  - CompletedDate (DATETIME, NULL)'
PRINT '  - CompletedBy (INT, NULL)'
PRINT '  - VerifiedDate (DATETIME, NULL)'
PRINT '  - VerifiedBy (INT, NULL)'
PRINT '  - Remarks (NVARCHAR(500), NULL)'
PRINT '  - CreatedDate (DATETIME, NOT NULL)'
PRINT '  - CreatedBy (INT, NULL)'
PRINT '  - ModifiedDate (DATETIME, NULL)'
PRINT '  - ModifiedBy (INT, NULL)'
PRINT '  - IsActive (BIT, Default: 1)'
PRINT ''
PRINT 'Constraints:'
PRINT '  - UQ_SurveyLocationStatus (SurveyID, LocID)'
PRINT ''
PRINT 'Indexes:'
PRINT '  - IX_SurveyLocationStatus_SurveyID'
PRINT '  - IX_SurveyLocationStatus_Status'
PRINT '================================================'
GO
