-- SQL Script to add @EmpID parameter to SpUsers stored procedure
-- This script modifies the SpUsers stored procedure to support EmpID field

-- First, let's check the current structure
-- Run this to see the current procedure definition:
-- EXEC sp_helptext 'SpUsers'

-- Update for SpType = 4 (Insert User)
-- Add @EmpID parameter and include it in the INSERT statement

/*
Example modification for INSERT (SpType = 4):

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
    @EmpID INT = NULL,  -- ADD THIS PARAMETER
    @NewPassword VARCHAR(255) = NULL
AS
BEGIN
    IF @SpType = 4  -- Insert User
    BEGIN
        INSERT INTO [dbo].[UserMaster] 
        (LoginId, LoginName, MobileNo, EmailID, LoginPassword, IsActive, RoleID, CreateBy, EmpID)
        VALUES 
        (@LoginId, @LoginName, @MobileNo, @EmailID, @LoginPassword, @IsActive, @RoleID, @CreateBy, @EmpID)
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
            EmpID = @EmpID
        WHERE UserID = @UserID
    END
    
    -- Other SpType conditions remain unchanged
    
END
*/

-- INSTRUCTIONS:
-- 1. First, get the current stored procedure definition:
--    EXEC sp_helptext 'SpUsers'
-- 2. Copy the output
-- 3. Add @EmpID INT = NULL to the parameter list
-- 4. Add EmpID to the column list in INSERT statement (SpType = 4)
-- 5. Add EmpID to the SET clause in UPDATE statement (SpType = 5)
-- 6. Execute the modified ALTER PROCEDURE statement

-- If UserMaster table doesn't have EmpID column, add it first:
-- ALTER TABLE [dbo].[UserMaster] ADD EmpID INT NULL

PRINT 'Please execute the ALTER PROCEDURE statement above after reviewing your current SpUsers procedure'
PRINT 'You may also need to add EmpID column to UserMaster table if it does not exist'
