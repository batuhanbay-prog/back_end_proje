FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["src/Shared/Shared.Common/Shared.Common.csproj", "Shared/Shared.Common/"]
RUN dotnet restore "ApiGateway/ApiGateway.csproj"

COPY src/ .
RUN dotnet publish "ApiGateway/ApiGateway.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
