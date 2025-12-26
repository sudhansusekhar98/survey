# Report Download OTP Access Control - Implementation Guide

## Overview

This feature implements OTP-based access control for report downloads to ensure secure access to reports by requiring OTP validation for non-Super Admin users.

## Requirements Implemented

1. **Access Restriction**: Non-Super Admin users (RoleId != 101) must undergo OTP verification before downloading any report.
2. **OTP Generation**: When a non-Super Admin attempts to download a report, the system generates a 6-digit OTP.
3. **OTP Distribution**: The OTP is sent to Super Admins who can view pending requests and share the code with the requesting user.
4. **OTP Validation**: The user enters the OTP provided by a Super Admin, and the system validates it.
5. **Report Download Authorization**: If valid, download proceeds; otherwise, it's denied.
6. **Audit & Logging**: All OTP requests and validations are logged.

## Files Created/Modified

### New Files

1. **SqlScripts/SQL_Create_ReportOTPLog_Table.sql**
   - Creates the `ReportOTPLog` table
   - Creates the `sp_ReportOTP` stored procedure

2. **Models/ReportOTPModel.cs**
   - `ReportOTPModel` - Database entity model
   - `OTPRequestModel` - Request ViewModel
   - `OTPValidationModel` - Validation ViewModel
   - `OTPResponseModel` - API response model
   - `ReportTypes` - Constants for report types

3. **Repo/IReportOTP.cs**
   - Interface for OTP repository operations

4. **Repo/ReportOTPRepo.cs**
   - Implementation of OTP repository
   - Methods: GenerateOTP, ValidateOTP, HasValidOTP, MarkDownloadCompleted, GetOTPHistory, GetPendingOTPs, ExpireOldOTPs, IsSuperAdmin

5. **Controllers/ReportOTPController.cs**
   - `CheckOTPRequired` - Check if current user needs OTP
   - `RequestOTP` - Generate new OTP
   - `ValidateOTP` - Validate entered OTP
   - `HasValidOTP` - Check if user has valid OTP
   - `GetPendingOTPs` - Get pending requests (Super Admin only)
   - `GetOTPHistory` - Get audit log (Super Admin only)
   - `AuditLog` - Audit log view
   - `PendingRequests` - Pending requests view

6. **wwwroot/js/reportOTP.js**
   - JavaScript module for OTP modal and validation
   - Methods: init, checkAndDownload, showOTPModal

7. **Views/ReportOTP/AuditLog.cshtml**
   - Audit log view for Super Admins

8. **Views/ReportOTP/PendingRequests.cshtml**
   - Pending OTP requests view for Super Admins

### Modified Files

1. **Program.cs**
   - Added `IReportOTP` and `ReportOTPRepo` dependency injection

2. **Controllers/SurveyReportsController.cs**
   - Added `IReportOTP` dependency
   - Added `IsAuthorizedForDownload()` helper method
   - Added `GetUnauthorizedResult()` helper method
   - Added OTP checks to:
     - `ExportToExcel`
     - `ExportDetailedReport`
     - `ExportDetailedReportNew`

3. **Views/SurveyReports/SummaryReport.cshtml**
   - Added OTP authorization warning banner
   - Modified Export Excel button for non-Super Admins
   - Added OTP JavaScript module integration

## Database Setup

Run the SQL script to create the required database objects:

```sql
-- Execute from SQL Server Management Studio
USE [VLDev]  -- or your database name
GO

-- Run the script
:r "D:\VL Access\Survey\CODES\Survey\survey\SqlScripts\SQL_Create_ReportOTPLog_Table.sql"
```

Or execute the contents of `SqlScripts/SQL_Create_ReportOTPLog_Table.sql` directly.

## User Flow

### For Non-Super Admin Users:

1. User clicks "Export Excel" on Summary Report
2. System checks if user has valid OTP
3. If no valid OTP, OTP modal appears
4. User clicks "Request OTP"
5. System generates OTP and **sends email notifications to all Super Admins**
6. Super Admin receives email with the OTP code
7. User contacts Super Admin (or Super Admin proactively shares the OTP)
8. User enters OTP in the modal
9. If valid, download proceeds automatically
10. All actions are logged for audit

### For Super Admins (RoleId = 101):

1. Super Admin can download reports directly (no OTP required)
2. **Super Admin receives email notification with OTP when a user requests report download**
3. Super Admin can also view pending OTP requests at `/ReportOTP/PendingRequests`
4. Super Admin can view audit log at `/ReportOTP/AuditLog`
5. Super Admin shares OTP code with requesting user

## OTP Specifications

- **Length**: 6 digits
- **Validity**: 10 minutes from generation
- **Status Values**: Pending, Validated, Expired, Cancelled
- **Reuse**: A validated OTP can be used for 5 minutes after validation

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/ReportOTP/CheckOTPRequired` | GET | Check if current user needs OTP |
| `/ReportOTP/RequestOTP` | POST | Generate new OTP |
| `/ReportOTP/ValidateOTP` | POST | Validate OTP |
| `/ReportOTP/HasValidOTP` | GET | Check if user has valid OTP |
| `/ReportOTP/GetPendingOTPs` | GET | Get pending OTPs (Super Admin) |
| `/ReportOTP/GetOTPHistory` | GET | Get audit log (Super Admin) |
| `/ReportOTP/AuditLog` | GET | Audit log view |
| `/ReportOTP/PendingRequests` | GET | Pending requests view |

## Audit Log Fields

- Log ID
- User ID and Name
- Report Type
- Report Parameters (JSON)
- OTP Generated At
- OTP Expires At
- OTP Status
- Validated At
- Downloaded At
- IP Address
- User Agent

## Security Considerations

1. OTP is never sent back to the requesting user in API responses
2. Only Super Admins can view the actual OTP codes
3. OTPs automatically expire after 10 minutes
4. All requests are logged with IP address and user agent
5. Old pending OTPs are marked as cancelled when new one is requested

## Future Enhancements

1. ~~**Email Notifications**: Implement email notification to Super Admins when OTP is requested~~ ✅ **IMPLEMENTED**
2. **SMS Notifications**: Add SMS option for OTP delivery
3. **Rate Limiting**: Limit OTP requests per user per hour
4. **Audit Dashboard**: Enhanced analytics on OTP usage patterns

## Email Template

When an OTP is requested, Super Admins receive a professionally styled email containing:
- Requesting user's name and ID
- Report type requested
- Large, clearly visible OTP code
- Expiry time (10 minutes from generation)
- Instructions for sharing the OTP

**Note**: For email to work, ensure the Super Admin users have their `EmailID` field populated in the `LoginMaster` table.
