using Imoveis.Domain.Enums;

namespace Imoveis.Application.Features.Imoveis.AtualizarImovel;

public record AtualizarImovelCommand(
    Guid Id,
    string Titulo,
    string Descricao,
    TipoImovel Tipo,
    string Cep,
    string Numero,
    string? Complemento,
    decimal Preco,
    int AreaM2,
    int Quartos
);
