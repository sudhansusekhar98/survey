-- SQL Script to add SpType = 3 for marking location as completed in SpSurveyDetails
-- This adds functionality to mark all item types for a specific location as completed

/*
Add this logic to your SpSurveyDetails stored procedure:

IF @SpType = 3  -- Mark Location as Completed
BEGIN
    -- Update all survey details for this location to mark as completed
    -- You can add a Status column or CompletedDate column to track completion
    
    -- Option 1: Add a CompletedDate column if it doesn't exist
    -- ALTER TABLE SurveyDetails ADD CompletedDate DATETIME NULL
    -- ALTER TABLE SurveyDetails ADD CompletedBy INT NULL
    
    UPDATE SurveyDetails
    SET 
        CompletedDate = GETDATE(),
        CompletedBy = @CreateBy
    WHERE 
        SurveyID = @SurveyID 
        AND LocID = @LocID
    
    -- Alternatively, update a status flag
    -- UPDATE SurveyDetails
    -- SET Status = 'Completed', CompletedDate = GETDATE(), CompletedBy = @CreateBy
    -- WHERE SurveyID = @SurveyID AND LocID = @LocID
    
    -- Return success
    SELECT 1 AS Result
END

-- OR if you want to update a separate LocationStatus table:

IF @SpType = 3  -- Mark Location as Completed
BEGIN
    -- Check if entry exists in LocationStatus table
    IF EXISTS (SELECT 1 FROM SurveyLocationStatus WHERE SurveyID = @SurveyID AND LocID = @LocID)
    BEGIN
        UPDATE SurveyLocationStatus
        SET 
            Status = 'Completed',
            CompletedDate = GETDATE(),
            CompletedBy = @CreateBy
        WHERE 
            SurveyID = @SurveyID 
            AND LocID = @LocID
    END
    ELSE
    BEGIN
        INSERT INTO SurveyLocationStatus (SurveyID, LocID, Status, CompletedDate, CompletedBy)
        VALUES (@SurveyID, @LocID, 'Completed', GETDATE(), @CreateBy)
    END
END
*/

-- First, add columns to track completion if they don't exist:
-- Run these statements if the columns are not present

-- For SurveyDetails table:
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SurveyDetails' AND COLUMN_NAME = 'CompletedDate')
BEGIN
    ALTER TABLE SurveyDetails ADD CompletedDate DATETIME NULL
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SurveyDetails' AND COLUMN_NAME = 'CompletedBy')
BEGIN
    ALTER TABLE SurveyDetails ADD CompletedBy INT NULL
END

-- OR create a separate status tracking table:
/*
CREATE TABLE SurveyLocationStatus (
    StatusID INT IDENTITY(1,1) PRIMARY KEY,
    SurveyID BIGINT NOT NULL,
    LocID INT NOT NULL,
    Status VARCHAR(50) DEFAULT 'Pending',
    CompletedDate DATETIME NULL,
    CompletedBy INT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SurveyLocationStatus_Survey FOREIGN KEY (SurveyID) REFERENCES Survey(SurveyID),
    CONSTRAINT FK_SurveyLocationStatus_Location FOREIGN KEY (LocID) REFERENCES SurveyLocation(LocID)
)
*/

PRINT 'Please update your SpSurveyDetails stored procedure to include SpType = 3 logic'
PRINT 'Add the completion tracking columns or create the SurveyLocationStatus table as needed'
