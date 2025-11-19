# Survey Application - AI Agent Instructions

## Architecture Overview

This is an ASP.NET Core 8.0 MVC application for managing infrastructure surveys with the following key characteristics:

- **Authentication**: Session-based authentication via `UserLoginController` - stores `UserID`, `UserName`, `RoleId` in session
- **Database**: SQL Server with stored procedures, accessed via `DBConnection.ConnectionString` (hardcoded to local VLDev database)
- **Repository Pattern**: Interfaces in `/Repo` (e.g., `ISurvey`, `IAdmin`) with concrete implementations using raw SQL and stored procedures

## Critical Patterns

### Data Access
- Uses `SqlDbHelper.DataTableToList<T>()` for mapping DataTable results to models
- All database operations use stored procedures with `SpType` parameter pattern:
  ```csharp
  cmd.Parameters.AddWithValue("@SpType", 1); // 1=Insert, 2=Update, 3=Delete, 4=Select
  ```
- Null handling: Use `DBNull.Value` for null parameters, not `null`

### Controller Structure
- Controllers inherit from `Controller` and use dependency injection for repositories
- Standard pattern: `Index()` for listing, `Create()` for add/edit forms, POST for submissions
- TempData used for success/error messages: `TempData["ResultMessage"]` and `TempData["ResultType"]`
- Session access: `HttpContext.Session.GetString("UserID")` for current user

### View Conventions
- Bootstrap-based UI with custom purple theme (`bg-purple` class)
- DataTables for listing views (`datatables-basic` class)
- Partial views for common elements in `Views/Shared/Sections/`
- Alert system uses `@Html.Raw(TempData["ResultMessage"])` with Bootstrap alert classes

## Development Workflow

### Running the Application
```bash
dotnet run --project SurveyApp.csproj
# Default URL: https://localhost:7041 (HTTPS) or http://localhost:5016 (HTTP)
# Default route: /UserLogin/Index (login page)
```

### Adding New Features
1. Create model in `/Models` with `[Display]` attributes for form labels
2. Add interface to `/Repo` following `ISurvey` pattern
3. Implement repository with stored procedure calls
4. Register in `Program.cs`: `builder.Services.AddScoped<IYourInterface, YourRepo>()`
5. Create controller with dependency injection
6. Add views following existing Bootstrap/DataTables patterns

### Session Management
- Login sets: `UserID`, `UserName`, `RoleId` in session
- Session timeout: 30 minutes (configured in `Program.cs`)
- Logout: `HttpContext.Session.Clear()` + cookie deletion

## Third-Party Integrations

- **QuestPDF**: Community license for PDF generation (`QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community`)
- **EPPlus**: Excel handling with non-commercial license (`ExcelPackage.License.SetNonCommercialOrganization("ABTMS")`)
- **Email**: SMTP via `EmailService` with settings in `appsettings.json`

## Database Configuration

Connection string is hardcoded in `DBConnection.cs`:
```csharp
private static readonly string cs = "Server=(Local);Database=VLDev;Integrated Security=True;Connect Timeout=360000;TrustServerCertificate=True";
```

## Common Issues & Solutions

1. **Null Reference in Views**: Always check `Model != null && Model.Count > 0` before iteration
2. **Session Loss**: Ensure session middleware is configured before routing in `Program.cs`
3. **SQL Parameter Issues**: Use `DBNull.Value` for null values, not C# `null`
4. **PDF Generation**: Remember to set QuestPDF license before using
5. **DataTables Integration**: Use `datatables-basic` class and ensure jQuery/DataTables scripts are loaded

## File Organization Notes

- Excluded folders in `.csproj` indicate this was refactored from a larger application (removed Charts, PowerReport, etc.)
- `pdfHelper/` contains QuestPDF document generators
- `wwwroot/vendor/` contains third-party UI libraries
- Some controllers/models are excluded but still referenced - check `.csproj` excludes before adding similar names