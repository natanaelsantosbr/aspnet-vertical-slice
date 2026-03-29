using Imoveis.Domain.Entities;
using Imoveis.Domain.Enums;

namespace Imoveis.Infrastructure.Data;

public static class SeedData
{
    public static void Populate(AppDbContext db)
    {
        if (db.Imoveis.Any()) return;

        var imoveis = new[]
        {
            Imovel.Criar(
                "Apartamento Vista Mar",
                "Lindo apartamento com vista panorâmica para o mar, 3 suítes, varanda gourmet.",
                TipoImovel.Apartamento,
                "Florianópolis", "SC",
                850_000m, 90, 3),

            Imovel.Criar(
                "Casa em Condomínio Fechado",
                "Casa espaçosa em condomínio fechado com segurança 24h, piscina e churrasqueira.",
                TipoImovel.Casa,
                "São Paulo", "SP",
                1_200_000m, 250, 4),

            Imovel.Criar(
                "Studio Moderno Centro",
                "Studio moderno no coração da cidade, próximo ao metrô e comércio.",
                TipoImovel.Apartamento,
                "Curitiba", "PR",
                280_000m, 35, 1),

            Imovel.Criar(
                "Terreno Industrial",
                "Terreno amplo em área industrial consolidada, com fácil acesso à rodovia.",
                TipoImovel.Terreno,
                "Campinas", "SP",
                450_000m, 1000, 0),
        };

        db.Imoveis.AddRange(imoveis);
        db.SaveChanges();
    }
}
