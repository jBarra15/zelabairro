# ZelaBairro — CitizenTech | Global Solution 2026.2 — FIAP

> Problemas do espaço público registrados, priorizados e acompanhados até a solução.

## Ficha da CitizenTech
| Campo | Resposta |
|---|---|
| **Nome da CitizenTech** | ZelaBairro |
| **Cidade e bairro** | São Paulo — Tatuapé (piloto), com a região da Av. Paulista (Bela Vista, Consolação, Paraíso) no caminho até a FIAP |
| **Cidadão beneficiado** | Moradores e quem circula pelo bairro a pé ou de carro. Pessoa ouvida: **Seu Carlos, 52 anos, motorista de aplicativo que mora e trabalha no Tatuapé** e circula pelas ruas do bairro o dia todo |
| **Problema, em uma frase** | Lixo acumulado e buracos ficam meses sem solução porque a reclamação é feita por telefone ou aplicativo e depois ninguém sabe se ela foi recebida, verificada ou resolvida. |
| **O que ouvimos** | "O que mais me incomoda é o lixo acumulado em alguns lugares e também alguns buracos na rua. Às vezes fica complicado até para passar de carro ou a pé. [...] Algumas pessoas já reclamaram, mas geralmente é pelo telefone ou por aplicativo, e depois a gente nem sabe se realmente vão resolver. Às vezes parece que a reclamação fica esquecida." Ele usaria um app para mandar foto e acompanhar, "principalmente se desse para saber se a prefeitura recebeu, se alguém foi verificar e quando vão resolver." |
| **Como funciona** | O morador registra o problema pelo app com foto e localização e acompanha cada etapa (**Aberta → Em atendimento → Resolvida**); sensores IoT (lixeira inteligente) abrem ocorrências sozinhos; a subprefeitura faz a triagem num terminal desktop com apoio de um modelo de Machine Learning que sugere a prioridade. |
| **O que cada parte faz** | `api/` backend REST (ASP.NET Core + Oracle) · `mobile/` app do morador (React Native/Expo) · `desktop/` terminal da subprefeitura (C# WinForms) · `ml/` modelo de prioridade · `iot/` lixeira inteligente (ESP32/Wokwi) · `infra/` Windows Server/IIS · `docs/` segurança e arquitetura |
| **Link do repositório** | https://github.com/jBarra15/zelabairro |
| **Indicador de impacto** | % de ocorrências resolvidas dentro do prazo da categoria (endpoint `GET /api/ocorrencias/resumo`, exibido no terminal da subprefeitura) |
| **ODS conectados** | ODS 11 (cidades e comunidades sustentáveis), ODS 10 (redução das desigualdades), ODS 9 (indústria, inovação e infraestrutura) |

**Integrante:** Joel — RM 560796 — 3ESPS

## Do que ouvimos para o que construímos
| O Seu Carlos disse | O ZelaBairro responde com |
|---|---|
| "Lixo acumulado [...] e buracos na rua" | Categorias **Lixo** e **Buraco na via**, cada uma com prazo de atendimento; lixeira com sensor que avisa sozinha |
| "Reclamam pelo telefone ou por aplicativo" | Registro em poucos toques no app, **com foto e localização**, e também pelo balcão da subprefeitura |
| "A gente nem sabe se vão resolver" | Status visível no app: **Aberta → Em atendimento → Resolvida**, com data de resolução |
| "Parece que a reclamação fica esquecida" | Alerta de **fora do prazo** no terminal e no app; indicador de % resolvidas no prazo |
| "Saber se alguém foi verificar" | O operador muda o status no terminal e o morador vê na hora; vizinhos confirmam o problema (👍), o que ajuda a priorizar |

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

    Morador (app) ───┐
    Sensor (ESP32) ──┼──► API ZelaBairro (ASP.NET Core) ──► Oracle
    Balcão (C#) ─────┘              │
                                    └──► Terminal da subprefeitura (C#) + modelo de ML

## Uso de IA
Registro dos prompts e de como cada resposta foi usada em [`prompts.md`](prompts.md).
