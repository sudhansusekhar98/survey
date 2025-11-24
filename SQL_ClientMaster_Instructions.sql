-- =============================================
-- Script: Add ClientMaster Operations to SpSurvey
-- Run this script in your VLDev database
-- =============================================

USE [VLDev]
GO

-- Check if SpSurvey exists
IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'SpSurvey')
BEGIN
    PRINT 'ERROR: SpSurvey stored procedure not found!'
    PRINT 'Please create SpSurvey first or use a different stored procedure name.'
    RETURN
END
GO

-- Add the ClientMaster operations to SpSurvey
-- You need to manually add these parameters to your existing SpSurvey parameter list:
/*
    @ClientID INT = NULL,
    @ClientName NVARCHAR(200) = NULL,
    @ClientType NVARCHAR(100) = NULL,
    @Address1 NVARCHAR(500) = NULL,
    @Address3 NVARCHAR(500) = NULL,
    @State NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @ContactPerson NVARCHAR(200) = NULL,
    @ContactNumber NVARCHAR(20) = NULL
*/

-- Then add these SpType operations to the body of SpSurvey:

/*
    -- =============================================
    -- ClientMaster CRUD Operations
    -- =============================================

    -- 1. INSERT CLIENT (SpType = 21)
    IF(@SpType = 21)
    BEGIN
        INSERT INTO ClientMaster
        (
            ClientName, ClientType, Address1, Address3,
            State, City, ContactPerson, ContactNumber,
            Isactive, CreatedOn, CreatedBy
        )
        VALUES
        (
            @ClientName, @ClientType, @Address1, @Address3,
            @State, @City, @ContactPerson, @ContactNumber,
            1, GETDATE(), @CreatedBy
        );

        SELECT SCOPE_IDENTITY() AS NewClientID;
        RETURN;
    END

    -- 2. UPDATE CLIENT (SpType = 22)
    IF(@SpType = 22)
    BEGIN
        UPDATE ClientMaster
        SET 
            ClientName = @ClientName,
            ClientType = @ClientType,
            Address1 = @Address1,
            Address3 = @Address3,
            State = @State,
            City = @City,
            ContactPerson = @ContactPerson,
            ContactNumber = @ContactNumber
        WHERE ClientID = @ClientID;

        SELECT 'Updated Successfully' AS Message;
        RETURN;
    END

    -- 3. DELETE CLIENT - Soft Delete (SpType = 23)
    IF(@SpType = 23)
    BEGIN
        UPDATE ClientMaster
        SET Isactive = 0
        WHERE ClientID = @ClientID;

        SELECT 'Deleted Successfully' AS Message;
        RETURN;
    END

    -- 4. SELECT CLIENT(S) (SpType = 24)
    IF(@SpType = 24)
    BEGIN
        IF(@ClientID IS NOT NULL)
        BEGIN
            SELECT * FROM ClientMaster WHERE ClientID = @ClientID;
        END
        ELSE 
        BEGIN
            SELECT * FROM ClientMaster WHERE Isactive = 1;
        END
        RETURN;
    END
*/

PRINT '======================================================'
PRINT 'MANUAL STEPS REQUIRED:'
PRINT '======================================================'
PRINT '1. Open SpSurvey stored procedure in SSMS'
PRINT '2. Add the parameters listed above to the parameter list'
PRINT '3. Add the SpType operation blocks (21-24) to the procedure body'
PRINT '4. Execute ALTER PROCEDURE to save changes'
PRINT '======================================================'
PRINT 'SpType 21 = INSERT Client'
PRINT 'SpType 22 = UPDATE Client'
PRINT 'SpType 23 = DELETE Client (Soft)'
PRINT 'SpType 24 = SELECT Client(s)'
PRINT '======================================================'
GO
