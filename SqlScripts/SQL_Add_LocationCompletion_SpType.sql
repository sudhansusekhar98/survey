-- =============================================
-- SQL Script: Create SurveyLocationStatus Table
-- Purpose: Track completion status of survey locations without altering existing tables
-- Date: November 24, 2025


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurveyLocationStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SurveyLocationStatus] (
        [StatusID] INT IDENTITY(1,1) PRIMARY KEY,
        [SurveyID] BIGINT NOT NULL,
        [LocID] INT NOT NULL,
        [Status] VARCHAR(50) NOT NULL DEFAULT 'Pending',  -- Pending, In Progress, Completed, Verified
        [StartedDate] DATETIME NULL,                      -- When location survey started
        [CompletedDate] DATETIME NULL,                    -- When location survey completed
        [CompletedBy] INT NULL,                           -- UserID who completed the survey
        [VerifiedDate] DATETIME NULL,                     -- When survey was verified
        [VerifiedBy] INT NULL,                            -- UserID who verified
        [Remarks] NVARCHAR(500) NULL,                     -- Any notes or comments
        [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatedBy] INT NULL,
        [ModifiedDate] DATETIME NULL,
        [ModifiedBy] INT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        
        -- Create unique constraint to prevent duplicate entries
        CONSTRAINT UQ_SurveyLocationStatus UNIQUE (SurveyID, LocID),
        
        -- Foreign key constraints (uncomment if FK constraints exist)
        -- CONSTRAINT FK_SurveyLocationStatus_Survey FOREIGN KEY (SurveyID) REFERENCES Survey(SurveyID),
        -- CONSTRAINT FK_SurveyLocationStatus_Location FOREIGN KEY (LocID) REFERENCES SurveyLocation(LocID)
    )
    
    PRINT 'Table SurveyLocationStatus created successfully'
END
ELSE
BEGIN
    PRINT 'Table SurveyLocationStatus already exists'
END
GO

-- Step 2: Create indexes for better performance
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SurveyLocationStatus_SurveyID' AND object_id = OBJECT_ID('SurveyLocationStatus'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SurveyLocationStatus_SurveyID
    ON [dbo].[SurveyLocationStatus] ([SurveyID])
    INCLUDE ([LocID], [Status], [CompletedDate])
    
    PRINT 'Index IX_SurveyLocationStatus_SurveyID created successfully'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SurveyLocationStatus_Status' AND object_id = OBJECT_ID('SurveyLocationStatus'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SurveyLocationStatus_Status
    ON [dbo].[SurveyLocationStatus] ([Status])
    INCLUDE ([SurveyID], [LocID], [CompletedDate])
    
    PRINT 'Index IX_SurveyLocationStatus_Status created successfully'
END
GO

-- Step 3: Create stored procedure to manage location status
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpSurveyLocationStatus]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SpSurveyLocationStatus]
GO

