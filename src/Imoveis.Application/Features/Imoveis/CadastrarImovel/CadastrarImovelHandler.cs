using FluentValidation;
using Imoveis.Application.Common;
using Imoveis.Domain.Entities;
using Imoveis.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.CadastrarImovel;

public class CadastrarImovelHandler
{
    private readonly AppDbContext _db;
    private readonly IValidator<CadastrarImovelCommand> _validator;
    private readonly ListaCacheInvalidador _invalidador;
    private readonly ILogger<CadastrarImovelHandler> _logger;

    public CadastrarImovelHandler(
        AppDbContext db,
        IValidator<CadastrarImovelCommand> validator,
        ListaCacheInvalidador invalidador,
        ILogger<CadastrarImovelHandler> logger)
    {
        _db = db;
        _validator = validator;
        _invalidador = invalidador;
        _logger = logger;
    }

    public async Task<Result<CadastrarImovelResponse>> HandleAsync(
        CadastrarImovelCommand command,
        CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(command, ct);
        if (!validacao.IsValid)
        {
            var erros = string.Join("; ", validacao.Errors.Select(e => e.ErrorMessage));
            return Result<CadastrarImovelResponse>.Falha(erros);
        }

        var imovel = Imovel.Criar(
            command.Titulo,
            command.Descricao,
            command.Tipo,
            command.Cidade,
            command.Estado,
            command.Preco,
            command.AreaM2,
            command.Quartos);

        _db.Imoveis.Add(imovel);
        await _db.SaveChangesAsync(ct);

        _invalidador.Invalidar();

        _logger.LogInformation("Imóvel cadastrado: {ImovelId} - {Titulo}", imovel.Id, imovel.Titulo);

        return Result<CadastrarImovelResponse>.Ok(new CadastrarImovelResponse(imovel.Id));
    }
}
