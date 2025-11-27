-- SQL Script to add Profile Picture fields to UserMaster table and SpUsers stored procedure
-- This script adds ProfilePictureUrl and ProfilePicturePublicId fields

-- ========================================
-- STEP 1: Add columns to UserMaster table
-- ========================================

-- Check if columns exist, if not add them
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.UserMaster') 
               AND name = 'ProfilePictureUrl')
BEGIN
    ALTER TABLE [dbo].[UserMaster] 
    ADD ProfilePictureUrl NVARCHAR(500) NULL
    PRINT 'ProfilePictureUrl column added to UserMaster table'
END
ELSE
BEGIN
    PRINT 'ProfilePictureUrl column already exists in UserMaster table'
END

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.UserMaster') 
               AND name = 'ProfilePicturePublicId')
BEGIN
    ALTER TABLE [dbo].[UserMaster] 
    ADD ProfilePicturePublicId NVARCHAR(200) NULL
    PRINT 'ProfilePicturePublicId column added to UserMaster table'
END
ELSE
BEGIN
    PRINT 'ProfilePicturePublicId column already exists in UserMaster table'
END

GO

-- ========================================
-- STEP 2: Update SpUsers stored procedure
-- ========================================

-- First, check the current procedure definition:
-- EXEC sp_helptext 'SpUsers'

/*
Add the following parameters to SpUsers:
    @ProfilePictureUrl NVARCHAR(500) = NULL,
    @ProfilePicturePublicId NVARCHAR(200) = NULL

Example modification:

ALTER PROCEDURE [dbo].[SpUsers]
    @SpType INT = NULL,
    @UserID INT = NULL,
    @LoginId VARCHAR(50) = NULL,
    @LoginName VARCHAR(100) = NULL,
    @MobileNo VARCHAR(15) = NULL,
    @EmailID VARCHAR(100) = NULL,
    @LoginPassword VARCHAR(255) = NULL,
    @IsActive VARCHAR(1) = NULL,
    @RoleID INT = NULL,
    @CreateBy INT = NULL,
    @EmpID INT = NULL,
    @NewPassword VARCHAR(255) = NULL,
    @ProfilePictureUrl NVARCHAR(500) = NULL,      -- ADD THIS
    @ProfilePicturePublicId NVARCHAR(200) = NULL  -- ADD THIS
AS
BEGIN
    IF @SpType = 1  -- Get all users
    BEGIN
        SELECT UserID, LoginId, LoginName, MobileNo, EmailID, LoginPassword, 
               IsActive, RoleID, CreateBy, EmpID, ProfilePictureUrl, ProfilePicturePublicId
        FROM [dbo].[UserMaster]
        ORDER BY UserID DESC
    END
    
    IF @SpType = 2  -- Get user by ID
    BEGIN
        SELECT UserID, LoginId, LoginName, MobileNo, EmailID, LoginPassword, 
               IsActive, RoleID, CreateBy, EmpID, ProfilePictureUrl, ProfilePicturePublicId
        FROM [dbo].[UserMaster]
        WHERE UserID = @UserID
    END
    
    IF @SpType = 3  -- Get user for login
    BEGIN
        SELECT UserID, LoginId, LoginName, LoginPassword, IsActive, RoleID, 
               ProfilePictureUrl, ProfilePicturePublicId
        FROM [dbo].[UserMaster]
        WHERE LoginId = @LoginId AND LoginPassword = @LoginPassword
    END
    
    IF @SpType = 4  -- Insert User
    BEGIN
        INSERT INTO [dbo].[UserMaster] 
        (LoginId, LoginName, MobileNo, EmailID, LoginPassword, IsActive, RoleID, 
         CreateBy, EmpID, ProfilePictureUrl, ProfilePicturePublicId)
        VALUES 
        (@LoginId, @LoginName, @MobileNo, @EmailID, @LoginPassword, @IsActive, 
         @RoleID, @CreateBy, @EmpID, @ProfilePictureUrl, @ProfilePicturePublicId)
    END
    
    IF @SpType = 5  -- Update User
    BEGIN
        UPDATE [dbo].[UserMaster]
        SET 
            LoginId = @LoginId,
            LoginName = @LoginName,
            MobileNo = @MobileNo,
            EmailID = @EmailID,
            LoginPassword = @LoginPassword,
            IsActive = @IsActive,
            RoleID = @RoleID,
            CreateBy = @CreateBy,
            EmpID = @EmpID,
            ProfilePictureUrl = @ProfilePictureUrl,
            ProfilePicturePublicId = @ProfilePicturePublicId
        WHERE UserID = @UserID
    END
    
    IF @SpType = 6  -- Change Password
    BEGIN
        UPDATE [dbo].[UserMaster]
        SET LoginPassword = @NewPassword
        WHERE UserID = @UserID
    END
    
    IF @SpType = 7  -- Update Profile Picture Only
    BEGIN
        UPDATE [dbo].[UserMaster]
        SET 
            ProfilePictureUrl = @ProfilePictureUrl,
            ProfilePicturePublicId = @ProfilePicturePublicId
        WHERE UserID = @UserID
    END
END
*/

-- ========================================
-- INSTRUCTIONS:
-- ========================================
-- 1. Run STEP 1 above to add columns to UserMaster table
-- 2. Get the current stored procedure definition:
--    EXEC sp_helptext 'SpUsers'
-- 3. Add the two new parameters to the parameter list
-- 4. Add ProfilePictureUrl and ProfilePicturePublicId to all SELECT statements (SpType = 1, 2, 3)
-- 5. Add ProfilePictureUrl and ProfilePicturePublicId to INSERT statement (SpType = 4)
-- 6. Add ProfilePictureUrl and ProfilePicturePublicId to UPDATE statement (SpType = 5)
-- 7. Optionally add SpType = 7 for updating profile picture only
-- 8. Execute the modified ALTER PROCEDURE statement

PRINT ''
PRINT '========================================'
PRINT 'STEP 1: Columns added to UserMaster table'
PRINT 'STEP 2: Please update the SpUsers stored procedure manually'
PRINT '========================================'
PRINT ''
PRINT 'To update SpUsers procedure:'
PRINT '1. EXEC sp_helptext ''SpUsers'''
PRINT '2. Add the new parameters and update all SpType conditions'
PRINT '3. See comments above for example modifications'
