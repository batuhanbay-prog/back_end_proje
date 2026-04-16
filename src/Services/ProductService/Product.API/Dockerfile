# ==========================================
# ADIM 10.5 — Multi-Stage Build (12-Factor)
# Stage 1: Build
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/ProductService/Product.API/Product.API.csproj", "Services/ProductService/Product.API/"]
COPY ["src/Services/ProductService/Product.Application/Product.Application.csproj", "Services/ProductService/Product.Application/"]
COPY ["src/Services/ProductService/Product.Domain/Product.Domain.csproj", "Services/ProductService/Product.Domain/"]
COPY ["src/Services/ProductService/Product.Infrastructure/Product.Infrastructure.csproj", "Services/ProductService/Product.Infrastructure/"]
COPY ["src/Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
RUN dotnet restore "Services/ProductService/Product.API/Product.API.csproj"

COPY src/ .
RUN dotnet publish "Services/ProductService/Product.API/Product.API.csproj" -c Release -o /app/publish --no-restore

# ==========================================
# Stage 2: Runtime
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5002

EXPOSE 5002

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Product.API.dll"]
