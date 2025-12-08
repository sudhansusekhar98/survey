-- =============================================
-- SQL Script to add MustChangePassword feature
-- Run this script on the VLDev database
-- =============================================

-- Step 1: Add MustChangePassword column to LoginMaster table (if not already added)
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'LoginMaster' AND COLUMN_NAME = 'MustChangePassword'
)
BEGIN
    ALTER TABLE LoginMaster
    ADD MustChangePassword BIT NOT NULL DEFAULT 0;
    
    PRINT 'Column MustChangePassword added to LoginMaster table.';
END
ELSE
BEGIN
    PRINT 'Column MustChangePassword already exists.';
END
GO

-- Step 2: Update the SpUsers stored procedure to include MustChangePassword
-- This adds a new section for SpType = 1 (Login) to return MustChangePassword
-- And updates SpType = 8 (Change Password) to properly verify current password

-- First, let's check the current SpType = 1 section and add MustChangePassword to the SELECT
-- You need to modify your existing SpUsers stored procedure

-- IMPORTANT: Find the SpType = 1 section in your SpUsers stored procedure and ensure 
-- the SELECT statement includes MustChangePassword:
/*
    IF @SpType = 1  -- Login
    BEGIN
        SELECT UserID, LoginID, LoginName, LoginPassword, RoleID, ISActive, 
               ProfilePictureUrl, ProfilePicturePublicId, MustChangePassword
        FROM LoginMaster 
        WHERE LoginID = @LoginId AND LoginPassword = @LoginPassword
    END
*/

-- Also verify SpType = 8 section for password change:
/*
    IF @SpType = 8  -- Change Password
    BEGIN
        -- First verify current password
        IF EXISTS (SELECT 1 FROM LoginMaster WHERE UserID = @UserID AND LoginPassword = @LoginPassword)
        BEGIN
            UPDATE LoginMaster 
            SET LoginPassword = @NewPassword,
                MustChangePassword = 0
            WHERE UserID = @UserID
            
            SELECT @@ROWCOUNT AS RowsAffected
        END
        ELSE
        BEGIN
            -- Return 0 to indicate password verification failed
            SELECT 0 AS RowsAffected
        END
    END
*/

PRINT 'Please verify your SpUsers stored procedure includes MustChangePassword in the login query (SpType = 1)'
PRINT 'and properly verifies current password in change password (SpType = 8).'
GO
