using Imoveis.Application.Common;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.RemoverImovel;

public class RemoverImovelHandler
{
    private readonly AppDbContext _db;
    private readonly ILogger<RemoverImovelHandler> _logger;

    public RemoverImovelHandler(AppDbContext db, ILogger<RemoverImovelHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<bool>> HandleAsync(
        RemoverImovelCommand command,
        CancellationToken ct = default)
    {
        var imovel = await _db.Imoveis.FirstOrDefaultAsync(i => i.Id == command.Id, ct);

        if (imovel is null)
            return Result<bool>.Falha("Imóvel não encontrado.");

        if (!imovel.Ativo)
            return Result<bool>.Falha("Imóvel já está inativo.");

        imovel.Desativar();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Imóvel removido (soft delete): {ImovelId}", imovel.Id);

        return Result<bool>.Ok(true);
    }
}
