FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DownloaderBot.Api/DownloaderBot.Api.csproj", "DownloaderBot.Api/"]
COPY ["DownloaderBot.Shared/DownloaderBot.Shared.csproj", "DownloaderBot.Shared/"]
RUN dotnet restore "DownloaderBot.Api/DownloaderBot.Api.csproj"
COPY . .
WORKDIR "/src/DownloaderBot.Api"
RUN dotnet build "./DownloaderBot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DownloaderBot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DownloaderBot.Api.dll"]
