# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY TaskManager.sln .
COPY nuget.config .
COPY Directory.Build.props .
COPY src/TaskManager.Domain/TaskManager.Domain.csproj src/TaskManager.Domain/
COPY src/TaskManager.Application/TaskManager.Application.csproj src/TaskManager.Application/
COPY src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj src/TaskManager.Infrastructure/
COPY src/TaskManager.API/TaskManager.API.csproj src/TaskManager.API/

# Restore dependencies
RUN dotnet restore src/TaskManager.API/TaskManager.API.csproj

# Copy all source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/TaskManager.API/TaskManager.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

COPY --from=build /app/publish .

# Set non-root user
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
