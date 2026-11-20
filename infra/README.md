# ZelaBairro — Infraestrutura Windows Server (Operating Systems)

| | |
|---|---|
| **Integrante** | Joel Barra Chuquimia — RM 560796 — 3ESPS (trabalho individual) |
| **Vídeo de demonstração** | https://youtu.be/E3lLoOFdynk |
| **Repositório** | https://github.com/jBarra15/zelabairro/tree/main/infra |

## Ficha da CitizenTech
| Campo | Resposta |
|---|---|
| **Nome** | ZelaBairro |
| **Cidade e bairro** | São Paulo — Tatuapé (piloto) |
| **Cidadão beneficiado** | Moradores e quem circula pelo bairro. Pessoa ouvida: Seu Carlos, 52 anos, motorista de aplicativo no Tatuapé |
| **Problema** | Lixo acumulado e buracos ficam meses sem solução, e quem reclama não sabe se o pedido foi recebido, verificado ou resolvido |
| **Como funciona** | App do morador + lixeiras com sensor + terminal da subprefeitura com ML, todos usando a **API ZelaBairro**, hospedada neste servidor (IIS) |
| **Indicador de impacto** | % de ocorrências resolvidas dentro do prazo da categoria |
| **ODS** | 11, 10 e 9 |

## O que este servidor entrega
| Requisito | Como foi feito | Script |
|---|---|---|
| **IP fixo e hostname** | `10.0.2.15/24`, gateway `10.0.2.2`, nome `SRV-ZELABAIRRO` | 01 |
| **Active Directory e DNS** | Domínio `zelabairro.local`, encaminhadores 8.8.8.8/1.1.1.1, registro `app.zelabairro.local` | 02, 03 |
| **Identidade** | OUs ZelaBairro/Operadores, TI e Grupos; grupos GG_Operadores e GG_TI; usuários op.ana, op.bruno e ti.admin; política de senha (10+ caracteres, bloqueio após 5 tentativas) | 03 |
| **File Server e NTFS** | `\\SRV-ZELABAIRRO\Relatorios`: GG_TI controle total, GG_Operadores modificar, herança removida | 03 |
| **GPO** | *ZB - Politica dos Operadores* (bloqueio de tela em 10 min, sem Painel de Controle) e *ZB - Aviso de Logon* (aviso legal antes do login) | 03 |
| **Acesso remoto** | Área de Trabalho Remota com NLA, somente GG_TI | 03 |
| **Auditoria** | Logon, sistema de arquivos (pasta Relatorios), contas e políticas | 03 |
| **IIS + aplicação** | .NET 8 Hosting Bundle; pool `ZelaBairroPool` (sem código gerenciado, sempre ativo); site `ZelaBairro` na porta 80; senha do banco como variável do pool; NTFS: pool só lê o site e só escreve em `uploads` | 04 |
| **Logs** | IIS em W3C diário (`C:\inetpub\logs\LogFiles`); eventos do ZelaBairro no Visualizador de Eventos | 04, 05 |
| **Monitoramento** | Monitor de Desempenho: alertas de **CPU > 80%**, **memória > 80%** e **disco livre < 15%**, mais coleta contínua em CSV | 05 |
| **Tolerância a falhas** | Recuperação automática dos serviços W3SVC/WAS (`sc failure`); proteção de falhas rápidas e reciclagem do pool; tarefa que verifica a API a cada minuto e reinicia serviço/pool | 04, 05 |
| **Teste de falha** | Menu interativo: derrubar o pool, matar o processo, parar o IIS, estressar a CPU | 06 |
| **Evidências** | Saídas de `ipconfig`, `nslookup`, `Test-NetConnection`, AD, IIS, NTFS, GPO, auditoria e eventos | 07 |

## Roteiro de configuração

### 1. Criar a máquina virtual (VirtualBox)
1. Baixe a **ISO do Windows Server 2022 Evaluation** em **inglês (en-US)**. Em inglês, os nomes dos contadores do Monitor de Desempenho batem com os scripts.
2. VirtualBox → **New**:
   - Name: `SRV-ZELABAIRRO` · ISO: a do Windows Server · marque **Skip Unattended Installation**
   - Memória: **4096 MB** (ou 6144 MB se o PC tiver 16 GB) · CPUs: **2** · Disco: **50 GB**
3. **Settings → Network → Adapter 1: NAT → Advanced → Port Forwarding**, adicione:
   | Nome | Protocolo | Porta do host | Porta do convidado |
   |---|---|---|---|
   | http | TCP | 8080 | 80 |
   | rdp | TCP | 33890 | 3389 |

   Assim, no seu PC, `http://localhost:8080` abre o site da VM.

### 2. Instalar o Windows Server
1. Inicie a VM e escolha **Windows Server 2022 Standard Evaluation (Desktop Experience)** (a versão *com* interface gráfica).
2. **Custom: Install Windows only** → disco → Next.
3. Defina a senha do **Administrator** (ex.: com maiúscula, minúscula, número e símbolo).
4. Para entrar: menu da VM **Input → Keyboard → Insert Ctrl-Alt-Del**.
5. Opcional, recomendado: **Devices → Insert Guest Additions CD image**, instale e reinicie (melhora a tela e libera copiar/colar).

