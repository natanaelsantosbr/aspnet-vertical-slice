using FluentValidation;
using Imoveis.Application.Common;
using Imoveis.Domain.Entities;
using Imoveis.Domain.Interfaces;
using Imoveis.Domain.ValueObjects;
using Imoveis.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.CadastrarImovel;

public class CadastrarImovelHandler
{
    private readonly AppDbContext _db;
    private readonly IValidator<CadastrarImovelCommand> _validator;
    private readonly ICepService _cepService;
    private readonly ListaCacheInvalidador _invalidador;
    private readonly ILogger<CadastrarImovelHandler> _logger;

    public CadastrarImovelHandler(
        AppDbContext db,
        IValidator<CadastrarImovelCommand> validator,
        ICepService cepService,
        ListaCacheInvalidador invalidador,
        ILogger<CadastrarImovelHandler> logger)
    {
        _db = db;
        _validator = validator;
        _cepService = cepService;
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

        var resultadoCep = await _cepService.ConsultarAsync(command.Cep, ct);

        switch (resultadoCep)
        {
            case ConsultarCepResultado.NaoEncontrado:
                return Result<CadastrarImovelResponse>.Falha(
                    $"CEP '{command.Cep}' não encontrado. Verifique o CEP informado.");

            case ConsultarCepResultado.ServicoIndisponivel ind:
                _logger.LogWarning(
                    "Falha ao consultar CEP {Cep} durante cadastro de imóvel: {Detalhe}",
                    command.Cep, ind.Detalhe);
                return Result<CadastrarImovelResponse>.Falha(
                    "Serviço de consulta de CEP temporariamente indisponível. Tente novamente em instantes.");
        }

        var encontrado = (ConsultarCepResultado.Encontrado)resultadoCep;

        var endereco = Endereco.Criar(
            encontrado.Cep,
            encontrado.Logradouro,
            encontrado.Bairro,
            encontrado.Cidade,
            encontrado.Estado,
            command.Numero,
            command.Complemento);

        var imovel = Imovel.Criar(
            command.Titulo,
            command.Descricao,
            command.Tipo,
            endereco,
            command.Preco,
            command.AreaM2,
            command.Quartos);

        _db.Imoveis.Add(imovel);
        await _db.SaveChangesAsync(ct);

        _invalidador.Invalidar();

        _logger.LogInformation(
            "Imóvel cadastrado: {ImovelId} - {Titulo} | {Logradouro}, {Numero} - {Cidade}/{Estado}",
            imovel.Id, imovel.Titulo,
            endereco.Logradouro, endereco.Numero,
            endereco.Cidade, endereco.Estado);

        return Result<CadastrarImovelResponse>.Ok(new CadastrarImovelResponse(imovel.Id));
    }
}
