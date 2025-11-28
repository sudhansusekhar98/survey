USE [VLDev]
GO

ALTER PROCEDURE [dbo].[SpAdminItemMasterType]
(
    @Mode        VARCHAR(30),        -- e.g. 'ITEMTYPE_INSERT', 'ITEM_INSERT', etc.

    -- ItemTypeMaster parameters
    @Id          INT             = NULL,      -- PK of ItemTypeMaster
    @TypeName    NVARCHAR(100)   = NULL,
    @TypeDesc    NVARCHAR(250)   = NULL,
    @GroupName   NVARCHAR(100)   = NULL,
    @IsActive    BIT             = 1,
    @UserId      INT             = NULL,      -- For CreatedBy / ModifiedBy

    -- ItemMaster parameters
    @ItemId      INT             = NULL,      -- PK of ItemMaster
    @TypeId      INT             = NULL,      -- FK to ItemTypeMaster.Id  (column name is TypeId)
    @ItemName    NVARCHAR(200)   = NULL,
    @ItemCode    NVARCHAR(100)   = NULL,
    @ItemDesc    NVARCHAR(500)   = NULL,
    @SqNo        INT             = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        /* ============================
           ITEM TYPE MASTER  OPERATIONS
           ============================ */

        IF (@Mode = 'ITEMTYPE_INSERT')
        BEGIN
            INSERT INTO [VLDev].[dbo].[ItemTypeMaster]
            (
                [TypeName],
                [TypeDesc],
                [GroupName],
                [IsActive],
                [CreatedOn],
                [CreatedBy],
                [ModifiedDate],
                [ModifiedBy]
            )
            VALUES
            (
                @TypeName,
                @TypeDesc,
                @GroupName,
                @IsActive,
                GETDATE(),
                @UserId,
                NULL,
                NULL
            );

            SELECT *
            FROM [VLDev].[dbo].[ItemTypeMaster]
            WHERE Id = SCOPE_IDENTITY();

            RETURN;
        END

        ELSE IF (@Mode = 'ITEMTYPE_UPDATE')
        BEGIN
            UPDATE [VLDev].[dbo].[ItemTypeMaster]
            SET
                [TypeName]     = @TypeName,
                [TypeDesc]     = @TypeDesc,
                [GroupName]    = @GroupName,
                [IsActive]     = @IsActive,
                [ModifiedDate] = GETDATE(),
                [ModifiedBy]   = @UserId
            WHERE [Id] = @Id;

            SELECT *
            FROM [VLDev].[dbo].[ItemTypeMaster]
            WHERE Id = @Id;

            RETURN;
        END

        ELSE IF (@Mode = 'ITEMTYPE_DELETE')
        BEGIN
            -- Soft delete; change to hard DELETE if you really want to remove row
            UPDATE [VLDev].[dbo].[ItemTypeMaster]
            SET
                [IsActive]     = 0,
                [ModifiedDate] = GETDATE(),
                [ModifiedBy]   = @UserId
            WHERE [Id] = @Id;

            SELECT *
            FROM [VLDev].[dbo].[ItemTypeMaster]
            WHERE Id = @Id;

            RETURN;
        END

        ELSE IF (@Mode = 'ITEMTYPE_GET')
        BEGIN
            SELECT *
            FROM [VLDev].[dbo].[ItemTypeMaster]
            WHERE [Id] = @Id;

            RETURN;
        END

        ELSE IF (@Mode = 'ITEMTYPE_GETALL')
        BEGIN
            SELECT *
            FROM [VLDev].[dbo].[ItemTypeMaster];
            -- or: WHERE IsActive = 1

            RETURN;
        END


        /* ============================
           ITEM MASTER  OPERATIONS
           ============================ */

        IF (@Mode = 'ITEM_INSERT')
        BEGIN
            INSERT INTO [VLDev].[dbo].[ItemMaster]
            (
                [TypeId],
                [ItemName],
                [ItemCode],
                [ItemDesc],
                [IsActive],
                [CreateOn],
                [Createby],
                [SqNo]
            )
            VALUES
            (
                @TypeId,        -- column is TypeId ✅
                @ItemName,
                @ItemCode,
                @ItemDesc,
                @IsActive,
                GETDATE(),
                @UserId,        -- or separate @CreateBy param if you want
                @SqNo
            );

            SELECT *
            FROM [VLDev].[dbo].[ItemMaster]
            WHERE ItemId = SCOPE_IDENTITY();

            RETURN;
        END

        ELSE IF (@Mode = 'ITEM_UPDATE')
        BEGIN
            UPDATE [VLDev].[dbo].[ItemMaster]
            SET
                [TypeId]   = @TypeId,
                [ItemName] = @ItemName,
                [ItemCode] = @ItemCode,
                [ItemDesc] = @ItemDesc,
                [IsActive] = @IsActive,
                [SqNo]     = @SqNo
            WHERE [ItemId] = @ItemId;

            SELECT *
            FROM [VLDev].[dbo].[ItemMaster]
            WHERE [ItemId] = @ItemId;

            RETURN;
        END

        ELSE IF (@Mode = 'ITEM_DELETE')
        BEGIN
            -- Soft delete
            UPDATE [VLDev].[dbo].[ItemMaster]
            SET [IsActive] = 0
            WHERE [ItemId] = @ItemId;

            SELECT *
            FROM [VLDev].[dbo].[ItemMaster]
            WHERE [ItemId] = @ItemId;

            RETURN;
        END

        ELSE IF (@Mode = 'ITEM_GET')
        BEGIN
            SELECT *
            FROM [VLDev].[dbo].[ItemMaster]
            WHERE [ItemId] = @ItemId;

            RETURN;
        END

        ELSE IF (@Mode = 'ITEM_GETALL')
        BEGIN
            SELECT *
            FROM [VLDev].[dbo].[ItemMaster];
            -- or: WHERE IsActive = 1

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
