# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ResumeBuilder.Web/ResumeBuilder.Web.csproj", "ResumeBuilder.Web/"]
COPY ["ResumeBuilder.Core/ResumeBuilder.Core.csproj", "ResumeBuilder.Core/"]
COPY ["ResumeBuilder.Infrastructure/ResumeBuilder.Infrastructure.csproj", "ResumeBuilder.Infrastructure/"]
RUN dotnet restore "ResumeBuilder.Web/ResumeBuilder.Web.csproj"

# Copy the rest of the source code and build it
COPY . .
WORKDIR "/src/ResumeBuilder.Web"
RUN dotnet build "ResumeBuilder.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ResumeBuilder.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install Chromium and dependencies for Puppeteer (PDF generator)
RUN apt-get update && apt-get install -y \
    chromium \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    libpangocairo-1.0-0 \
    && rm -rf /var/lib/apt/lists/*

# Set Environment Variables for Puppeteer in Docker
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium
ENV ASPNETCORE_URLS=http://+:80

# Copy published files from publish stage
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ResumeBuilder.Web.dll"]
