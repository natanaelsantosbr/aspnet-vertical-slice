using Imoveis.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Imoveis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase("ImoveisDb"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(connectionString));
        }

        return services;
    }
}
