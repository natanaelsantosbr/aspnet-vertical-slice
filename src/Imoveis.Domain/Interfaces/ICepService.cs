using Imoveis.Domain.ValueObjects;

namespace Imoveis.Domain.Interfaces;

public interface ICepService
{
    Task<ConsultarCepResultado> ConsultarAsync(string cep, CancellationToken ct = default);
}
