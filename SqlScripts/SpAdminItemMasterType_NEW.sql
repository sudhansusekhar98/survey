-- =============================================
-- Author: System Generated
-- Create date: 2025-12-01
-- Description: Comprehensive stored procedure for managing ItemTypeMaster and ItemMaster
-- =============================================
USE [VLDev]
GO

-- Drop if exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SpAdminItemMasterType]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[SpAdminItemMasterType]
GO

CREATE PROCEDURE [dbo].[SpAdminItemMasterType]
(
    @SpType      INT              = NULL,      -- Operation type (1-10)
    
    -- ItemTypeMaster parameters
    @Id          INT              = NULL,      -- PK of ItemTypeMaster
    @TypeName    NVARCHAR(100)    = NULL,
    @TypeDesc    NVARCHAR(300)    = NULL,
    @GroupName   NVARCHAR(100)    = NULL,
    @IsActive    VARCHAR(1)       = '1',       -- '1' or '0' as string
    @UserId      INT              = NULL,      -- For CreatedBy / ModifiedBy

    -- ItemMaster parameters
    @ItemId      INT              = NULL,      -- PK of ItemMaster
    @TypeId      INT              = NULL,      -- FK to ItemTypeMaster.Id
    @ItemName    NVARCHAR(100)    = NULL,
    @ItemCode    NVARCHAR(50)     = NULL,
    @ItemUOM     NVARCHAR(50)     = NULL,      -- Unit of Measurement
    @ItemDesc    NVARCHAR(300)    = NULL,
    @SqNo        INT              = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        /* =============================================
           DEVICE MODULE (ItemTypeMaster) OPERATIONS
           ============================================= */

        -- SpType = 1: Insert Device Module
        IF (@SpType = 1)
        BEGIN
            INSERT INTO [dbo].[ItemTypeMaster]
            (
                [TypeName],
                [TypeDesc],
                [GroupName],
                [IsActive],
                [CreatedOn],
                [CreatedBy]
            )
            VALUES
            (
                @TypeName,
                @TypeDesc,
                @GroupName,
                CASE WHEN @IsActive = '1' THEN 1 ELSE 0 END,
                GETDATE(),
                @UserId
            );

            -- Return the inserted record
            SELECT 
                [Id],
                [TypeName],
                [TypeDesc],
                [GroupName],
                CASE WHEN [IsActive] = 1 THEN '1' ELSE '0' END AS [IsActive],
                [CreatedOn],
                [CreatedBy],
                [ModifiedDate],
                [ModifiedBy]
            FROM [dbo].[ItemTypeMaster]
            WHERE Id = SCOPE_IDENTITY();

            RETURN;
        END

        -- SpType = 2: Update Device Module
        ELSE IF (@SpType = 2)
        BEGIN
            UPDATE [dbo].[ItemTypeMaster]
            SET
                [TypeName]     = @TypeName,
                [TypeDesc]     = @TypeDesc,
                [GroupName]    = @GroupName,
                [IsActive]     = CASE WHEN @IsActive = '1' THEN 1 ELSE 0 END,
                [ModifiedDate] = GETDATE(),
                [ModifiedBy]   = @UserId
            WHERE [Id] = @Id;

            -- Return the updated record
            SELECT 
                [Id],
                [TypeName],
                [TypeDesc],
                [GroupName],
                CASE WHEN [IsActive] = 1 THEN '1' ELSE '0' END AS [IsActive],
                [CreatedOn],
                [CreatedBy],
                [ModifiedDate],
                [ModifiedBy]
            FROM [dbo].[ItemTypeMaster]
            WHERE Id = @Id;

            RETURN;
        END

        -- SpType = 3: Soft Delete Device Module
        ELSE IF (@SpType = 3)
        BEGIN
            UPDATE [dbo].[ItemTypeMaster]
            SET
                [IsActive]     = 0,
                [ModifiedDate] = GETDATE(),
                [ModifiedBy]   = @UserId
            WHERE [Id] = @Id;

            -- Return success indicator
            SELECT 
                [Id],
                [TypeName],
                [TypeDesc],
                [GroupName],
                CASE WHEN [IsActive] = 1 THEN '1' ELSE '0' END AS [IsActive],
                [CreatedOn],
                [CreatedBy],
                [ModifiedDate],
                [ModifiedBy]
            FROM [dbo].[ItemTypeMaster]
            WHERE Id = @Id;

            RETURN;
        END

        -- SpType = 4: Get Single Device Module by Id
        ELSE IF (@SpType = 4)
        BEGIN
            SELECT 
                [Id],
                [TypeName],
                [TypeDesc],
                [GroupName],
                CASE WHEN [IsActive] = 1 THEN '1' ELSE '0' END AS [IsActive],
                [CreatedOn],
                [CreatedBy],
                [ModifiedDate],
                [ModifiedBy]
            FROM [dbo].[ItemTypeMaster]
            WHERE [Id] = @Id;

            RETURN;
        END

        -- SpType = 5: Get All Device Modules
        ELSE IF (@SpType = 5)
        BEGIN
            SELECT 
                [Id],
                [TypeName],
                [TypeDesc],
                [GroupName],
                CASE WHEN [IsActive] = 1 THEN '1' ELSE '0' END AS [IsActive],
                [CreatedOn],
                [CreatedBy],
                [ModifiedDate],
                [ModifiedBy]
            FROM [dbo].[ItemTypeMaster]
            ORDER BY [Id];

            RETURN;
        END


        /* =============================================
           DEVICE (ItemMaster) OPERATIONS
           ============================================= */

        -- SpType = 6: Insert Device
        ELSE IF (@SpType = 6)
        BEGIN
            INSERT INTO [dbo].[ItemMaster]
            (
                [TypeId],
                [ItemName],
                [ItemCode],
                [ItemUOM],
                [ItemDesc],
                [IsActive],
                [CreateOn],
                [Createby],
                [SqNo]
            )
            VALUES
            (
                @TypeId,
                @ItemName,
                @ItemCode,
                @ItemUOM,
                @ItemDesc,
                @IsActive,
                GETDATE(),
                @UserId,
                ISNULL(@SqNo, 0)
            );

            -- Return the inserted record with TypeName
            SELECT 
                im.[ItemId],
                im.[TypeId],
                im.[ItemName],
                im.[ItemCode],
                im.[ItemUOM],
                im.[ItemDesc],
                im.[IsActive],
                im.[SqNo],
                itm.[TypeName]
            FROM [dbo].[ItemMaster] im
            LEFT JOIN [dbo].[ItemTypeMaster] itm ON im.[TypeId] = itm.[Id]
            WHERE im.ItemId = SCOPE_IDENTITY();

            RETURN;
        END

        -- SpType = 7: Update Device
        ELSE IF (@SpType = 7)
        BEGIN
            UPDATE [dbo].[ItemMaster]
            SET
                [TypeId]   = @TypeId,
                [ItemName] = @ItemName,
                [ItemCode] = @ItemCode,
                [ItemUOM]  = @ItemUOM,
                [ItemDesc] = @ItemDesc,
                [IsActive] = @IsActive,
                [SqNo]     = @SqNo
            WHERE [ItemId] = @ItemId;

            -- Return the updated record with TypeName
            SELECT 
                im.[ItemId],
                im.[TypeId],
                im.[ItemName],
                im.[ItemCode],
                im.[ItemUOM],
                im.[ItemDesc],
                im.[IsActive],
                im.[SqNo],
                itm.[TypeName]
            FROM [dbo].[ItemMaster] im
            LEFT JOIN [dbo].[ItemTypeMaster] itm ON im.[TypeId] = itm.[Id]
            WHERE im.[ItemId] = @ItemId;

            RETURN;
        END

        -- SpType = 8: Soft Delete Device
        ELSE IF (@SpType = 8)
        BEGIN
            UPDATE [dbo].[ItemMaster]
            SET [IsActive] = '0'
            WHERE [ItemId] = @ItemId;

            -- Return the updated record
            SELECT 
                im.[ItemId],
                im.[TypeId],
                im.[ItemName],
                im.[ItemCode],
                im.[ItemUOM],
                im.[ItemDesc],
                im.[IsActive],
                im.[SqNo],
                itm.[TypeName]
            FROM [dbo].[ItemMaster] im
            LEFT JOIN [dbo].[ItemTypeMaster] itm ON im.[TypeId] = itm.[Id]
            WHERE im.[ItemId] = @ItemId;

            RETURN;
        END

        -- SpType = 9: Get Single Device by ItemId
        ELSE IF (@SpType = 9)
        BEGIN
            SELECT 
                im.[ItemId],
                im.[TypeId],
                im.[ItemName],
                im.[ItemCode],
                im.[ItemUOM],
                im.[ItemDesc],
                im.[IsActive],
                im.[SqNo],
                itm.[TypeName]
            FROM [dbo].[ItemMaster] im
            LEFT JOIN [dbo].[ItemTypeMaster] itm ON im.[TypeId] = itm.[Id]
            WHERE im.[ItemId] = @ItemId;

            RETURN;
        END

        -- SpType = 10: Get All Devices (optionally filtered by TypeId)
        ELSE IF (@SpType = 10)
        BEGIN
            SELECT 
                im.[ItemId],
                im.[TypeId],
                im.[ItemName],
                im.[ItemCode],
                im.[ItemUOM],
                im.[ItemDesc],
                im.[IsActive],
                im.[SqNo],
                itm.[TypeName]
            FROM [dbo].[ItemMaster] im
            LEFT JOIN [dbo].[ItemTypeMaster] itm ON im.[TypeId] = itm.[Id]
            WHERE (@TypeId IS NULL OR im.[TypeId] = @TypeId)
            ORDER BY im.[SqNo], im.[ItemId];

            RETURN;
        END

    END TRY
    BEGIN CATCH
        DECLARE
            @ErrorMessage NVARCHAR(4000),
            @ErrorSeverity INT,
            @ErrorState INT;

        SELECT
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- Grant execute permissions (adjust as needed)
-- GRANT EXECUTE ON [dbo].[SpAdminItemMasterType] TO [YourRole];

PRINT 'Stored Procedure SpAdminItemMasterType created successfully!'
GO
