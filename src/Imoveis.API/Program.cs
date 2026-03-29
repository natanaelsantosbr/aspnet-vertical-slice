using FluentValidation;
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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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

// ─── API ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Imoveis API", Version = "v1" }));

var app = builder.Build();

// ─── Seed ─────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Populate(db);
}

// ─── Middleware ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ─── Health Check endpoints ───────────────────────────────────────────────────
// /health      → todos os checks
// /health/cep  → apenas ViaCEP (útil para monitoramento de integrações externas)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = EscreverRespostaHealth
});

app.MapHealthChecks("/health/cep", new HealthCheckOptions
{
    Predicate = c => c.Tags.Contains("external"),
    ResponseWriter = EscreverRespostaHealth
});

app.Run();

// ─── Helper local ─────────────────────────────────────────────────────────────
static Task EscreverRespostaHealth(HttpContext ctx, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    ctx.Response.ContentType = "application/json";
    var resultado = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name    = e.Key,
            status  = e.Value.Status.ToString(),
            descricao = e.Value.Description,
            duracao_ms = (int)e.Value.Duration.TotalMilliseconds
        })
    });
    return ctx.Response.WriteAsync(resultado);
}
