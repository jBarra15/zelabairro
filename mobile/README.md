# ZelaBairro — App Mobile (Mobile Development & IoT)

## Ficha da CitizenTech
| Campo | Resposta |
|---|---|
| **Nome da CitizenTech** | ZelaBairro |
| **Cidade e bairro** | São Paulo — região da Av. Paulista (Bela Vista, Consolação, Paraíso) |
| **Cidadão beneficiado** | Moradores e quem circula a pé pelo bairro, especialmente idosos e pessoas com mobilidade reduzida |
| **Problema, em uma frase** | Lixo acumulado, postes apagados e buracos ficam semanas sem solução porque reportar é difícil e ninguém acompanha o andamento. |
| **Como funciona** | O morador fotografa o problema e marca a localização pelo app; a subprefeitura faz a triagem; sensores IoT também abrem ocorrências; todos acompanham o status. |
| **O que cada parte faz** | **Este app**: porta de entrada do cidadão · API REST (backend) · terminal WinForms (subprefeitura) · modelo de ML (prioridade) · ESP32 (lixeira inteligente) · Windows Server/IIS (hospedagem) |
| **Link do repositório** | _[link do GitHub]_ |
| **Indicador de impacto** | % de ocorrências resolvidas dentro do prazo da categoria |
| **ODS conectados** | ODS 11 (cidades sustentáveis), ODS 10 (redução das desigualdades), ODS 9 (infraestrutura) |

**Integrante:** Joel — RM 560796 — 2ESPS

## Funcionalidades
| Requisito | Onde está |
|---|---|
| 3+ telas com navegação | Identificação → Lista → Detalhe → Nova ocorrência (React Navigation, native-stack) |
| Lista com carregando, erro e vazio | `OcorrenciasScreen` + `components/EstadoTela.tsx` (com "Tentar novamente" e puxar para atualizar) |
| Formulário validado que cria um item | `NovaOcorrenciaScreen` + `validation/ocorrencia.ts` → `POST /api/ocorrencias` |
| Identificação lembrada ao reabrir | `context/MoradorContext.tsx` + `services/storage.ts` (AsyncStorage) |
| Recurso nativo com permissão tratada | **Câmera** (expo-image-picker) e **Localização** (expo-location), com mensagem e atalho para Configurações quando negado |
| TypeScript estrito, sem `any` | `tsconfig.json` com `strict: true`; `npm run typecheck` limpo |

## Estrutura
```
App.tsx                 Navegação (rotas condicionais: sem morador → só Identificação)
src/
├── config.ts           URL da API (IP do computador)
├── theme.ts            Cores com contraste AA e tamanhos de fonte
├── types/models.ts     Tipos espelhando os DTOs da API
├── navigation/types.ts Tipos das rotas
├── services/           Toda comunicação externa (API e armazenamento)
├── validation/         Regras de formulário puras (sem React)
├── context/            Morador identificado (lembrado ao reabrir)
├── components/         Botão, Campo, Chip, Card, estados de tela
├── screens/            As 4 telas
└── utils/              Formatação de datas e rótulos
```

## Como rodar

**Pré-requisitos:** Node.js 20+, app **Expo Go** no celular e a API ZelaBairro rodando.

**1. Suba a API aceitando conexões da rede** (na pasta `api/ZelaBairro.Api`):
```bash
dotnet run --urls http://0.0.0.0:5080
```
Na primeira vez, libere a porta no firewall (PowerShell **como administrador**):
```powershell
New-NetFirewallRule -DisplayName "ZelaBairro API" -Direction Inbound -Protocol TCP -LocalPort 5080 -Action Allow
```

**2. Descubra o IP do computador** com `ipconfig` (linha "Endereço IPv4", ex.: `192.168.0.15`) e coloque em `src/config.ts`.
Teste no navegador do celular: `http://SEU_IP:5080/api/bairros` deve mostrar os bairros.

**3. Instale e rode o app:**
```bash
npm install
npx expo start
```
Escaneie o QR Code com o Expo Go. Celular e computador na **mesma rede Wi-Fi**.

**4. Verificar tipos:**
```bash
npm run typecheck
```
