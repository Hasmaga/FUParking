FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        locales \
        libicu-dev \
    && rm -rf /var/lib/apt/lists/* \
    && locale-gen en_US.UTF-8 \
    && update-locale LANG=en_US.UTF-8
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
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