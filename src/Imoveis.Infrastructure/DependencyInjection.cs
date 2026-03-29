using Imoveis.Domain.Interfaces;
using Imoveis.Infrastructure.Data;
using Imoveis.Infrastructure.HealthChecks;
using Imoveis.Infrastructure.Integrations.ViaCep;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Imoveis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? connectionString = null)
    {
        // ─── Banco de dados ───────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(connectionString))
            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("ImoveisDb"));
        else
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

        // ─── Options ──────────────────────────────────────────────────────────
        services.Configure<ViaCepOptions>(
            configuration.GetSection(ViaCepOptions.SectionName));

        // ─── HTTP Client: ViaCEP ──────────────────────────────────────────────
        // Registrado como ICepService — Application não depende de ViaCepClient diretamente.
        // AddStandardResilienceHandler configura retry com backoff exponencial,
        // circuit breaker e timeout por tentativa (Polly v8 via Microsoft.Extensions.Http.Resilience).
        var viaCepOptions = configuration
            .GetSection(ViaCepOptions.SectionName)
            .Get<ViaCepOptions>() ?? new ViaCepOptions();

        services.AddHttpClient<ICepService, ViaCepClient>(client =>
        {
            client.BaseAddress = new Uri(viaCepOptions.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler(opts =>
        {
            opts.Retry.MaxRetryAttempts = viaCepOptions.MaxTentativas;
            opts.Retry.UseJitter = true; // evita thundering herd em falhas simultâneas
            opts.TotalRequestTimeout.Timeout =
                TimeSpan.FromSeconds(viaCepOptions.TimeoutSegundos * (viaCepOptions.MaxTentativas + 1));
        });

        // ─── Health Checks ────────────────────────────────────────────────────
        services.AddScoped<ViaCepHealthCheck>();

        return services;
    }
}
