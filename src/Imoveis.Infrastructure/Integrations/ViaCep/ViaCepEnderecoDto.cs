using System.Text.Json.Serialization;

namespace Imoveis.Infrastructure.Integrations.ViaCep;

/// <summary>
/// Mapeamento direto da resposta JSON do ViaCEP.
/// "erro" é string porque a API retorna "true" (string) quando o CEP não existe.
/// </summary>
internal record ViaCepEnderecoDto(
    [property: JsonPropertyName("cep")] string Cep,
    [property: JsonPropertyName("logradouro")] string Logradouro,
    [property: JsonPropertyName("bairro")] string Bairro,
    [property: JsonPropertyName("localidade")] string Cidade,
    [property: JsonPropertyName("uf")] string Estado,
    [property: JsonPropertyName("erro")] string? Erro
);
