using FluentValidation;
using Imoveis.Application.Common;
using Imoveis.Application.Common.Logging;
using Imoveis.Domain.Interfaces;
using Imoveis.Domain.ValueObjects;
using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Imoveis.Application.Features.Imoveis.AtualizarImovel;

public class AtualizarImovelHandler
{
    private readonly AppDbContext _db;
    private readonly IValidator<AtualizarImovelCommand> _validator;
    private readonly ICepService _cepService;
    private readonly IMemoryCache _cache;
    private readonly ListaCacheInvalidador _invalidador;
    private readonly ILogger<AtualizarImovelHandler> _logger;

    public AtualizarImovelHandler(
        AppDbContext db,
        IValidator<AtualizarImovelCommand> validator,
        ICepService cepService,
        IMemoryCache cache,
        ListaCacheInvalidador invalidador,
        ILogger<AtualizarImovelHandler> logger)
    {
        _db = db;
        _validator = validator;
        _cepService = cepService;
        _cache = cache;
        _invalidador = invalidador;
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

        var resultadoCep = await _cepService.ConsultarAsync(command.Cep, ct);

        switch (resultadoCep)
        {
            case ConsultarCepResultado.NaoEncontrado:
                return Result<bool>.Falha(
                    $"CEP '{command.Cep}' não encontrado. Verifique o CEP informado.");

            case ConsultarCepResultado.ServicoIndisponivel ind:
                _logger.CepIndisponivelNaAtualizacao(command.Cep, command.Id, ind.Detalhe);
                return Result<bool>.Falha(
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

        imovel.Atualizar(
            command.Titulo,
            command.Descricao,
            command.Tipo,
            endereco,
            command.Preco,
            command.AreaM2,
            command.Quartos);

        await _db.SaveChangesAsync(ct);

        _cache.Remove(ImovelCacheKeys.PorId(command.Id));
        _invalidador.Invalidar();

        _logger.ImovelAtualizado(imovel.Id);

        return Result<bool>.Ok(true);
    }
}
