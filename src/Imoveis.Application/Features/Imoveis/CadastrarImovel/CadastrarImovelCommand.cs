using Imoveis.Domain.Enums;

namespace Imoveis.Application.Features.Imoveis.CadastrarImovel;

public record CadastrarImovelCommand(
    string Titulo,
    string Descricao,
    TipoImovel Tipo,
    string Cidade,
    string Estado,
    decimal Preco,
    int AreaM2,
    int Quartos
);

public record CadastrarImovelResponse(Guid Id);
