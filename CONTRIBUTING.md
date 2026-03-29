# Contribuindo

Obrigado pelo interesse em contribuir! Abaixo estão as diretrizes para manter o projeto consistente.

## Como contribuir

1. Faça um **fork** do repositório
2. Crie uma branch a partir de `main`:
   ```bash
   git checkout -b feature/nome-da-feature
   ```
3. Faça suas alterações seguindo os padrões abaixo
4. Abra um **Pull Request** com uma descrição clara do que foi alterado e por quê

## Padrões do projeto

### Adicionando uma nova feature

Cada feature deve ser autossuficiente e isolada em sua própria pasta. Siga a estrutura existente:

```
Features/
└── NomeDaFeature/
    ├── NomeDaFeatureCommand.cs   # ou Query.cs
    ├── NomeDaFeatureHandler.cs
    └── NomeDaFeatureValidator.cs # se houver entrada a validar
```

### Regras gerais

- **Sem abstrações prematuras** — não crie interfaces ou classes base para um único uso
- **DTOs específicos por caso de uso** — evite reutilizar DTOs entre features diferentes
- **Handlers focados** — cada handler deve executar exatamente um caso de uso
- **Erros esperados via `Result<T>`** — não use exceções para fluxos de negócio (not found, validação, etc.)
- **Logging** nos handlers para operações relevantes (`LogInformation` para sucesso, `LogWarning` para not found)

### Cache

- Handlers de **leitura** (`ObterPorId`, `ConsultarImoveis`) devem checar o cache antes de ir ao banco
- Handlers de **mutação** (`Cadastrar`, `Atualizar`, `Remover`) devem invalidar o cache após salvar no banco
- Use `ImovelCacheKeys` para gerar chaves — nunca escreva strings de chave inline nos handlers
- Para invalidar listas, use `ListaCacheInvalidador.Invalidar()` — não tente remover entradas de lista individualmente

### Commits

Use mensagens de commit claras e no imperativo:

```
Adiciona endpoint de destaque de imóvel
Corrige validação de telefone no lead
Remove duplicação no handler de consulta
```

## Rodando localmente

```bash
dotnet restore
dotnet build
dotnet run --project src/Imoveis.API
```

## Dúvidas

Abra uma [issue](../../issues) descrevendo sua dúvida ou sugestão.
