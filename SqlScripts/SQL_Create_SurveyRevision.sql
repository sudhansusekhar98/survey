-- =============================================
-- Survey Revision Feature - Database Setup
-- Purpose: Enable supervisors to assign revisions to approved surveys
-- Created: 2025-12-26
-- =============================================

-- Step 1: Add Revision Tracking Columns to Survey Table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Survey]') AND name = 'IsRevised')
BEGIN
    ALTER TABLE Survey ADD IsRevised BIT DEFAULT 0;
    PRINT 'Added IsRevised column to Survey table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Survey]') AND name = 'OriginalSurveyId')
BEGIN
    ALTER TABLE Survey ADD OriginalSurveyId BIGINT NULL;
    PRINT 'Added OriginalSurveyId column to Survey table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Survey]') AND name = 'RevisionNumber')
BEGIN
    ALTER TABLE Survey ADD RevisionNumber INT DEFAULT 0;
    PRINT 'Added RevisionNumber column to Survey table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Survey]') AND name = 'RevisionReason')
BEGIN
    ALTER TABLE Survey ADD RevisionReason NVARCHAR(500) NULL;
    PRINT 'Added RevisionReason column to Survey table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Survey]') AND name = 'RevisionAssignedBy')
BEGIN
    ALTER TABLE Survey ADD RevisionAssignedBy INT NULL;
    PRINT 'Added RevisionAssignedBy column to Survey table';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Survey]') AND name = 'RevisionAssignedDate')
BEGIN
    ALTER TABLE Survey ADD RevisionAssignedDate DATETIME NULL;
    PRINT 'Added RevisionAssignedDate column to Survey table';
END
GO

