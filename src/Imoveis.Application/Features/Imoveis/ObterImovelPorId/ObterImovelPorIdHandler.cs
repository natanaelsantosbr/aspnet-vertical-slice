using Imoveis.Application.Common;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.ObterImovelPorId;

public class ObterImovelPorIdHandler
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);

    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ObterImovelPorIdHandler> _logger;

    public ObterImovelPorIdHandler(AppDbContext db, IMemoryCache cache, ILogger<ObterImovelPorIdHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<ObterImovelPorIdResponse>> HandleAsync(
        ObterImovelPorIdQuery query,
        CancellationToken ct = default)
    {
        var cacheKey = ImovelCacheKeys.PorId(query.Id);

        if (_cache.TryGetValue(cacheKey, out ObterImovelPorIdResponse? cached))
        {
            _logger.LogInformation("Cache hit — imóvel {ImovelId}", query.Id);
            return Result<ObterImovelPorIdResponse>.Ok(cached!);
        }

        var imovel = await _db.Imoveis
            .AsNoTracking()
            .Where(i => i.Id == query.Id)
            .Select(i => new ObterImovelPorIdResponse(
                i.Id, i.Titulo, i.Descricao, i.Tipo,
                i.Endereco.Cep, i.Endereco.Logradouro, i.Endereco.Bairro,
                i.Endereco.Cidade, i.Endereco.Estado,
                i.Endereco.Numero, i.Endereco.Complemento,
                i.Preco, i.AreaM2, i.Quartos, i.CriadoEm, i.Ativo))
            .FirstOrDefaultAsync(ct);

        if (imovel is null)
        {
            _logger.LogWarning("Imóvel não encontrado: {ImovelId}", query.Id);
            return Result<ObterImovelPorIdResponse>.Falha("Imóvel não encontrado.");
        }

        _cache.Set(cacheKey, imovel, Ttl);
        return Result<ObterImovelPorIdResponse>.Ok(imovel);
    }
}
