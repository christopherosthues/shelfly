#!/usr/bin/env pwsh
# Adds an EF Core migration to Shelfly.App.Migrations
# Usage: .\.agents\scripts\add-migration.ps1 <MigrationName>

param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$MigrationName
)

dotnet ef migrations add `
    --project "Shelfly.App.Migrations\Shelfly.App.Migrations.csproj" `
    --startup-project "Shelfly.App.Migrations\Shelfly.App.Migrations.csproj" `
    --context Shelfly.App.Data.LocalDbContext `
    --configuration Release `
    --framework net10.0 `
    $MigrationName `
    --output-dir Migrations
