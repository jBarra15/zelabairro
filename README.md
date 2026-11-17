# ZelaBairro — CitizenTech | Global Solution 2026.2 — FIAP

## Ficha da CitizenTech
| Campo | Resposta |
|---|---|
| **Nome da CitizenTech** | ZelaBairro |
| **Cidade e bairro** | São Paulo — _[seu bairro]_ |
| **Cidadão beneficiado** | _[ex.: moradores que circulam a pé pelo bairro, especialmente idosos — descreva a pessoa real com quem conversou]_ |
| **Problema, em uma frase** | _[ex.: Problemas como lixo acumulado e postes apagados ficam semanas sem solução porque reportar é difícil e ninguém acompanha o andamento.]_ |
| **Como funciona** | O morador registra o problema pelo app com foto e localização; sensores IoT abrem ocorrências automaticamente; a subprefeitura faz a triagem num terminal desktop; um modelo de ML sugere a prioridade; todos acompanham o status. |
| **O que cada parte faz** | `api/` backend REST · `mobile/` app do morador · `desktop/` terminal da subprefeitura (WinForms) · `ml/` modelo de prioridade · `iot/` lixeira inteligente ESP32 · `infra/` Windows Server/IIS · `docs/` segurança e arquitetura |
| **Link do repositório** | _[link do GitHub]_ |
| **Indicador de impacto** | % de ocorrências resolvidas dentro do prazo da categoria (endpoint `/api/ocorrencias/resumo`) |
| **ODS conectados** | ODS 11 (cidades sustentáveis), ODS 10 (redução das desigualdades), ODS 9 (infraestrutura) |

**Integrante:** Joel — RM 560796 — 2ESPS

## Estrutura do repositório
| Pasta | Disciplina |
|---|---|
| `api/` | Arquitetura Orientada a Serviços (SOA) e Web Services |
| `mobile/` | Mobile Development & IoT |
| `desktop/` | C# Software Development |
| `ml/` | Inteligência Artificial & Machine Learning |
| `iot/` | Physical Computing: IoT & IoB |
| `infra/` | Operating Systems |
| `docs/` | Cybersecurity e Testing (TOGAF/Archimate) |

## Uso de IA
Registro dos prompts em [`prompts.md`](prompts.md).
