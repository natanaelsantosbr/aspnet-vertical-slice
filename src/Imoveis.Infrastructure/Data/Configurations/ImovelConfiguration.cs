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

        builder.Property(i => i.Cidade)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Estado)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(i => i.Preco)
            .HasPrecision(18, 2);

        builder.Property(i => i.Tipo)
            .HasConversion<int>();

        builder.HasMany(i => i.Leads)
               .WithOne(l => l.Imovel)
               .HasForeignKey(l => l.ImovelId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
