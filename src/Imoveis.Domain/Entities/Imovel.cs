using Imoveis.Domain.Enums;
using Imoveis.Domain.ValueObjects;

namespace Imoveis.Domain.Entities;

public class Imovel
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public TipoImovel Tipo { get; private set; }
    public Endereco Endereco { get; private set; } = null!;
    public decimal Preco { get; private set; }
    public int AreaM2 { get; private set; }
    public int Quartos { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool Ativo { get; private set; }

    private readonly List<Lead> _leads = new();
    public IReadOnlyCollection<Lead> Leads => _leads.AsReadOnly();

    private Imovel() { } // EF Core

    public static Imovel Criar(
        string titulo,
        string descricao,
        TipoImovel tipo,
        Endereco endereco,
        decimal preco,
        int areaM2,
        int quartos)
    {
        return new Imovel
        {
            Id = Guid.NewGuid(),
            Titulo = titulo,
            Descricao = descricao,
            Tipo = tipo,
            Endereco = endereco,
            Preco = preco,
            AreaM2 = areaM2,
            Quartos = quartos,
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };
    }

    public void Atualizar(
        string titulo,
        string descricao,
        TipoImovel tipo,
        Endereco endereco,
        decimal preco,
        int areaM2,
        int quartos)
    {
        Titulo = titulo;
        Descricao = descricao;
        Tipo = tipo;
        Endereco = endereco;
        Preco = preco;
        AreaM2 = areaM2;
        Quartos = quartos;
    }

    public void Desativar() => Ativo = false;
}
