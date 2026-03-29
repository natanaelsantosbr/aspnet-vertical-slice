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

var builder = WebApplication.CreateBuilder(args);

// ─── Cache ───────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ListaCacheInvalidador>();

// ─── Infrastructure ──────────────────────────────────────────────────────────
// Passa null para usar InMemory; ou passe a connection string para SQL Server
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection"));

// ─── Validators (FluentValidation) ───────────────────────────────────────────
builder.Services.AddScoped<IValidator<CadastrarImovelCommand>, CadastrarImovelValidator>();
builder.Services.AddScoped<IValidator<AtualizarImovelCommand>, AtualizarImovelValidator>();
builder.Services.AddScoped<IValidator<RegistrarLeadCommand>, RegistrarLeadValidator>();

// ─── Handlers (um por caso de uso) ───────────────────────────────────────────
builder.Services.AddScoped<CadastrarImovelHandler>();
builder.Services.AddScoped<ConsultarImoveisHandler>();
builder.Services.AddScoped<ObterImovelPorIdHandler>();
builder.Services.AddScoped<AtualizarImovelHandler>();
builder.Services.AddScoped<RemoverImovelHandler>();
builder.Services.AddScoped<RegistrarLeadHandler>();

// ─── API ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Imoveis API", Version = "v1" });
});

var app = builder.Build();

// ─── Seed de dados para demonstração ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Populate(db);
}

// ─── Middleware ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
