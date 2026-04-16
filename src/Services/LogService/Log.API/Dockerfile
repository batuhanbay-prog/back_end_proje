# ==========================================
# ADIM 10.5 — Multi-Stage Build (12-Factor)
# Stage 1: Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/LogService/Log.API/Log.API.csproj", "Services/LogService/Log.API/"]
COPY ["src/Services/LogService/Log.Application/Log.Application.csproj", "Services/LogService/Log.Application/"]
COPY ["src/Services/LogService/Log.Domain/Log.Domain.csproj", "Services/LogService/Log.Domain/"]
COPY ["src/Services/LogService/Log.Infrastructure/Log.Infrastructure.csproj", "Services/LogService/Log.Infrastructure/"]
COPY ["src/Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
RUN dotnet restore "Services/LogService/Log.API/Log.API.csproj"

COPY src/ .
RUN dotnet publish "Services/LogService/Log.API/Log.API.csproj" -c Release -o /app/publish --no-restore

# ==========================================
# Stage 2: Runtime
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5003

EXPOSE 5003

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Log.API.dll"]
