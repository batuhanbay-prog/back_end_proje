# Microservice Project

.NET 8 tabanlı, mikroservis mimarisiyle geliştirilmiş bir backend projesi.

## Mimari

```
MicroserviceProject/
├── src/
│   ├── ApiGateway/              # YARP Reverse Proxy + JWT doğrulama + Rate Limiting
│   ├── Services/
│   │   ├── AuthService/         # Kimlik doğrulama (ASP.NET Core Identity + JWT)
│   │   ├── ProductService/      # Ürün yönetimi (CQRS + MediatR + Redis Cache)
│   │   └── LogService/          # Merkezi loglama (Serilog + RabbitMQ Consumer)
│   └── Shared/
│       └── Shared.Common/       # Ortak modeller (Events, ApiResponse, BaseEntity)
└── tests/
    ├── Auth.UnitTests/
    ├── Product.UnitTests/
    └── Log.UnitTests/
```

## Kullanılan Tasarım Desenleri

| Desen | Kullanım |
|---|---|
| **Onion Architecture** | Her servis Domain → Application → Infrastructure → API |
| **CQRS + MediatR** | ProductService okuma/yazma ayrımı |
| **Repository Pattern** | Tüm servisler `IRepository<T>` |
| **Cache-Aside Pattern** | Redis ile ProductService |
| **Event-Driven** | RabbitMQ + MassTransit (ProductCreated/Updated → Log) |
| **API Gateway** | YARP ile tek giriş noktası |
| **Dependency Inversion** | Interface-based DI |

## Teknolojiler

- **.NET 8** — Ana framework
- **ASP.NET Core Identity** — Kullanıcı yönetimi
- **Entity Framework Core 8** — ORM + Code First Migrations
- **SQL Server 2022** — Veritabanı (AuthDb, ProductDb, LogDb)
- **Redis** — Cache
- **RabbitMQ + MassTransit** — Message broker
- **Serilog + Seq** — Structured logging
- **YARP** — API Gateway / Reverse Proxy
- **JWT Bearer** — Kimlik doğrulama
- **Docker + Docker Compose** — Containerization
- **GitHub Actions** — CI/CD
- **xUnit + Moq + FluentAssertions** — Unit testler

## Kurulum ve Çalıştırma

### Gereksinimler

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (yerel geliştirme için)

### Docker ile Çalıştırma (Önerilen)

```bash
# Projeyi klonla
git clone https://github.com/batuhanbay-prog/back_end_proje.git
cd MicroserviceProject

# Tüm servisleri başlat
docker compose up -d

# Logları izle
docker compose logs -f
```

Servisler hazır olduğunda:

| Servis | URL |
|---|---|
| API Gateway | http://localhost:5000 |
| Auth API | http://localhost:5001 |
| Product API | http://localhost:5002 |
| Log API | http://localhost:5003 |
| RabbitMQ Yönetim | http://localhost:15672 (guest/guest) |

### Yerel Geliştirme

```bash
# Bağımlılıkları başlat (SQL Server, Redis, RabbitMQ)
docker compose up -d sqlserver redis rabbitmq

# Her servis için ayrı terminal:
cd src/Services/AuthService/Auth.API
dotnet run

cd src/Services/ProductService/Product.API
dotnet run

cd src/Services/LogService/Log.API
dotnet run

cd src/ApiGateway
dotnet run
```

## API Kullanımı

### 1. Kayıt Ol

```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "Test123!"
}
```

### 2. Giriş Yap

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "admin@admin.com",
  "password": "Admin123!"
}
```

Yanıt içindeki `accessToken` değerini sonraki isteklerde kullan.

### 3. Ürün Listele (JWT gerekli)

```http
GET http://localhost:5000/api/product
Authorization: Bearer {accessToken}
```

## Swagger API Dökümantasyonu

Her servis kendi Swagger UI'ına sahiptir (Development modunda):

- Auth API: http://localhost:5001/swagger
- Product API: http://localhost:5002/swagger
- Log API: http://localhost:5003/swagger

## Unit Testler

```bash
# Tüm testleri çalıştır
dotnet test MicroserviceProject.sln --verbosity normal
```

| Test Projesi | Test Sayısı |
|---|---|
| Auth.UnitTests | 4 |
| Product.UnitTests | 6 |
| Log.UnitTests | 2 |
| **Toplam** | **12** |

## CI/CD

GitHub Actions pipeline (`.github/workflows/ci-cd.yml`):

1. **Build** — `dotnet build`
2. **Test** — `dotnet test` (12 unit test)
3. **Docker Build & Push** — 4 image (Docker Hub secrets gerekli)

`test/v1.0.0` veya `prod/v1.0.0` branch'e push edildiğinde otomatik tetiklenir.

## Sağlık Kontrolü

```bash
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # Product
curl http://localhost:5003/health  # Log
```

## Dağıtım

```bash
# Production branch'e merge
git checkout -b prod/v1.0.0
git merge test/v1.0.0
git push origin prod/v1.0.0
```

## Kod Deposu

https://github.com/batuhanbay-prog/back_end_proje
