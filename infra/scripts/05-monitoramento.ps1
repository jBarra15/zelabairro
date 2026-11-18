#Requires -RunAsAdministrator
# ETAPA 5 — Monitor de Desempenho (alertas de CPU, memória e disco + coleta contínua)
#           e verificação de saúde a cada minuto com recuperação automática
. "$PSScriptRoot\config.ps1"
Titulo 'ETAPA 5 — Monitoramento e recuperação automática'

if ((Get-UICulture).Name -notlike 'en-*') {
    Aviso "O Windows está em $((Get-UICulture).Name). Os nomes dos contadores do Monitor de Desempenho mudam de idioma."
    Aviso 'Se algum alerta falhar abaixo, crie-o pelo Monitor de Desempenho (passo a passo no README da pasta infra).'
}

$logs = Join-Path $PastaDados 'PerfLogs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null

$alertas = @(
    @{ Nome = 'ZB-Alerta-CPU';     Limite = '\Processor(_Total)\% Processor Time>80' },
    @{ Nome = 'ZB-Alerta-Memoria'; Limite = '\Memory\% Committed Bytes In Use>80' },
    @{ Nome = 'ZB-Alerta-Disco';   Limite = '\LogicalDisk(C:)\% Free Space<15' }
)
foreach ($a in $alertas) {
    logman stop $a.Nome 2>$null | Out-Null; logman delete $a.Nome 2>$null | Out-Null
    logman create alert $a.Nome -th $a.Limite -si 00:00:15 -el | Out-Null
    logman start $a.Nome | Out-Null
    if ($LASTEXITCODE -eq 0) { Ok "Alerta $($a.Nome): $($a.Limite) (amostra a cada 15 s, grava no log de eventos)" } else { Aviso "Falha ao criar $($a.Nome)" }
}

logman stop 'ZB-Desempenho' 2>$null | Out-Null; logman delete 'ZB-Desempenho' 2>$null | Out-Null
logman create counter 'ZB-Desempenho' -c '\Processor(_Total)\% Processor Time' '\Memory\Available MBytes' '\Memory\% Committed Bytes In Use' `
    '\LogicalDisk(C:)\% Free Space' '\Web Service(_Total)\Current Connections' '\Web Service(_Total)\Total Method Requests/sec' `
    -si 00:00:15 -f csv -o "$logs\desempenho" -v mmddhhmm | Out-Null
logman start 'ZB-Desempenho' | Out-Null
Ok "Coleta contínua ZB-Desempenho (CSV em $logs)"

# Verificação de saúde a cada minuto (recuperação automática do serviço e do pool)
$destino = Join-Path $PastaDados 'scripts'
New-Item -ItemType Directory -Path $destino -Force | Out-Null
Copy-Item "$PSScriptRoot\verificar-saude.ps1" $destino -Force
if (-not [Diagnostics.EventLog]::SourceExists('ZelaBairro')) { New-EventLog -LogName Application -Source 'ZelaBairro' }
$acao = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$destino\verificar-saude.ps1`""
$gatilho = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes 1)
$quem = New-ScheduledTaskPrincipal -UserId 'SYSTEM' -LogonType ServiceAccount -RunLevel Highest
Register-ScheduledTask -TaskName 'ZB-Verificar-Saude' -Action $acao -Trigger $gatilho -Principal $quem -Force `
    -Description 'Verifica a API ZelaBairro a cada minuto e recupera o serviço/pool do IIS' | Out-Null
Ok 'Tarefa agendada ZB-Verificar-Saude (a cada 1 minuto, como SYSTEM)'

Titulo 'ETAPA 5 concluída. Veja em: Monitor de Desempenho > Conjuntos de Coletores de Dados > Definidos pelo Usuário'
Write-Host '  Alertas disparados aparecem no Visualizador de Eventos em:' -ForegroundColor Gray
Write-Host '  Logs de Aplicativos e Serviços > Microsoft > Windows > Diagnosis-PLA > Operational (evento 2031)' -ForegroundColor Gray
