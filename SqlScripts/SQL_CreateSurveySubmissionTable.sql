-- Create Survey Submission Tracking Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SurveySubmission]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SurveySubmission](
        [SubmissionId] [bigint] IDENTITY(1,1) NOT NULL,
        [SurveyId] [bigint] NOT NULL,
        [SubmissionStatus] [nvarchar](50) NOT NULL DEFAULT 'Draft',
        [SubmittedBy] [int] NULL,
        [SubmissionDate] [datetime] NULL,
        [ReviewedBy] [int] NULL,
        [ReviewDate] [datetime] NULL,
        [ReviewComments] [nvarchar](max) NULL,
        [IsLockedForEditing] [bit] NOT NULL DEFAULT 0,
        [CreatedOn] [datetime] NULL DEFAULT GETDATE(),
        [ModifiedOn] [datetime] NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_SurveySubmission] PRIMARY KEY CLUSTERED ([SubmissionId] ASC)
    )
END
GO

-- Create Stored Procedure for Survey Submission Operations
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpSurveySubmission]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SpSurveySubmission]
GO

CREATE PROCEDURE [dbo].[SpSurveySubmission]
    @SpType INT,
    @SubmissionId BIGINT = NULL,
    @SurveyId BIGINT = NULL,
    @SubmissionStatus NVARCHAR(50) = NULL,
    @SubmittedBy INT = NULL,
    @SubmissionDate DATETIME = NULL,
    @ReviewedBy INT = NULL,
    @ReviewDate DATETIME = NULL,
    @ReviewComments NVARCHAR(MAX) = NULL,
    @IsLockedForEditing BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- SpType 1: Insert/Submit Survey
    IF @SpType = 1
    BEGIN
        -- Check if survey already has a submission record
        IF EXISTS (SELECT 1 FROM SurveySubmission WHERE SurveyId = @SurveyId)
        BEGIN
            -- Update existing submission
            UPDATE SurveySubmission
            SET SubmissionStatus = @SubmissionStatus,
                SubmittedBy = @SubmittedBy,
                SubmissionDate = @SubmissionDate,
                IsLockedForEditing = CASE WHEN @SubmissionStatus = 'Submitted' THEN 1 ELSE 0 END,
                ModifiedOn = GETDATE()
            WHERE SurveyId = @SurveyId
        END
        ELSE
        BEGIN
            -- Insert new submission
            INSERT INTO SurveySubmission (SurveyId, SubmissionStatus, SubmittedBy, SubmissionDate, IsLockedForEditing, CreatedOn, ModifiedOn)
            VALUES (@SurveyId, @SubmissionStatus, @SubmittedBy, @SubmissionDate, 
                    CASE WHEN @SubmissionStatus = 'Submitted' THEN 1 ELSE 0 END, GETDATE(), GETDATE())
        END
    END
    
    -- SpType 2: Update Review Status
    IF @SpType = 2
    BEGIN
        UPDATE SurveySubmission
        SET SubmissionStatus = @SubmissionStatus,
            ReviewedBy = @ReviewedBy,
            ReviewDate = @ReviewDate,
            ReviewComments = @ReviewComments,
            IsLockedForEditing = CASE WHEN @SubmissionStatus IN ('Approved', 'Submitted') THEN 1 ELSE 0 END,
            ModifiedOn = GETDATE()
        WHERE SubmissionId = @SubmissionId
    END
    
    -- SpType 3: Delete Submission
    IF @SpType = 3
    BEGIN
        DELETE FROM SurveySubmission WHERE SubmissionId = @SubmissionId
    END
    
    -- SpType 4: Select All Submissions
    IF @SpType = 4
    BEGIN
        SELECT 
            ss.SubmissionId,
            ss.SurveyId,
            s.SurveyName,
            ss.SubmissionStatus,
            ss.SubmittedBy,
            ISNULL(e1.EmpName, 'N/A') AS SubmittedByName,
            ss.SubmissionDate,
            ss.ReviewedBy,
            ISNULL(e2.EmpName, 'N/A') AS ReviewedByName,
            ss.ReviewDate,
            ss.ReviewComments,
            ss.IsLockedForEditing,
            ss.CreatedOn,
            ss.ModifiedOn
        FROM SurveySubmission ss
        INNER JOIN Survey s ON ss.SurveyId = s.SurveyId
        LEFT JOIN EmpMaster e1 ON ss.SubmittedBy = e1.EmpID
        LEFT JOIN EmpMaster e2 ON ss.ReviewedBy = e2.EmpID
        WHERE (@SubmittedBy IS NULL OR ss.SubmittedBy = @SubmittedBy)
        ORDER BY ss.SubmissionDate DESC
    END
    
    -- SpType 5: Select by Survey ID
    IF @SpType = 5
    BEGIN
        SELECT 
            ss.SubmissionId,
            ss.SurveyId,
            s.SurveyName,
            ss.SubmissionStatus,
            ss.SubmittedBy,
            ISNULL(e1.EmpName, 'N/A') AS SubmittedByName,
            ss.SubmissionDate,
            ss.ReviewedBy,
            ISNULL(e2.EmpName, 'N/A') AS ReviewedByName,
            ss.ReviewDate,
            ss.ReviewComments,
            ss.IsLockedForEditing,
            ss.CreatedOn,
            ss.ModifiedOn
        FROM SurveySubmission ss
        INNER JOIN Survey s ON ss.SurveyId = s.SurveyId
        LEFT JOIN EmpMaster e1 ON ss.SubmittedBy = e1.EmpID
        LEFT JOIN EmpMaster e2 ON ss.ReviewedBy = e2.EmpID
        WHERE ss.SurveyId = @SurveyId
    END
    
    -- SpType 6: Check if survey can be edited
    IF @SpType = 6
    BEGIN
        SELECT 
            CASE 
                WHEN EXISTS (SELECT 1 FROM SurveySubmission WHERE SurveyId = @SurveyId AND IsLockedForEditing = 1)
                THEN 0
                ELSE 1
            END AS CanEdit
    END
END
GO

PRINT 'Survey Submission table and stored procedure created successfully!'
