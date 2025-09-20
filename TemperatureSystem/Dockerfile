# Stage 1: The Base Image - Defines the final runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Create a non-root user for security
RUN adduser -u 5678 --disabled-password --gecos "" app
USER app

# Stage 2: The Build Image - Contains the SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project and solution files first to leverage Docker layer caching
COPY ["TemperatureSystem/TemperatureSystem.csproj", "TemperatureSystem/"]
COPY ["TemperatureSystem.sln", "."]
COPY ["TemperatureSystem.UnitTests/TemperatureSystem.UnitTests.csproj", "TemperatureSystem.UnitTests/"]
COPY ["TemperatureSystem.IntegrationTests/TemperatureSystem.IntegrationTests.csproj", "TemperatureSystem.IntegrationTests/"]

# Restore dependencies for the entire solution
RUN dotnet restore "TemperatureSystem.sln"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/TemperatureSystem"

# Publish the application, which includes building it
RUN dotnet publish "TemperatureSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: The Final Image - Copies the published output to the lean base image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TemperatureSystem.dll"]