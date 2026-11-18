#Requires -RunAsAdministrator
# ETAPA 1 — IP fixo, DNS inicial, fuso horário e nome do servidor (reinicia no final)
. "$PSScriptRoot\config.ps1"
Titulo 'ETAPA 1 — Rede e identidade do servidor'

$nic = Placa
if (-not $nic) { throw 'Nenhuma placa de rede ativa encontrada.' }
Ok "Placa de rede: $($nic.Name)"

Set-NetIPInterface -InterfaceIndex $nic.ifIndex -Dhcp Disabled
Get-NetIPAddress -InterfaceIndex $nic.ifIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue | Remove-NetIPAddress -Confirm:$false -ErrorAction SilentlyContinue
Get-NetRoute -InterfaceIndex $nic.ifIndex -DestinationPrefix '0.0.0.0/0' -ErrorAction SilentlyContinue | Remove-NetRoute -Confirm:$false -ErrorAction SilentlyContinue
New-NetIPAddress -InterfaceIndex $nic.ifIndex -IPAddress $IP -PrefixLength $Prefixo -DefaultGateway $Gateway | Out-Null
Set-DnsClientServerAddress -InterfaceIndex $nic.ifIndex -ServerAddresses $DnsInicial
Ok "IP fixo $IP/$Prefixo, gateway $Gateway, DNS $DnsInicial"

Set-TimeZone -Id 'E. South America Standard Time'
Ok 'Fuso horário: Brasília'

if ($env:COMPUTERNAME -ne $Hostname) {
    Rename-Computer -NewName $Hostname -Force
    Ok "Nome do servidor alterado para $Hostname"
    Aviso 'O servidor vai reiniciar em 10 segundos. Depois rode a ETAPA 2 (02-ad-dns.ps1).'
    Start-Sleep 10
    Restart-Computer -Force
} else {
    Ok "O servidor já se chama $Hostname. Pode seguir para a ETAPA 2."
}
