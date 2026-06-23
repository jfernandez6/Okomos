using Accounting.Api.Endpoints.CreateJournalEntry;
using Accounting.Application.Features.CreateJournalEntry;
using Accounting.Module;
using Billing.Api.Endpoints.CreateInvoice;
using Billing.Application.Features.CreateInvoice;
using Billing.Module;
using Identity.Api.Endpoints;
using Identity.Application.Features.Register;
using Identity.Module;
using Inventory.Api.Endpoints.CreateProduct;
using Inventory.Application.Features.CreateProduct;
using Inventory.Module;
using Okomos.Api.Endpoints;
using Okomos.Api.Middleware;
using Okomos.Infrastructure;
using Okomos.Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddBillingModule(builder.Configuration);
builder.Services.AddAccountingModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);

builder.Services.AddFastEndpoints(o =>
{
    o.Assemblies =
    [
        typeof(HealthEndpoint).Assembly,
        typeof(RegisterEndpoint).Assembly,
        typeof(RegisterHandler).Assembly,
        typeof(CreateInvoiceEndpoint).Assembly,
        typeof(CreateInvoiceHandler).Assembly,
        typeof(CreateJournalEntryEndpoint).Assembly,
        typeof(CreateJournalEntryHandler).Assembly,
        typeof(CreateProductEndpoint).Assembly,
        typeof(CreateProductHandler).Assembly
    ];
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("OkomosCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Okomos API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
else
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Okomos API");
    });
}

app.UseHttpsRedirection();
app.UseCors("OkomosCors");
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.UseFastEndpoints();

app.Run();
