# Okomos - Monolito Modular

Solución .NET 10 con arquitectura de monolito modular, módulos autónomos y Vertical Slice Architecture.

## Estructura

```
src/
├── Okomos.Api/                 # Host API
├── Okomos.SharedKernel/        # Abstracciones, integration event contracts
├── Okomos.Infrastructure/        # BaseDbContext, Outbox, EventBus, Multitenancy, IUnitOfWork
└── Modules/
    ├── Identity/               # Identity.{Api,Application,Domain,Infrastructure,Module}
    ├── Billing/
    ├── Accounting/
    └── Inventory/

tests/
├── Modules/                    # Tests por módulo
│   ├── Identity.Tests/
│   ├── Billing.Tests/
│   ├── Accounting.Tests/
│   └── Inventory.Tests/
└── Okomos.Infrastructure.Tests/
```

## Arquitectura

- **Módulos autónomos**: cada módulo tiene su DbContext, migraciones, dominio, features y endpoints.
- **Vertical Slices**: Request, Handler, Response, Validator en Application; Endpoint en Api.
- **FastEndpoints**: validación nativa con `Validator<T>` (sin paquete FluentValidation directo).
- **Handlers planos**: inyectados en endpoints; sin pipeline de decorators CQRS.
- **IUnitOfWork**: transacción + domain events + `SaveChanges` centralizados en Infrastructure.
- **Bases de datos separadas**: `Okomos_Identity`, `Okomos_Billing`, `Okomos_Accounting`, `Okomos_Inventory`.
- **Outbox + Integration Events**: comunicación entre módulos sin referencias cruzadas.

Ver [architecture.md](architecture.md) para la guía completa.

## Connection strings

Configurar en `appsettings.Development.json` (o Azure Application Settings):

```json
"ConnectionStrings": {
  "IdentityConnection": "Server=...;Database=Okomos_Identity;...",
  "BillingConnection": "Server=...;Database=Okomos_Billing;...",
  "AccountingConnection": "Server=...;Database=Okomos_Accounting;...",
  "InventoryConnection": "Server=...;Database=Okomos_Inventory;..."
}
```

## Migraciones (ejecutar manualmente)

Las migraciones NO se ejecutan en runtime. Cada módulo usa su proyecto Infrastructure como startup:

```bash
# Identity
dotnet ef migrations add Initial \
  --project src/Modules/Identity/Identity.Infrastructure \
  --startup-project src/Modules/Identity/Identity.Infrastructure \
  --context IdentityDbContext \
  --output-dir Persistence/Migrations

# Billing
dotnet ef migrations add Initial \
  --project src/Modules/Billing/Billing.Infrastructure \
  --startup-project src/Modules/Billing/Billing.Infrastructure \
  --context BillingDbContext \
  --output-dir Persistence/Migrations

# Accounting
dotnet ef migrations add Initial \
  --project src/Modules/Accounting/Accounting.Infrastructure \
  --startup-project src/Modules/Accounting/Accounting.Infrastructure \
  --context AccountingDbContext \
  --output-dir Persistence/Migrations

# Inventory
dotnet ef migrations add Initial \
  --project src/Modules/Inventory/Inventory.Infrastructure \
  --startup-project src/Modules/Inventory/Inventory.Infrastructure \
  --context InventoryDbContext \
  --output-dir Persistence/Migrations

# Aplicar migraciones (usar Okomos.Api como startup para connection strings reales)
# Los IDesignTimeDbContextFactory leen appsettings desde Okomos.Api (Development por defecto).
$env:ASPNETCORE_ENVIRONMENT = "Development"   # PowerShell
# export ASPNETCORE_ENVIRONMENT=Development   # bash

dotnet ef database update --project src/Modules/Identity/Identity.Infrastructure --startup-project src/Okomos.Api --context IdentityDbContext
dotnet ef database update --project src/Modules/Billing/Billing.Infrastructure --startup-project src/Okomos.Api --context BillingDbContext
dotnet ef database update --project src/Modules/Accounting/Accounting.Infrastructure --startup-project src/Okomos.Api --context AccountingDbContext
dotnet ef database update --project src/Modules/Inventory/Inventory.Infrastructure --startup-project src/Okomos.Api --context InventoryDbContext
```

## Azure App Service

- `ConnectionStrings__IdentityConnection`, `ConnectionStrings__BillingConnection`, etc.
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`
- `ApplicationInsights__ConnectionString`
- `Outbox__PollingIntervalSeconds`, `Outbox__BatchSize`
- `Cors__AllowedOrigins__0`, etc.

## Multitenancy

Enviar header `X-Tenant` (GUID) en cada request. También acepta `X-Tenant-Id` (deprecado). El claim JWT `tenant` se usa cuando el usuario está autenticado.

## Endpoints

- `POST /identity/register` — Registro
- `POST /identity/login` — Login JWT
- `POST /identity/refresh` — Refresh token
- `POST /billing/invoices` — Crear factura
- `GET /billing/invoices/{id}` — Obtener factura
- `POST /accounting/journal-entries` — Crear asiento
- `GET /accounting/journal-entries/{id}` — Obtener asiento
- `POST /inventory/products` — Crear producto
- `GET /inventory/products/{id}` — Obtener producto
- `GET /health` — Health check

## Documentación API

- OpenAPI: `/openapi/v1.json`
- Scalar UI: `/scalar/v1`

## Ejecutar localmente

```bash
dotnet run --project src/Okomos.Api
```

## Pruebas

```bash
# Todas las pruebas
dotnet test Okomos.slnx

# Solo unitarias (sin Docker)
dotnet test tests/Modules/Billing.Tests
dotnet test tests/Okomos.Infrastructure.Tests
```

Las pruebas de integración con **Testcontainers** requieren Docker Desktop. Sin Docker, se omiten automáticamente (`[DockerFact]`).
