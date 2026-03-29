namespace Imoveis.Domain.ValueObjects;

/// <summary>
/// Resultado discriminado da consulta de CEP.
/// Permite que o handler trate cada caso sem depender de exceções ou nulos.
/// </summary>
public abstract record ConsultarCepResultado
{
    /// <summary>CEP encontrado e endereço retornado com sucesso.</summary>
    public sealed record Encontrado(
        string Cep,
        string Logradouro,
        string Bairro,
        string Cidade,
        string Estado) : ConsultarCepResultado;

    /// <summary>CEP inexistente na base do ViaCEP.</summary>
    public sealed record NaoEncontrado : ConsultarCepResultado;

    /// <summary>Falha temporária de comunicação ou timeout com o serviço externo.</summary>
    public sealed record ServicoIndisponivel(string Detalhe) : ConsultarCepResultado;
}
