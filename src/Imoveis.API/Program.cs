using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using FluentValidation;
using Imoveis.API.Auth;
using Imoveis.API.Middleware;
using Imoveis.Application.Common;
using Imoveis.Application.Features.Imoveis.AtualizarImovel;
using Imoveis.Application.Features.Imoveis.CadastrarImovel;
using Imoveis.Application.Features.Imoveis.ConsultarImoveis;
using Imoveis.Application.Features.Imoveis.ObterImovelPorId;
using Imoveis.Application.Features.Imoveis.RemoverImovel;
using Imoveis.Application.Features.Leads.RegistrarLead;
using Imoveis.Infrastructure;
using Imoveis.Infrastructure.Data;
using Imoveis.Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Exception handling ───────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ─── Autenticação JWT ─────────────────────────────────────────────────────────
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtOptions.Issuer,
            ValidAudience            = jwtOptions.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.Zero   // sem tolerância de clock — expira exatamente no prazo
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();

// ─── Rate Limiting (nativo .NET 8) ───────────────────────────────────────────
// Particiona por IP de origem. Em produção com proxy reverso: use X-Forwarded-For.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Leitura pública: 60 req/min por IP
    options.AddPolicy("leitura", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window      = TimeSpan.FromMinutes(1),
                PermitLimit = 60,
                QueueLimit  = 0
            }));

    // Escrita autenticada: 20 req/min por IP
    options.AddPolicy("escrita", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window      = TimeSpan.FromMinutes(1),
                PermitLimit = 20,
                QueueLimit  = 0
            }));

    // Login: 5 tentativas/min por IP — proteção contra brute force
    options.AddPolicy("autenticacao", ctx =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                Window             = TimeSpan.FromMinutes(1),
                SegmentsPerWindow  = 4,
                PermitLimit        = 5,
                QueueLimit         = 0
            }));
});

// ─── Cache ────────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ListaCacheInvalidador>();

// ─── Infrastructure (DB + HTTP Client ViaCEP + HealthChecks) ─────────────────
builder.Services.AddInfrastructure(
    builder.Configuration,
    builder.Configuration.GetConnectionString("DefaultConnection"));

// ─── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck<ViaCepHealthCheck>("viacep", tags: ["external"]);

// ─── Validators ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<IValidator<CadastrarImovelCommand>, CadastrarImovelValidator>();
builder.Services.AddScoped<IValidator<AtualizarImovelCommand>, AtualizarImovelValidator>();
builder.Services.AddScoped<IValidator<RegistrarLeadCommand>, RegistrarLeadValidator>();

// ─── Handlers ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<CadastrarImovelHandler>();
builder.Services.AddScoped<ConsultarImoveisHandler>();
builder.Services.AddScoped<ObterImovelPorIdHandler>();
builder.Services.AddScoped<AtualizarImovelHandler>();
builder.Services.AddScoped<RemoverImovelHandler>();
builder.Services.AddScoped<RegistrarLeadHandler>();

// ─── API + Swagger com suporte a Bearer ──────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Imoveis API", Version = "v1" });

    // Define o esquema Bearer no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. Informe: Bearer {seu_token}",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT"
    });

    // Aplica o requisito globalmente — o cadeado aparece em todos os endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Seed ─────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Populate(db);
}

// ─── Middleware pipeline (ordem importa) ──────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();      // 1. captura exceções antes de tudo
app.UseHttpsRedirection();      // 2. redireciona para HTTPS
app.UseRateLimiter();           // 3. rate limit antes de processar a requisição
app.UseAuthentication();        // 4. valida o token JWT
app.UseAuthorization();         // 5. verifica se o usuário pode acessar o recurso
app.MapControllers();

// ─── Health Check endpoints ───────────────────────────────────────────────────
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = EscreverRespostaHealth
});

app.MapHealthChecks("/health/cep", new HealthCheckOptions
{
    Predicate      = c => c.Tags.Contains("external"),
    ResponseWriter = EscreverRespostaHealth
});

app.Run();

// ─── Helper local ─────────────────────────────────────────────────────────────
static Task EscreverRespostaHealth(
    HttpContext ctx,
    Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var resultado = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name       = e.Key,
            status     = e.Value.Status.ToString(),
            descricao  = e.Value.Description,
            duracao_ms = (int)e.Value.Duration.TotalMilliseconds
        })
    });
    return ctx.Response.WriteAsync(resultado);
}
