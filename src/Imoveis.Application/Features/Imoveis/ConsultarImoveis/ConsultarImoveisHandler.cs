using Imoveis.Application.Common;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Imoveis.Application.Features.Imoveis.ConsultarImoveis;

public class ConsultarImoveisHandler
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(2);

    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ListaCacheInvalidador _invalidador;
    private readonly ILogger<ConsultarImoveisHandler> _logger;

    public ConsultarImoveisHandler(
        AppDbContext db,
        IMemoryCache cache,
        ListaCacheInvalidador invalidador,
        ILogger<ConsultarImoveisHandler> logger)
    {
        _db = db;
        _cache = cache;
        _invalidador = invalidador;
        _logger = logger;
    }

    public async Task<Result<ConsultarImoveisResponse>> HandleAsync(
        ConsultarImoveisQuery query,
        CancellationToken ct = default)
    {
        var cacheKey = ImovelCacheKeys.Lista(query);

        if (_cache.TryGetValue(cacheKey, out ConsultarImoveisResponse? cached))
        {
            _logger.LogInformation("Cache hit — lista de imóveis");
            return Result<ConsultarImoveisResponse>.Ok(cached!);
        }

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

        var response = new ConsultarImoveisResponse(imoveis, total, query.Pagina, query.TamanhoPagina);

        // Vincula esta entrada ao token de invalidação das listas.
        // Quando ListaCacheInvalidador.Invalidar() for chamado, esta entrada expira imediatamente.
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(Ttl)
            .AddExpirationToken(new CancellationChangeToken(_invalidador.Token));

        _cache.Set(cacheKey, response, options);

        return Result<ConsultarImoveisResponse>.Ok(response);
    }
}
