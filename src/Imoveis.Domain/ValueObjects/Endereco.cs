namespace Imoveis.Domain.ValueObjects;

public class Endereco
{
    public string Cep { get; private set; } = string.Empty;
    public string Logradouro { get; private set; } = string.Empty;
    public string Bairro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Estado { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string? Complemento { get; private set; }

    private Endereco() { } // EF Core

    public static Endereco Criar(
        string cep,
        string logradouro,
        string bairro,
        string cidade,
        string estado,
        string numero,
        string? complemento)
    {
        return new Endereco
        {
            Cep = cep,
            Logradouro = logradouro,
            Bairro = bairro,
            Cidade = cidade,
            Estado = estado,
            Numero = numero,
            Complemento = complemento
        };
    }
}
