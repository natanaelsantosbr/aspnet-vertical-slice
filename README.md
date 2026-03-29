# Imoveis API

API REST para gerenciamento de imóveis, construída com **ASP.NET Core 8** seguindo uma arquitetura modular e pragmática — sem overengineering, focada em clareza e manutenibilidade.

> Projeto de referência demonstrando Clean Architecture simplificada com organização por feature (vertical slice), segurança com JWT, cache, resiliência em integrações externas e logging estruturado.

---

## Sumário

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Como rodar](#como-rodar)
- [Autenticação](#autenticação)
- [Endpoints](#endpoints)
- [Exemplos de uso](#exemplos-de-uso)
- [Rate limiting](#rate-limiting)
- [Cache](#cache)
- [Logging](#logging)
- [Integração ViaCEP](#integração-viacep)
- [Health checks](#health-checks)
- [Configuração do banco de dados](#configuração-do-banco-de-dados)
- [Variáveis de ambiente](#variáveis-de-ambiente)
- [Decisões de design](#decisões-de-design)

---

## Tecnologias

| Tecnologia | Uso |
|---|---|
| [.NET 8](https://dotnet.microsoft.com/) | Framework principal |
| [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core) | Camada HTTP |
| [Entity Framework Core 8](https://docs.microsoft.com/ef/core) | ORM |
| [EF Core InMemory](https://docs.microsoft.com/ef/core/providers/in-memory) | Banco de dados para desenvolvimento |
| [SQL Server](https://www.microsoft.com/sql-server) | Banco de dados para produção |
| [JWT Bearer](https://learn.microsoft.com/aspnet/core/security/authentication/jwt-authn) | Autenticação stateless |
| [ASP.NET Core Rate Limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit) | Proteção contra abuso (nativo .NET 8) |
| [IMemoryCache](https://docs.microsoft.com/aspnet/core/performance/caching/memory) | Cache em memória (nativo .NET) |
| [FluentValidation](https://docs.fluentvalidation.net/) | Validação de entrada |
| [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/dotnet/core/resilience) | Retry, circuit breaker e timeout (Polly v8) |
| [ILogger / LoggerMessage](https://learn.microsoft.com/aspnet/core/fundamentals/logging) | Logging estruturado com source generator |
| [Swagger / Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) | Documentação interativa da API |

---

## Arquitetura

O projeto segue uma **Clean Architecture simplificada** com separação em 4 camadas e organização interna **por feature**:

```
┌─────────────────────────────────────────────────────────────────┐
│                         Imoveis.API                             │
│         Controllers · Auth (JWT) · Middleware · Program.cs      │
├─────────────────────────────────────────────────────────────────┤
│                      Imoveis.Application                        │
│         Handlers · Commands/Queries · Validators · DTOs         │
│                 Cache · Logging (AppLogMessages)                 │
├─────────────────────────────────────────────────────────────────┤
│                     Imoveis.Infrastructure                      │
│          DbContext · EF Configs · SeedData · ViaCepClient       │
│                    Logging (InfraLogMessages)                    │
├─────────────────────────────────────────────────────────────────┤
│                        Imoveis.Domain                           │
│         Entidades · Value Objects · Interfaces · Enums          │
└─────────────────────────────────────────────────────────────────┘
```

### Dependências entre camadas

```
API → Application → Infrastructure → Domain
```

- **Domain** não depende de nada
- **Infrastructure** depende apenas de Domain
- **Application** depende de Domain e Infrastructure (acesso direto ao DbContext — sem repositório intermediário desnecessário)
- **API** depende de Application e Infrastructure (DI, seed e TokenService)

---

## Estrutura de pastas

```
src/
├── Imoveis.Domain/
│   ├── Entities/
│   │   ├── Imovel.cs                    # Entidade com factory method e soft delete
│   │   └── Lead.cs
│   ├── Enums/
│   │   └── TipoImovel.cs                # Casa · Apartamento · Terreno · Comercial
│   ├── Interfaces/
│   │   └── ICepService.cs               # Contrato do serviço de CEP (evita dependência circular)
│   └── ValueObjects/
│       ├── Endereco.cs                  # Value object com factory method
│       └── ConsultarCepResultado.cs     # Discriminated union: Encontrado | NaoEncontrado | ServicoIndisponivel
│
├── Imoveis.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── SeedData.cs
│   │   └── Configurations/
│   │       ├── ImovelConfiguration.cs   # OwnsOne(Endereco) → colunas na mesma tabela
│   │       └── LeadConfiguration.cs
│   ├── HealthChecks/
│   │   └── ViaCepHealthCheck.cs         # Probe ativo do ViaCEP para /health/cep
│   ├── Integrations/ViaCep/
│   │   ├── ViaCepClient.cs              # Typed HttpClient com resiliência (Polly)
│   │   ├── ViaCepOptions.cs             # BaseUrl · Timeout · MaxTentativas
│   │   └── ViaCepEnderecoDto.cs
│   ├── Logging/
│   │   └── InfraLogMessages.cs          # [LoggerMessage] source generator
│   └── DependencyInjection.cs
│
├── Imoveis.Application/
│   ├── Common/
│   │   ├── Result.cs                    # Result<T>: Ok(dado) | Falha(mensagem)
│   │   ├── ImovelCacheKeys.cs
│   │   ├── ListaCacheInvalidador.cs     # Invalida listas via CancellationChangeToken
│   │   └── Logging/
│   │       └── AppLogMessages.cs        # [LoggerMessage] source generator
│   └── Features/
│       ├── Imoveis/
│       │   ├── CadastrarImovel/
│       │   ├── ConsultarImoveis/
│       │   ├── ObterImovelPorId/
│       │   ├── AtualizarImovel/
│       │   └── RemoverImovel/
│       └── Leads/
│           └── RegistrarLead/
│
└── Imoveis.API/
    ├── Auth/
    │   ├── JwtOptions.cs                # Options: Secret · Issuer · Audience · ExpiracaoMinutos
    │   └── TokenService.cs              # Gera JWT com claims (sub, name, role, jti)
    ├── Features/
    │   ├── Auth/
    │   │   └── AuthController.cs        # POST /api/auth/login
    │   ├── Imoveis/
    │   │   └── ImovelController.cs      # CRUD com [Authorize] e [EnableRateLimiting]
    │   └── Leads/
    │       └── LeadController.cs
    ├── Middleware/
    │   └── GlobalExceptionHandler.cs    # IExceptionHandler → ProblemDetails padronizado
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## Como rodar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/imoveis-api.git
cd imoveis-api
```

### 2. Restaure os pacotes

```bash
dotnet restore
```

### 3. Execute a API

```bash
dotnet run --project src/Imoveis.API
```

A API sobe em `https://localhost:7000` (ou a porta indicada no terminal).

### 4. Acesse o Swagger

```
https://localhost:{porta}/swagger
```

> Por padrão o projeto usa **banco de dados InMemory** com dados de seed já carregados. Nenhuma configuração adicional é necessária para rodar localmente.

---

## Autenticação

A API usa **JWT Bearer** para proteger os endpoints de escrita. Os endpoints de leitura são públicos.

### Obter um token

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@imoveis.com",
  "senha": "Admin@123"
}
```

**Resposta `200 OK`:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiraEm": "2024-01-15T11:00:00Z"
}
```

### Credenciais de teste

| Usuário | Email | Senha | Role |
|---|---|---|---|
| Administrador | `admin@imoveis.com` | `Admin@123` | `Admin` |
| Corretor | `corretor@imoveis.com` | `Corretor@123` | `Corretor` |

> **Nota:** O sistema de usuários é mockado para fins de demonstração. Em produção, use ASP.NET Core Identity com banco de dados e hash de senha (BCrypt / Argon2).

### Usando o token

Adicione o header em todas as requisições autenticadas:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### No Swagger

1. Clique em **Authorize** (cadeado)
2. Informe: `Bearer {seu_token}`
3. Clique em **Authorize** — todos os endpoints protegidos ficam acessíveis

### Claims do token

| Claim | Valor |
|---|---|
| `sub` | ID do usuário |
| `name` | Nome do usuário |
| `role` | Role (`Admin` ou `Corretor`) |
| `jti` | UUID único por token |
| `iat` | Unix timestamp de emissão |

---

## Endpoints

### Autenticação

| Método | Rota | Descrição | Auth | Rate Limit |
|---|---|---|---|---|
| `POST` | `/api/auth/login` | Gera um JWT | Público | 5 req/min por IP |

### Imóveis

| Método | Rota | Descrição | Auth | Cache | Rate Limit |
|---|---|---|---|---|---|
| `GET` | `/api/imoveis` | Lista com filtros e paginação | Público | 2 min | 60 req/min |
| `GET` | `/api/imoveis/{id}` | Retorna imóvel pelo Id | Público | 10 min | 60 req/min |
| `POST` | `/api/imoveis` | Cadastra novo imóvel | **JWT** | Invalida listas | 20 req/min |
| `PUT` | `/api/imoveis/{id}` | Atualiza imóvel | **JWT** | Invalida item + listas | 20 req/min |
| `DELETE` | `/api/imoveis/{id}` | Remove imóvel (soft delete) | **JWT** | Invalida item + listas | 20 req/min |

### Leads

| Método | Rota | Descrição | Auth | Rate Limit |
|---|---|---|---|---|
| `POST` | `/api/leads` | Registra interesse em um imóvel | Público | — |

### Health Checks

| Rota | Descrição |
|---|---|
| `/health` | Todos os checks |
| `/health/cep` | Apenas ViaCEP |

---

## Exemplos de uso

### 1. Login e obtenção do token

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@imoveis.com",
  "senha": "Admin@123"
}
```

---

### 2. Cadastrar imóvel (autenticado)

O endereço é enriquecido automaticamente via CEP — informe apenas o CEP, número e complemento.

```http
POST /api/imoveis
Content-Type: application/json
Authorization: Bearer {token}

{
  "titulo": "Apartamento Vista Mar",
  "descricao": "3 suítes, varanda gourmet com vista para o mar.",
  "tipo": 2,
  "cep": "88015600",
  "numero": "450",
  "complemento": "Apto 302",
  "preco": 850000,
  "areaM2": 90,
  "quartos": 3
}
```

**Resposta `201 Created`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

### 3. Consultar imóveis com filtros

```http
GET /api/imoveis?cidade=São Paulo&tipo=1&precoMax=1500000&pagina=1&tamanhoPagina=10
```

**Resposta `200 OK`:**
```json
{
  "imoveis": [
    {
      "id": "...",
      "titulo": "Casa em Condomínio Fechado",
      "tipo": 1,
      "cep": "01310100",
      "cidade": "São Paulo",
      "estado": "SP",
      "preco": 1200000,
      "areaM2": 250,
      "quartos": 4,
      "criadoEm": "2024-01-15T10:00:00Z"
    }
  ],
  "total": 1,
  "pagina": 1,
  "tamanhoPagina": 10
}
```

**Filtros disponíveis:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `cidade` | `string` | Filtra por nome da cidade (parcial, case-insensitive) |
| `tipo` | `int` | `1` Casa · `2` Apartamento · `3` Terreno · `4` Comercial |
| `precoMin` | `decimal` | Preço mínimo |
| `precoMax` | `decimal` | Preço máximo |
| `pagina` | `int` | Página atual (padrão: `1`) |
| `tamanhoPagina` | `int` | Itens por página (padrão: `20`) |

---

### 4. Obter imóvel por Id

```http
GET /api/imoveis/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Resposta `200 OK`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "titulo": "Apartamento Vista Mar",
  "descricao": "3 suítes, varanda gourmet.",
  "tipo": 2,
  "cep": "88015600",
  "logradouro": "Avenida Beira-Mar Norte",
  "bairro": "Centro",
  "cidade": "Florianópolis",
  "estado": "SC",
  "numero": "450",
  "complemento": "Apto 302",
  "preco": 850000,
  "areaM2": 90,
  "quartos": 3,
  "criadoEm": "2024-01-15T10:00:00Z",
  "ativo": true
}
```

---

### 5. Atualizar imóvel (autenticado)

```http
PUT /api/imoveis/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json
Authorization: Bearer {token}

{
  "titulo": "Apartamento Vista Mar - Reformado",
  "descricao": "Recém reformado, 3 suítes amplas.",
  "tipo": 2,
  "cep": "88015600",
  "numero": "450",
  "complemento": "Apto 302",
  "preco": 920000,
  "areaM2": 90,
  "quartos": 3
}
```

**Resposta `204 No Content`**

---

### 6. Remover imóvel (autenticado)

```http
DELETE /api/imoveis/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer {token}
```

**Resposta `204 No Content`**

> O DELETE realiza **soft delete** — o imóvel é marcado como `Ativo = false` e não aparece mais nas consultas, mas o registro e os leads associados são preservados no banco.

---

### 7. Registrar lead

```http
POST /api/leads
Content-Type: application/json

{
  "imovelId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "João Silva",
  "email": "joao@email.com",
  "telefone": "11999999999",
  "mensagem": "Tenho interesse, gostaria de agendar uma visita."
}
```

**Resposta `201 Created`:**
```json
{
  "leadId": "7cb12a33-1234-4abc-9def-000000000001"
}
```

---

## Rate Limiting

O projeto usa o **Rate Limiter nativo do .NET 8** (`Microsoft.AspNetCore.RateLimiting`) sem dependências externas. As requisições são particionadas por IP de origem.

| Política | Janela | Limite | Aplicado em |
|---|---|---|---|
| `leitura` | 1 min (fixed window) | 60 req/IP | `GET /api/imoveis` e `GET /api/imoveis/{id}` |
| `escrita` | 1 min (fixed window) | 20 req/IP | `POST`, `PUT`, `DELETE /api/imoveis` |
| `autenticacao` | 1 min (sliding window) | 5 req/IP | `POST /api/auth/login` |

Requisições que excedem o limite recebem **HTTP 429 Too Many Requests**.

> Em produção com load balancer ou proxy reverso (nginx, Cloudflare), configure o rate limiter para usar o header `X-Forwarded-For` em vez de `RemoteIpAddress`.

---

## Cache

O projeto utiliza `IMemoryCache` (nativo do .NET, sem dependência externa) com invalidação automática por evento.

### Comportamento por operação

| Operação | Comportamento |
|---|---|
| `GET /imoveis/{id}` | Hit → retorna do cache. Miss → busca no banco e armazena por **10 min** |
| `GET /imoveis` | Hit → retorna do cache. Miss → busca no banco e armazena por **2 min** |
| `POST /imoveis` | Salva no banco → invalida todas as entradas de lista |
| `PUT /imoveis/{id}` | Salva no banco → remove entrada do item + invalida todas as listas |
| `DELETE /imoveis/{id}` | Salva no banco → remove entrada do item + invalida todas as listas |

### Estratégia de invalidação das listas

Como a consulta aceita múltiplos filtros, cada combinação gera uma chave de cache distinta. Para invalidar todas de uma vez, usamos `CancellationChangeToken`:

1. Cada entrada de lista é armazenada com um token vinculado a um `CancellationTokenSource` gerenciado pelo `ListaCacheInvalidador`
2. `ListaCacheInvalidador.Invalidar()` cancela o token — todas as entradas expiram imediatamente
3. Um novo `CancellationTokenSource` é criado atomicamente via `Interlocked.Exchange`

```
POST / PUT / DELETE
      │
      ▼
ListaCacheInvalidador.Invalidar()
      │
      ├─► CancellationTokenSource.Cancel()   → expira todas as listas em cache
      └─► Interlocked.Exchange(ref _cts, new CancellationTokenSource())
```

---

## Logging

O projeto usa `ILogger<T>` nativo com **`[LoggerMessage]` source generator** em todos os handlers e no cliente ViaCEP.

### Por que [LoggerMessage]?

O atributo `[LoggerMessage]` gera código em tempo de compilação que elimina:
- Boxing de parâmetros value type
- Alocação de string interpolada no hot path
- Verificação manual de `IsEnabled`

### Organização

| Arquivo | Responsabilidade |
|---|---|
| `Application/Common/Logging/AppLogMessages.cs` | Mensagens dos handlers (cadastro, atualização, cache hits, etc.) |
| `Infrastructure/Logging/InfraLogMessages.cs` | Mensagens do ViaCepClient (consultas, timeouts, erros HTTP) |

### Níveis por namespace

| Namespace | Produção | Desenvolvimento |
|---|---|---|
| `Imoveis.*` | `Information` | `Debug` (inclui cache hits) |
| `Microsoft.AspNetCore` | `Warning` | `Information` |
| `Microsoft.EntityFrameworkCore.Database.Command` | `Warning` | `Information` (exibe SQLs) |
| `System.Net.Http` | `Warning` | `Information` (exibe chamadas HTTP) |

### Convenção de níveis

| Nível | Quando usar |
|---|---|
| `LogDebug` | Cache hits (frequentes, sem valor em produção) |
| `LogInformation` | Operações concluídas com sucesso |
| `LogWarning` | Erros esperados: not found, CEP inválido, imóvel já inativo |
| `LogError` | Exceções inesperadas capturadas pelo `GlobalExceptionHandler` |

---

## Integração ViaCEP

Os endpoints de cadastro e atualização de imóvel consultam o [ViaCEP](https://viacep.com.br) para enriquecer o endereço automaticamente a partir do CEP informado. Você não precisa enviar logradouro, bairro, cidade ou estado — apenas CEP, número e complemento.

### Fluxo

```
POST /api/imoveis
  │
  ├─► Validação do command (FluentValidation)
  │
  ├─► ICepService.ConsultarAsync(cep)
  │       ├── Encontrado      → cria Endereco com dados do ViaCEP
  │       ├── NaoEncontrado   → retorna 400 "CEP não encontrado"
  │       └── ServicoIndisponivel → retorna 400 "Serviço temporariamente indisponível"
  │
  └─► Salva Imovel com Endereco enriquecido
```

### Resiliência

O `ViaCepClient` usa `AddStandardResilienceHandler` (Polly v8 via `Microsoft.Extensions.Http.Resilience`), que configura automaticamente:

- **Retry** com backoff exponencial e jitter
- **Circuit breaker** — para de tentar após falhas consecutivas
- **Timeout por tentativa** — evita requisições travadas

### Configuração

```json
"Integracoes": {
  "ViaCep": {
    "BaseUrl": "https://viacep.com.br",
    "TimeoutSegundos": 5,
    "MaxTentativas": 3
  }
}
```

---

## Health Checks

| Endpoint | Descrição |
|---|---|
| `GET /health` | Todos os checks registrados |
| `GET /health/cep` | Apenas a integração ViaCEP (tag `external`) |

**Exemplo de resposta:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "viacep",
      "status": "Healthy",
      "descricao": "ViaCEP respondendo normalmente",
      "duracao_ms": 142
    }
  ]
}
```

O `ViaCepHealthCheck` faz uma consulta ao CEP `01310100` (Av. Paulista) como probe ativo a cada ciclo de health check.

---

## Configuração do banco de dados

### InMemory (padrão — desenvolvimento)

Nenhuma configuração necessária. Os dados de seed são carregados automaticamente ao subir a aplicação.

### SQL Server (produção)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ImoveisDb;Trusted_Connection=True;"
  }
}
```

```bash
dotnet ef migrations add InitialCreate --project src/Imoveis.Infrastructure --startup-project src/Imoveis.API
dotnet ef database update --project src/Imoveis.Infrastructure --startup-project src/Imoveis.API
```

---

## Variáveis de ambiente

Todas as configurações sensíveis podem (e devem) ser sobrescritas via variáveis de ambiente. O .NET mapeia `__` para `:` na hierarquia de configuração.

| Variável | Equivalente em appsettings | Descrição |
|---|---|---|
| `Jwt__Secret` | `Jwt.Secret` | Segredo para assinar o JWT — **obrigatório em produção** |
| `Jwt__ExpiracaoMinutos` | `Jwt.ExpiracaoMinutos` | Expiração do token (padrão: 60) |
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings.DefaultConnection` | String de conexão do banco |
| `Integracoes__ViaCep__BaseUrl` | `Integracoes.ViaCep.BaseUrl` | URL base do ViaCEP |
| `Integracoes__ViaCep__MaxTentativas` | `Integracoes.ViaCep.MaxTentativas` | Tentativas de retry |

**Exemplo com Docker:**
```bash
docker run -e Jwt__Secret="seu-segredo-real-forte-aqui" \
           -e ConnectionStrings__DefaultConnection="Server=db;..." \
           imoveis-api
```

> O segredo JWT deve ter no mínimo 32 caracteres (256 bits) para HMAC-SHA256.

---

## Decisões de design

### Sem repositório genérico
Os handlers acessam o `AppDbContext` diretamente. O EF Core já é a abstração sobre o banco — uma interface `IImovelRepository` não agregaria nada neste contexto.

### Sem MediatR
Os handlers são classes simples registradas no DI. MediatR adiciona indireção sem benefício real neste porte. Se o projeto crescer e precisar de pipelines transversais, considere adicioná-lo.

### Result\<T\> em vez de exceções
Fluxos esperados — validação falhou, imóvel não encontrado, CEP inválido — não são exceções. `Result<T>` força o tratamento explícito pelo código chamador, tornando o fluxo de erro previsível.

### Exceções inesperadas centralizadas
O `GlobalExceptionHandler` (implementa `IExceptionHandler` do .NET 8) captura qualquer exceção não tratada, loga com `LogError` e retorna `ProblemDetails` padronizado. Cancelamentos do cliente (`OperationCanceledException`) são tratados como `LogInformation` — não são erros.

### Cache sem biblioteca externa
`IMemoryCache` é suficiente para cache local em processo. Se a aplicação escalar para múltiplas instâncias, substitua por `IDistributedCache` com Redis — a interface dos handlers não muda.

### Soft delete no DELETE
O endpoint `DELETE` desativa o imóvel (`Ativo = false`) em vez de removê-lo. Leads associados são preservados e o histórico fica intacto.

### JWT sem sistema de identidade completo
O foco é demonstrar o padrão de autenticação com JWT. Usuários são mockados intencionalmente — em produção, use ASP.NET Core Identity com banco de dados e hash de senha (BCrypt / Argon2).

### ICepService no Domain (não em Application)
Para evitar dependência circular — Infrastructure implementa `ICepService` e precisaria referenciar Application, mas Application já referencia Infrastructure. Mover a interface para Domain (sem dependências) resolve o problema sem alterar a arquitetura.

### [LoggerMessage] source generator
Mensagens de log são definidas como métodos `partial` em classes estáticas. O compilador gera código otimizado: sem boxing, sem alocação de string interpolada no hot path. Isso é especialmente relevante para cache hits em endpoints de alta frequência.

### Rate Limiting nativo (.NET 8)
Sem dependências externas — `Microsoft.AspNetCore.RateLimiting` está incluído no SDK. Políticas distintas por tipo de endpoint (leitura pública vs. escrita autenticada vs. autenticação) com particionamento por IP.

### DTOs específicos por caso de uso
Cada feature tem seu próprio DTO. Isso evita acoplamento entre casos de uso e permite que cada um evolua independentemente.
