#!/bin/sh
set -e

echo "Running EF Core database migrations..."
dotnet ef database update --project DatabaseAdapters --startup-project TemperatureSystem --context SqLiteDatabaseContext
echo "Done migrating the database"

exec "$@"