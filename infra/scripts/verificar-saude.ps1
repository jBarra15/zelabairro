# Executado pelo Agendador de Tarefas a cada minuto (como SYSTEM).
# Recupera automaticamente o serviço e o pool do IIS e registra tudo no Visualizador de Eventos.
$Pool = 'ZelaBairroPool'; $Fonte = 'ZelaBairro'
Import-Module WebAdministration
if (-not [Diagnostics.EventLog]::SourceExists($Fonte)) { New-EventLog -LogName Application -Source $Fonte }
function Evento($id, $tipo, $msg) { Write-EventLog -LogName Application -Source $Fonte -EventId $id -EntryType $tipo -Message $msg }

foreach ($s in 'WAS', 'W3SVC') {
    if ((Get-Service $s).Status -ne 'Running') {
        Start-Service $s
        Evento 1002 Warning "Serviço $s estava parado e foi reiniciado automaticamente pelo monitor do ZelaBairro."
    }
}
if ((Get-WebAppPoolState -Name $Pool).Value -ne 'Started') {
    Start-WebAppPool -Name $Pool
    Evento 1001 Warning "Pool $Pool estava parado e foi reiniciado automaticamente pelo monitor do ZelaBairro."
}
try {
    Invoke-WebRequest 'http://localhost/api/saude' -UseBasicParsing -TimeoutSec 20 | Out-Null
} catch {
    $codigo = $_.Exception.Response.StatusCode.value__
    if ($codigo -eq 503 -and $_.ErrorDetails.Message -match 'degradado') {
        Evento 1003 Warning 'A aplicação está no ar, mas o banco Oracle não respondeu (status degradado).'
    } else {
        Restart-WebAppPool -Name $Pool -ErrorAction SilentlyContinue
        Evento 1004 Error "A aplicação não respondeu (código $codigo). O pool $Pool foi reciclado automaticamente."
    }
}
