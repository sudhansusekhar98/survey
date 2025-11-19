# EF Core Migration Instructions

## Install Required NuGet Packages

Run these commands in your project directory:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

## Create Initial Migration

```bash
dotnet ef migrations add InitialItemMasterSchema --context AppDbContext
```

## Apply Migration to Database

```bash
dotnet ef database update --context AppDbContext
```

## Verify Tables

After running the migration, verify in SQL Server that:
- `ItemTypeMaster` and `ItemMaster` tables exist
- Foreign key constraint `FK_ItemMaster_ItemTypeMaster` is created
- `IsActive` columns are `char(1)` type
- Default constraint on `CreatedOn` uses `GETDATE()`

## Rollback Migration (if needed)

```bash
dotnet ef database update 0 --context AppDbContext
dotnet ef migrations remove --context AppDbContext
```

## Generate SQL Script (without applying)

```bash
dotnet ef migrations script --context AppDbContext --output migration.sql
```

---

**Note:** Since your tables already exist in the database, you may want to scaffold from existing database instead:

```bash
dotnet ef dbcontext scaffold "Server=(Local);Database=VLDev;Integrated Security=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models/Generated --context-dir Data --context AppDbContext --table ItemTypeMaster --table ItemMaster --force
```

Then merge the generated models with your existing ones.
