# Stage 1: The Base Image - Defines the final runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# --- THIS IS THE NEW, CRUCIAL SECTION FOR SECRETS ---
# 1. Define build arguments that the 'docker build' command can pass in.
#    These are temporary and only exist during the build.
ARG JWT_SECRET_KEY
ARG EMAIL_SENDER_PASSWORD

# 2. Set permanent environment variables in the final image using the values from the ARGs.
#    The double-underscore format allows ASP.NET Core to map them to nested JSON keys.
ENV Jwt__Key=$JWT_SECRET_KEY
ENV Email__SenderPassword=$EMAIL_SENDER_PASSWORD
# ---------------------------------------------------

# Create a non-root user for security
RUN adduser -u 5678 --disabled-password --gecos "" app
USER app

# Stage 2: The Build Image - Contains the SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project and solution files first to leverage Docker layer caching
COPY ["TemperatureSystem.sln", "."]
COPY ["TemperatureSystem/TemperatureSystem.csproj", "TemperatureSystem/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["DatabaseAdapters/DatabaseAdapters.csproj", "DatabaseAdapters/"]
COPY ["ExternalServiceAdapters/ExternalServiceAdapters.csproj", "ExternalServiceAdapters/"]
# You should also add your other projects here for a complete restore
COPY ["ExternalServiceAdapters/ExternalServiceAdapters.csproj", "ExternalServiceAdapters/"]
COPY ["UnitTests/UnitTests.csproj", "UnitTests/"]
COPY ["IntegrationTests/IntegrationTests.csproj", "IntegrationTests/"]

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