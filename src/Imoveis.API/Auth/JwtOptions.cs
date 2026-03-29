namespace Imoveis.API.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Segredo para assinar o token. Mínimo 32 caracteres (256 bits para HMAC-SHA256).
    /// Em produção: sobrescrever via variável de ambiente Jwt__Secret
    /// </summary>
    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = "imoveis-api";
    public string Audience { get; init; } = "imoveis-client";

    /// <summary>Expiração em minutos. Default: 60.</summary>
    public int ExpiracaoMinutos { get; init; } = 60;
}
