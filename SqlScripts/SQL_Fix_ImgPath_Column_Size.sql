-- SQL Script to fix ImgPath and ImgID column sizes for multiple images
-- These columns need to be larger to store multiple Cloudinary URLs (comma-separated)
-- Each Cloudinary URL is approximately 80-100 characters

-- Step 1: Alter the SurveyDetails table columns to NVARCHAR(MAX)
-- This allows unlimited images to be stored as comma-separated URLs

ALTER TABLE SurveyDetails ALTER COLUMN ImgPath NVARCHAR(MAX) NULL;
GO

ALTER TABLE SurveyDetails ALTER COLUMN ImgID NVARCHAR(MAX) NULL;
GO

-- Step 2: Update the stored procedure parameters to NVARCHAR(MAX)

ALTER PROCEDURE [dbo].[SpSurveyDetails]
(
      @SpType         INT
    , @TransID        INT = NULL
    , @SurveyID       NUMERIC(11,0) = NULL
    , @LocID          INT = NULL
    , @ItemTypeID     INT = NULL
    , @ItemID         INT = NULL
    , @ItemQtyExist   INT = NULL
    , @ItemQtyReq     INT = NULL
    , @ImgPath        NVARCHAR(MAX) = NULL
    , @ImgID          NVARCHAR(MAX) = NULL
    , @Remarks        VARCHAR(300) = NULL
    , @CreateBy       INT = NULL
    -- New parameters for status section:
    , @Status         VARCHAR(50) = NULL
    , @UserID         INT = NULL
    , @StatusID       INT = NULL
    , @SubmittedDate  DATETIME = NULL
)
AS
BEGIN TRY

    BEGIN TRANSACTION;

    ------------------------------------------------------------
    -- 1. Insert / Update SurveyDetails
    ------------------------------------------------------------
    IF @SpType = 1
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM SurveyDetails
            WHERE SurveyID  = @SurveyID
              AND LocID     = @LocID
              AND ItemTypeID = @ItemTypeID
              AND ItemID    = @ItemID
        )
        BEGIN
            UPDATE SurveyDetails
            SET   ItemQtyExist = @ItemQtyExist
                , ItemQtyReq   = @ItemQtyReq
                , ImgPath      = @ImgPath
                , ImgID        = @ImgID
                , Remarks      = @Remarks
                , CreateOn     = SYSDATETIME()
                , CreateBy     = @CreateBy
            WHERE SurveyID   = @SurveyID
              AND LocID      = @LocID
              AND ItemTypeID = @ItemTypeID
              AND ItemID     = @ItemID;
        END
        ELSE
        BEGIN
            INSERT INTO SurveyDetails
                ( SurveyID, LocID, ItemTypeID, ItemID
                , ItemQtyExist, ItemQtyReq
                , ImgPath, ImgID, Remarks
                , CreateOn, CreateBy
                )
            VALUES
                ( @SurveyID, @LocID, @ItemTypeID, @ItemID
                , @ItemQtyExist, @ItemQtyReq
                , @ImgPath, @ImgID, @Remarks
                , SYSDATETIME(), @CreateBy
                );
        END
    END

    ------------------------------------------------------------
    -- 2. Get Assigned Item Types
    ------------------------------------------------------------
    IF @SpType = 2
    BEGIN
        SELECT
              A.SurveyId
            , SurveyName
            , A.LocID
            , LocName
            , ItemTypeID
            , B.TypeName
            , TypeDesc
            , GroupName
        FROM AssignedItems A
        JOIN ItemTypeMaster B ON A.ItemTypeID = B.Id
        JOIN SurveyLocation C ON A.LocID = C.LocID
        JOIN Survey D        ON A.SurveyId = D.SurveyId
        WHERE A.SurveyId  = @SurveyID
          AND A.LocID     = @LocID
          AND A.IsAssigned = 1;
    END

    ------------------------------------------------------------
    -- 3. Get SurveyDetails by ItemType
    ------------------------------------------------------------
    IF @SpType = 3
    BEGIN
        SELECT
              A.ItemID
            , ItemCode
            , ItemName
            , ItemDesc
            , ItemQtyExist
            , ItemQtyReq
            , ImgID
            , ImgPath
            , Remarks
        FROM SurveyDetails A
        JOIN ItemMaster B ON A.ItemID = B.ItemId
        WHERE SurveyId   = @SurveyID
          AND LocID      = @LocID
          AND ItemTypeID = @ItemTypeID
          AND (ItemQtyExist > 0 OR ItemQtyReq > 0);
    END

    ------------------------------------------------------------
    -- 4. Get All Items for ItemType + Existing SurveyDetails
    ------------------------------------------------------------
    IF @SpType = 4
    BEGIN
        SELECT
              A.ItemTypeID
            , A.ItemId
            , ItemName
            , ItemCode
            , ItemDesc
            , ItemQtyExist
            , ItemQtyReq
            , ImgID
            , ImgPath
            , B.Remarks
        FROM
        (
            SELECT
                  TypeId AS ItemTypeID
                , ItemId
                , ItemName
                , ItemCode
                , ItemDesc
            FROM ItemMaster
            WHERE TypeId = @ItemTypeID
        ) A
        LEFT JOIN
        (
            SELECT
                  ItemTypeID
                , ItemID
                , ItemQtyExist
                , ItemQtyReq
                , ImgID
                , ImgPath
                , Remarks
            FROM SurveyDetails
            WHERE SurveyId   = @SurveyID
              AND LocID      = @LocID
              AND ItemTypeID = @ItemTypeID
        ) B
            ON A.ItemId    = B.ItemID
           AND A.ItemTypeID = B.ItemTypeID;
    END

    ------------------------------------------------------------
    -- 5–10 : SurveyLocationStatus operations
    ------------------------------------------------------------

    -- 5. Insert or Update Status
    IF @SpType = 5
    BEGIN
        IF EXISTS (SELECT 1
                   FROM SurveyLocationStatus
                   WHERE SurveyID = @SurveyID AND LocID = @LocID)
        BEGIN
            UPDATE SurveyLocationStatus
            SET   Status       = ISNULL(@Status, Status)
                , Remarks      = ISNULL(@Remarks, Remarks)
                , ModifiedDate = GETDATE()
                , ModifiedBy   = @UserID
            WHERE SurveyID = @SurveyID
              AND LocID    = @LocID;

            SELECT 1 AS Result, 'Status updated successfully' AS Message;
        END
        ELSE
        BEGIN
            INSERT INTO SurveyLocationStatus
                ( SurveyID, LocID, Status, Remarks
                , CreatedBy, CreatedDate, IsActive
                )
            VALUES
                ( @SurveyID, @LocID
                , ISNULL(@Status, 'Pending')
                , @Remarks
                , @UserID, GETDATE(), 1
                );

            SELECT 1 AS Result, 'Status created successfully' AS Message;
        END
    END
    -- 6. Delete Status
    ELSE IF @SpType = 6
    BEGIN
        DELETE FROM SurveyLocationStatus
        WHERE StatusID = @StatusID
           OR (SurveyID = @SurveyID AND LocID = @LocID);

        SELECT 1 AS Result, 'Status deleted successfully' AS Message;
    END
    -- 7. Mark as Completed
    ELSE IF @SpType = 7
    BEGIN
        IF EXISTS (SELECT 1
                   FROM SurveyLocationStatus
                   WHERE SurveyID = @SurveyID AND LocID = @LocID)
        BEGIN
            UPDATE SurveyLocationStatus
            SET   Status        = 'Completed'
                , CompletedDate = GETDATE()
                , CompletedBy   = @UserID
                , Remarks       = ISNULL(@Remarks, Remarks)
                , ModifiedDate  = GETDATE()
                , ModifiedBy    = @UserID
            WHERE SurveyID = @SurveyID
              AND LocID    = @LocID;
        END
        ELSE
        BEGIN
            INSERT INTO SurveyLocationStatus
                ( SurveyID, LocID, Status
                , CompletedDate, CompletedBy
                , Remarks, CreatedBy, CreatedDate, IsActive
                )
            VALUES
                ( @SurveyID, @LocID, 'Completed'
                , GETDATE(), @UserID
                , @Remarks, @UserID, GETDATE(), 1
                );
        END

        SELECT 1 AS Result, 'Location marked as completed' AS Message;
    END
    -- 8. Select Status
    IF @SpType = 8 -- Select Status
    BEGIN
        SELECT 
            sls.*,
            u.EmpName AS CompletedByName,  -- Use EmpMaster table
            u2.EmpName AS VerifiedByName
        FROM SurveyLocationStatus sls
        LEFT JOIN EmpMaster u ON sls.CompletedBy = u.EmpID  -- Correct table
        LEFT JOIN EmpMaster u2 ON sls.VerifiedBy = u2.EmpID  -- Correct table
        WHERE sls.SurveyID = @SurveyID
            AND (@LocID IS NULL OR sls.LocID = @LocID)
    END

    -- 9. Mark as In Progress
    ELSE IF @SpType = 9
    BEGIN
        IF EXISTS (SELECT 1
                   FROM SurveyLocationStatus
                   WHERE SurveyID = @SurveyID AND LocID = @LocID)
        BEGIN
            UPDATE SurveyLocationStatus
            SET   Status       = 'In Progress'
                , StartedDate  = ISNULL(StartedDate, GETDATE())
                , Remarks      = ISNULL(@Remarks, Remarks)
                , ModifiedDate = GETDATE()
                , ModifiedBy   = @UserID
            WHERE SurveyID = @SurveyID
              AND LocID    = @LocID;
        END
        ELSE
        BEGIN
            INSERT INTO SurveyLocationStatus
                ( SurveyID, LocID, Status
                , StartedDate, Remarks
                , CreatedBy, CreatedDate, IsActive
                )
            VALUES
                ( @SurveyID, @LocID, 'In Progress'
                , GETDATE(), @Remarks
                , @UserID, GETDATE(), 1
                );
        END

        SELECT 1 AS Result, 'Location marked as in progress' AS Message;
    END
    -- 10. Mark as Verified
    ELSE IF @SpType = 10
    BEGIN
        UPDATE SurveyLocationStatus
        SET   Status       = 'Verified'
            , VerifiedDate = GETDATE()
            , VerifiedBy   = @UserID
            , Remarks      = ISNULL(@Remarks, Remarks)
            , ModifiedDate = GETDATE()
            , ModifiedBy   = @UserID
        WHERE SurveyID = @SurveyID
          AND LocID    = @LocID;

        SELECT 1 AS Result, 'Location marked as verified' AS Message;
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000);
    SELECT @ErrorMessage = ERROR_MESSAGE();

    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;
GO

PRINT 'Successfully updated ImgPath and ImgID to NVARCHAR(MAX) in both table and stored procedure';
