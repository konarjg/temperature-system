FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files
COPY ["TemperatureSystem.sln", "."]
COPY ["TemperatureSystem/TemperatureSystem.csproj", "TemperatureSystem/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["DatabaseAdapters/DatabaseAdapters.csproj", "DatabaseAdapters/"]
COPY ["ExternalServiceAdapters/ExternalServiceAdapters.csproj", "ExternalServiceAdapters/"]
COPY ["UnitTests/UnitTests.csproj", "UnitTests/"]
COPY ["IntegrationTests/IntegrationTests.csproj", "IntegrationTests/"]

RUN dotnet restore "TemperatureSystem.sln"

COPY . .
WORKDIR "/src"
RUN dotnet publish "TemperatureSystem/TemperatureSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the user to non-root for security
USER app
ENTRYPOINT ["dotnet", "TemperatureSystem.dll"]