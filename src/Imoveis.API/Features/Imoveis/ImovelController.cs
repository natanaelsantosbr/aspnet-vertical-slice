using Imoveis.Application.Features.Imoveis.AtualizarImovel;
using Imoveis.Application.Features.Imoveis.CadastrarImovel;
using Imoveis.Application.Features.Imoveis.ConsultarImoveis;
using Imoveis.Application.Features.Imoveis.ObterImovelPorId;
using Imoveis.Application.Features.Imoveis.RemoverImovel;
using Imoveis.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Imoveis.API.Features.Imoveis;

[ApiController]
[Route("api/imoveis")]
[Produces("application/json")]
public class ImovelController : ControllerBase
{
    private readonly CadastrarImovelHandler _cadastrarHandler;
    private readonly ConsultarImoveisHandler _consultarHandler;
    private readonly ObterImovelPorIdHandler _obterPorIdHandler;
    private readonly AtualizarImovelHandler _atualizarHandler;
    private readonly RemoverImovelHandler _removerHandler;

    public ImovelController(
        CadastrarImovelHandler cadastrarHandler,
        ConsultarImoveisHandler consultarHandler,
        ObterImovelPorIdHandler obterPorIdHandler,
        AtualizarImovelHandler atualizarHandler,
        RemoverImovelHandler removerHandler)
    {
        _cadastrarHandler = cadastrarHandler;
        _consultarHandler = consultarHandler;
        _obterPorIdHandler = obterPorIdHandler;
        _atualizarHandler = atualizarHandler;
        _removerHandler = removerHandler;
    }

    /// <summary>
    /// Cadastra um novo imóvel. O endereço é enriquecido automaticamente via CEP.
    /// Requer autenticação.
    /// </summary>
    [HttpPost]
    [Authorize]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(typeof(CadastrarImovelResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarImovelCommand command,
        CancellationToken ct)
    {
        var result = await _cadastrarHandler.HandleAsync(command, ct);

        if (!result.Sucesso)
            return BadRequest(new { erro = result.Erro });

        return CreatedAtAction(nameof(ObterPorId), new { id = result.Dado!.Id }, result.Dado);
    }

    /// <summary>
    /// Consulta imóveis com filtros opcionais.
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(ConsultarImoveisResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Consultar(
        [FromQuery] string? cidade,
        [FromQuery] TipoImovel? tipo,
        [FromQuery] decimal? precoMin,
        [FromQuery] decimal? precoMax,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        var query = new ConsultarImoveisQuery(cidade, tipo, precoMin, precoMax, pagina, tamanhoPagina);
        var result = await _consultarHandler.HandleAsync(query, ct);
        return Ok(result.Dado);
    }

    /// <summary>
    /// Retorna um imóvel pelo Id com endereço completo.
    /// </summary>
    [HttpGet("{id:guid}")]
    [EnableRateLimiting("leitura")]
    [ProducesResponseType(typeof(ObterImovelPorIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var result = await _obterPorIdHandler.HandleAsync(new ObterImovelPorIdQuery(id), ct);

        if (!result.Sucesso)
            return NotFound(new { erro = result.Erro });

        return Ok(result.Dado);
    }

    /// <summary>
    /// Atualiza os dados de um imóvel. O endereço é re-enriquecido via CEP.
    /// Requer autenticação.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarImovelRequest request,
        CancellationToken ct)
    {
        var command = new AtualizarImovelCommand(
            id,
            request.Titulo,
            request.Descricao,
            request.Tipo,
            request.Cep,
            request.Numero,
            request.Complemento,
            request.Preco,
            request.AreaM2,
            request.Quartos);

        var result = await _atualizarHandler.HandleAsync(command, ct);

        if (!result.Sucesso)
        {
            if (result.Erro!.Contains("não encontrado"))
                return NotFound(new { erro = result.Erro });

            return BadRequest(new { erro = result.Erro });
        }

        return NoContent();
    }

    /// <summary>
    /// Remove (desativa) um imóvel. Soft delete.
    /// Requer autenticação.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [EnableRateLimiting("escrita")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        var result = await _removerHandler.HandleAsync(new RemoverImovelCommand(id), ct);

        if (!result.Sucesso)
            return NotFound(new { erro = result.Erro });

        return NoContent();
    }
}

/// <summary>Body do PUT — sem o Id (vem pela rota).</summary>
public record AtualizarImovelRequest(
    string Titulo,
    string Descricao,
    TipoImovel Tipo,
    string Cep,
    string Numero,
    string? Complemento,
    decimal Preco,
    int AreaM2,
    int Quartos
);
