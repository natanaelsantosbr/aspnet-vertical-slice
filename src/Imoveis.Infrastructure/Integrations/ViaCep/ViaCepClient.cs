using System.Net.Http.Json;
using Imoveis.Domain.Interfaces;
using Imoveis.Domain.ValueObjects;
using Imoveis.Infrastructure.Logging;
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
            _logger.CepFormatoInvalido(cep);
            return new ConsultarCepResultado.NaoEncontrado();
        }

        _logger.ConsultandoCep(cepLimpo);

        try
        {
            var response = await _httpClient.GetAsync($"/ws/{cepLimpo}/json/", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.ViaCepStatusCodeInesperado((int)response.StatusCode, cepLimpo);

                return new ConsultarCepResultado.ServicoIndisponivel(
                    $"ViaCEP retornou HTTP {(int)response.StatusCode}");
            }

            var dto = await response.Content.ReadFromJsonAsync<ViaCepEnderecoDto>(
                cancellationToken: ct);

            // ViaCEP retorna { "erro": "true" } quando o CEP não existe
            if (dto is null || string.Equals(dto.Erro, "true", StringComparison.OrdinalIgnoreCase))
            {
                _logger.CepNaoEncontrado(cepLimpo);
                return new ConsultarCepResultado.NaoEncontrado();
            }

            _logger.CepResolvido(cepLimpo, dto.Logradouro, dto.Cidade, dto.Estado);

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
            _logger.ViaCepTimeout(cepLimpo);
            return new ConsultarCepResultado.ServicoIndisponivel("Timeout na consulta ao ViaCEP");
        }
        catch (OperationCanceledException)
        {
            throw; // Propagar cancelamento legítimo do caller
        }
        catch (Exception ex)
        {
            // Captura HttpRequestException, BrokenCircuitException (Polly) e demais falhas
            _logger.ViaCepFalha(ex, cepLimpo);
            return new ConsultarCepResultado.ServicoIndisponivel(ex.Message);
        }
    }
}
