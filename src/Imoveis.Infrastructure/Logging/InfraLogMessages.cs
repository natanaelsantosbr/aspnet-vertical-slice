using Microsoft.Extensions.Logging;

namespace Imoveis.Infrastructure.Logging;

/// <summary>
/// Mensagens de log da camada Infrastructure usando [LoggerMessage] source generator.
/// </summary>
internal static partial class InfraLogMessages
{
    [LoggerMessage(Level = LogLevel.Warning,
        Message = "CEP com formato inválido ignorado: {Cep}")]
    internal static partial void CepFormatoInvalido(this ILogger logger, string cep);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Consultando CEP {Cep} no ViaCEP")]
    internal static partial void ConsultandoCep(this ILogger logger, string cep);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "ViaCEP retornou {StatusCode} para CEP {Cep}")]
    internal static partial void ViaCepStatusCodeInesperado(
        this ILogger logger, int statusCode, string cep);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "CEP {Cep} não encontrado no ViaCEP")]
    internal static partial void CepNaoEncontrado(this ILogger logger, string cep);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "CEP {Cep} resolvido: {Logradouro}, {Cidade}/{Estado}")]
    internal static partial void CepResolvido(
        this ILogger logger, string cep, string logradouro, string cidade, string estado);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Timeout ao consultar CEP {Cep} no ViaCEP")]
    internal static partial void ViaCepTimeout(this ILogger logger, string cep);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Falha ao consultar CEP {Cep} no ViaCEP")]
    internal static partial void ViaCepFalha(
        this ILogger logger, Exception ex, string cep);
}
