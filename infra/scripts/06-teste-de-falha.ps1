#Requires -RunAsAdministrator
# TESTE DE FALHA — use durante o vídeo. Cada opção simula um problema e mostra a recuperação.
. "$PSScriptRoot\config.ps1"
Import-Module WebAdministration

function Saude {
    try {
        $r = Invoke-RestMethod 'http://localhost/api/saude' -TimeoutSec 10
        Write-Host ("  {0}  SITE NO AR  | servidor {1} | processo {2} | banco {3}" -f (Get-Date -f HH:mm:ss), $r.servidor, $r.processoId, $r.banco) -ForegroundColor Green
        return $true
    } catch {
        Write-Host ("  {0}  SITE FORA DO AR  ({1})" -f (Get-Date -f HH:mm:ss), $_.Exception.Message) -ForegroundColor Red
        return $false
    }
}
function Acompanhar($segundos) {
    $fim = (Get-Date).AddSeconds($segundos)
    while ((Get-Date) -lt $fim) {
        $ok = Saude
        Write-Host ("            pool: {0} | W3SVC: {1}" -f (Get-WebAppPoolState $Pool).Value, (Get-Service W3SVC).Status) -ForegroundColor Gray
        if ($ok -and $script:caiu) { Write-Host '  >>> RECUPERADO AUTOMATICAMENTE <<<' -ForegroundColor Green; break }
        if (-not $ok) { $script:caiu = $true }
        Start-Sleep 10
    }
}

do {
    Titulo 'TESTE DE FALHA — ZelaBairro'
    Saude | Out-Null
    Write-Host ''
    Write-Host '  1) Derrubar o pool de aplicativos (recupera em até 1 min pela tarefa de monitoramento)'
    Write-Host '  2) Matar o processo da aplicação (w3wp) — o IIS cria outro na próxima requisição'
    Write-Host '  3) Parar o serviço do IIS (W3SVC) — recuperado pelo monitoramento'
    Write-Host '  4) Estressar a CPU por 90 s — dispara o alerta do Monitor de Desempenho'
    Write-Host '  5) Mostrar eventos recentes (recuperações, alertas, IIS)'
    Write-Host '  0) Sair'
    $op = Read-Host 'Opção'
    $script:caiu = $false
    switch ($op) {
        '1' { Stop-WebAppPool -Name $Pool; Write-Host '  Pool parado.' -ForegroundColor Yellow; Acompanhar 120 }
        '2' { $antes = (Invoke-RestMethod 'http://localhost/api/saude' -TimeoutSec 10).processoId
              Get-Process w3wp -ErrorAction SilentlyContinue | Stop-Process -Force
              Write-Host "  Processo $antes encerrado à força (simula travamento)." -ForegroundColor Yellow
              Start-Sleep 2; Saude | Out-Null
              Write-Host '  Note que o número do processo mudou: o IIS subiu um novo processo sozinho.' -ForegroundColor Cyan }
        '3' { Stop-Service W3SVC -Force; Write-Host '  Serviço W3SVC parado.' -ForegroundColor Yellow; Acompanhar 120 }
        '4' { $n = [Environment]::ProcessorCount
              Write-Host "  Ocupando $n núcleos por 90 s... abra o Gerenciador de Tarefas e o Monitor de Desempenho." -ForegroundColor Yellow
              $jobs = 1..$n | ForEach-Object { Start-Job { $f = (Get-Date).AddSeconds(90); while ((Get-Date) -lt $f) { [Math]::Sqrt(12345.678) | Out-Null } } }
              $jobs | Wait-Job | Remove-Job
              Write-Host '  Fim do estresse. Veja o evento 2031 em Diagnosis-PLA > Operational.' -ForegroundColor Cyan }
        '5' { Get-WinEvent -FilterHashtable @{ LogName = 'Application'; ProviderName = 'ZelaBairro' } -MaxEvents 10 -ErrorAction SilentlyContinue |
                Format-Table TimeCreated, Id, LevelDisplayName, Message -AutoSize -Wrap
              Get-WinEvent -LogName 'Microsoft-Windows-Diagnosis-PLA/Operational' -MaxEvents 5 -ErrorAction SilentlyContinue |
                Where-Object Id -eq 2031 | Format-Table TimeCreated, Id, Message -AutoSize -Wrap
              Get-WinEvent -FilterHashtable @{ LogName = 'System'; ProviderName = 'Microsoft-Windows-WAS' } -MaxEvents 5 -ErrorAction SilentlyContinue |
                Format-Table TimeCreated, Id, Message -AutoSize -Wrap }
    }
} while ($op -ne '0')
