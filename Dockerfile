# Placeholder Dockerfile for ASHATAIServer
# This will be completed in Phase 4 (Packaging & Deployment)

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7077

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ASHATAIServer/ASHATAIServer.csproj", "ASHATAIServer/"]
RUN dotnet restore "ASHATAIServer/ASHATAIServer.csproj"
COPY . .
WORKDIR "/src/ASHATAIServer"
RUN dotnet build "ASHATAIServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ASHATAIServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for models (can be mounted as volume)
RUN mkdir -p /app/models

ENTRYPOINT ["dotnet", "ASHATAIServer.dll"]
