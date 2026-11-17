# ZelaBairro — CitizenTech | Global Solution 2026.2 — FIAP

> Problemas do espaço público registrados, priorizados e acompanhados até a solução.

## Ficha da CitizenTech
| Campo | Resposta |
|---|---|
| **Nome da CitizenTech** | ZelaBairro |
| **Cidade e bairro** | São Paulo — Bela Vista, Consolação e Paraíso (entorno da Av. Paulista, no caminho até a FIAP) |
| **Cidadão beneficiado** | Moradores e quem circula a pé pelo bairro, especialmente idosos e pessoas com mobilidade reduzida. Pessoa ouvida: **[QUEM VOCÊ OUVIU — ex.: "Seu R., porteiro de um prédio na Rua X"]** |
| **Problema, em uma frase** | Lixo acumulado, postes apagados, buracos e calçadas quebradas ficam semanas sem solução porque reportar é difícil e ninguém acompanha o andamento. |
| **O que ouvimos** | **[1 ou 2 frases do que a pessoa contou, com as palavras dela]** |
| **Como funciona** | O morador registra o problema pelo app com foto e localização; sensores IoT (lixeira inteligente) abrem ocorrências sozinhos; a subprefeitura faz a triagem num terminal desktop com apoio de um modelo de Machine Learning que sugere a prioridade; todos acompanham o status até a resolução. |
| **O que cada parte faz** | `api/` backend REST (ASP.NET Core + Oracle) · `mobile/` app do morador (React Native/Expo) · `desktop/` terminal da subprefeitura (C# WinForms) · `ml/` modelo de prioridade · `iot/` lixeira inteligente (ESP32/Wokwi) · `infra/` Windows Server/IIS · `docs/` segurança e arquitetura |
| **Link do repositório** | https://github.com/jBarra15/zelabairro |
| **Indicador de impacto** | % de ocorrências resolvidas dentro do prazo da categoria (endpoint `GET /api/ocorrencias/resumo`, exibido no terminal da subprefeitura) |
| **ODS conectados** | ODS 11 (cidades e comunidades sustentáveis), ODS 10 (redução das desigualdades), ODS 9 (indústria, inovação e infraestrutura) |

**Integrante:** Joel — RM 560796 — 3ESPS

## Estrutura do repositório
| Pasta | Disciplina | Entrega |
|---|---|---|
| [`api/`](api/) | Arquitetura Orientada a Serviços (SOA) e Web Services | API REST + Oracle + migrations + testes Insomnia |
| [`mobile/`](mobile/) | Mobile Development & IoT | App do morador com câmera e localização |
| [`desktop/`](desktop/) | C# Software Development | Terminal de triagem da subprefeitura |
| [`ml/`](ml/) | Inteligência Artificial & Machine Learning | Modelo que prevê a prioridade das ocorrências |
| [`iot/`](iot/) | Physical Computing: IoT & IoB | Lixeira inteligente com ESP32 no Wokwi |
| `infra/` | Operating Systems | Windows Server, AD/DNS e IIS |
| `docs/` | Cybersecurity e Testing (TOGAF) | Relatório de riscos e arquitetura Archimate |

## Como as partes se conectam