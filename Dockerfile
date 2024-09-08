#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-windowsservercore-ltsc2022 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0-windowsservercore-ltsc2022 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FUParkingApi/FUParkingApi.csproj", "FUParkingApi/"]
COPY ["FUParkingModel/FUParkingModel.csproj", "FUParkingModel/"]
COPY ["FUParkingRepository/FUParkingRepository.csproj", "FUParkingRepository/"]
COPY ["FUParkingService/FUParkingService.csproj", "FUParkingService/"]
RUN dotnet restore "./FUParkingApi/FUParkingApi.csproj"
COPY . .
WORKDIR "/src/FUParkingApi"
RUN dotnet build "./FUParkingApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FUParkingApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FUParkingApi.dll"]