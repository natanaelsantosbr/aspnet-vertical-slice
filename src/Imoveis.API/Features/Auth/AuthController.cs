using Imoveis.API.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Imoveis.API.Features.Auth;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    // Usuários de demonstração (mock sem banco de dados).
    // Em produção: use ASP.NET Core Identity, EF Core e hash de senha (BCrypt / Argon2).
    private static readonly (string Id, string Nome, string Email, string Senha, string Role)[] Usuarios =
    [
        ("1", "Admin",    "admin@imoveis.com",    "Admin@123",    "Admin"),
        ("2", "Corretor", "corretor@imoveis.com", "Corretor@123", "Corretor"),
    ];

    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Autentica e retorna um JWT Bearer.
    /// Credenciais de teste: admin@imoveis.com / Admin@123
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("autenticacao")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var usuario = Usuarios.FirstOrDefault(u =>
            string.Equals(u.Email, request.Email, StringComparison.OrdinalIgnoreCase) &&
            u.Senha == request.Senha);

        if (usuario == default)
            return Unauthorized(new { erro = "Credenciais inválidas." });

        var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, usuario.Role);
        return Ok(token);
    }
}

public record LoginRequest(string Email, string Senha);
