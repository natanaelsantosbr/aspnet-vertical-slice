using Imoveis.Domain.Enums;

namespace Imoveis.Application.Features.Imoveis.ObterImovelPorId;

public record ObterImovelPorIdResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    TipoImovel Tipo,
    string Cep,
    string Logradouro,
    string Bairro,
    string Cidade,
    string Estado,
    string Numero,
    string? Complemento,
    decimal Preco,
    int AreaM2,
    int Quartos,
    DateTime CriadoEm,
    bool Ativo
);
