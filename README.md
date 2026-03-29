# Imoveis API

API REST para gerenciamento de imóveis, construída com **ASP.NET Core 8** seguindo uma arquitetura modular e pragmática — sem overengineering, focada em clareza e manutenibilidade.

> Projeto de referência demonstrando Clean Architecture simplificada com organização por feature (vertical slice).

---

## Sumário

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Como rodar](#como-rodar)
- [Endpoints](#endpoints)
- [Exemplos de uso](#exemplos-de-uso)
- [Cache](#cache)
- [Configuração do banco de dados](#configuração-do-banco-de-dados)
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
| [IMemoryCache](https://docs.microsoft.com/aspnet/core/performance/caching/memory) | Cache em memória (nativo do .NET) |
| [FluentValidation](https://docs.fluentvalidation.net/) | Validação de entrada |
| [Swagger / Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) | Documentação da API |

---

## Arquitetura

O projeto segue uma **Clean Architecture simplificada** com separação em 4 camadas e organização interna **por feature**:

```
┌─────────────────────────────────────────────┐
│                  Imoveis.API                │  ← Controllers, Program.cs
├─────────────────────────────────────────────┤
│             Imoveis.Application             │  ← Handlers, Commands, Queries, Validators, DTOs, Cache
├─────────────────────────────────────────────┤
│             Imoveis.Infrastructure          │  ← DbContext, Configurações EF, SeedData
├─────────────────────────────────────────────┤
│               Imoveis.Domain                │  ← Entidades, Enums (sem dependências externas)
└─────────────────────────────────────────────┘
```

### Dependências entre camadas

```
API → Application → Infrastructure → Domain
                  ↗
         Domain ──
```

- **Domain** não depende de nada
- **Infrastructure** depende apenas de Domain
- **Application** depende de Domain e Infrastructure (acesso direto ao DbContext — sem repositório intermediário desnecessário)
- **API** depende de Application e Infrastructure (para DI e seed)

---

## Estrutura de pastas

```
src/
├── Imoveis.Domain/
│   ├── Entities/
│   │   ├── Imovel.cs               # Entidade principal com factory method
│   │   └── Lead.cs                 # Entidade de lead
│   └── Enums/
│       └── TipoImovel.cs           # Casa, Apartamento, Terreno, Comercial
│
├── Imoveis.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── SeedData.cs
│   │   └── Configurations/
│   │       ├── ImovelConfiguration.cs
│   │       └── LeadConfiguration.cs
│   └── DependencyInjection.cs      # Extensão para registrar infraestrutura
│
├── Imoveis.Application/
│   ├── Common/
│   │   ├── Result.cs               # Result<T> para tratamento de erros sem exceções
│   │   ├── ImovelCacheKeys.cs      # Centraliza as chaves de cache
│   │   └── ListaCacheInvalidador.cs # Invalida todas as listas via CancellationToken
│   └── Features/
│       ├── Imoveis/
│       │   ├── CadastrarImovel/
│       │   │   ├── CadastrarImovelCommand.cs
│       │   │   ├── CadastrarImovelHandler.cs
│       │   │   └── CadastrarImovelValidator.cs
│       │   ├── ConsultarImoveis/
│       │   │   ├── ConsultarImoveisQuery.cs
│       │   │   ├── ConsultarImoveisHandler.cs  # Lê/escreve no cache
│       │   │   └── ImovelResumoDto.cs
│       │   ├── ObterImovelPorId/
│       │   │   ├── ObterImovelPorIdQuery.cs
│       │   │   ├── ObterImovelPorIdDto.cs
│       │   │   └── ObterImovelPorIdHandler.cs  # Lê/escreve no cache
│       │   ├── AtualizarImovel/
│       │   │   ├── AtualizarImovelCommand.cs
│       │   │   ├── AtualizarImovelHandler.cs   # Invalida cache ao salvar
│       │   │   └── AtualizarImovelValidator.cs
│       │   └── RemoverImovel/
│       │       ├── RemoverImovelCommand.cs
│       │       └── RemoverImovelHandler.cs     # Invalida cache ao salvar
│       └── Leads/
│           └── RegistrarLead/
│               ├── RegistrarLeadCommand.cs
│               ├── RegistrarLeadHandler.cs
│               └── RegistrarLeadValidator.cs
│
└── Imoveis.API/
    ├── Features/
    │   ├── Imoveis/
    │   │   └── ImovelController.cs
    │   └── Leads/
    │       └── LeadController.cs
    ├── Program.cs
    └── appsettings.json
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

A API sobe por padrão em `https://localhost:7000` (ou a porta indicada no terminal).

### 4. Acesse o Swagger

```
https://localhost:{porta}/swagger
```

> Por padrão, o projeto usa **banco de dados InMemory** com dados de seed já carregados. Nenhuma configuração adicional é necessária para rodar localmente.

---

## Endpoints

### Imóveis

| Método | Rota | Descrição | Cache |
|---|---|---|---|
| `POST` | `/api/imoveis` | Cadastra um novo imóvel | Invalida listas |
| `GET` | `/api/imoveis` | Lista imóveis com filtros e paginação | 2 min |
| `GET` | `/api/imoveis/{id}` | Retorna um imóvel pelo Id | 10 min |
| `PUT` | `/api/imoveis/{id}` | Atualiza os dados de um imóvel | Invalida item + listas |
| `DELETE` | `/api/imoveis/{id}` | Remove um imóvel (soft delete) | Invalida item + listas |

### Leads

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/leads` | Registra interesse de um lead em um imóvel |

---

## Exemplos de uso

### Cadastrar imóvel

```http
POST /api/imoveis
Content-Type: application/json

{
  "titulo": "Apartamento Vista Mar",
  "descricao": "3 suítes, varanda gourmet com vista para o mar.",
  "tipo": 2,
  "cidade": "Florianópolis",
  "estado": "SC",
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

### Consultar imóveis com filtros

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
| `cidade` | `string` | Filtra por nome da cidade (parcial) |
| `tipo` | `int` | `1` Casa · `2` Apartamento · `3` Terreno · `4` Comercial |
| `precoMin` | `decimal` | Preço mínimo |
| `precoMax` | `decimal` | Preço máximo |
| `pagina` | `int` | Página atual (padrão: `1`) |
| `tamanhoPagina` | `int` | Itens por página (padrão: `20`) |

---

### Obter imóvel por Id

```http
GET /api/imoveis/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

### Atualizar imóvel

```http
PUT /api/imoveis/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "titulo": "Apartamento Vista Mar - Reformado",
  "descricao": "Recém reformado, 3 suítes amplas.",
  "tipo": 2,
  "cidade": "Florianópolis",
  "estado": "SC",
  "preco": 920000,
  "areaM2": 90,
  "quartos": 3
}
```

**Resposta `204 No Content`**

---

### Remover imóvel

```http
DELETE /api/imoveis/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Resposta `204 No Content`**

> O DELETE realiza **soft delete** — o imóvel é marcado como inativo (`Ativo = false`) e não aparece mais nas consultas, mas o registro é preservado no banco junto com os leads associados.

---

### Registrar lead

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

## Cache

O projeto utiliza `IMemoryCache` (nativo do .NET, sem dependência externa) com invalidação automática por evento.

### Comportamento por operação

| Operação | Comportamento |
|---|---|
| `GET /imoveis/{id}` | Hit → retorna do cache. Miss → busca no banco e armazena por **10 min** |
| `GET /imoveis` | Hit → retorna do cache. Miss → busca no banco e armazena por **2 min** |
| `POST /imoveis` | Salva no banco → invalida todas as entradas de lista |
| `PUT /imoveis/{id}` | Salva no banco → remove entrada do item específico + invalida listas |
| `DELETE /imoveis/{id}` | Salva no banco → remove entrada do item específico + invalida listas |

### Estratégia de invalidação das listas

Como a consulta aceita múltiplos filtros, cada combinação diferente gera uma chave de cache distinta. Para invalidar todas essas entradas de uma vez sem iterar o cache, usamos o padrão `CancellationChangeToken`:

1. Cada entrada de lista é armazenada com um token vinculado a um `CancellationTokenSource` compartilhado (gerenciado pelo `ListaCacheInvalidador`)
2. Ao chamar `ListaCacheInvalidador.Invalidar()`, o token é cancelado — todas as entradas que dependem dele expiram imediatamente
3. Um novo `CancellationTokenSource` é criado atomicamente via `Interlocked.Exchange` para as próximas entradas

```
POST/PUT/DELETE
      │
      ▼
ListaCacheInvalidador.Invalidar()
      │
      ├─► CancellationTokenSource.Cancel()  ──► expira todas as listas em cache
      │
      └─► Interlocked.Exchange(ref _cts, new CancellationTokenSource())
```

---

## Configuração do banco de dados

### InMemory (padrão — desenvolvimento)

Nenhuma configuração necessária. Basta rodar a aplicação e os dados de seed são carregados automaticamente.

### SQL Server (produção)

Adicione a connection string no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ImoveisDb;Trusted_Connection=True;"
  }
}
```

Para criar o banco e aplicar as migrations:

```bash
dotnet ef migrations add InitialCreate --project src/Imoveis.Infrastructure --startup-project src/Imoveis.API
dotnet ef database update --project src/Imoveis.Infrastructure --startup-project src/Imoveis.API
```

---

## Decisões de design

### Sem repositório genérico
Os handlers acessam o `AppDbContext` diretamente. O EF Core já é a abstração sobre o banco — uma interface `IImovelRepository` não agregaria nada neste contexto e tornaria o código mais difícil de seguir.

### Sem MediatR
Os handlers são classes simples registradas no DI container. MediatR adiciona indireção sem benefício real num projeto deste porte. Se o projeto crescer e precisar de pipelines transversais (logging, cache, autorização), considere adicioná-lo.

### Result&lt;T&gt; em vez de exceções
Fluxos esperados — validação falhou, imóvel não encontrado — não são exceções. `Result<T>` força o código chamador a tratar esses casos explicitamente, tornando o fluxo de erro previsível e visível.

### Cache sem biblioteca externa
`IMemoryCache` é suficiente para cache local em processo. Se a aplicação escalar para múltiplas instâncias, troque por `IDistributedCache` com Redis — a interface dos handlers não muda, apenas a implementação registrada no DI.

### Soft delete no DELETE
O endpoint `DELETE` desativa o imóvel (`Ativo = false`) em vez de removê-lo fisicamente. Leads associados são preservados e o histórico fica intacto.

### DTOs específicos por caso de uso
Cada feature tem seu próprio DTO em vez de um DTO genérico de `Imovel`. Isso evita acoplamento entre casos de uso e permite que cada um evolua independentemente.
