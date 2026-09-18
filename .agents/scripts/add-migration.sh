#!/usr/bin/env bash
# Adds an EF Core migration to Shelfly.App.Migrations
# Usage: ./.agents/scripts/add-migration.sh <MigrationName>

set -e

if [ -z "$1" ]; then
    echo "Usage: $0 <MigrationName>"
    exit 1
fi

MIGRATION_NAME="$1"

dotnet ef migrations add \
    --project Shelfly.App.Migrations\Shelfly.App.Migrations.csproj \
    --startup-project Shelfly.App.Migrations\Shelfly.App.Migrations.csproj \
    --context Shelfly.App.Data.LocalDbContext \
    --configuration Release \
    --framework net10.0 \
    "$MIGRATION_NAME" \
    --output-dir Migrations
