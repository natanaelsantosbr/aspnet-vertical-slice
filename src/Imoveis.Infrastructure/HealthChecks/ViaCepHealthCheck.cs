using Imoveis.Domain.Interfaces;
using Imoveis.Domain.ValueObjects;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Imoveis.Infrastructure.HealthChecks;

public class ViaCepHealthCheck : IHealthCheck
{
    private readonly ICepService _cepService;

    public ViaCepHealthCheck(ICepService cepService)
    {
        _cepService = cepService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // CEP da Av. Paulista usado como probe — endereço estável e bem conhecido
        var resultado = await _cepService.ConsultarAsync("01310100", cancellationToken);

        return resultado switch
        {
            ConsultarCepResultado.Encontrado => HealthCheckResult.Healthy("ViaCEP respondendo normalmente"),
            ConsultarCepResultado.NaoEncontrado => HealthCheckResult.Degraded("ViaCEP respondeu mas o CEP de teste não foi encontrado"),
            ConsultarCepResultado.ServicoIndisponivel ind => HealthCheckResult.Unhealthy($"ViaCEP indisponível: {ind.Detalhe}"),
            _ => HealthCheckResult.Unhealthy("Resultado inesperado do ViaCEP")
        };
    }
}
