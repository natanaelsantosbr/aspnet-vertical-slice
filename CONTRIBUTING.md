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

---

## Padrões do projeto

### Adicionando uma nova feature

Cada feature deve ser autossuficiente e isolada em sua própria pasta. Siga a estrutura existente:

```
Application/Features/
└── NomeDaFeature/
    ├── NomeDaFeatureCommand.cs    # ou Query.cs para leitura
    ├── NomeDaFeatureHandler.cs
    └── NomeDaFeatureValidator.cs  # se houver entrada a validar
```

```
API/Features/
└── NomeDaFeature/
    └── NomeDaFeatureController.cs
```

### Regras gerais

- **Sem abstrações prematuras** — não crie interfaces ou classes base para um único uso
- **DTOs específicos por caso de uso** — evite reutilizar DTOs entre features diferentes
- **Handlers focados** — cada handler executa exatamente um caso de uso
- **Erros esperados via `Result<T>`** — não use exceções para fluxos de negócio (not found, validação, CEP inválido, etc.)
- **Exceções inesperadas** são capturadas pelo `GlobalExceptionHandler` — não trate-as nos handlers

---

### Logging

- Use **`[LoggerMessage]` source generator** para mensagens novas — nunca `LogInformation("texto {param}", valor)` inline nos handlers
- Adicione métodos em `AppLogMessages.cs` (Application) ou `InfraLogMessages.cs` (Infrastructure)
- Siga a convenção de níveis:

| Nível | Quando |
|---|---|
| `LogDebug` | Cache hits e eventos frequentes sem valor em produção |
| `LogInformation` | Operações concluídas com sucesso |
| `LogWarning` | Erros esperados: not found, input inválido, estado inconsistente |
| `LogError` | Apenas no `GlobalExceptionHandler` para exceções inesperadas |

**Exemplo:**
```csharp
// Em AppLogMessages.cs
[LoggerMessage(Level = LogLevel.Information, Message = "Imóvel destacado: {ImovelId}")]
internal static partial void ImovelDestacado(this ILogger logger, Guid imovelId);

// No handler
_logger.ImovelDestacado(imovel.Id);
```

---

### Cache

- Handlers de **leitura** (`ObterPorId`, `ConsultarImoveis`) devem verificar o cache antes de ir ao banco
- Handlers de **mutação** (`Cadastrar`, `Atualizar`, `Remover`) devem invalidar o cache após salvar
- Use `ImovelCacheKeys` para gerar chaves — nunca escreva strings de chave inline
- Para invalidar listas, use `ListaCacheInvalidador.Invalidar()` — não tente remover entradas individualmente

---

### Autenticação e autorização

- Endpoints de **leitura pública** (`GET`) não precisam de `[Authorize]`
- Endpoints de **mutação** (`POST`, `PUT`, `DELETE`) devem ter `[Authorize]`
- Use `[Authorize]` simples — sem roles por enquanto, a menos que haja necessidade explícita
- Não adicione lógica de autorização dentro dos handlers — isso pertence ao pipeline HTTP

---

### Rate limiting

- Endpoints de **leitura pública**: adicione `[EnableRateLimiting("leitura")]`
- Endpoints de **escrita autenticada**: adicione `[EnableRateLimiting("escrita")]`
- Novos endpoints de autenticação: use `[EnableRateLimiting("autenticacao")]`
- Se precisar de uma nova política, defina-a em `Program.cs` junto às demais

---

### Integração com serviços externos

- Novos clientes HTTP devem usar `AddHttpClient<IInterface, Implementacao>()` com `AddStandardResilienceHandler()`
- A interface do serviço deve ficar em **Domain** (não em Application) para evitar dependência circular
- O resultado de chamadas externas deve usar o padrão discriminated union (veja `ConsultarCepResultado`)
- Adicione um `IHealthCheck` correspondente para monitorar a disponibilidade do serviço externo

---

## Rodando localmente

```bash
dotnet restore
dotnet build
dotnet run --project src/Imoveis.API
```

Acesse o Swagger em `https://localhost:{porta}/swagger` para testar os endpoints interativamente.

Para testar endpoints autenticados no Swagger:
1. Chame `POST /api/auth/login` com `admin@imoveis.com` / `Admin@123`
2. Copie o `token` da resposta
3. Clique em **Authorize** e informe `Bearer {token}`

---

## Dúvidas

Abra uma [issue](../../issues) descrevendo sua dúvida ou sugestão.
