# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY host/Byd.GtfsRealtimeManagement.HttpApi.Host/Byd.GtfsRealtimeManagement.HttpApi.Host.csproj ./
COPY NuGet.Config ./
RUN dotnet restore && rm NuGet.Config
# Copy everything else and build
COPY . ./
RUN dotnet publish host/Byd.GtfsRealtimeManagement.HttpApi.Host/Byd.GtfsRealtimeManagement.HttpApi.Host.csproj -c Release -o out
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Byd.GtfsRealtimeManagement.HttpApi.Host.dll"]
