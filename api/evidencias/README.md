# Evidências — SOA & Web Services

Prints da aplicação funcionando, organizados pelos critérios de avaliação.

## 1. Estruturação do domínio (3,0)
| Arquivo | O que mostra |
|---|---|
| `01-estrutura-pastas.png` | Projeto no VS Code: Domain, Data, Dtos, Controllers, Migrations |
| `02-ocorrencia-regras.png` | `Domain/Ocorrencia.cs` com as regras (IniciarAtendimento, Resolver, Cancelar) |

## 2. Conexão com banco de dados (3,0)
| Arquivo | O que mostra |
|---|---|
| `03-swagger-bairros.png` | `GET /api/bairros` devolvendo os dados do Oracle |
| `04-sqldeveloper-tabelas.png` | Tabelas `ZB_*` no SQL Developer |
| `05-select-ocorrencias.png` | `SELECT * FROM ZB_OCORRENCIA;` com as ocorrências criadas pelo app, pelo terminal e pelo sensor |

## 3. Migrações (2,5)
| Arquivo | O que mostra |
|---|---|
| `06-migration-initialcreate.png` | `dotnet ef migrations add InitialCreate` |
| `07-database-update-initial.png` | `dotnet ef database update` criando as tabelas e inserindo os dados iniciais |
| `08-migration-descricaoopcional.png` | Segunda migration: `ALTER TABLE ... MODIFY "Descricao" NULL` |
| `09-migrations-history.png` | `SELECT * FROM "__EFMigrationsHistory";` com as duas migrations |

## 4. Testes manuais — Insomnia (1,5)
Coleção: [`../insomnia/zelabairro-insomnia.json`](../insomnia/zelabairro-insomnia.json)

| Arquivo | Requisição | Esperado |
|---|---|---|
| `10-identificar-morador.png` | POST /api/moradores/identificar | 200 |
| `11-criar-ocorrencia.png` | POST /api/ocorrencias | 201 |
| `12-listar-ocorrencias.png` | GET /api/ocorrencias | 200 |
| `13-confirmar.png` | POST /api/ocorrencias/{id}/confirmar | 200 |
| `14-prioridade.png` | PATCH /api/ocorrencias/{id}/prioridade | 200 |
| `15-iniciar-atendimento.png` | PATCH /api/ocorrencias/{id}/status | 200 |
| `16-resolver-e-foto.png` | PATCH status Resolvida + POST foto | 200 |
| `17-resumo.png` | GET /api/ocorrencias/resumo | 200 |
| `18-erro-titulo-vazio.png` | Título vazio | 400 |
| `19-erro-categoria.png` | Categoria inexistente | 400 |
| `20-erro-transicao.png` | Resolver sem atender | 400 |
| `21-erro-404.png` | Ocorrência inexistente | 404 |
| `22-erro-sql-injection.png` | `bairroId=1 OR 1=1` | 400 |
