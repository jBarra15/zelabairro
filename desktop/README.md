# ZelaBairro — Terminal da Subprefeitura (C# Software Development)

**Equipe:** Joel — RM 560796 — 2ESPS
**Versão:** 1.1.0 (ver [CHANGELOG.md](CHANGELOG.md))

## O problema
Lixo acumulado, postes apagados, buracos e calçadas quebradas ficam semanas sem solução no bairro, porque reportar é difícil e ninguém acompanha o andamento. Os moradores registram pelo app, mas **alguém precisa fazer a triagem**: decidir o que é mais urgente e acompanhar até resolver.

## A pessoa real beneficiada
- **Operador da subprefeitura / posto comunitário:** recebe as ocorrências do app e dos sensores, prioriza e muda o status.
- **Cidadão sem smartphone (ex.: idosos):** liga ou vai ao balcão, e o operador registra a ocorrência por ele.
- _[Descreva aqui a pessoa real com quem você conversou.]_

## O que o sistema faz
| Função | Como |
|---|---|
| Fila de triagem | Lista ordenada: **fora do prazo primeiro**, depois prioridade, depois as mais antigas; filtros por bairro e status |
| Indicadores | Total, abertas, em atendimento, fora do prazo e **% resolvidas no prazo** (indicador de impacto) |
| Previsão de prioridade (ML) | `Services/ModeloPrioridade.cs` carrega o modelo treinado em `ml/` (`modelo_prioridade.json`) e mostra a prioridade prevista, a confiança e os fatores que mais pesaram |
| Regra de pontos (referência) | `Services/Priorizador.cs`: risco da categoria, uso do prazo, confirmações e sensor, com a explicação de cada ponto |
| Atendimento | Iniciar atendimento → Resolver, ou Cancelar. **Só os botões permitidos pela regra ficam habilitados** |
| Cadastro no balcão | Formulário validado que registra a ocorrência com origem "Terminal" |
| Persistência local | `%AppData%\ZelaBairro\historico.json` (auditoria de quem fez o quê) e `cache-ocorrencias.json` |
| Modo offline | Se a API cair, mostra a última lista salva, só para consulta |
| Foto | Exibe a foto enviada pelo morador no app |

## Organização (POO)
```
ZelaBairro.Desktop/
├── Models/      Ocorrencia (regras de transição: PodeResolver, PodeCancelar...), Categoria, Bairro, Resumo, RegistroAcao, enums
├── Services/    ApiClient (HTTP), RegistroLocal (JSON), ModeloPrioridade (ML), Priorizador (regra), ValidacaoOcorrencia, Configuracao
├── Forms/       MainForm (triagem), NovaOcorrenciaForm (balcão), HistoricoForm, Estilo
└── Program.cs   Monta as dependências e abre o MainForm
```
- **Encapsulamento:** as telas não fazem HTTP nem gravam arquivo; usam `ApiClient` e `RegistroLocal`.
- **Regras no modelo:** `Ocorrencia` sabe quais transições são permitidas; a tela só consulta.
- **Algoritmos isolados:** `Priorizador` (regra) e `ModeloPrioridade` (ML) recebem a ocorrência e devolvem a sugestão com os motivos; a tela só exibe.

## Tecnologias
C# 12 · .NET 8 · Windows Forms · System.Text.Json · HttpClient · API REST ZelaBairro (ASP.NET Core + Oracle)

## Como executar
**Pré-requisitos:** Windows, .NET 8 SDK e a **API ZelaBairro rodando** (pasta `api/`).

1. Suba a API:
   ```bash
   cd api/ZelaBairro.Api
   dotnet run
   ```
2. Se a API estiver em outro endereço, edite `ZelaBairro.Desktop/zelabairro.config.json`:
   ```json
   { "ApiUrl": "http://localhost:5080", "Operador": "Seu nome" }
   ```
3. Em outro terminal:
   ```bash
   cd desktop/ZelaBairro.Desktop
   dotnet run
   ```
   Ou abra o `ZelaBairro.Desktop.csproj` no **Visual Studio 2022** e aperte **F5**.

## Versionamento
Versionamento semântico (`MAJOR.MINOR.PATCH`), com cada entrega feita por **pull request** a partir de uma branch `feature/*` e uma tag por versão. Histórico em [CHANGELOG.md](CHANGELOG.md).
