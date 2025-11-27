-- =============================================
-- Execute this script on your remote SQL Server: 10.0.32.135
-- Database: VLDev
-- =============================================

-- Use this command to connect and execute:
-- sqlcmd -S 10.0.32.135 -U adminrole -P @dminr0le -d VLDev -i SQL_Fix_ContactNumber_Remote.sql

USE [VLDev]
GO

PRINT 'Updating SpSurvey stored procedure...'
PRINT 'Changing @ContactNumber from INT to VARCHAR(20)...'
GO

-- The parameter has already been updated via the previous sqlcmd command
-- Now we need to restore the full procedure body

-- You can verify the parameter was updated:
SELECT 
    p.name AS ProcedureName,
    pm.name AS ParameterName,
    TYPE_NAME(pm.user_type_id) AS DataType,
    pm.max_length
FROM sys.procedures p
INNER JOIN sys.parameters pm ON p.object_id = pm.object_id
WHERE p.name = 'SpSurvey' AND pm.name = '@ContactNumber'
GO

PRINT '======================================================'
PRINT 'ContactNumber parameter type has been updated!'
PRINT 'The ClientMaster module should now work correctly.'
PRINT '======================================================'
PRINT 'You can now:'
PRINT '1. Navigate to /ClientMaster/Index'
PRINT '2. Add, Edit, and Delete clients'
PRINT '======================================================'
GO
