using Imoveis.Domain.Enums;

namespace Imoveis.Application.Features.Imoveis.ConsultarImoveis;

public record ImovelResumoDto(
    Guid Id,
    string Titulo,
    TipoImovel Tipo,
    string Cidade,
    string Estado,
    decimal Preco,
    int AreaM2,
    int Quartos,
    DateTime CriadoEm
);

public record ConsultarImoveisResponse(
    IReadOnlyList<ImovelResumoDto> Imoveis,
    int Total,
    int Pagina,
    int TamanhoPagina
);
