-- =============================================
-- Report OTP Log Table for Access Control
-- Purpose: To store OTP requests and validations for report downloads
-- Created: 2025-12-26
-- =============================================

-- Create ReportOTPLog table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ReportOTPLog')
BEGIN
    CREATE TABLE [dbo].[ReportOTPLog] (
        [LogId]             BIGINT IDENTITY(1,1) NOT NULL,
        [UserId]            INT NOT NULL,                           -- User requesting the download
        [UserName]          NVARCHAR(100) NULL,                     -- User name for reference
        [ReportType]        NVARCHAR(50) NOT NULL,                  -- Type of report (Summary, Detailed, etc.)
        [ReportParameters]  NVARCHAR(MAX) NULL,                     -- JSON of report parameters (surveyId, filters, etc.)
        [OTP]               NVARCHAR(10) NOT NULL,                  -- Generated OTP code
        [OTPGeneratedAt]    DATETIME NOT NULL DEFAULT GETDATE(),    -- When OTP was generated
        [OTPExpiresAt]      DATETIME NOT NULL,                      -- When OTP expires (e.g., 10 minutes after generation)
        [OTPStatus]         NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Validated, Expired, Cancelled
        [ValidatedAt]       DATETIME NULL,                          -- When OTP was validated
        [ValidatedBy]       INT NULL,                               -- Super Admin who provided the OTP (if tracked)
        [DownloadedAt]      DATETIME NULL,                          -- When report was actually downloaded
        [IPAddress]         NVARCHAR(50) NULL,                      -- Client IP address
        [UserAgent]         NVARCHAR(500) NULL,                     -- Browser/client info
        [Remarks]           NVARCHAR(500) NULL,                     -- Any additional notes
        [CreatedDate]       DATETIME NOT NULL DEFAULT GETDATE(),
        [ModifiedDate]      DATETIME NULL,
        
        CONSTRAINT [PK_ReportOTPLog] PRIMARY KEY CLUSTERED ([LogId] ASC)
    );

    -- Create index for faster lookups
    CREATE NONCLUSTERED INDEX [IX_ReportOTPLog_UserId] ON [dbo].[ReportOTPLog] ([UserId]);
    CREATE NONCLUSTERED INDEX [IX_ReportOTPLog_OTP] ON [dbo].[ReportOTPLog] ([OTP], [OTPStatus]);
    CREATE NONCLUSTERED INDEX [IX_ReportOTPLog_Status] ON [dbo].[ReportOTPLog] ([OTPStatus], [OTPExpiresAt]);

    PRINT 'Table ReportOTPLog created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ReportOTPLog already exists.';
END
GO

-- =============================================
-- Stored Procedure: sp_ReportOTP
-- Purpose: Handle all OTP operations for report downloads
-- =============================================

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ReportOTP')
    DROP PROCEDURE sp_ReportOTP;
GO

