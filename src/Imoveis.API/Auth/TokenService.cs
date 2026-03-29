using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Imoveis.API.Auth;

public class TokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenResponse GerarToken(string userId, string nome, string role)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(_options.ExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  userId),
            new Claim(JwtRegisteredClaimNames.Name, nome),
            new Claim(ClaimTypes.Role,              role),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer:            _options.Issuer,
            audience:          _options.Audience,
            claims:            claims,
            expires:           expira,
            signingCredentials: creds);

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expira);
    }
}

public record TokenResponse(string Token, DateTime ExpiraEm);
