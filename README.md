# Okomos - Monolito Modular

Solución .NET 10 con arquitectura de monolito modular y Vertical Slice Architecture, compatible con Azure App Service.

## Estructura

```
src/
├── Okomos.Api/              # API principal (host)
├── Okomos.SharedKernel/     # CQRS, Decorators, Events, Outbox, Multitenancy
├── Okomos.Identity/         # Autenticación JWT, usuarios, roles
├── Okomos.Billing/          # Facturación
├── Okomos.Accounting/       # Contabilidad
└── Okomos.Inventory/        # Inventario
```

## Arquitectura

- **Vertical Slices**: cada feature agrupa Command/Query, Handler, Validator y endpoints.
- **CQRS manual**: sin MediatR, handlers registrados con pipeline de decorators.
- **Pipeline Decorator**: Validación → Logging → Multitenancy → Transacciones → Domain Events.
- **Domain Events**: dispatch automático al final de `SaveChanges`.
- **Integration Events**: Outbox Pattern + `OutboxProcessorHostedService`.
- **Outbox por módulo**: cada módulo registra `IOutboxStore<TDbContext>` (ej. `IOutboxStore<BillingDbContext>`).

## Migraciones (ejecutar manualmente)

Las migraciones NO se ejecutan en runtime. Ejecutar externamente:

```bash
# Identity
dotnet ef migrations add InitialIdentity --project src/Okomos.Identity --startup-project src/Okomos.Api --context IdentityDbContext --output-dir Persistence/Migrations

# Billing
dotnet ef migrations add InitialBilling --project src/Okomos.Billing --startup-project src/Okomos.Api --context BillingDbContext --output-dir Persistence/Migrations

# Accounting
dotnet ef migrations add InitialAccounting --project src/Okomos.Accounting --startup-project src/Okomos.Api --context AccountingDbContext --output-dir Persistence/Migrations

# Inventory
dotnet ef migrations add InitialInventory --project src/Okomos.Inventory --startup-project src/Okomos.Api --context InventoryDbContext --output-dir Persistence/Migrations

# Aplicar migraciones
dotnet ef database update --project src/Okomos.Identity --startup-project src/Okomos.Api --context IdentityDbContext
dotnet ef database update --project src/Okomos.Billing --startup-project src/Okomos.Api --context BillingDbContext
dotnet ef database update --project src/Okomos.Accounting --startup-project src/Okomos.Api --context AccountingDbContext
dotnet ef database update --project src/Okomos.Inventory --startup-project src/Okomos.Api --context InventoryDbContext
```

## Azure App Service

Configurar en Application Settings:

- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__IdentityConnection` (opcional, usa DefaultConnection si vacío)
- `Jwt__Key`
- `ApplicationInsights__ConnectionString`
- `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.

## Multitenancy

Enviar header `X-Tenant-Id` (GUID) en cada request. Opcionalmente `X-Tenant-Slug` para connection string por tenant (`ConnectionStrings__Tenant_{slug}`).

## Endpoints

- `POST /api/identity/register` - Registro
- `POST /api/identity/login` - Login JWT
- `POST /api/identity/refresh` - Refresh token
- `POST /api/billing/invoices` - Crear factura
- `GET /api/billing/invoices/{id}` - Obtener factura
- `POST /api/accounting/journal-entries` - Crear asiento
- `GET /api/accounting/journal-entries/{id}` - Obtener asiento
- `POST /api/inventory/products` - Crear producto
- `GET /api/inventory/products/{id}` - Obtener producto
- `GET /health` - Health check

## Documentación API

- OpenAPI: `/openapi/v1.json`
- Scalar UI: `/scalar/v1` (en Development y Production)

## Ejecutar localmente

```bash
dotnet run --project src/Okomos.Api
```

## Pruebas

```bash
# Todas las pruebas
dotnet test Okomos.slnx

# Solo unitarias (sin Docker)
dotnet test tests/Okomos.SharedKernel.Tests
dotnet test tests/Okomos.Billing.Tests
```

### Estructura de tests

```
tests/
├── Directory.Build.props      # xUnit + FluentAssertions + NSubstitute + Testcontainers
├── Shared/DockerHelper.cs     # [DockerFact] para integración con SQL Server
├── Okomos.SharedKernel.Tests/ # Decorators, EventBus, Outbox, Domain Events
├── Okomos.Identity.Tests/
├── Okomos.Billing.Tests/
├── Okomos.Accounting.Tests/
└── Okomos.Inventory.Tests/
```

Cada proyecto de pruebas referencia únicamente su módulo correspondiente.

Las pruebas de integración con **Testcontainers** requieren Docker Desktop en ejecución. Sin Docker, se omiten automáticamente (`[DockerFact]`).
