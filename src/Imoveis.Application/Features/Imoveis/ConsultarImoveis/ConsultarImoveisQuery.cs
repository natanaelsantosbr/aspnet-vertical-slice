using Imoveis.Domain.Enums;

namespace Imoveis.Application.Features.Imoveis.ConsultarImoveis;

public record ConsultarImoveisQuery(
    string? Cidade = null,
    TipoImovel? Tipo = null,
    decimal? PrecoMin = null,
    decimal? PrecoMax = null,
    int Pagina = 1,
    int TamanhoPagina = 20
);
