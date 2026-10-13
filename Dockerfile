# =============================================================================
# Multi-Stage Dockerfile for Escandallo SaaS (.NET 10.0 Alpine Non-Root)
# Architecture Reference: ADR-001 (Modular Monolith) & 07-deployment-view.md
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY Escandallo.sln ./
COPY src/Escandallo.Core/Escandallo.Core.csproj src/Escandallo.Core/
COPY src/Escandallo.Web/Escandallo.Web.csproj src/Escandallo.Web/
COPY tests/Escandallo.Tests/Escandallo.Tests.csproj tests/Escandallo.Tests/
RUN dotnet restore Escandallo.sln

COPY src/ src/
COPY tests/ tests/
RUN dotnet test tests/Escandallo.Tests/Escandallo.Tests.csproj -c Release --no-restore
RUN dotnet publish src/Escandallo.Web/Escandallo.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
COPY --from=build /app/publish ./
RUN chown -R appuser:appgroup /app

USER appuser
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Escandallo.Web.dll"]
