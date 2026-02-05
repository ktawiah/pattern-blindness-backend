#!/bin/bash
set -e

# Run database migrations
dotnet ef database update

# Start the application
exec dotnet PatternBlindness.Api.dll