using System.Net.Http.Json;
using Imoveis.Domain.Interfaces;
using Imoveis.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Imoveis.Infrastructure.Integrations.ViaCep;

/// <summary>
/// Cliente tipado para o ViaCEP. Registrado como HttpClient e implementa ICepService,
/// mantendo a camada de Application desacoplada do provedor específico.
/// A resiliência (retry + circuit breaker + timeout) é configurada via
/// AddStandardResilienceHandler no registro do HttpClient.
/// </summary>
public class ViaCepClient : ICepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepClient> _logger;

    public ViaCepClient(HttpClient httpClient, ILogger<ViaCepClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ConsultarCepResultado> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

        if (cepLimpo.Length != 8)
        {
            _logger.LogWarning("CEP com formato inválido ignorado antes da chamada externa: {Cep}", cep);
            return new ConsultarCepResultado.NaoEncontrado();
        }

        _logger.LogInformation("Consultando CEP {Cep} no ViaCEP", cepLimpo);

        try
        {
            var response = await _httpClient.GetAsync($"/ws/{cepLimpo}/json/", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "ViaCEP retornou {StatusCode} para CEP {Cep}",
                    (int)response.StatusCode, cepLimpo);

                return new ConsultarCepResultado.ServicoIndisponivel(
                    $"ViaCEP retornou HTTP {(int)response.StatusCode}");
            }

            var dto = await response.Content.ReadFromJsonAsync<ViaCepEnderecoDto>(
                cancellationToken: ct);

            // ViaCEP retorna { "erro": "true" } quando o CEP não existe
            if (dto is null || string.Equals(dto.Erro, "true", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("CEP {Cep} não encontrado no ViaCEP", cepLimpo);
                return new ConsultarCepResultado.NaoEncontrado();
            }

            _logger.LogInformation(
                "CEP {Cep} resolvido: {Logradouro}, {Cidade}/{Estado}",
                cepLimpo, dto.Logradouro, dto.Cidade, dto.Estado);

            return new ConsultarCepResultado.Encontrado(
                Cep: cepLimpo,
                Logradouro: dto.Logradouro,
                Bairro: dto.Bairro,
                Cidade: dto.Cidade,
                Estado: dto.Estado);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // Timeout do próprio HttpClient ou do pipeline de resiliência
            _logger.LogWarning("Timeout ao consultar CEP {Cep} no ViaCEP", cepLimpo);
            return new ConsultarCepResultado.ServicoIndisponivel("Timeout na consulta ao ViaCEP");
        }
        catch (OperationCanceledException)
        {
            throw; // Propagar cancelamento legítimo do caller
        }
        catch (Exception ex)
        {
            // Captura HttpRequestException, BrokenCircuitException (Polly) e demais falhas
            _logger.LogError(ex, "Falha ao consultar CEP {Cep} no ViaCEP", cepLimpo);
            return new ConsultarCepResultado.ServicoIndisponivel(ex.Message);
        }
    }
}
