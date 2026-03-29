using Imoveis.Application.Common.Cep;

namespace Imoveis.Application.Common.Interfaces;

public interface ICepService
{
    Task<ConsultarCepResultado> ConsultarAsync(string cep, CancellationToken ct = default);
}
