# AGENTS.md

## Overview

ASP.NET Core 8 Web API for a library management system. Database-first with EF Core 9 against MSSQL. JWT auth, Serilog logging, Scalar/Swagger API docs.

## Solution structure

```
MiniLibraryMgmtSys.sln
├── MiniLibraryMgmtSys/            # Web API host (Program.cs, controllers via Domain)
├── MiniLibraryMgmtSys.Domain/     # Business logic, controllers, DTOs, DI wiring
├── MiniLibraryMgmtSys.Database/   # EF Core scaffolded DbContext + entity models
└── MiniLibraryMgmtSys.Shared/ 
```

**Dependency flow:** `Web API → Domain → Database`, `Domain → Shared`, `Shared → Database`

## Critical gotchas

- **Directory ≠ project name for Shared:** The directory is `MiniLibraryMgmtSys.Infrastructure/` but the `.csproj` and namespace are `MiniLibraryMgmtSys.Shared`. Project references use `..\\MiniLibraryMgmtSys.Shared\\MiniLibraryMgmtSys.Shared.csproj` **which does not exist on disk** — the Domain `.csproj` has this path but the actual file is at `MiniLibraryMgmtSys.Infrastructure\\MiniLibraryMgmtSys.Shared.csproj`. Do not rename the directory without fixing all project references.
- **Response wrapper types:** Code uses `ApiResponse<T>` extensively but `Result.cs` defines `Result<T>`. The scaffolded model entity files and code in `obj/` may contain the actual `ApiResponse<T>` class. When adding new service methods, follow existing code and use `ApiResponse<T>`.
- **Database-first, not code-first:** Entity models in `AppDbContextModels/` are scaffolded via `dotnet ef dbcontext scaffold`. Do **not** hand-edit these files — they will be overwritten on re-scaffold. The DB schema may have columns (`Role`, `IsActive`, `LastLoginDate`) not present in the SQL creation script in `MiniLibMgmtSysDatabase.md` — the scaffolded models are the source of truth.
- **`AppDbContext.OnConfiguring` has hardcoded connection string** with a `#warning` pragma. Runtime uses `appsettings.json` via `FeaturesManager.AddDomain()`. The hardcoded string only applies if the context is constructed without options.
- **EF Core version mismatch:** The web API targets `net8.0` but uses EF Core **9.0.10** packages. This works but be aware of it when troubleshooting.

## Commands

All commands run from the `MiniLibraryMgmtSys/MiniLibraryMgmtSys/` directory (the solution root containing `.sln`).

```sh
# Build
dotnet build

# Run the API (https profile uses port 7222, http uses 5033)
dotnet run --project MiniLibraryMgmtSys

# Re-scaffold database models (overwrites AppDbContextModels/)
dotnet ef dbcontext scaffold "Server=.;Database=MiniLibraryMgmtSys;User ID=sa;Password=sasa@123;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o AppDbContextModels -c AppDbContext -f --no-onconfiguring --project MiniLibraryMgmtSys.Database
```

## Architecture conventions

- **Feature-folder pattern in Domain:** Each feature (Book, User, Borrow, Dashboard, Auth) has its own folder under `Features/` containing `IXxxService.cs`, `XxxService.cs`, and `XxxController.cs`.
- **DI registration:** All services are manually registered in `FeaturesManager.cs` via `builder.AddDomain()`. When adding a new feature, add its `AddScoped<>()` call there.
- **Controllers live in the Domain project**, not the Web API project. The Web API project has no controllers — it just calls `builder.AddDomain()` and `app.MapControllers()`.
- **Soft delete pattern:** All entities use `DeleteFlag` (bool). Queries should filter `!DeleteFlag` — see `ActiveBooks` / `ActiveUser` query properties in services.
- **Audit fields:** Entities have `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`. Services set these manually (not via EF interceptors).
- **Auth:** JWT Bearer with roles `Admin`, `Librarian`, `Member`. Controllers use `[Authorize(Roles = "...")]`.
- **Password hashing:** `PasswordHasher.Hash()` / `PasswordHasher.Verify()` (BCrypt) in the Shared project.
- **API docs:** Scalar at `/scalar` (https profile), Swagger also configured. Development environment only.

## Database

- **MSSQL** with `sa` / `sasa@123` (local dev only)
- Main DB: `MiniLibraryMgmtSys`, Log DB: `MiniLibraryMgmtSys_Log`
- Tables: `tbl_users`, `tbl_books`, `tbl_borrowedBooks`
- Primary keys are `VARCHAR(50)`, set as `Guid.NewGuid().ToString()` in application code
- Serilog auto-creates `Tbl_LogEvent` in the log database

## Logging

Serilog writes to console, rolling file (`logs/mini_library_mgmt_sys_log.txt`), and MSSQL (`Tbl_LogEvent` table, auto-created).
