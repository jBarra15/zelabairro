# ZelaBairro — API (SOA & Web Services)

Backend REST da CitizenTech **ZelaBairro**: moradores registram problemas do espaço público (lixo, iluminação, buracos, calçadas, árvores), a subprefeitura faz a triagem e todos acompanham a resolução.

**Integrante:** Joel — RM 560796 — 2ESPS

## Tecnologias
- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8 + Oracle (`Oracle.EntityFrameworkCore`)
- Migrations do EF Core
- Swagger (ambiente de desenvolvimento) e Insomnia para testes manuais

## Arquitetura
```
ZelaBairro.Api/
├── Domain/        Entidades com regras de negócio (Bairro, Categoria, Morador, Ocorrencia)
├── Data/          AppDbContext + mapeamento Fluent API (tabelas ZB_*) + dados iniciais
├── Dtos/          Contratos de entrada/saída da API (com validação)
├── Controllers/   Endpoints REST
└── Migrations/    InitialCreate (cria as tabelas e os dados iniciais) e DescricaoOpcional (ajuste de coluna)
```
As regras ficam no domínio, não no controller. Exemplo: uma ocorrência só pode ser **Resolvida** se estiver **EmAtendimento**; ocorrências do app exigem morador identificado; ocorrências do sensor ESP32 não.

### Modelo de dados
| Tabela | Descrição |
|---|---|
| `ZB_BAIRRO` | Bairros atendidos (dados iniciais via migration) |
| `ZB_CATEGORIA` | Tipos de problema + prazo de atendimento (SLA) |
| `ZB_MORADOR` | Morador identificado no app (e-mail único) |
| `ZB_OCORRENCIA` | O problema: local, foto, status, prioridade, origem (App/Sensor/Terminal), confirmações |

## Como executar

**1. Pré-requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download) e a ferramenta do EF:
```bash
dotnet tool install --global dotnet-ef
```

**2. Configurar o banco (sem colocar senha no código):**
```bash
cd ZelaBairro.Api
dotnet user-secrets set "ConnectionStrings:Oracle" "User Id=rm560796;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL"
```
Em servidor (IIS), use a variável de ambiente `ConnectionStrings__Oracle`.

**3. Aplicar as migrations** (as migrations já estão no repositório):
```bash
dotnet ef database update
```

| Migration | O que faz |
|---|---|
| `InitialCreate` | Cria `ZB_BAIRRO`, `ZB_CATEGORIA`, `ZB_MORADOR`, `ZB_OCORRENCIA`, índices, chaves estrangeiras e insere bairros e categorias |
| `DescricaoOpcional` | Torna `ZB_OCORRENCIA.Descricao` opcional. O Oracle grava texto vazio como `NULL`, o que fazia ocorrências sem descrição (balcão) falharem com erro 500 |

**4. Rodar:**
```bash
dotnet run
```
Swagger: http://localhost:5080/swagger

Para o celular (app Expo) acessar a API na mesma rede Wi-Fi:
```bash
dotnet run --urls http://0.0.0.0:5080
```

**5. Testes manuais:** importe `insomnia/zelabairro-insomnia.json` no Insomnia (Import → From File).

## Endpoints
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/bairros` | Lista bairros |
| GET | `/api/categorias` | Lista categorias e prazos |
| POST | `/api/moradores/identificar` | Cadastra ou atualiza morador pelo e-mail |
| GET | `/api/moradores/{id}` | Obtém morador |
| GET | `/api/ocorrencias` | Lista (filtros: `bairroId`, `categoriaId`, `moradorId`, `status`) |
| GET | `/api/ocorrencias/{id}` | Detalhe |
| POST | `/api/ocorrencias` | Cria ocorrência |
| PATCH | `/api/ocorrencias/{id}/status` | `EmAtendimento`, `Resolvida` ou `Cancelada` |
| PATCH | `/api/ocorrencias/{id}/prioridade` | `Baixa`, `Media`, `Alta` |
| POST | `/api/ocorrencias/{id}/confirmar` | Vizinho confirma o problema |
| POST | `/api/ocorrencias/{id}/foto` | Upload de foto (JPG/PNG, até 5 MB) |
| GET | `/api/ocorrencias/{id}/foto` | Baixa a foto |
| DELETE | `/api/ocorrencias/{id}` | Exclui |
| GET | `/api/ocorrencias/resumo` | Indicador de impacto (% resolvidas no prazo, atrasadas) |

Erros de regra de negócio retornam **400** no formato `{ "erro": "mensagem" }`.

## Decisões técnicas
- **Oracle 19 da FIAP:** o driver `Oracle.EntityFrameworkCore` 8 gera SQL no formato do Oracle 23 por padrão. Em `Program.cs`, `UseOracleSQLCompatibility(DatabaseVersion19)` corrige isso (sem o ajuste, consultas com `AnyAsync` davam erro 500).
- **Enums como texto** no banco (`Status`, `Prioridade`, `Origem`): legíveis no SQL Developer e seguros se a ordem do enum mudar.
- **Senha fora do código:** `user-secrets` em desenvolvimento e variável de ambiente `ConnectionStrings__Oracle` no servidor.
- **Upload seguro:** extensão e *magic bytes* conferidos, nome gerado pela API e bloqueio de `../` (path traversal).
- **Erros sem vazamento:** exceções inesperadas viram 500 com mensagem genérica; o detalhe fica só no log do servidor.

## Quem consome esta API
| Cliente | Uso |
|---|---|
| App mobile (`mobile/`) | Morador identifica-se, registra ocorrência com foto e localização, confirma problemas |
| Terminal WinForms (`desktop/`) | Subprefeitura prioriza, atende e resolve; cadastra pelo balcão (origem `Terminal`) |
| ESP32 / Wokwi (`iot/`) | Lixeira inteligente abre ocorrência sozinha (origem `Sensor`) |

## Evidências
Ver pasta [`evidencias/`](evidencias/).
