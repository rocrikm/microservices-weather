# BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./CloudWeather.Precipitation.csproj" --disable-parallel
RUN dotnet publish "./CloudWeather.Precipitation.csproj" -c release -o /app --no-restore

# SERVE STAGE
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
WORKDIR /app
COPY --from=build /app ./

# All services will listen to the same port
EXPOSE 5000

ENTRYPOINT ["dotnet", "./CloudWeather.Precipitation.dll"]