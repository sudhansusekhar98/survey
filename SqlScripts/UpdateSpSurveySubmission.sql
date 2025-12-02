ALTER PROCEDURE [dbo].[SpSurveySubmission]
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
BEGIN TRY
    BEGIN TRANSACTION;
    
    IF @SpType = 1
    BEGIN
        IF EXISTS (SELECT 1 FROM SurveySubmission WHERE SurveyId = @SurveyId)
        BEGIN
            UPDATE SurveySubmission
            SET SubmissionStatus = @SubmissionStatus,
                SubmittedBy = @SubmittedBy,
                SubmissionDate = @SubmissionDate,
                IsLockedForEditing = CASE WHEN @SubmissionStatus = 'Submitted' THEN 1 ELSE 0 END,
                ModifiedOn = GETDATE()
            WHERE SurveyId = @SurveyId;
        END
        ELSE
        BEGIN
            INSERT INTO SurveySubmission
                (SurveyId, SubmissionStatus, SubmittedBy, SubmissionDate, IsLockedForEditing, CreatedOn, ModifiedOn)
            VALUES
                (@SurveyId, @SubmissionStatus, @SubmittedBy, @SubmissionDate, 
                 CASE WHEN @SubmissionStatus = 'Submitted' THEN 1 ELSE 0 END, GETDATE(), GETDATE());
        END
    END
    
    IF @SpType = 2
    BEGIN
        UPDATE SurveySubmission
        SET SubmissionStatus = @SubmissionStatus,
            ReviewedBy = @ReviewedBy,
            ReviewDate = @ReviewDate,
            ReviewComments = @ReviewComments,
            IsLockedForEditing = CASE WHEN @SubmissionStatus IN ('Approved', 'Submitted') THEN 1 ELSE 0 END,
            ModifiedOn = GETDATE()
        WHERE SubmissionId = @SubmissionId;
    END
    
    IF @SpType = 3
    BEGIN
        DELETE FROM SurveySubmission
        WHERE SubmissionId = @SubmissionId;
    END
    
    IF @SpType = 4
    BEGIN
        SELECT 
            ss.SubmissionId,
            ss.SurveyId,
            s.SurveyName,
            ss.SubmissionStatus,
            ss.SubmittedBy,
            ISNULL(u1.LoginName, 'N/A') AS SubmittedByName,
            ss.SubmissionDate,
            ss.ReviewedBy,
            ISNULL(u2.LoginName, 'N/A') AS ReviewedByName,
            ss.ReviewDate,
            ss.ReviewComments,
            ss.IsLockedForEditing,
            ss.CreatedOn,
            ss.ModifiedOn
        FROM SurveySubmission ss
        INNER JOIN Survey s ON ss.SurveyId = s.SurveyId
        LEFT JOIN LoginMaster u1 ON ss.SubmittedBy = u1.UserID
        LEFT JOIN LoginMaster u2 ON ss.ReviewedBy = u2.UserID
        WHERE (@SubmittedBy IS NULL OR ss.SubmittedBy = @SubmittedBy)
        ORDER BY ss.SubmissionDate DESC;
    END
    
    IF @SpType = 5
    BEGIN
        SELECT 
            ss.SubmissionId,
            ss.SurveyId,
            s.SurveyName,
            ss.SubmissionStatus,
            ss.SubmittedBy,
            ISNULL(u1.LoginName, 'N/A') AS SubmittedByName,
            ss.SubmissionDate,
            ss.ReviewedBy,
            ISNULL(u2.LoginName, 'N/A') AS ReviewedByName,
            ss.ReviewDate,
            ss.ReviewComments,
            ss.IsLockedForEditing,
            ss.CreatedOn,
            ss.ModifiedOn
        FROM SurveySubmission ss
        INNER JOIN Survey s ON ss.SurveyId = s.SurveyId
        LEFT JOIN LoginMaster u1 ON ss.SubmittedBy = u1.UserID
        LEFT JOIN LoginMaster u2 ON ss.ReviewedBy = u2.UserID
        WHERE ss.SurveyId = @SurveyId;
    END
    
    IF @SpType = 6
    BEGIN
        SELECT 
            CASE 
                WHEN EXISTS (
                    SELECT 1
                    FROM SurveySubmission
                    WHERE SurveyId = @SurveyId
                      AND IsLockedForEditing = 1
                )
                THEN 0
                ELSE 1
            END AS CanEdit;
    END

    COMMIT;
END TRY
BEGIN CATCH
    ROLLBACK;
    DECLARE @ErrorMessage NVARCHAR(4000);
    SELECT @ErrorMessage = ERROR_MESSAGE();
    RAISERROR (@ErrorMessage, 16, 1);
END CATCH;
GO