CREATE PROCEDURE [dbo].[sp_ReportOTP]
    @Type INT,
    @LogId BIGINT = NULL,
    @UserId INT = NULL,
    @UserName NVARCHAR(100) = NULL,
    @ReportType NVARCHAR(50) = NULL,
    @ReportParameters NVARCHAR(MAX) = NULL,
    @OTP NVARCHAR(10) = NULL,
    @OTPExpiryMinutes INT = 10,
    @IPAddress NVARCHAR(50) = NULL,
    @UserAgent NVARCHAR(500) = NULL,
    @Remarks NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Type 1: Generate new OTP
    IF @Type = 1
    BEGIN
        -- Cancel any existing pending OTPs for this user
        UPDATE ReportOTPLog 
        SET OTPStatus = 'Cancelled', ModifiedDate = GETDATE()
        WHERE UserId = @UserId 
        AND OTPStatus = 'Pending' 
        AND OTPExpiresAt > GETDATE();
        
        -- Insert new OTP request
        INSERT INTO ReportOTPLog (
            UserId, UserName, ReportType, ReportParameters, 
            OTP, OTPGeneratedAt, OTPExpiresAt, OTPStatus,
            IPAddress, UserAgent, CreatedDate
        )
        VALUES (
            @UserId, @UserName, @ReportType, @ReportParameters,
            @OTP, GETDATE(), DATEADD(MINUTE, @OTPExpiryMinutes, GETDATE()), 'Pending',
            @IPAddress, @UserAgent, GETDATE()
        );
        
        SELECT SCOPE_IDENTITY() AS LogId;
    END
    
    -- Type 2: Validate OTP
    ELSE IF @Type = 2
    BEGIN
        DECLARE @ValidLogId BIGINT;
        DECLARE @ValidStatus NVARCHAR(20);
        
        SELECT TOP 1 @ValidLogId = LogId, @ValidStatus = OTPStatus
        FROM ReportOTPLog
        WHERE UserId = @UserId 
        AND OTP = @OTP
        AND OTPStatus = 'Pending'
        AND OTPExpiresAt > GETDATE()
        ORDER BY LogId DESC;
        
        IF @ValidLogId IS NOT NULL
        BEGIN
            -- Valid OTP found - update status
            UPDATE ReportOTPLog
            SET OTPStatus = 'Validated', 
                ValidatedAt = GETDATE(),
                ModifiedDate = GETDATE(),
                Remarks = ISNULL(@Remarks, Remarks)
            WHERE LogId = @ValidLogId;
            
            SELECT 1 AS IsValid, @ValidLogId AS LogId, 'OTP validated successfully' AS Message;
        END
        ELSE
        BEGIN
            -- Check if OTP exists but expired
            IF EXISTS (SELECT 1 FROM ReportOTPLog WHERE UserId = @UserId AND OTP = @OTP AND OTPStatus = 'Pending' AND OTPExpiresAt <= GETDATE())
            BEGIN
                -- Mark as expired
                UPDATE ReportOTPLog
                SET OTPStatus = 'Expired', ModifiedDate = GETDATE()
                WHERE UserId = @UserId AND OTP = @OTP AND OTPStatus = 'Pending';
                
                SELECT 0 AS IsValid, NULL AS LogId, 'OTP has expired' AS Message;
            END
            ELSE
            BEGIN
                SELECT 0 AS IsValid, NULL AS LogId, 'Invalid OTP' AS Message;
            END
        END
    END
    
    -- Type 3: Mark download completed
    ELSE IF @Type = 3
    BEGIN
        UPDATE ReportOTPLog
        SET DownloadedAt = GETDATE(), ModifiedDate = GETDATE()
        WHERE LogId = @LogId;
        
        SELECT 1 AS Success;
    END
    
    -- Type 4: Get OTP log history for a user
    ELSE IF @Type = 4
    BEGIN
        SELECT LogId, UserId, UserName, ReportType, ReportParameters,
               OTPGeneratedAt, OTPExpiresAt, OTPStatus, ValidatedAt, 
               DownloadedAt, IPAddress, CreatedDate
        FROM ReportOTPLog
        WHERE (@UserId IS NULL OR UserId = @UserId)
        ORDER BY CreatedDate DESC;
    END
    
    -- Type 5: Get pending OTP for user (for checking if user has valid OTP)
    ELSE IF @Type = 5
    BEGIN
        SELECT TOP 1 LogId, UserId, UserName, ReportType, ReportParameters,
               OTP, OTPGeneratedAt, OTPExpiresAt, OTPStatus
        FROM ReportOTPLog
        WHERE UserId = @UserId 
        AND OTPStatus = 'Validated'
        AND OTPExpiresAt > DATEADD(MINUTE, -5, GETDATE()) -- Valid for 5 more minutes after validation
        ORDER BY ValidatedAt DESC;
    END
    
    -- Type 6: Expire old pending OTPs (cleanup)
    ELSE IF @Type = 6
    BEGIN
        UPDATE ReportOTPLog
        SET OTPStatus = 'Expired', ModifiedDate = GETDATE()
        WHERE OTPStatus = 'Pending' AND OTPExpiresAt < GETDATE();
        
        SELECT @@ROWCOUNT AS ExpiredCount;
    END
    
    -- Type 7: Get all pending OTPs (for Super Admin dashboard)
    ELSE IF @Type = 7
    BEGIN
        SELECT LogId, UserId, UserName, ReportType, ReportParameters,
               OTP, OTPGeneratedAt, OTPExpiresAt, OTPStatus, IPAddress
        FROM ReportOTPLog
        WHERE OTPStatus = 'Pending' AND OTPExpiresAt > GETDATE()
        ORDER BY OTPGeneratedAt DESC;
    END
END
GO

PRINT 'Stored procedure sp_ReportOTP created successfully.';
GO
