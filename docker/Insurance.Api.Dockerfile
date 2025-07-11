# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Set environment variables to avoid Windows-specific paths
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV NUGET_XMLDOC_MODE=skip
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

# Copy Directory.Packages.props for centralized package management
COPY Directory.Packages.props ./

# Copy solution file
COPY ThreadPilot.sln ./

# Copy all project files for proper dependency resolution
COPY src/Services/Insurance/Insurance.Api/Insurance.Api.csproj ./src/Services/Insurance/Insurance.Api/
COPY src/Services/Insurance/Insurance.Core/Insurance.Core.csproj ./src/Services/Insurance/Insurance.Core/
COPY src/Services/Insurance/Insurance.Infrastructure/Insurance.Infrastructure.csproj ./src/Services/Insurance/Insurance.Infrastructure/
COPY src/Shared/ThreadPilot.Common/ThreadPilot.Common.csproj ./src/Shared/ThreadPilot.Common/

# Clear NuGet cache and configure NuGet to avoid Windows-specific paths
RUN dotnet nuget locals all --clear

# Create a clean NuGet.config to avoid Windows-specific package paths
RUN echo '<?xml version="1.0" encoding="utf-8"?>' > nuget.config && \
    echo '<configuration>' >> nuget.config && \
    echo '  <packageSources>' >> nuget.config && \
    echo '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />' >> nuget.config && \
    echo '  </packageSources>' >> nuget.config && \
    echo '</configuration>' >> nuget.config

# Restore NuGet packages with verbose logging to debug issues
RUN dotnet restore src/Services/Insurance/Insurance.Api/Insurance.Api.csproj --verbosity normal

# Copy the source code
COPY src/Services/Insurance/ ./src/Services/Insurance/
COPY src/Shared/ ./src/Shared/

# Clean any existing build artifacts that might have Windows-specific paths
RUN find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true && \
    find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

# Build the application (remove --no-restore to avoid cache issues)
RUN dotnet build src/Services/Insurance/Insurance.Api/Insurance.Api.csproj -c Release

# Publish the application
RUN dotnet publish src/Services/Insurance/Insurance.Api/Insurance.Api.csproj -c Release --no-restore -o /app/publish

# Use the official .NET 9.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r insuranceapi && useradd -r -g insuranceapi insuranceapi

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Change ownership of the application directory to the non-root user
RUN chown -R insuranceapi:insuranceapi /app

# Switch to the non-root user
USER insuranceapi

# Expose the port the app runs on
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

# Configure health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Set the entry point
ENTRYPOINT ["dotnet", "Insurance.Api.dll"]