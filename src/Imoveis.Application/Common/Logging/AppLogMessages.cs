using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Common.Logging;

/// <summary>
/// Mensagens de log da camada Application usando [LoggerMessage] source generator.
/// Geração em tempo de compilação elimina boxing de parâmetros e alocação de strings
/// no hot path — especialmente relevante para cache hits e consultas frequentes.
/// </summary>
internal static partial class AppLogMessages
{
    // ─── Imóvel: Cadastrar ─────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Imóvel cadastrado: {ImovelId} - {Titulo} | {Logradouro}, {Numero} - {Cidade}/{Estado}")]
    internal static partial void ImovelCadastrado(
        this ILogger logger,
        Guid imovelId, string titulo, string logradouro, string numero, string cidade, string estado);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Falha ao consultar CEP {Cep} durante cadastro de imóvel: {Detalhe}")]
    internal static partial void CepIndisponivelNoCadastro(
        this ILogger logger, string cep, string detalhe);

    // ─── Imóvel: Atualizar ─────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Imóvel atualizado: {ImovelId}")]
    internal static partial void ImovelAtualizado(this ILogger logger, Guid imovelId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Falha ao consultar CEP {Cep} durante atualização do imóvel {ImovelId}: {Detalhe}")]
    internal static partial void CepIndisponivelNaAtualizacao(
        this ILogger logger, string cep, Guid imovelId, string detalhe);

    // ─── Imóvel: Remover ───────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Tentativa de remover imóvel já inativo: {ImovelId}")]
    internal static partial void ImovelJaInativo(this ILogger logger, Guid imovelId);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Imóvel removido (soft delete): {ImovelId}")]
    internal static partial void ImovelRemovido(this ILogger logger, Guid imovelId);

    // ─── Imóvel: Consultar ─────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Cache hit — lista de imóveis")]
    internal static partial void CacheHitLista(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Consulta de imóveis: cidade={Cidade} tipo={Tipo} precoMin={PrecoMin} precoMax={PrecoMax} — {Total} resultado(s)")]
    internal static partial void ConsultaImoveisRealizada(
        this ILogger logger,
        string? cidade, string? tipo, decimal? precoMin, decimal? precoMax, int total);

    // ─── Imóvel: ObterPorId ────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Cache hit — imóvel {ImovelId}")]
    internal static partial void CacheHitPorId(this ILogger logger, Guid imovelId);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Imóvel não encontrado: {ImovelId}")]
    internal static partial void ImovelNaoEncontrado(this ILogger logger, Guid imovelId);

    // ─── Lead: Registrar ───────────────────────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Lead registrado: {LeadId} | Imóvel: {ImovelId} | Contato: {Email}")]
    internal static partial void LeadRegistrado(
        this ILogger logger, Guid leadId, Guid imovelId, string email);
}
