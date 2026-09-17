# Shelfly.App.Migrations

**Framework**: `net10.0`

## Purpose

EF Core migration scaffolding project for `Shelfly.App.Data`. Contains design-time context factory and generated migration classes.

## When to Use This Project

Use this project when:
- Adding new entities or modifying existing ones in `Shelfly.App.Data`
- Running EF Core CLI commands (`dotnet ef migrations add`, `update-database`)

## Key Files

| File | Purpose |
|------|---------|
| `DesignTimeDbContextFactory.cs` | Creates `LocalDbContext` for design-time migration scaffolding (SQLite) |
| `Migrations/` | Generated migration up/down scripts and model snapshots |

## Commands

**Add migration** (use the script):
```bash
./.agents/scripts/add-migration.sh <MigrationName>       # bash
.\.agents\scripts\add-migration.ps1 <MigrationName>      # PowerShell
```

## EF Core Skill

When working with migrations, load the `ef-core` skill for best practices:
- Small, focused migrations
- Descriptive naming conventions
- Verify SQL before applying to production
- Use migration bundles for deployment scenarios

## Important Notes

- **Configuration**: Always use `--configuration Release` when generating migrations to include all build configurations
- **Assembly**: The migrations assembly is explicitly set in `DesignTimeDbContextFactory` — this project's own assembly name
- **Database Provider**: SQLite (local file-based) — matches the app's runtime database
- **Model Snapshot**: `LocalDbContextModelSnapshot.cs` tracks the current model state; EF Core diffs against it to generate new migrations

## Migration Workflow

1. Add/modify entities in `Shelfly.App.Data/Entities/`
2. Add configurations in `Shelfly.App.Data/Configurations/` using `IEntityTypeConfiguration<T>`
3. Register configurations in `LocalDbContext.OnModelCreating()` via `modelBuilder.ApplyConfiguration()`
4. Run migration command from solution root with `--project Shelfly.App.Migrations`
5. Review generated files in `Migrations/` for correctness
