# Evidências — SOA & Web Services

Prints da aplicação funcionando, organizados pelos critérios de avaliação.

## 1. Estruturação do domínio (3,0)
| Arquivo | O que mostra |
|---|---|
| [`01-estrutura-pastas.png`](01-estrutura-pastas.png) | Projeto no VS Code: Domain, Data, Dtos, Controllers, Migrations |
| [`02-ocorrencia-regras.png`](02-ocorrencia-regras.png) | `Domain/Ocorrencia.cs` com as regras de negócio (IniciarAtendimento, Resolver, Cancelar) |

## 2. Conexão com banco de dados (3,0)
| Arquivo | O que mostra |
|---|---|
| [`03-swagger-bairros.png`](03-swagger-bairros.png) | `GET /api/bairros` devolvendo os dados gravados no Oracle da FIAP |
| [`09b-api-rodando.png`](09b-api-rodando.png) | API no ar (`dotnet run`) escutando em `http://localhost:5080` |

## 3. Migrações (2,5)
| Arquivo | O que mostra |
|---|---|
| [`06-migration-initialcreate.png`](06-migration-initialcreate.png) | `dotnet ef migrations add InitialCreate` |
| [`07-database-update-initial.png`](07-database-update-initial.png) | `dotnet ef database update` criando as tabelas `ZB_*` no Oracle |
| [`07b-database-update-done.png`](07b-database-update-done.png) | Fim do `database update`: dados iniciais (bairros e categorias), índices e registro em `__EFMigrationsHistory` |
| [`08-migration-descricaoopcional.png`](08-migration-descricaoopcional.png) | Segunda migration: `ALTER TABLE ... MODIFY "Descricao" NULL` |

As três migrations do projeto estão em [`../ZelaBairro.Api/Migrations`](../ZelaBairro.Api/Migrations): `InitialCreate`, `DescricaoOpcional` e `BairroTatuape` (inclui o bairro piloto Tatuapé).

## 4. Testes manuais — Insomnia (1,5)
Coleção completa, com todas as requisições do fluxo feliz e dos erros: [`../insomnia/zelabairro-insomnia.json`](../insomnia/zelabairro-insomnia.json)

| Arquivo | Requisição | Resultado |
|---|---|---|
| [`10-obter-morador.png`](10-obter-morador.png) | `GET /api/moradores/1` | 200 com os dados do morador |
| [`16-resolver-e-foto.png`](16-resolver-e-foto.png) | `PATCH` status Resolvida + `POST` foto | 200 |
| [`18-erro-titulo-vazio.png`](18-erro-titulo-vazio.png) | Título vazio | 400 com mensagem de validação |
| [`19-erro-categoria.png`](19-erro-categoria.png) | Categoria inexistente | 400 |
| [`21-erro-404.png`](21-erro-404.png) | Ocorrência inexistente | 404 |
| [`22-erro-sql-injection.png`](22-erro-sql-injection.png) | `bairroId=1 OR 1=1` | 400 (entrada rejeitada; consultas parametrizadas pelo EF Core) |

## 5. Integração com o resto da solução
A mesma API é usada pelo app mobile, pelo terminal da subprefeitura (C#), pelo ESP32 do Wokwi e pelo painel web hospedado no IIS. Os prints de [`../../infra/evidencias`](../../infra/evidencias) mostram a API publicada no Windows Server com `"banco":"conectado"`, e o [README do IoT](../../iot/README.md) mostra o ESP32 abrindo uma ocorrência (`HTTP 201`).
