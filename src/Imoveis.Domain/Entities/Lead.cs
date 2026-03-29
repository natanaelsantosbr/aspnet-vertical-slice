namespace Imoveis.Domain.Entities;

public class Lead
{
    public Guid Id { get; private set; }
    public Guid ImovelId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string? Mensagem { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public Imovel Imovel { get; private set; } = null!;

    private Lead() { } // EF Core

    public static Lead Criar(Guid imovelId, string nome, string email, string telefone, string? mensagem)
    {
        return new Lead
        {
            Id = Guid.NewGuid(),
            ImovelId = imovelId,
            Nome = nome,
            Email = email,
            Telefone = telefone,
            Mensagem = mensagem,
            CriadoEm = DateTime.UtcNow
        };
    }
}
