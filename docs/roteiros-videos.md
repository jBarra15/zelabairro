# Roteiros dos vídeos — ZelaBairro (GS 2026.2)

**Como gravar:** **Win+Alt+R** começa e para a gravação da tela (Barra de Jogo do Windows; os vídeos vão para *Vídeos → Capturas*). Fale por cima, sem editar. Suba no **YouTube como "Não listado"** e entregue o link. Se passar do tempo, regrave: um take inteiro é melhor que vários cortes.

**Frase de abertura (todos os vídeos):** "Olá, sou o Joel, RM 560796, turma 3ESPS. Este é o ZelaBairro, uma CitizenTech para o Tatuapé: o morador registra lixo acumulado e buracos com foto e acompanha até a prefeitura resolver."

---

## 1. Inteligência Artificial & Machine Learning — até 2 min
**Antes de gravar:** notebook já executado no Colab (Ambiente de execução → Executar tudo); API rodando e terminal da subprefeitura (desktop) aberto com uma ocorrência aberta.

| Tempo | Mostrar | Falar |
|---|---|---|
| 0:00–0:15 | Topo do notebook | Abertura. "O operador da subprefeitura precisa decidir o que atender primeiro. O modelo prevê a prioridade: baixa, média ou alta." |
| 0:15–0:35 | Seção 2 e 3 (dados) | "Os dados são simulados, porque não podemos usar dados pessoais reais: 3.000 ocorrências com premissas documentadas e semente fixa. O bairro ficou de fora para o modelo não favorecer um bairro." |
| 0:35–0:55 | Gráficos da seção 4 | "Árvore e iluminação têm mais casos altos; a prioridade cresce com o prazo consumido e com as confirmações de vizinhos." |
| 0:55–1:20 | Gráfico de comparação e tabela do teste | "Comparei 5 abordagens com F1-macro, porque as classes são desbalanceadas. A regressão logística ganhou: F1 0,76 contra 0,61 da regra antiga, e acerta 89% dos casos altos." |
| 1:20–1:35 | Gráfico de pesos | "O modelo é explicável: prazo consumido e confirmações são o que mais pesa." |
| 1:35–2:00 | **Terminal C#**: clicar numa ocorrência → "Previsão do modelo: … % de confiança" → **Usar previsão do modelo** | "O modelo foi exportado em JSON e roda dentro do terminal da subprefeitura. O operador vê a previsão, os fatores e decide. A decisão final é humana." |

---

## 2. Mobile Development — demo de até 2 min
**Antes de gravar:** API com `dotnet run --urls http://0.0.0.0:5080`; IP certo em `src/config.ts`; `npx expo start`; **espelhe o celular no PC** (app *Vincular ao Celular* do Windows, ou grave a tela do próprio celular). Apague os dados do app antes (ou use "Sair") para mostrar a identificação.

| Tempo | Mostrar | Falar |
|---|---|---|
| 0:00–0:10 | Tela de identificação | Abertura curta |
| 0:10–0:25 | Preencher nome, e-mail e bairro **Tatuapé** → tentar com e-mail inválido primeiro | "Validação antes de enviar." |
| 0:25–0:45 | Lista do bairro (puxar para atualizar) | "Lista com carregamento, erro e vazio tratados." |
| 0:45–1:20 | **Registrar problema**: categoria Buraco, título, **Usar minha localização** (aceitar permissão), **Fotografar** (aceitar permissão) → Enviar | "Câmera e localização são os recursos nativos. Se o morador negar a permissão, o app continua e oferece digitar o endereço." |
| 1:20–1:35 | Detalhe da ocorrência com foto e status | "Ele vê o status: aberta, em atendimento, resolvida. Era isso que o Seu Carlos pedia." |
| 1:35–1:50 | **Fechar o app e reabrir** | "O app lembra do morador." |
| 1:50–2:00 | (Opcional) terminal no PC mostrando a ocorrência nova | "E ela chega na hora para a subprefeitura." |

---

## 3. Physical Computing: IoT & IoB (Wokwi) — até 4 min
**Antes de gravar:** projeto montado e salvo no Wokwi; API rodando; porta 5080 encaminhada na aba **Ports** do VS Code (pública) e o endereço colado em `API_URL`; terminal da subprefeitura aberto.

