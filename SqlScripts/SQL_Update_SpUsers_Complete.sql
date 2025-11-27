USE [VLDev]
GO

-- ========================================
-- STEP 1: Add Profile Picture columns to LoginMaster table if they don't exist
-- ========================================

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.LoginMaster') 
               AND name = 'ProfilePictureUrl')
BEGIN
    ALTER TABLE [dbo].[LoginMaster] 
    ADD ProfilePictureUrl NVARCHAR(500) NULL
    PRINT 'ProfilePictureUrl column added to LoginMaster table'
END
ELSE
BEGIN
    PRINT 'ProfilePictureUrl column already exists in LoginMaster table'
END

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.LoginMaster') 
               AND name = 'ProfilePicturePublicId')
BEGIN
    ALTER TABLE [dbo].[LoginMaster] 
    ADD ProfilePicturePublicId NVARCHAR(200) NULL
    PRINT 'ProfilePicturePublicId column added to LoginMaster table'
END
ELSE
BEGIN
    PRINT 'ProfilePicturePublicId column already exists in LoginMaster table'
END

GO

-- ========================================
-- STEP 2: Update SpUsers stored procedure
-- ========================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[SpUsers]
( 
	@SpType INT = NULL,
	@UserID INT = NULL,
	@LoginID VARCHAR(20) = NULL,
	@LoginName VARCHAR(60) = NULL,
	@EmailID VARCHAR(100) = NULL,
	@MobileNo VARCHAR(10) = NULL,
	@LoginPassword VARCHAR(10) = NULL,
	@IsActive CHAR(1) = NULL,
	@RoleID VARCHAR(20) = NULL,
	@CreateBy INT = NULL,
	@EmpID INT = NULL,
	@ProfilePictureUrl NVARCHAR(500) = NULL,
	@ProfilePicturePublicId NVARCHAR(200) = NULL,
	@NewPassword VARCHAR(10) = NULL
)
AS
BEGIN TRY
	BEGIN TRANSACTION
	
	-- SpType = 1: User Login - Get user by LoginID and Password
	IF @SpType = 1
	BEGIN
		SELECT UserID, LoginID, LoginName, MobileNo, PinNo, EmailID, LoginPassword, 
		       A.RoleID, B.RoleName, A.ISActive,
		       CASE A.ISActive WHEN 'Y' THEN 'Active' ELSE 'InActive' END AS ISActivedesc,
		       A.EmpID, A.ProfilePictureUrl, A.ProfilePicturePublicId
		FROM LoginMaster A 
		INNER JOIN RoleMaster B ON A.RoleID = B.RoleID 
		WHERE LoginID = @LoginID AND LoginPassword = @LoginPassword
	END

	-- SpType = 2: Get All Users
	IF @SpType = 2 
	BEGIN
		SELECT ROW_NUMBER() OVER (ORDER BY LoginName) AS Srno,
		       UserID, LoginID, LoginName, MobileNo, PinNo, EmailID, LoginPassword,
		       A.RoleID, B.RoleName, A.ISActive,
		       CASE A.ISActive WHEN 'Y' THEN 'Active' ELSE 'InActive' END AS ISActivedesc,
		       CreateBy, A.EmpID, A.ProfilePictureUrl, A.ProfilePicturePublicId
		FROM LoginMaster A 
		INNER JOIN RoleMaster B ON A.RoleID = B.RoleID 
	END

	-- SpType = 3: Get User by UserID
	IF @SpType = 3
	BEGIN
		SELECT UserID, LoginID, LoginName, MobileNo, PinNo, EmailID, LoginPassword,
		       A.RoleID, B.RoleName, A.ISActive,
		       CASE A.ISActive WHEN 'Y' THEN 'Active' ELSE 'InActive' END AS ISActivedesc,
		       A.EmpID, A.ProfilePictureUrl, A.ProfilePicturePublicId
		FROM LoginMaster A 
		INNER JOIN RoleMaster B ON A.RoleID = B.RoleID 
		WHERE UserID = @UserID
	END

	-- SpType = 4: Insert New User
	IF @SpType = 4
	BEGIN
		DECLARE @Pinno NUMERIC(5,0);

		SELECT @Pinno = LEFT(SUBSTRING(RTRIM(RAND()) + SUBSTRING(RTRIM(RAND()), 3, 11), 3, 11), 5)
		WHERE LEFT(SUBSTRING(RTRIM(RAND()) + SUBSTRING(RTRIM(RAND()), 3, 11), 3, 11), 5) 
		      NOT IN (SELECT PinNo FROM LoginMaster)

		SET @Pinno = LEFT(CAST(@Pinno AS VARCHAR(5)) + '000', 5)
		
		INSERT INTO LoginMaster 
		(LoginID, LoginName, EmailID, MobileNo, LoginPassword, PinNo, ISActive, RoleID, 
		 CreateBy, EmpID, ProfilePictureUrl, ProfilePicturePublicId) 
		VALUES 
		(@LoginID, @LoginName, @EmailID, @MobileNo, @LoginPassword, @Pinno, @IsActive, 
		 @RoleID, @CreateBy, @EmpID, @ProfilePictureUrl, @ProfilePicturePublicId);
	END

	-- SpType = 5: Update User
	IF @SpType = 5
	BEGIN
		UPDATE LoginMaster 
		SET LoginID = @LoginID,
		    LoginName = @LoginName,
		    EmailID = @EmailID,
		    MobileNo = @MobileNo,
		    LoginPassword = @LoginPassword,
		    ISActive = @IsActive,
		    RoleID = @RoleID,
		    CreateBy = @CreateBy,
		    EmpID = @EmpID,
		    ProfilePictureUrl = @ProfilePictureUrl,
		    ProfilePicturePublicId = @ProfilePicturePublicId
		WHERE UserID = @UserID
	END

	-- SpType = 6: Change Password
	IF @SpType = 6
	BEGIN
		UPDATE LoginMaster
		SET LoginPassword = @NewPassword
		WHERE UserID = @UserID
	END

	-- SpType = 7: Update Profile Picture Only
	IF @SpType = 7
	BEGIN
		UPDATE LoginMaster
		SET ProfilePictureUrl = @ProfilePictureUrl,
		    ProfilePicturePublicId = @ProfilePicturePublicId
		WHERE UserID = @UserID
	END

	COMMIT
END TRY                                                              
BEGIN CATCH                                                                
	ROLLBACK                                                                            
	DECLARE @ErrorMessage NVARCHAR(4000);
	SELECT @ErrorMessage = ERROR_MESSAGE();                                                                            
	RAISERROR (@ErrorMessage, 16, 1);                                                                            
END CATCH

GO

PRINT ''
PRINT '========================================'
PRINT 'SpUsers stored procedure updated successfully!'
PRINT 'New features added:'
PRINT '- EmpID support in all operations'
PRINT '- ProfilePictureUrl and ProfilePicturePublicId fields'
PRINT '- SpType = 6: Change Password'
PRINT '- SpType = 7: Update Profile Picture Only'
PRINT '========================================'
