using Imoveis.Application.Common;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.ObterImovelPorId;

public class ObterImovelPorIdHandler
{
    private readonly AppDbContext _db;
    private readonly ILogger<ObterImovelPorIdHandler> _logger;

    public ObterImovelPorIdHandler(AppDbContext db, ILogger<ObterImovelPorIdHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<ObterImovelPorIdResponse>> HandleAsync(
        ObterImovelPorIdQuery query,
        CancellationToken ct = default)
    {
        var imovel = await _db.Imoveis
            .AsNoTracking()
            .Where(i => i.Id == query.Id)
            .Select(i => new ObterImovelPorIdResponse(
                i.Id, i.Titulo, i.Descricao, i.Tipo,
                i.Cidade, i.Estado, i.Preco, i.AreaM2,
                i.Quartos, i.CriadoEm, i.Ativo))
            .FirstOrDefaultAsync(ct);

        if (imovel is null)
        {
            _logger.LogWarning("Imóvel não encontrado: {ImovelId}", query.Id);
            return Result<ObterImovelPorIdResponse>.Falha("Imóvel não encontrado.");
        }

        return Result<ObterImovelPorIdResponse>.Ok(imovel);
    }
}
