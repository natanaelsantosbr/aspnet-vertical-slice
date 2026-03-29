namespace Imoveis.Infrastructure.Integrations.ViaCep;

public class ViaCepOptions
{
    public const string SectionName = "Integracoes:ViaCep";

    public string BaseUrl { get; init; } = "https://viacep.com.br";
    public int TimeoutSegundos { get; init; } = 5;
    public int MaxTentativas { get; init; } = 3;
}
