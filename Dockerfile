# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# copier et compiler
COPY TaskOrchestrator.sln .
COPY TaskOrchestrator.Domain/TaskOrchestrator.Domain.csproj TaskOrchestrator.Domain/
COPY TaskOrchestrator.Application/TaskOrchestrator.Application.csproj TaskOrchestrator.Application/
COPY TaskOrchestrator.Infrastructure/TaskOrchestrator.Infrastructure.csproj TaskOrchestrator.Infrastructure/
COPY TaskOrchestrator.Api/TaskOrchestrator.Api.csproj TaskOrchestrator.Api/
COPY TaskOrchestrator.Domain.Tests/TaskOrchestrator.Domain.Tests.csproj TaskOrchestrator.Domain.Tests/
COPY TaskOrchestrator.Application.Tests/TaskOrchestrator.Application.Tests.csproj TaskOrchestrator.Application.Tests/

RUN dotnet restore

# Copier tout le code et compiler
COPY . .
RUN dotnet publish TaskOrchestrator.Api/TaskOrchestrator.Api.csproj -c Release -o /app/publish

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# seulement le code compilé — image plus légère
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskOrchestrator.Api.dll"]