# 🏛️ Arquitectura del Monolito Modular con Módulos Autónomos (Self‑Contained Modules)

Este documento define la arquitectura oficial del sistema Okomos.  
Es la **fuente de verdad** para Cursor y para todos los desarrolladores del proyecto.

Incluye:

- Monolito Modular con Módulos Autónomos  
- FastEndpoints  
- Validadores nativos (`Validator<T>` de FastEndpoints, sin FluentValidation directo)  
- Microsoft Identity  
- SQL Server (una base de datos por módulo)  
- Handlers planos inyectados en endpoints  
- Vertical Slice Architecture  
- Multitenancy  
- Outbox + Integration Events entre módulos  

---

# 1. Objetivo

Construir un **Monolito Modular** en .NET 10 donde cada módulo sea autónomo, aislado y fácilmente migrable a microservicio sin reescritura.

Cada módulo debe contener:

- Su propio **DbContext**
- Sus **migraciones**
- Su **dominio**
- Sus **features** (Vertical Slice)
- Sus **endpoints FastEndpoints**
- Sus **validadores**
- Su **infraestructura interna**
- Sus **eventos de dominio**
- Su **registro de servicios**

---

# 2. Estructura general del proyecto

```
/src
  /Okomos.Api
  /Okomos.SharedKernel
  /Okomos.Infrastructure
  /Modules
    /Identity
    /Billing
    /Accounting
    /Inventory
```

---

# 3. Estructura interna de cada módulo

```
/Modules
  /<ModuleName>
    /<ModuleName>.Api
      /Endpoints
      /Contracts
      ModuleRoutes.cs

    /<ModuleName>.Application
      /Features
      /Mappings

    /<ModuleName>.Domain
      /Entities
      /ValueObjects
      /Enums
      /Specifications
      /DomainEvents

    /<ModuleName>.Infrastructure
      /Persistence
        <ModuleName>DbContext.cs
        /Configurations
        /Migrations
      /Repositories
      /Services

    <ModuleName>.Module.cs
```

---

# 4. Reglas de diseño

## 4.1 Aislamiento
- Un módulo **no depende de otro módulo**.
- Los módulos dependen de **Okomos.SharedKernel** (abstracciones) y **Okomos.Infrastructure** (implementaciones compartidas).
- No se comparten DbContexts entre módulos.

## 4.2 Persistencia
- Cada módulo tiene su **propio DbContext** y **su propia base de datos SQL Server** (`Okomos_Identity`, `Okomos_Billing`, etc.).
- Cada módulo tiene **sus propias migraciones** en `{Module}.Infrastructure/Persistence/Migrations`.
- No existen tablas compartidas entre módulos; todas usan schema `dbo`.

## 4.3 Okomos.SharedKernel vs Okomos.Infrastructure

| Proyecto | Contenido |
|----------|-----------|
| `Okomos.SharedKernel` | Abstracciones (`Entity`, `IDomainEvent`, `IIntegrationEvent`, `ITenantProvider`, `IOutboxStore`, …), contratos de integration events, `ValidationException` |
| `Okomos.Infrastructure` | `BaseDbContext`, `OutboxStore`, `EventBus`, `DomainEventDispatcher`, `OutboxProcessorHostedService`, `TenantMiddleware`, `IUnitOfWork<TDbContext>` |

Los handlers llaman `IUnitOfWork<TDbContext>.SaveChangesAsync()` para transacción + domain events + persistencia en un solo punto.

---

# 5. FastEndpoints como capa API

Cada endpoint debe:

- Vivir en `<ModuleName>.Api/Endpoints/<FeatureName>`
- Heredar de `Endpoint<TRequest, TResponse>`
- Usar validadores nativos
- Usar handlers planos (`*Handler`) inyectados directamente en el endpoint

Ejemplo:

```csharp
public sealed class CreateInvoiceEndpoint 
    : Endpoint<CreateInvoiceRequest, CreateInvoiceResponse>
{
    private readonly CreateInvoiceHandler _handler;

    public CreateInvoiceEndpoint(CreateInvoiceHandler handler) => _handler = handler;

    public override void Configure()
    {
        Post("/invoices");
        Group<ModuleRoutes>();
    }

    public override async Task HandleAsync(CreateInvoiceRequest req, CancellationToken ct)
    {
        var result = await _handler.Handle(req, ct);
        await SendAsync(result, cancellation: ct);
    }
}
```

Rutas por módulo: `/identity/...`, `/billing/...`, `/accounting/...`, `/inventory/...` (sin prefijo `/api`).

---

# 6. Validadores nativos de FastEndpoints

Usar **solo** `Validator<TRequest>` del namespace `FastEndpoints`. No agregar el paquete `FluentValidation` ni registrar validadores manualmente en DI; FastEndpoints los descubre y ejecuta antes de `HandleAsync`.

Cada feature debe incluir un validador en `{Module}.Application/Features/<FeatureName>/`:

