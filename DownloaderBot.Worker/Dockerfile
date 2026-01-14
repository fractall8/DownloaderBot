FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

USER root

RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*
   
RUN pip3 install yt-dlp --break-system-packages

RUN useradd -u 1000 -m appuser

RUN mkdir -p /app/downloads && chown -R appuser:appuser /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DownloaderBot.Worker/DownloaderBot.Worker.csproj", "DownloaderBot.Worker/"]
COPY ["DownloaderBot.Shared/DownloaderBot.Shared.csproj", "DownloaderBot.Shared/"]
RUN dotnet restore "DownloaderBot.Worker/DownloaderBot.Worker.csproj"
COPY . .
WORKDIR "/src/DownloaderBot.Worker"
RUN dotnet build "./DownloaderBot.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DownloaderBot.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER appuser 

ENTRYPOINT ["dotnet", "DownloaderBot.Worker.dll"]
