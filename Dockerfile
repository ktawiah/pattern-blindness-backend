# Runtime-only Dockerfile
# Build locally first with: dotnet publish src/PatternBlindness.Api/PatternBlindness.Api.csproj -c Release -o publish
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy pre-built publish output
COPY publish/ .

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PatternBlindness.Api.dll"]
