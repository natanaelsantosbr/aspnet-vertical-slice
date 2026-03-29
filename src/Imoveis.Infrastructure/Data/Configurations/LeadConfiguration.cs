using Imoveis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Imoveis.Infrastructure.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Telefone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Mensagem)
            .HasMaxLength(1000);
    }
}
