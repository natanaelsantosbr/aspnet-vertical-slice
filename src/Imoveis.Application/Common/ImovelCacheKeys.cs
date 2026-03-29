namespace Imoveis.Application.Common;

public static class ImovelCacheKeys
{
    public static string PorId(Guid id) => $"imovel:{id}";

    public static string Lista(object filtros) => $"imoveis:lista:{filtros.GetHashCode()}";
}