-- Step 2: Create Survey Revision Log Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SurveyRevisionLog')
BEGIN
    CREATE TABLE [dbo].[SurveyRevisionLog] (
        [RevisionLogId] BIGINT IDENTITY(1,1) NOT NULL,
        [OriginalSurveyId] NUMERIC(11,0) NOT NULL,         -- Original (parent) survey
        [RevisedSurveyId] NUMERIC(11,0) NOT NULL,          -- New revised survey
        [RevisionNumber] INT NOT NULL,               -- Which revision (1, 2, 3...)
        [RevisionReason] NVARCHAR(500) NULL,
        [AssignedBy] INT NOT NULL,                   -- Supervisor UserID who assigned
        [AssignedToTeam] NVARCHAR(MAX) NULL,        -- JSON array of assigned EmpIDs
        [AssignedDate] DATETIME NOT NULL DEFAULT GETDATE(),
        [CompletedDate] DATETIME NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Assigned', -- Assigned, InProgress, Completed, Cancelled
        [Notes] NVARCHAR(MAX) NULL,
        [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
        [ModifiedOn] DATETIME NULL,
        
        CONSTRAINT [PK_SurveyRevisionLog] PRIMARY KEY CLUSTERED ([RevisionLogId] ASC),
        CONSTRAINT [FK_SurveyRevisionLog_OriginalSurvey] FOREIGN KEY ([OriginalSurveyId]) 
            REFERENCES [dbo].[Survey]([SurveyId]),
        CONSTRAINT [FK_SurveyRevisionLog_RevisedSurvey] FOREIGN KEY ([RevisedSurveyId]) 
            REFERENCES [dbo].[Survey]([SurveyId])
    );

    -- Create indexes for faster lookups
    CREATE NONCLUSTERED INDEX [IX_SurveyRevisionLog_OriginalSurvey] 
        ON [dbo].[SurveyRevisionLog] ([OriginalSurveyId]);
    CREATE NONCLUSTERED INDEX [IX_SurveyRevisionLog_RevisedSurvey] 
        ON [dbo].[SurveyRevisionLog] ([RevisedSurveyId]);
    CREATE NONCLUSTERED INDEX [IX_SurveyRevisionLog_Status] 
        ON [dbo].[SurveyRevisionLog] ([Status]);

    PRINT 'Created SurveyRevisionLog table with indexes';
END
GO

-- Step 3: Create Stored Procedure for Survey Revision Operations
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SpSurveyRevision')
    DROP PROCEDURE SpSurveyRevision;
GO

CREATE PROCEDURE [dbo].[SpSurveyRevision]
    @SpType INT,
    @SurveyId BIGINT = NULL,
    @OriginalSurveyId BIGINT = NULL,
    @RevisionLogId BIGINT = NULL,
    @RevisionReason NVARCHAR(500) = NULL,
    @AssignedBy INT = NULL,
    @AssignedToTeam NVARCHAR(MAX) = NULL,  -- JSON array of EmpIDs
    @NewDueDate DATETIME = NULL,
    @Status NVARCHAR(50) = NULL,
    @Notes NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- SpType 1: Create New Revision (Copy Survey)
    IF @SpType = 1
    BEGIN
        DECLARE @NewSurveyId NUMERIC(11,0);
        DECLARE @CurrentRevisionNumber INT;
        DECLARE @RootSurveyId NUMERIC(11,0);
        DECLARE @TodayPrefix NUMERIC(11,0);
        DECLARE @MaxIdToday NUMERIC(11,0);
        
        -- Generate new SurveyId in format YYYYMMDD### (same as SpSurvey)
        SET @TodayPrefix = CAST(FORMAT(GETDATE(), 'yyyyMMdd') + '000' AS NUMERIC(11,0));
        
        SELECT @MaxIdToday = ISNULL(MAX(SurveyId), @TodayPrefix)
        FROM Survey
        WHERE SurveyId >= @TodayPrefix AND SurveyId < @TodayPrefix + 1000;
        
        SET @NewSurveyId = @MaxIdToday + 1;
        
        -- Find the root original survey (in case this is already a revision)
        SET @RootSurveyId = ISNULL(
            (SELECT OriginalSurveyId FROM Survey WHERE SurveyId = @SurveyId AND OriginalSurveyId IS NOT NULL),
            @SurveyId
        );
        
        -- Get the next revision number
        SELECT @CurrentRevisionNumber = ISNULL(MAX(RevisionNumber), 0) + 1
        FROM Survey 
        WHERE SurveyId = @RootSurveyId OR OriginalSurveyId = @RootSurveyId;
        
        -- Copy the survey (using only columns that exist in the table)
        INSERT INTO Survey (
            SurveyId,  -- Explicitly include SurveyId since it's not IDENTITY
            SurveyName, ImplementationType, SurveyDate, 
            SurveyTeamName, SurveyTeamContact, AgencyName, LocationSiteName,
            MapMarking, CityDistrict, ScopeOfWork,
            Latitude, Longitude, SurveyStatus, CreatedBy, 
            RegionID, ClientID,
            -- Revision fields
            IsRevised, OriginalSurveyId, RevisionNumber, RevisionReason,
            RevisionAssignedBy, RevisionAssignedDate
        )
        SELECT 
            @NewSurveyId,  -- Use generated SurveyId
            SurveyName + ' (Rev ' + CAST(@CurrentRevisionNumber AS VARCHAR) + ')',
            ImplementationType, GETDATE(),
            SurveyTeamName, SurveyTeamContact, AgencyName, LocationSiteName,
            MapMarking, CityDistrict, ScopeOfWork,
            Latitude, Longitude, 'Assigned', CreatedBy, 
            RegionID, ClientID,
            -- Revision fields
            1, @RootSurveyId, @CurrentRevisionNumber, @RevisionReason,
            @AssignedBy, GETDATE()
        FROM Survey
        WHERE SurveyId = @SurveyId;
        
        -- Copy Survey Locations (using correct column names)
        INSERT INTO SurveyLocation (SurveyID, LocName, LocationType, LocLat, LocLog, IsGlobal)
        SELECT @NewSurveyId, LocName, LocationType, LocLat, LocLog, IsGlobal
        FROM SurveyLocation
        WHERE SurveyID = @SurveyId;
        
        -- Copy Survey Details (device quantities) - Map old LocID to new LocID
        ;WITH LocationMapping AS (
            SELECT 
                old.LocID AS OldLocId,
                new.LocID AS NewLocId
            FROM SurveyLocation old
            INNER JOIN SurveyLocation new ON old.LocName = new.LocName 
                AND old.SurveyID = @SurveyId 
                AND new.SurveyID = @NewSurveyId
        )
        INSERT INTO SurveyDetails (SurveyID, LocID, ItemTypeID, ItemID, ItemQtyExist, ItemQtyReq, Remarks, ImgPath, CreateBy, CreateOn)
        SELECT 
            @NewSurveyId, 
            ISNULL(lm.NewLocId, sd.LocID),  -- Map to new location ID or keep 0 for global items
            sd.ItemTypeID, sd.ItemID, sd.ItemQtyExist, sd.ItemQtyReq, sd.Remarks, sd.ImgPath,
            @AssignedBy, GETDATE()
        FROM SurveyDetails sd
        LEFT JOIN LocationMapping lm ON sd.LocID = lm.OldLocId
        WHERE sd.SurveyID = @SurveyId;
        
        -- Copy AssignedItems (ItemTypeMaster selections) - Map old LocID to new LocID
        -- This preserves the item type selections for each location
        ;WITH LocationMapping AS (
            SELECT 
                old.LocID AS OldLocId,
                new.LocID AS NewLocId
            FROM SurveyLocation old
            INNER JOIN SurveyLocation new ON old.LocName = new.LocName 
                AND old.SurveyID = @SurveyId 
                AND new.SurveyID = @NewSurveyId
        )
        INSERT INTO AssignedItems (SurveyId, LocID, ItemTypeID, TypeName, IsAssigned, CreatedBy, CreatedOn)
        SELECT 
            @NewSurveyId, 
            ISNULL(lm.NewLocId, ai.LocID),  -- Map to new location ID
            ai.ItemTypeID, ai.TypeName, ai.IsAssigned,
            @AssignedBy, GETDATE()
        FROM AssignedItems ai
        LEFT JOIN LocationMapping lm ON ai.LocID = lm.OldLocId
        WHERE ai.SurveyId = @SurveyId;
        
        -- Create Revision Log Entry
        INSERT INTO SurveyRevisionLog (
            OriginalSurveyId, RevisedSurveyId, RevisionNumber, RevisionReason,
            AssignedBy, AssignedToTeam, AssignedDate, Status
        )
        VALUES (
            @RootSurveyId, @NewSurveyId, @CurrentRevisionNumber, @RevisionReason,
            @AssignedBy, @AssignedToTeam, GETDATE(), 'Assigned'
        );
        
        -- Add Team Leader to SurveyAssignment table
        -- Parse the first team member from JSON array (team leader)
        DECLARE @TeamLeaderId INT;
        
        -- Handle both single value and JSON array format
        IF @AssignedToTeam IS NOT NULL AND LEN(@AssignedToTeam) > 0
        BEGIN
            -- Check if it's a single number or JSON array
            IF LEFT(LTRIM(@AssignedToTeam), 1) = '['
            BEGIN
                -- JSON array format: [123] or [123,456,...]
                SELECT TOP 1 @TeamLeaderId = CAST(value AS INT)
                FROM OPENJSON(@AssignedToTeam);
            END
            ELSE
            BEGIN
                -- Single number format: 123
                SET @TeamLeaderId = CAST(@AssignedToTeam AS INT);
            END
            
            -- Insert team leader into SurveyAssignment
            IF @TeamLeaderId IS NOT NULL
            BEGIN
                INSERT INTO SurveyAssignment (SurveyID, EmpID, CreateBy, CreateOn, DueDate)
                VALUES (@NewSurveyId, @TeamLeaderId, @AssignedBy, GETDATE(), @NewDueDate);
            END
        END
        
        -- Step 6: Update Location Status - Create SurveyLocationStatus entries for all locations
        -- Set status to 'In Progress' for all copied locations
        INSERT INTO SurveyLocationStatus (
            SurveyID, LocID, Status, StartedDate, CreatedDate, CreatedBy, IsActive
        )
        SELECT 
            @NewSurveyId, 
            LocID, 
            'In Progress',  -- All locations start as In Progress
            GETDATE(),      -- Started date
            GETDATE(),      -- Created date
            @AssignedBy,    -- Created by
            1               -- IsActive
        FROM SurveyLocation
        WHERE SurveyID = @NewSurveyId;
        
        -- Return the new survey ID
        SELECT @NewSurveyId AS NewSurveyId, @CurrentRevisionNumber AS RevisionNumber;
    END
    
    -- SpType 2: Get Revision History for a Survey (all revisions of an original)
    ELSE IF @SpType = 2
    BEGIN
        DECLARE @RootId BIGINT;
        
        -- Find root survey ID
        SET @RootId = ISNULL(
            (SELECT OriginalSurveyId FROM Survey WHERE SurveyId = @SurveyId AND OriginalSurveyId IS NOT NULL),
            @SurveyId
        );
        
        SELECT 
            rl.RevisionLogId,
            rl.OriginalSurveyId,
            orig.SurveyName AS OriginalSurveyName,
            rl.RevisedSurveyId,
            rev.SurveyName AS RevisedSurveyName,
            rl.RevisionNumber,
            rl.RevisionReason,
            rl.AssignedBy,
            ISNULL(u.LoginName, 'Unknown') AS AssignedByName,
            rl.AssignedToTeam,
            rl.AssignedDate,
            rl.CompletedDate,
            rl.Status,
            rl.Notes,
            rev.SurveyStatus AS CurrentSurveyStatus,
            ss.SubmissionStatus
        FROM SurveyRevisionLog rl
        INNER JOIN Survey orig ON rl.OriginalSurveyId = orig.SurveyId
        INNER JOIN Survey rev ON rl.RevisedSurveyId = rev.SurveyId
        LEFT JOIN LoginMaster u ON rl.AssignedBy = u.UserID
        LEFT JOIN SurveySubmission ss ON rl.RevisedSurveyId = ss.SurveyId
        WHERE rl.OriginalSurveyId = @RootId
        ORDER BY rl.RevisionNumber DESC;
    END
    
    -- SpType 3: Get All Pending Revisions (for supervisor dashboard)
    ELSE IF @SpType = 3
    BEGIN
        SELECT 
            rl.RevisionLogId,
            rl.OriginalSurveyId,
            orig.SurveyName AS OriginalSurveyName,
            rl.RevisedSurveyId,
            rev.SurveyName AS RevisedSurveyName,
            rl.RevisionNumber,
            rl.RevisionReason,
            rl.AssignedBy,
            ISNULL(u.LoginName, 'Unknown') AS AssignedByName,
            rl.AssignedToTeam,
            rl.AssignedDate,
            rl.Status,
            rev.SurveyStatus AS CurrentSurveyStatus,
            rl.AssignedDate AS DueDate,  -- Using AssignedDate as fallback since DueDate doesn't exist
            c.ClientName,
            r.RegionName
        FROM SurveyRevisionLog rl
        INNER JOIN Survey orig ON rl.OriginalSurveyId = orig.SurveyId
        INNER JOIN Survey rev ON rl.RevisedSurveyId = rev.SurveyId
        LEFT JOIN LoginMaster u ON rl.AssignedBy = u.UserID
        LEFT JOIN ClientMaster c ON rev.ClientID = c.ClientID
        LEFT JOIN RegionMaster r ON rev.RegionID = r.RegionID
        WHERE rl.Status IN ('Assigned', 'InProgress')
        ORDER BY rl.AssignedDate DESC;
    END
    
    -- SpType 4: Update Revision Status
    ELSE IF @SpType = 4
    BEGIN
        UPDATE SurveyRevisionLog
        SET Status = @Status,
            Notes = ISNULL(@Notes, Notes),
            CompletedDate = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE CompletedDate END,
            ModifiedOn = GETDATE()
        WHERE RevisionLogId = @RevisionLogId;
        
        -- If completed, also update the survey status
        IF @Status = 'Completed'
        BEGIN
            UPDATE Survey
            SET SurveyStatus = 'Completed'
            WHERE SurveyId = (SELECT RevisedSurveyId FROM SurveyRevisionLog WHERE RevisionLogId = @RevisionLogId);
        END
    END
    
    -- SpType 5: Get Original Survey Chain (trace back to root)
    ELSE IF @SpType = 5
    BEGIN
        ;WITH SurveyChain AS (
            -- Anchor: Start with the given survey
            SELECT 
                SurveyId, SurveyName, OriginalSurveyId, RevisionNumber, IsRevised, 0 AS Depth
            FROM Survey
            WHERE SurveyId = @SurveyId
            
            UNION ALL
            
            -- Recursive: Go to parent
            SELECT 
                s.SurveyId, s.SurveyName, s.OriginalSurveyId, s.RevisionNumber, s.IsRevised, sc.Depth + 1
            FROM Survey s
            INNER JOIN SurveyChain sc ON s.SurveyId = sc.OriginalSurveyId
            WHERE s.OriginalSurveyId IS NOT NULL OR sc.Depth < 10  -- Safety limit
        )
        SELECT * FROM SurveyChain
        ORDER BY Depth DESC;
    END
    
    -- SpType 6: Check if survey can be revised (must be approved)
    ELSE IF @SpType = 6
    BEGIN
        SELECT 
            CASE 
                WHEN ss.SubmissionStatus = 'Approved' THEN 1
                ELSE 0
            END AS CanRevise,
            s.SurveyName,
            s.SurveyStatus,
            ss.SubmissionStatus,
            s.IsRevised,
            s.RevisionNumber
        FROM Survey s
        LEFT JOIN SurveySubmission ss ON s.SurveyId = ss.SurveyId
        WHERE s.SurveyId = @SurveyId;
    END
END
GO

PRINT 'Stored procedure SpSurveyRevision created successfully.';
GO
