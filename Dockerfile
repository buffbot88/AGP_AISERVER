# Production Dockerfile for ASHATAIServer
# Multi-stage build for optimal image size and security

# Stage 1: Build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ASHATAIServer/ASHATAIServer.csproj", "ASHATAIServer/"]
RUN dotnet restore "ASHATAIServer/ASHATAIServer.csproj"

# Copy source code
COPY ["ASHATAIServer/", "ASHATAIServer/"]

# Build the application
WORKDIR "/src/ASHATAIServer"
RUN dotnet build "ASHATAIServer.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ASHATAIServer.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 3: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create non-root user for security
RUN useradd -m -u 1000 ashatuser && \
    mkdir -p /app/models /app/data && \
    chown -R ashatuser:ashatuser /app

# Copy published application
COPY --from=publish --chown=ashatuser:ashatuser /app/publish .

# Switch to non-root user
USER ashatuser

# Expose HTTP port
EXPOSE 7077

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:7077/api/ai/health || exit 1

# Environment variables with defaults
ENV ASPNETCORE_URLS=http://+:7077 \
    ASPNETCORE_ENVIRONMENT=Production \
    ModelsDirectory=/app/models \
    Database__Path=/app/data/users.db

# Volume for persistent data
VOLUME ["/app/models", "/app/data"]

ENTRYPOINT ["dotnet", "ASHATAIServer.dll"]
