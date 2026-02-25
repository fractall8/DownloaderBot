# DownloaderBot

### A high-performance, asynchronous Telegram bot designed for downloading audio content from multiple platforms, including YouTube, TikTok, and SoundCloud. The project is built with a focus on scalability, clean architecture, and modern DevOps practices.

## Architecture
The project is built using a distributed Producer-Consumer architecture to decouple request handling from resource-intensive media processing:

- <b>The Producer</b> (`DownloaderBot.Api`): An ASP.NET Core Web API that handles incoming Telegram Webhooks. It performs initial link validation and enqueues download tasks into a Redis list.
- <b>The Consumer</b> (`DownloaderBot.Worker`): A dedicated .NET Background Service that polls the Redis queue. It processes tasks using a structured pipeline, handles the physical download of media, and uploads the results back to Telegram.
- <b>Task Queue</b>: Redis acts as the reliable message broker between the API and the Worker, ensuring no tasks are lost even during high traffic.

## Key Technical Features

- <b>Pipeline Processing</b>: The Worker implements a Pipeline Pattern for processing downloads, consisting of discrete steps: Video Info Retrieval, Validation, Cache Checking, Downloading, Uploading, and Cleanup.
- <b>Smart Telegram Caching</b>: Implements `FileId` caching in Redis. If a user requests a previously downloaded URL, the bot re-sends the existing Telegram file instantly instead of downloading it again.
- <b>Concurrency Control</b>: Uses `SemaphoreSlim` in the Worker to manage and limit the number of simultaneous downloads, preventing server resource exhaustion.
- <b>Strict Validation</b>: Uses <b>FluentValidation</b> to enforce business rules such as maximum duration, file size limits, and allowed domains.

## Tech Stack

- <b>Language & Framework</b>: .NET 10 (C#).
- <b>Messaging & Storage</b>: Redis (via StackExchange.Redis).
- <b>Communication</b>: Telegram.Bot API.
- <b>Media Processing</b>: yt-dlp (via YoutubeDLSharp) and FFmpeg.
- <b>Patterns</b>: CQRS (MediatR in API), Producer-Consumer, Pipeline Pattern.

## Deployment & Containerization

The entire project is <b>fully containerized</b> with a clear separation between development and production environments through Docker Compose orchestration:

- <b>Development Mode</b>: By default, running `docker compose up` utilizes `docker-compose.yml` and the automatic `docker-compose.override.yml` to start the stack with exposed local ports (e.g., API on port 5189) for easy debugging.
- <b>Production Deployment</b>: Production environments are managed via dedicated shell scripts in the `/commands` folder. These scripts (e.g., `deploy.sh`) explicitly merge the base configuration with `docker-compose.prod.yml`, which adds production-grade features like:
  - <b>Nginx Proxy Manager</b> for SSL termination and reverse proxying.
  - <b>Resource Limits</b> (CPU/Memory) for services like Redis.
  - <b>Restart Policies</b> (`restart: always`) to ensure high availability.
- <b>Multi-stage Dockerfiles</b>: Optimized build process for both the API and Worker to ensure small, secure production images by separating the build environment from the final runtime.

## Code Quality & Standards

- <b>Static Analysis</b>: Enforces strict coding standards through `.editorconfig` and <b>StyleCop.Analyzers</b>.
- <b>Centralized Management</b>: Uses `Directory.Build.props` to maintain consistent dependencies and compiler settings across all projects in the solution.
- <b>Structured Logging</b>: Integrated <b>Serilog</b> for detailed, searchable logs in both console and file formats.