| Tempo | Mostrar | Falar |
|---|---|---|
| 0:00–0:30 | Wokwi | Abertura. "Lixo é a categoria de prazo mais curto, e muitas vezes ninguém reclama. A lixeira inteligente avisa sozinha." |
| 0:30–1:20 | Circuito: apontar cada peça | "Sensor ultrassônico na tampa mede a distância até o lixo; LCD mostra nível e status; LEDs verde, amarelo e vermelho; buzzer; botão de coleta para o gari; ESP32 com Wi-Fi." |
| 1:20–1:50 | Código: `NIVEL_ALERTA`, `LEITURAS_CONFIRMACAO`, função `abrirOcorrencia` | "Acima de 80% por 3 leituras seguidas, evita alarme falso, ele faz um POST na mesma API do app, com origem Sensor." |
| 1:50–2:50 | ▶ Rodar → arrastar o sensor para < 20 cm → LCD "CHEIA 1/3, 2/3, 3/3" → "Ocorrencia #N", LED vermelho, bipe; Serial Monitor com a resposta 201 | Narre o que acontece. |
| 2:50–3:20 | Terminal da subprefeitura → Atualizar → ocorrência com origem **Sensor** e previsão do ML | "Ela entra na fila da subprefeitura, e o modelo considera que foi medida por sensor." |
| 3:20–3:45 | Apertar **Coleta realizada** (ou subir o sensor) → "Coleta OK!"; mencionar "API offline" com reenvio a cada 30 s | "Uma ocorrência por ciclo; se a API cair, ele tenta de novo." |
| 3:45–4:00 | Fechamento | "O problema é registrado antes de alguém precisar reclamar." |

---

## 4. Operating Systems — até 7 min
**Antes de gravar:** VM configurada (scripts 01 a 05 e 07 rodados); Server Manager aberto; PowerShell como admin em `C:\zelabairro-main\infra\scripts`.

| Tempo | Mostrar | Falar |
|---|---|---|
| 0:00–0:30 | Painel no navegador da VM (**http://app.zelabairro.local**) | Abertura. "Esta é a aplicação web do ZelaBairro hospedada no IIS deste Windows Server; o topo mostra o servidor SRV-ZELABAIRRO." |
| 0:30–1:15 | PowerShell: `hostname`, `ipconfig`, `nslookup app.zelabairro.local`, `Test-NetConnection app.zelabairro.local -Port 80` | "IP fixo, nome do servidor, DNS do domínio e o site respondendo." |
| 1:15–2:00 | **AD Users and Computers** (OUs, usuários, grupos) e **DNS** (registro `app`) | "Domínio zelabairro.local com OUs para operadores e TI, política de senha com bloqueio." |
| 2:00–2:40 | Pasta **Relatorios** → Security e Sharing; **Group Policy Management** | "Permissões NTFS por grupo; GPO com bloqueio de tela para operadores e aviso de logon." |
| 2:40–3:40 | **IIS Manager**: pool ZelaBairroPool (No Managed Code, AlwaysRunning, Rapid-Fail) e site; `dotnet --list-runtimes`; NTFS da pasta do site | "Pool dedicado, runtime do .NET 8, o pool só lê o site e só escreve em uploads; a senha do banco fica fora do código." |
| 3:40–4:30 | **Performance Monitor → Data Collector Sets → User Defined** (os 3 alertas e a coleta) | "Alertas de CPU acima de 80%, memória acima de 80% e disco com menos de 15% livre." |
| 4:30–5:50 | `.\06-teste-de-falha.ps1` → **opção 1** (derrubar pool → "FORA DO AR" → "RECUPERADO AUTOMATICAMENTE") e **opção 2** (processo muda) | "Simulo uma queda: o monitor percebe e religa o pool sozinho em menos de um minuto." |
| 5:50–6:30 | **Opção 4** (CPU) → **Event Viewer**: Diagnosis-PLA (2031), Application fonte ZelaBairro (1001), Security (4624) | "O alerta dispara e tudo fica registrado no Visualizador de Eventos: recuperações, alertas e auditoria de logon." |
| 6:30–6:50 | Logs do IIS (`C:\inetpub\logs\LogFiles`) | "Cada requisição registrada no log W3C." |
| 6:50–7:00 | Painel no ar de novo | "Infraestrutura funcionando, monitorada e tolerante a falhas." |
