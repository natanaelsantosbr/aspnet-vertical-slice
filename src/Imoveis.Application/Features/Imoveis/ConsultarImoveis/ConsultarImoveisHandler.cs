using Imoveis.Application.Common;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.ConsultarImoveis;

public class ConsultarImoveisHandler
{
    private readonly AppDbContext _db;
    private readonly ILogger<ConsultarImoveisHandler> _logger;

    public ConsultarImoveisHandler(AppDbContext db, ILogger<ConsultarImoveisHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<ConsultarImoveisResponse>> HandleAsync(
        ConsultarImoveisQuery query,
        CancellationToken ct = default)
    {
        var queryable = _db.Imoveis
            .Where(i => i.Ativo)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Cidade))
            queryable = queryable.Where(i => i.Cidade.ToLower().Contains(query.Cidade.ToLower()));

        if (query.Tipo.HasValue)
            queryable = queryable.Where(i => i.Tipo == query.Tipo.Value);

        if (query.PrecoMin.HasValue)
            queryable = queryable.Where(i => i.Preco >= query.PrecoMin.Value);

        if (query.PrecoMax.HasValue)
            queryable = queryable.Where(i => i.Preco <= query.PrecoMax.Value);

        var total = await queryable.CountAsync(ct);

        var imoveis = await queryable
            .OrderByDescending(i => i.CriadoEm)
            .Skip((query.Pagina - 1) * query.TamanhoPagina)
            .Take(query.TamanhoPagina)
            .Select(i => new ImovelResumoDto(
                i.Id, i.Titulo, i.Tipo, i.Cidade,
                i.Estado, i.Preco, i.AreaM2, i.Quartos, i.CriadoEm))
            .ToListAsync(ct);

        _logger.LogInformation(
            "Consulta de imóveis: {Filtros} — {Total} resultado(s)",
            new { query.Cidade, query.Tipo, query.PrecoMin, query.PrecoMax },
            total);

        return Result<ConsultarImoveisResponse>.Ok(
            new ConsultarImoveisResponse(imoveis, total, query.Pagina, query.TamanhoPagina));
    }
}
