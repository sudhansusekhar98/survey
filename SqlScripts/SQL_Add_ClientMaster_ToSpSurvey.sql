-- =============================================
-- Script: Add ClientMaster CRUD operations to SpSurvey
-- Description: Adds INSERT, UPDATE, DELETE, and SELECT operations for ClientMaster table
-- SpType values: 21 (INSERT), 22 (UPDATE), 23 (DELETE), 24 (SELECT)
-- =============================================

USE [VLDev]
GO

-- First, ensure the stored procedure exists and add parameters if needed
-- This script assumes SpSurvey already exists

ALTER PROCEDURE [dbo].[SpSurvey]
    -- ... (existing parameters remain unchanged)
    
    -- ClientMaster parameters (add these to the existing parameter list)
    @ClientID INT = NULL,
    @ClientName NVARCHAR(200) = NULL,
    @ClientType NVARCHAR(100) = NULL,
    @Address1 NVARCHAR(500) = NULL,
    @Address3 NVARCHAR(500) = NULL,
    @State NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @ContactPerson NVARCHAR(200) = NULL,
    @ContactNumber NVARCHAR(20) = NULL
    -- Note: CreatedBy parameter should already exist in SpSurvey
AS
BEGIN
    SET NOCOUNT ON;

    -- =============================================
    -- ClientMaster Operations
    -- =============================================

    -- 1. INSERT - Create new client
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

    -- 2. UPDATE - Update existing client
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

    -- 3. DELETE - Soft delete (set Isactive = 0)
    IF(@SpType = 23)
    BEGIN
        UPDATE ClientMaster
        SET Isactive = 0
        WHERE ClientID = @ClientID;

        SELECT 'Deleted Successfully' AS Message;
        RETURN;
    END

    -- 4. SELECT - Get client(s)
    IF(@SpType = 24)
    BEGIN
        IF(@ClientID IS NOT NULL)
        BEGIN
            -- Get specific client by ID
            SELECT * FROM ClientMaster WHERE ClientID = @ClientID;
        END
        ELSE 
        BEGIN
            -- Get all active clients
            SELECT * FROM ClientMaster WHERE Isactive = 1;
        END
        RETURN;
    END

    -- ... (existing SpType operations continue below)

END
GO

PRINT 'ClientMaster operations added to SpSurvey successfully!'
PRINT 'SpType 21 = INSERT Client'
PRINT 'SpType 22 = UPDATE Client'
PRINT 'SpType 23 = DELETE Client (Soft)'
PRINT 'SpType 24 = SELECT Client(s)'
GO