CREATE PROCEDURE [dbo].[SpSurveyLocationStatus]
    @SpType INT,                -- 1=Insert/Update, 2=Delete, 3=Mark Complete, 4=Select, 5=Mark In Progress, 6=Mark Verified
    @StatusID INT = NULL,
    @SurveyID BIGINT = NULL,
    @LocID INT = NULL,
    @Status VARCHAR(50) = NULL,
    @Remarks NVARCHAR(500) = NULL,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Insert or Update Status
    IF @SpType = 1
    BEGIN
        IF EXISTS (SELECT 1 FROM SurveyLocationStatus WHERE SurveyID = @SurveyID AND LocID = @LocID)
        BEGIN
            UPDATE SurveyLocationStatus
            SET 
                Status = ISNULL(@Status, Status),
                Remarks = ISNULL(@Remarks, Remarks),
                ModifiedDate = GETDATE(),
                ModifiedBy = @UserID
            WHERE SurveyID = @SurveyID AND LocID = @LocID
            
            SELECT 1 AS Result, 'Status updated successfully' AS Message
        END
        ELSE
        BEGIN
            INSERT INTO SurveyLocationStatus (SurveyID, LocID, Status, Remarks, CreatedBy, CreatedDate, IsActive)
            VALUES (@SurveyID, @LocID, ISNULL(@Status, 'Pending'), @Remarks, @UserID, GETDATE(), 1)
            
            SELECT 1 AS Result, 'Status created successfully' AS Message
        END
    END
    
    -- Delete Status
    ELSE IF @SpType = 2
    BEGIN
        DELETE FROM SurveyLocationStatus
        WHERE StatusID = @StatusID OR (SurveyID = @SurveyID AND LocID = @LocID)
        
        SELECT 1 AS Result, 'Status deleted successfully' AS Message
    END
    
    -- Mark as Completed
    ELSE IF @SpType = 3
    BEGIN
        IF EXISTS (SELECT 1 FROM SurveyLocationStatus WHERE SurveyID = @SurveyID AND LocID = @LocID)
        BEGIN
            UPDATE SurveyLocationStatus
            SET 
                Status = 'Completed',
                CompletedDate = GETDATE(),
                CompletedBy = @UserID,
                Remarks = ISNULL(@Remarks, Remarks),
                ModifiedDate = GETDATE(),
                ModifiedBy = @UserID
            WHERE SurveyID = @SurveyID AND LocID = @LocID
        END
        ELSE
        BEGIN
            INSERT INTO SurveyLocationStatus (SurveyID, LocID, Status, CompletedDate, CompletedBy, Remarks, CreatedBy, CreatedDate, IsActive)
            VALUES (@SurveyID, @LocID, 'Completed', GETDATE(), @UserID, @Remarks, @UserID, GETDATE(), 1)
        END
        
        SELECT 1 AS Result, 'Location marked as completed' AS Message
    END
    
    -- Select Status
    ELSE IF @SpType = 4
    BEGIN
        IF @StatusID IS NOT NULL
        BEGIN
            -- Get specific status by ID
            SELECT * FROM SurveyLocationStatus WHERE StatusID = @StatusID
        END
        ELSE IF @SurveyID IS NOT NULL AND @LocID IS NOT NULL
        BEGIN
            -- Get status for specific survey and location
            SELECT * FROM SurveyLocationStatus WHERE SurveyID = @SurveyID AND LocID = @LocID
        END
        ELSE IF @SurveyID IS NOT NULL
        BEGIN
            -- Get all statuses for a survey
            SELECT 
                sls.*,
                sl.LocName,
                u1.UserName AS CompletedByName,
                u2.UserName AS VerifiedByName
            FROM SurveyLocationStatus sls
            LEFT JOIN SurveyLocation sl ON sls.LocID = sl.LocID
            LEFT JOIN Users u1 ON sls.CompletedBy = u1.UserID
            LEFT JOIN Users u2 ON sls.VerifiedBy = u2.UserID
            WHERE sls.SurveyID = @SurveyID
            ORDER BY sls.CreatedDate DESC
        END
        ELSE
        BEGIN
            -- Get all statuses
            SELECT * FROM SurveyLocationStatus ORDER BY CreatedDate DESC
        END
    END
    
    -- Mark as In Progress
    ELSE IF @SpType = 5
    BEGIN
        IF EXISTS (SELECT 1 FROM SurveyLocationStatus WHERE SurveyID = @SurveyID AND LocID = @LocID)
        BEGIN
            UPDATE SurveyLocationStatus
            SET 
                Status = 'In Progress',
                StartedDate = ISNULL(StartedDate, GETDATE()),
                Remarks = ISNULL(@Remarks, Remarks),
                ModifiedDate = GETDATE(),
                ModifiedBy = @UserID
            WHERE SurveyID = @SurveyID AND LocID = @LocID
        END
        ELSE
        BEGIN
            INSERT INTO SurveyLocationStatus (SurveyID, LocID, Status, StartedDate, Remarks, CreatedBy, CreatedDate, IsActive)
            VALUES (@SurveyID, @LocID, 'In Progress', GETDATE(), @Remarks, @UserID, GETDATE(), 1)
        END
        
        SELECT 1 AS Result, 'Location marked as in progress' AS Message
    END
    
    -- Mark as Verified
    ELSE IF @SpType = 6
    BEGIN
        UPDATE SurveyLocationStatus
        SET 
            Status = 'Verified',
            VerifiedDate = GETDATE(),
            VerifiedBy = @UserID,
            Remarks = ISNULL(@Remarks, Remarks),
            ModifiedDate = GETDATE(),
            ModifiedBy = @UserID
        WHERE SurveyID = @SurveyID AND LocID = @LocID
        
        SELECT 1 AS Result, 'Location marked as verified' AS Message
    END
END
GO

PRINT '================================================'
PRINT 'SurveyLocationStatus table and procedures created successfully!'
PRINT '================================================'
PRINT ''
PRINT 'Available SpTypes:'
PRINT '  1 = Insert/Update Status'
PRINT '  2 = Delete Status'
PRINT '  3 = Mark as Completed'
PRINT '  4 = Select/Get Status'
PRINT '  5 = Mark as In Progress'
PRINT '  6 = Mark as Verified'
PRINT ''
PRINT 'Example Usage:'
PRINT '  -- Mark location as completed'
PRINT '  EXEC SpSurveyLocationStatus @SpType=3, @SurveyID=1, @LocID=1, @UserID=1'
PRINT ''
PRINT '  -- Get status for a survey'
PRINT '  EXEC SpSurveyLocationStatus @SpType=4, @SurveyID=1'
PRINT '================================================'
