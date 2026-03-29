using Imoveis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Imoveis.Infrastructure.Data.Configurations;

public class ImovelConfiguration : IEntityTypeConfiguration<Imovel>
{
    public void Configure(EntityTypeBuilder<Imovel> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Titulo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Descricao)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(i => i.Preco)
            .HasPrecision(18, 2);

        builder.Property(i => i.Tipo)
            .HasConversion<int>();

        // Endereco como owned entity: colunas ficam na mesma tabela (Imoveis)
        // com prefixo "Endereco_" gerado automaticamente pelo EF Core.
        builder.OwnsOne(i => i.Endereco, e =>
        {
            e.Property(x => x.Cep).HasMaxLength(8).IsRequired();
            e.Property(x => x.Logradouro).HasMaxLength(200).IsRequired();
            e.Property(x => x.Bairro).HasMaxLength(100).IsRequired();
            e.Property(x => x.Cidade).HasMaxLength(100).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(2).IsRequired();
            e.Property(x => x.Numero).HasMaxLength(20).IsRequired();
            e.Property(x => x.Complemento).HasMaxLength(100);
        });

        builder.HasMany(i => i.Leads)
               .WithOne(l => l.Imovel)
               .HasForeignKey(l => l.ImovelId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