### 3. Baixar os scripts dentro da VM
Abra o **PowerShell como administrador** (botão direito no menu Iniciar → *Windows PowerShell (Admin)*) e digite:
```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
iwr https://github.com/jBarra15/zelabairro/archive/refs/heads/main.zip -OutFile C:\zb.zip -UseBasicParsing
Expand-Archive C:\zb.zip C:\ -Force
cd C:\zelabairro-main\infra\scripts
```

### 4. Rodar as etapas, em ordem
Sempre no PowerShell como administrador, dentro de `C:\zelabairro-main\infra\scripts`, e sempre começando com `Set-ExecutionPolicy Bypass -Scope Process -Force`.

| Etapa | Comando | O que acontece |
|---|---|---|
| 1 | `.\01-rede-e-nome.ps1` | IP fixo, fuso e nome → **reinicia** |
| 2 | `.\02-ad-dns.ps1` | Pede a senha DSRM, cria o domínio → **reinicia sozinho** (5–10 min) |
| — | Entre como **ZELABAIRRO\Administrator** | mesma senha do Administrator |
| 3 | `.\03-identidade-pastas-gpo.ps1` | Pede a senha dos usuários de teste; cria OUs, usuários, pasta, GPO, auditoria |
| 4 | `.\04-iis-aplicacao.ps1` | Instala IIS e .NET 8, **pede a senha do Oracle**, publica e sobe o site (10–15 min) |
| 5 | `.\05-monitoramento.ps1` | Alertas do Monitor de Desempenho e tarefa de verificação |
| 6 | `.\07-evidencias.ps1` | Gera os arquivos de evidência e abre a pasta |
| Vídeo | `.\06-teste-de-falha.ps1` | Menu do teste de falha |

### 5. Conferir
- Na VM: abra o Edge em **http://app.zelabairro.local** → painel do ZelaBairro, com **"Servidor: SRV-ZELABAIRRO"** no topo.
- No seu PC: **http://localhost:8080** → o mesmo painel (via encaminhamento de porta).
- Saúde: **http://app.zelabairro.local/api/saude**

## Onde mostrar cada item no vídeo
| Item | Onde |
|---|---|
| IP, hostname, DNS | `ipconfig /all`, `nslookup app.zelabairro.local`, `Test-NetConnection app.zelabairro.local -Port 80` |
| Domínio, OUs, usuários | **Server Manager → Tools → Active Directory Users and Computers** |
| DNS | **Tools → DNS** → Forward Lookup Zones → zelabairro.local → registro `app` |
| Pasta e NTFS | Botão direito em `C:\ZelaBairro\Relatorios` → Properties → **Security** e **Sharing** |
| GPO | **Tools → Group Policy Management** → ZelaBairro → Operadores |
| IIS | **Tools → Internet Information Services (IIS) Manager** → Application Pools e Sites → Basic/Advanced Settings |
| Dependências | `dotnet --list-runtimes` (aparece Microsoft.AspNetCore.App 8.x) |
| Monitoramento | **Tools → Performance Monitor → Data Collector Sets → User Defined** (ZB-Alerta-CPU, ZB-Alerta-Memoria, ZB-Alerta-Disco, ZB-Desempenho) |
| Alertas disparados | **Event Viewer → Applications and Services Logs → Microsoft → Windows → Diagnosis-PLA → Operational** (evento 2031) |
| Recuperação automática | **Event Viewer → Windows Logs → Application**, fonte *ZelaBairro* (eventos 1001–1004); **Task Scheduler** → ZB-Verificar-Saude; **services.msc** → W3SVC → aba Recovery |
| Auditoria | **Event Viewer → Windows Logs → Security** (4624 logon, 4625 falha de logon, 4663 acesso a arquivo) |
| Logs do IIS | `C:\inetpub\logs\LogFiles\W3SVC2\` |

## Teste de falha
`.\06-teste-de-falha.ps1` mostra o estado do site a cada 10 segundos:
1. **Derrubar o pool:** o site cai (503) e, em até 1 minuto, a tarefa ZB-Verificar-Saude o religa e registra o evento 1001.
2. **Matar o processo (w3wp):** simula um travamento. O IIS sobe um processo novo na próxima requisição; o número do processo em `/api/saude` muda.
3. **Parar o W3SVC:** o monitor religa o serviço e registra o evento 1002. Se o serviço *travar* (em vez de ser parado), o próprio Windows o reinicia pela configuração de recuperação (`sc qfailure W3SVC`).
4. **Estressar a CPU:** dispara o alerta ZB-Alerta-CPU (evento 2031).

## Solução de problemas
- **`/api/saude` mostra "banco sem conexão" (503):** confira a senha do Oracle (rode o 04 de novo) e `Test-NetConnection oracle.fiap.com.br -Port 1521`.
- **Windows em português:** se o 05 não criar algum alerta, crie pelo Monitor de Desempenho: *Data Collector Sets → User Defined → New → Data Collector Set → Create manually → Performance Counter Alert* → contador *Processador(_Total)\% Tempo de Processador*, *Acima de 80* → em *Alert Action*, marque *Log an entry in the application event log*.
- **Site não abre e aparece erro 500.19:** o Hosting Bundle não foi instalado. Rode o 04 de novo.

## Evidências
Os prints vão em [`evidencias/`](evidencias/). Os arquivos gerados pelo `07-evidencias.ps1` ficam em `C:\ZelaBairro\evidencias` dentro da VM.
