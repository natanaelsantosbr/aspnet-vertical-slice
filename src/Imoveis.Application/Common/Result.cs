namespace Imoveis.Application.Common;

/// <summary>
/// Representa o resultado de uma operação que retorna um valor.
/// Evita exceções para fluxos esperados (validação, not found, etc).
/// </summary>
public class Result<T>
{
    public bool Sucesso { get; }
    public T? Dado { get; }
    public string? Erro { get; }

    private Result(bool sucesso, T? dado, string? erro)
    {
        Sucesso = sucesso;
        Dado = dado;
        Erro = erro;
    }

    public static Result<T> Ok(T dado) => new(true, dado, null);
    public static Result<T> Falha(string erro) => new(false, default, erro);
}
