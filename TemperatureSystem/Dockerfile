# Stage 1: The Base Image - Defines the final runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

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
COPY ["UnitTests/UnitTests.csproj", "UnitTests/"]
COPY ["IntegrationTests/IntegrationTests.csproj", "IntegrationTests/"]

# Restore dependencies for the entire solution
RUN dotnet restore "TemperatureSystem.sln"

# --- INSTALL THE EF TOOL IN THE SDK STAGE ---
RUN dotnet tool install --global dotnet-ef

# Copy the rest of the source code
COPY . .
WORKDIR "/src"

# Publish the application, which includes building it
RUN dotnet publish "TemperatureSystem/TemperatureSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: The Final Image - Copies the published output to the lean base image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# --- SECRETS CONFIGURATION ---
ARG JWT_SECRET_KEY
ARG EMAIL_SENDER_PASSWORD
ENV Jwt__Key=$JWT_SECRET_KEY
ENV Email__SenderPassword=$EMAIL_SENDER_PASSWORD
# -----------------------------

# --- EF CORE MIGRATIONS SETUP ---
# 1. Copy the pre-installed dotnet-ef tool from the build stage.
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools
# 2. Add the tool's directory to the system's PATH environment variable.
ENV PATH="${PATH}:/root/.dotnet/tools"
# 3. Copy the entrypoint script into the container's working directory.
COPY ["TemperatureSystem/setup.sh", "."]
# 4. Make the script executable. This is a required step on Linux.
RUN chmod +x ./setup.sh
# --------------------------------

# --- PERMISSIONS FIX FOR SQLITE DATABASE (QUICK METHOD) ---
RUN chown -R app:app /app
# --------------------------------------------------------

# --- SECURITY BEST PRACTICE ---
USER app
# ----------------------------

# --- EXECUTION ---
ENTRYPOINT ["./setup.sh"]
CMD ["dotnet", "TemperatureSystem.dll"]