```
/Features/<FeatureName>/<FeatureName>Validator.cs
```

Ejemplo:

```csharp
public sealed class CreateInvoiceValidator : Validator<CreateInvoiceRequest>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CustomerName).NotEmpty();
    }
}
```

Errores de negocio en handlers usan `ValidationException` + `ExceptionHandlingMiddleware` (no el pipeline de validación de entrada).

---

# 7. Vertical Slice + Handlers

Cada feature se reparte en capas:

```
{Module}.Application/Features/<FeatureName>/
  <FeatureName>Request.cs
  <FeatureName>Handler.cs
  <FeatureName>Response.cs
  <FeatureName>Validator.cs

{Module}.Api/Endpoints/<FeatureName>/
  <FeatureName>Endpoint.cs
```

Ejemplo de handler:

```csharp
public sealed class CreateInvoiceHandler
{
    private readonly BillingDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IUnitOfWork<BillingDbContext> _unitOfWork;

    public CreateInvoiceHandler(
        BillingDbContext db,
        ITenantProvider tenant,
        IUnitOfWork<BillingDbContext> unitOfWork)
    {
        _db = db;
        _tenant = tenant;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInvoiceResponse> Handle(CreateInvoiceRequest req, CancellationToken ct)
    {
        var tenantId = _tenant.CurrentTenantId ?? throw new InvalidOperationException("Tenant is required.");
        var invoice = Invoice.Create(tenantId, req.CustomerName, req.Amount, req.Currency);
        _db.Invoices.Add(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
        return new CreateInvoiceResponse(invoice.Id);
    }
}
```

---

# 7.1 Outbox e Integration Events

- **Domain events** → handlers en `{Module}.Infrastructure/EventHandlers/` → persisten en outbox del módulo.
- **Integration events** → contratos en `Okomos.SharedKernel/IntegrationEvents/` → consumidores en `{Module}.Infrastructure/IntegrationHandlers/`.
- `OutboxProcessorHostedService` (en `Okomos.Infrastructure`) publica mensajes pendientes vía `IEventBus`.

---

# 8. Seguridad con Microsoft Identity + SQL Server

El módulo **Identity** es autónomo y contiene:

```
/Identity
  /Identity.Api
  /Identity.Application
  /Identity.Domain
  /Identity.Infrastructure
  Identity.Module.cs
```

## 8.1 DbContext

```csharp
public sealed class IdentityDbContext 
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }
}
```

## 8.2 Entidades personalizadas

```csharp
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
}

public sealed class ApplicationRole : IdentityRole<Guid> { }
```

## 8.3 Registro de Identity

```csharp
services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();
```

## 8.4 Autenticación JWT

```csharp
services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:Key"]))
        };
    });
```

---

# 9. Multitenancy integrado con Identity

El TenantId se obtiene desde:

- Claim JWT `tenant` (o `tenant_id` por compatibilidad)
- Header `X-Tenant` (o `X-Tenant-Id` deprecado)

`TenantMiddleware` resuelve el tenant y lo expone vía `ITenantProvider` (no usar `HttpContext.Items`).

---

# 10. Registro del módulo (<ModuleName>.Module.cs)

```csharp
public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BillingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("BillingConnection")));

        services.AddOutboxStore<BillingDbContext>();
        services.AddUnitOfWork<BillingDbContext>();
        services.AddScoped<CreateInvoiceHandler>();
        // Domain + integration event handlers...
        return services;
    }
}
```

En `Okomos.Api/Program.cs`:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBillingModule(builder.Configuration);

builder.Services.AddFastEndpoints(o => o.Assemblies = [
    typeof(CreateInvoiceEndpoint).Assembly,  // {Module}.Api
    typeof(CreateInvoiceHandler).Assembly,   // {Module}.Application (validadores)
    // ...
]);
```

---

# 11. Reglas para Cursor

Cursor debe:

1. Crear módulos siguiendo esta estructura.  
2. Crear endpoints usando FastEndpoints.  
3. Crear validadores usando `Validator<TRequest>`.  
4. Crear handlers dentro de `/Application/Features/<FeatureName>`.  
5. Registrar Identity con SQL Server.  
6. Generar JWT usando `IJwtTokenService`.  
7. Incluir TenantId en ApplicationUser.  
8. Mantener aislamiento entre módulos.  
9. Crear migraciones dentro de `<ModuleName>.Infrastructure/Persistence/Migrations`.  

---

# 12. Convenciones de nombres

- Endpoints: `<FeatureName>Endpoint.cs`
- Validadores: `<FeatureName>Validator.cs`
- Handlers: `<FeatureName>Handler.cs`
- Requests: `<FeatureName>Request.cs`
- Responses: `<FeatureName>Response.cs`
- Identity DbContext: `IdentityDbContext.cs`

---

# 13. Licencia interna

Este documento es la **fuente de verdad** para la arquitectura del monolito modular.  
Todo nuevo código debe alinearse con estas reglas.
