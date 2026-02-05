# Multi-stage build for Render deployment
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/PatternBlindness.Api/PatternBlindness.Api.csproj", "src/PatternBlindness.Api/"]
COPY ["src/PatternBlindness.Application/PatternBlindness.Application.csproj", "src/PatternBlindness.Application/"]
COPY ["src/PatternBlindness.Domain/PatternBlindness.Domain.csproj", "src/PatternBlindness.Domain/"]
COPY ["src/PatternBlindness.Infrastructure/PatternBlindness.Infrastructure.csproj", "src/PatternBlindness.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/PatternBlindness.Api/PatternBlindness.Api.csproj"

# Copy source code
COPY . .

# Publish
RUN dotnet publish "src/PatternBlindness.Api/PatternBlindness.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "PatternBlindness.Api.dll"]
