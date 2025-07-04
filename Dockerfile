# Dockerfile for TaskTracking.Blazor (Blazor Server Application)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src

# Copy solution and config files
COPY ["TaskTracking.sln", "."]
COPY ["common.props", "."]
COPY ["NuGet.Config", "."]

# Copy all project files for dependency resolution
COPY ["src/TaskTracking.Blazor/TaskTracking.Blazor.csproj", "src/TaskTracking.Blazor/"]
COPY ["src/TaskTracking.Blazor.Client/TaskTracking.Blazor.Client.csproj", "src/TaskTracking.Blazor.Client/"]
COPY ["src/TaskTracking.Application/TaskTracking.Application.csproj", "src/TaskTracking.Application/"]
COPY ["src/TaskTracking.Application.Contracts/TaskTracking.Application.Contracts.csproj", "src/TaskTracking.Application.Contracts/"]
COPY ["src/TaskTracking.Domain/TaskTracking.Domain.csproj", "src/TaskTracking.Domain/"]
COPY ["src/TaskTracking.Domain.Shared/TaskTracking.Domain.Shared.csproj", "src/TaskTracking.Domain.Shared/"]
COPY ["src/TaskTracking.EntityFrameworkCore/TaskTracking.EntityFrameworkCore.csproj", "src/TaskTracking.EntityFrameworkCore/"]
COPY ["src/TaskTracking.HttpApi/TaskTracking.HttpApi.csproj", "src/TaskTracking.HttpApi/"]
COPY ["src/TaskTracking.HttpApi.Client/TaskTracking.HttpApi.Client.csproj", "src/TaskTracking.HttpApi.Client/"]

# Restore dependencies
RUN dotnet restore "src/TaskTracking.Blazor/TaskTracking.Blazor.csproj"

# Copy full source
COPY . .

# Build the application
WORKDIR "/src/src/TaskTracking.Blazor"
RUN dotnet build "TaskTracking.Blazor.csproj" --configuration Release -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskTracking.Blazor.csproj" --configuration Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TaskTracking.Blazor.dll"]