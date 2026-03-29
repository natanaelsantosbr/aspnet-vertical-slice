using FluentValidation;
using Imoveis.Application.Common;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.AtualizarImovel;

public class AtualizarImovelHandler
{
    private readonly AppDbContext _db;
    private readonly IValidator<AtualizarImovelCommand> _validator;
    private readonly ILogger<AtualizarImovelHandler> _logger;

    public AtualizarImovelHandler(
        AppDbContext db,
        IValidator<AtualizarImovelCommand> validator,
        ILogger<AtualizarImovelHandler> logger)
    {
        _db = db;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<bool>> HandleAsync(
        AtualizarImovelCommand command,
        CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(command, ct);
        if (!validacao.IsValid)
        {
            var erros = string.Join("; ", validacao.Errors.Select(e => e.ErrorMessage));
            return Result<bool>.Falha(erros);
        }

        var imovel = await _db.Imoveis.FirstOrDefaultAsync(i => i.Id == command.Id, ct);

        if (imovel is null)
            return Result<bool>.Falha("Imóvel não encontrado.");

        if (!imovel.Ativo)
            return Result<bool>.Falha("Não é possível atualizar um imóvel inativo.");

        imovel.Atualizar(
            command.Titulo,
            command.Descricao,
            command.Tipo,
            command.Cidade,
            command.Estado,
            command.Preco,
            command.AreaM2,
            command.Quartos);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Imóvel atualizado: {ImovelId}", imovel.Id);

        return Result<bool>.Ok(true);
    }
}
