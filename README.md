# Survey Application

A comprehensive enterprise survey management system built with **ASP.NET Core 8** and **SQL Server**. This application enables organizations to create, assign, execute, review, and report on surveys with a complete workflow from creation to approval.

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-blue)
![License](https://img.shields.io/badge/License-Proprietary-red)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Modules](#modules)
- [Database Schema](#database-schema)
- [User Roles & Permissions](#user-roles--permissions)
- [Workflow](#workflow)
- [Support](#support)

---

## 🎯 Overview

The Survey Application is designed for field survey management with comprehensive features for:

- **Survey Creation & Management** - Create and configure surveys with client association, locations, and device/item types
- **Team Assignment** - Assign surveys to field teams with due dates and automatic notifications
- **Field Execution** - Mobile-friendly interface for field teams to complete surveys with device specifications and photo uploads
- **Submission & Approval** - Multi-level approval workflow with revision support
- **Reporting** - Detailed reports with Excel/PDF export and OTP-protected downloads
- **Administration** - User management, device configuration, and granular access control

---

## ✨ Features

### Core Features

| Feature                    | Description                                                                     |
| -------------------------- | ------------------------------------------------------------------------------- |
| 📝 **Survey Creation**     | Create surveys with name, client, region, implementation type, and due dates    |
| 📍 **Sub-Locations**       | Add multiple sub-locations to a survey with GPS coordinates and map integration |
| 🌐 **Global Location**     | Special location for survey-wide items (cables, centralized equipment)          |
| 👥 **Team Assignment**     | Assign surveys to employees with automatic status tracking                      |
| 📱 **Survey Execution**    | Complete surveys with device selection, quantities, and specifications          |
| � **Photo Documentation**  | Upload photos with watermarks/comments for each device/item                     |
| 🎥 **Camera Remarks**      | Specify deployment direction/location for each camera instance                  |
| 📏 **Pole Specifications** | Enter owner (for existing poles) and height (for new poles)                     |
| ✅ **Approval Workflow**   | Submit → Review → Approve/Reject with email notifications                       |
| 🔄 **Survey Revision**     | Assign revisions to approved surveys for resurvey                               |
| 📊 **Dashboard**           | Real-time statistics with status breakdown and overdue alerts                   |
| 📈 **Reports**             | Detailed/Summary reports with Excel and PDF export                              |

### Additional Features

- **PWA Support** - Progressive Web App with offline capabilities and service worker
- **Email Notifications** - Automated notifications for submissions, approvals, rejections, and revisions
- **Cloud Image Storage** - Cloudinary integration for survey photos
- **Conditional Specifications** - Item specifications that appear based on quantity values
- **OTP-Protected Reports** - Secure report downloads with OTP verification for non-admin users
- **Location Status Tracking** - Track completion status per location (In Progress/Completed/Verified)
- **Preview Before Submit** - Modal preview of all survey data before submission
- **Unlock Submitted Surveys** - Option to withdraw/unlock submitted surveys before approval

---

## 🛠 Technology Stack

### Backend

- **Framework:** ASP.NET Core 8.0 (MVC)
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core 9.0 + ADO.NET (Stored Procedures)
- **PDF Generation:** QuestPDF (Community License)
- **Excel Export:** EPPlus 8.1
- **Cloud Storage:** Cloudinary

### Frontend

- **Views:** Razor (.cshtml)
- **Styling:** Custom CSS + Bootstrap 5
- **JavaScript:** Vanilla JS + jQuery
- **Charts:** Chart.js
- **Date Picker:** Flatpickr
- **Icons:** Bootstrap Icons

### Infrastructure

- **Session Management:** Distributed Memory Cache (30 min timeout)
- **Email:** SMTP (Gmail App Passwords)
- **Location API:** External location service for State/City dropdowns

---

## 📁 Project Structure

```
survey/
├── Controllers/           # MVC Controllers
│   ├── ClientMasterController.cs        # Client management
│   ├── DashboardController.cs           # Dashboard & analytics
│   ├── DevicesAdminController.cs        # Device/module/specification configuration
│   ├── EmployeeMasterController.cs      # Employee management
│   ├── HelpController.cs                # Help documentation pages
│   ├── ProfileController.cs             # User profile & password management
│   ├── ReportOTPController.cs           # OTP-protected report access
│   ├── SurveyCamRemarksController.cs    # Camera remarks management
│   ├── SurveyCreationController.cs      # Survey CRUD, locations, assignments, revisions
│   ├── SurveyDetailsController.cs       # Survey execution & data entry
│   ├── SurveyReportsController.cs       # Report generation & export
│   ├── SurveySubmissionController.cs    # Submission & approval workflow
│   ├── UserLoginController.cs           # Authentication & forced password change
│   ├── UserRightsController.cs          # Permission management
│   └── UsersController.cs               # User administration & sync
│
├── Models/               # Data models & view models
│   ├── SurveyModel.cs                   # Core survey model
│   ├── SurveyLocationModel.cs           # Location data
│   ├── SurveyDetailsModel.cs            # Item-level survey data
│   ├── SurveySubmissionModel.cs         # Submission workflow
│   ├── SurveyRevisionModel.cs           # Revision tracking
│   ├── SurveyCamRemarksModel.cs         # Camera remarks
│   ├── ItemSpecificationModel.cs        # Device specifications
│   ├── UserModel.cs                     # User/login data
│   ├── UsersRightsModel.cs              # Permission configuration
│   ├── ClientMasterModel.cs             # Client data
│   ├── EmpMasterModel.cs                # Employee data
│   ├── DashboardViewModel.cs            # Dashboard statistics
│   └── ...                              # Additional models
│
├── Views/                # Razor views
│   ├── ClientMaster/                    # Client management views
│   ├── Dashboard/                       # Dashboard view
│   ├── DevicesAdmin/                    # Device configuration views
│   ├── EmployeeMaster/                  # Employee views
│   ├── Help/                            # Help documentation (9 pages)
│   ├── Home/                            # Home page
│   ├── Profile/                         # User profile
│   ├── ReportOTP/                       # OTP verification
│   ├── Shared/                          # Layouts, partials, components
│   ├── SurveyCreation/                  # Survey management views
│   ├── SurveyDetails/                   # Survey execution views
│   ├── SurveyReports/                   # Report views
│   ├── SurveySubmission/                # Submission views
│   ├── UserLogin/                       # Login & password change views
│   ├── UserRights/                      # Permission views
│   └── Users/                           # User management views
│
├── Repo/                 # Repository layer (interfaces & implementations)
│   ├── IAdmin.cs / AdminRepo.cs         # Admin operations
│   ├── ISurvey.cs / SurveyRepo.cs       # Survey operations
│   ├── ISurveySubmission.cs / ...       # Submission operations
│   ├── ISurveyRevision.cs / ...         # Revision operations
│   ├── ISurveyCamRemarks.cs / ...       # Camera remarks operations
│   ├── IEmailService.cs / EmailService.cs
│   ├── ICloudinaryService.cs / CloudinaryService.cs
│   └── ...
│
├── Data/                 # Database context
│   └── AppDbContext.cs                  # EF Core context
│
├── SqlScripts/           # Database scripts & documentation (43 files)
│
├── wwwroot/              # Static files
│   ├── css/                             # Stylesheets (11 files)
│   ├── js/                              # JavaScript files (33 files)
│   ├── img/                             # Images
│   ├── vendor/                          # Third-party libraries
│   ├── manifest.json                    # PWA manifest
│   └── service-worker.js                # PWA service worker
│
├── Program.cs            # Application entry point & DI configuration
├── appsettings.json      # Configuration
└── SurveyApp.csproj      # Project file
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd survey
   ```

2. **Restore NuGet packages**

   ```bash
   dotnet restore
   ```

3. **Configure the database connection**

   Update `Program.cs` or `appsettings.json` with your SQL Server connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(Local);Database=VLDev;Integrated Security=True;TrustServerCertificate=True"
     }
   }
   ```

4. **Run database migrations**

   ```bash
   dotnet ef database update
   ```

5. **Execute SQL scripts** (required)

   Run the scripts in the `SqlScripts/` folder in order to set up stored procedures, tables, and constraints.

6. **Run the application**

   ```bash
   dotnet run
   ```

## ⚙ Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  },
  "Email": {
    "From": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "AllowedHosts": "*"
}
```

### Email Configuration (Gmail SMTP)

1. Enable 2-Factor Authentication on your Gmail account
2. Generate an App Password (Security → App Passwords)
3. Update the `Email` section in `appsettings.json`

### Cloudinary Configuration

1. Create a [Cloudinary account](https://cloudinary.com/)
2. Get your Cloud Name, API Key, and API Secret from Dashboard
3. Update the `Cloudinary` section in `appsettings.json`

---

## 📦 Modules

### 1. Dashboard

- Real-time survey statistics with status counts
- Status breakdown: Created, Assigned, In Progress, Submitted, Completed, Pending, On Hold
- Regional distribution and implementation type charts
- Missed deadline alerts (overdue surveys)
- Quick access to recent surveys (last 100)
- Completion rate percentage

### 2. Survey Creation & Management

- **Create Survey**: Name, client, region, implementation type, due date
- **Manage Locations**: Add sub-locations with GPS coordinates, map picker
- **Global Location**: For survey-wide items (cables, main equipment)
- **Device Type Selection**: Choose which devices to survey at each location
- **Team Assignment**: Assign employees with due dates
- **View Completed Surveys**: Archive of approved surveys

### 3. Survey Execution

- **Add Sub-Locations**: Indoor/Outdoor, Aerial/Underground/Wall-mounted types
- **Select Device Types**: Choose modules and devices to survey
- **Enter Quantities**: Existing vs Required quantities for each item
- **Specifications**: Model numbers, capacities, custom fields
- **Camera Remarks**: Direction/deployment location for each camera (#Cam1: Towards temple, #Cam2: Main entrance)
- **Pole Specifications**: Owner dropdown (Telecom/Electrical/Municipality) for existing, Height dropdown (4m-12m) for new poles
- **Photo Upload**: Multiple photos per item with watermarks
- **Auto-Save**: Save progress frequently

### 4. Submission & Approval

- **Preview & Submit**: Modal preview of all data before submission
- **Submit for Approval**: Locks survey, notifies creator/admins via email
- **Pending Reviews**: Supervisor view of submitted surveys
- **Approve/Reject**: With comments; rejection unlocks survey for editing
- **My Submissions**: Track status of submitted surveys
- **Unlock Survey**: Withdraw submission before approval if needed

### 5. Survey Revision

- **Assign Revision**: For approved surveys needing resurvey
- **Revision History**: Track all revisions with reasons and status
- **Pending Revisions**: Dashboard for assigned revisions
- **Revision Workflow**: Copy survey data, reassign, track separately

### 6. Reports & Analytics

- **Summary Report**: High-level statistics and totals
- **Detailed Report**: Complete survey data with all locations and items
- **Excel Export**: Structured data for pivot tables and analysis
- **PDF Export**: Professional reports for documentation
- **OTP Protection**: Non-admin users require OTP from admin for downloads

### 7. Administration (Admin Only)

- **User Rights Management**: Granular permissions per module (View/Create/Update/Execute/Delete)
- **Employee Master**: Manage employee records (name, email, mobile, department)
- **Login Master (Users)**: Create/edit user credentials, roles, password reset
- **Sync to Login**: Bulk create login accounts for employees
- **Client Master**: Manage client organizations with contact details
- **Devices Admin**:
  - Modules (Categories): Networking, Power, Security, etc.
  - Devices: Individual items within modules
  - Specifications: Custom fields with input types and options

---

## 🗃 Database Schema

### Core Survey Tables

| Table                      | Description                                       |
| -------------------------- | ------------------------------------------------- |
| `Survey`                   | Main survey records with revision tracking fields |
| `SurveyLocation`           | Survey sub-locations with GPS coordinates         |
| `SurveyDetails`            | Item-level data (quantities, remarks, photos)     |
| `SurveySubmission`         | Submission workflow records                       |
| `SurveyRevisionLog`        | Revision history and tracking                     |
| `SurveyAssignment`         | Team assignments with due dates                   |
| `SurveyLocationStatus`     | Location completion status tracking               |
| `AssignedItems`            | Selected item types per location                  |
| `SurveyCamRemarks`         | Camera direction/location remarks                 |
| `SurveyLocationCableCount` | Global cable count per location                   |

### Master Data Tables

| Table                            | Description                             |
| -------------------------------- | --------------------------------------- |
| `LoginMaster`                    | User credentials and authentication     |
| `UserRights`                     | Permission assignments per user/module  |
| `EmpMaster`                      | Employee records                        |
| `ClientMaster`                   | Client organization information         |
| `ItemTypeMaster`                 | Device modules/categories               |
| `ItemMaster`                     | Devices/items within modules            |
| `ItemSpecificationMaster`        | Device specification definitions        |
| `ItemSpecificationOptionsMaster` | Dropdown options for specifications     |
| `SpecificationDetails`           | Saved specification values              |
| `RegionMaster`                   | Geographic regions                      |
| `ReportOTPLog`                   | OTP verification logs for report access |

### Key Stored Procedures

| Procedure          | Purpose                                               |
| ------------------ | ----------------------------------------------------- |
| `SpSurvey`         | Survey CRUD operations (Create, Update, Delete, List) |
| `SpSurveyDetails`  | Survey detail management and reporting                |
| `SpSurveyRevision` | Revision workflow (create, history, status updates)   |
| `SpUsers`          | User management operations                            |
| `SpUserRights`     | Permission management                                 |

---

## 🔐 User Roles & Permissions

### Role IDs

| Role ID | Role Name   | Description                                     |
| ------- | ----------- | ----------------------------------------------- |
| 101     | Super Admin | Full system access, cannot be restricted        |
| 102     | Admin       | Admin panel access, can be partially restricted |
| 103+    | User        | Standard user, fully customizable permissions   |

### Permission Types

| Permission  | Description                            | Use Case                                         |
| ----------- | -------------------------------------- | ------------------------------------------------ |
| **View**    | Read-only access to module data        | Required for menu visibility                     |
| **Create**  | Add new records                        | Creating surveys, employees, clients             |
| **Update**  | Modify existing records (full edit)    | Editing survey settings, locations               |
| **Execute** | Survey participation without full edit | Field workers - fill data, upload photos, submit |
| **Delete**  | Remove records                         | Use with caution, grant only to trusted users    |

### Module-Specific Permissions

| Module                   | Description                            |
| ------------------------ | -------------------------------------- |
| **Survey**               | Survey creation, execution, submission |
| **Client Master**        | Client organization management         |
| **Employee Master**      | Employee record management             |
| **Users (Login Master)** | User credential management             |
| **User Rights**          | Permission configuration (Admin only)  |

### Best Practice: Field Workers

Grant **View + Execute** permissions for field surveyors. This allows them to:

- View assigned surveys
- Fill in device quantities and specifications
- Upload photos
- Submit for approval

**Without** ability to:

- Create new surveys
- Modify survey settings
- Delete data

---

## 🔄 Workflow

### Survey Lifecycle

```
┌─────────────┐
│   Created   │  ← Survey created with basic info
└──────┬──────┘
       ▼
┌─────────────┐
│  Assigned   │  ← Team assigned to survey
└──────┬──────┘
       ▼
┌─────────────┐
│ In Progress │  ← First location added, work started
└──────┬──────┘
       ▼
┌─────────────┐
│  Submitted  │  ← Survey locked, pending review
└──────┬──────┘
       ▼
   ┌───┴───┐
   ▼       ▼
┌─────┐ ┌──────────┐
│Aprv.│ │ Rejected │
└──┬──┘ └────┬─────┘
   │         │
   ▼         └──→ Survey unlocked, back to "In Progress"
┌─────────┐       └──→ Submitter makes corrections
│Completed│       └──→ Resubmit when ready
└────┬────┘
     │
     ▼
┌─────────────────┐
│ Assign Revision │ (Optional - for resurvey)
└─────────────────┘
```

### Submission Process

1. **Field User** completes all survey locations
2. **Field User** clicks "Preview & Submit" to review data
3. **Field User** confirms submission
4. Survey is **locked** for editing
5. **Creator and Super Admins** receive email notification
6. **Reviewer** checks "Pending Reviews" page
7. **Reviewer** views detailed report
8. **Reviewer** approves or rejects with comments
9. **Submitter** receives email with result
10. If rejected, survey unlocks for corrections

### Auto-Unlock Feature

Users can unlock their own submitted surveys before approval by accessing the Locations page. This withdraws the submission without requiring rejection.

---

## 📖 Help Documentation

The application includes comprehensive built-in help pages accessible from the **Help** menu:

| Help Page             | Content                                        |
| --------------------- | ---------------------------------------------- |
| **Help Center**       | Overview and navigation to all topics          |
| **Quick Start Guide** | 5-minute getting started for each role         |
| **Survey Creation**   | Creating and configuring surveys               |
| **Team Assignment**   | Assigning employees to surveys                 |
| **Survey Execution**  | Detailed guide for field work                  |
| **Survey Submission** | Submit, review, approve/reject workflow        |
| **Reports**           | Report types and export options                |
| **Admin Functions**   | User rights, employee/client/device management |
| **FAQ**               | Frequently asked questions                     |

---


## 🆘 Support

### Contact Information

- **IT Support:** support@vlaccess.com
- **HR Department:** hr@vlaccess.com
- **Web:** support@vlaccess.com

### Troubleshooting

**Problem:** Can't login

- Verify credentials are correct
- Check if account is active (contact admin if locked)
- If admin set temporary password, you'll be prompted to change it

**Problem:** Survey not saving

- Check internet connection
- Verify all required fields are filled
- Save frequently (every 5-10 items)
- Clear browser cache and retry

**Problem:** Photo upload failing

- Check file size (should be under 5MB)
- Use JPG or PNG format
- Ensure stable internet connection
- Try uploading one photo at a time

**Problem:** Cannot submit survey

- Ensure all locations have data entered
- Check if already submitted (view My Submissions)
- Verify all required specifications are complete

**Problem:** Report download fails

- Non-admin users need OTP from admin
- Check permissions include report access
- Try a different browser

---

## 📄 License

This application is proprietary software developed for VL Access. All rights reserved.

---

## 🔄 Version History

| Version | Date     | Description                                     |
| ------- | -------- | ----------------------------------------------- |
| 1.0     | Dec 2025 | Initial release with core survey functionality  |
| 1.1     | Dec 2025 | Added submission workflow and approval system   |
| 1.2     | Dec 2025 | Added revision system and OTP-protected reports |
| 1.3     | Dec 2025 | Added camera remarks and pole specifications    |
| 1.4     | Dec 2025 | Enhanced help documentation and user rights     |

---

_Last Updated: December 27, 2025_
