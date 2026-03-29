using Imoveis.Domain.Enums;

namespace Imoveis.Application.Features.Imoveis.ObterImovelPorId;

public record ObterImovelPorIdResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    TipoImovel Tipo,
    string Cidade,
    string Estado,
    decimal Preco,
    int AreaM2,
    int Quartos,
    DateTime CriadoEm,
    bool Ativo
);
