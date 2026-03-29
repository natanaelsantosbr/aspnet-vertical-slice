namespace Imoveis.Application.Common;

/// <summary>
/// Singleton que permite invalidar todas as entradas de cache de listagem
/// de imóveis de uma vez, via CancellationToken vinculado às entradas do cache.
/// </summary>
public sealed class ListaCacheInvalidador
{
    private CancellationTokenSource _cts = new();

    public CancellationToken Token => _cts.Token;

    public void Invalidar()
    {
        // Troca atômica: cria um novo CTS antes de cancelar o antigo,
        // garantindo que novas entradas não herdem um token já cancelado.
        var antigo = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
        antigo.Cancel();
        antigo.Dispose();
    }
}
