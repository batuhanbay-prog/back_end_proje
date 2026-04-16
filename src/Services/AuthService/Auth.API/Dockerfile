# ==========================================
# ADIM 10.5 — Multi-Stage Build (12-Factor)
# Stage 1: Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarini kopyala ve restore et
COPY ["src/Services/AuthService/Auth.API/Auth.API.csproj", "Services/AuthService/Auth.API/"]
COPY ["src/Services/AuthService/Auth.Application/Auth.Application.csproj", "Services/AuthService/Auth.Application/"]
COPY ["src/Services/AuthService/Auth.Domain/Auth.Domain.csproj", "Services/AuthService/Auth.Domain/"]
COPY ["src/Services/AuthService/Auth.Infrastructure/Auth.Infrastructure.csproj", "Services/AuthService/Auth.Infrastructure/"]
COPY ["src/Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
RUN dotnet restore "Services/AuthService/Auth.API/Auth.API.csproj"

# Tum kaynak kodunu kopyala ve publish et
COPY src/ .
RUN dotnet publish "Services/AuthService/Auth.API/Auth.API.csproj" -c Release -o /app/publish --no-restore

# ==========================================
# Stage 2: Runtime (kucuk imaj)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Ortam degiskenleri (12-Factor: Config)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

EXPOSE 5001

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Auth.API.dll"]
