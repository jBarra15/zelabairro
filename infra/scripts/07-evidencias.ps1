#Requires -RunAsAdministrator
# EVIDÊNCIAS — gera um arquivo .txt por item em C:\ZelaBairro\evidencias (tire prints destes resultados)
. "$PSScriptRoot\config.ps1"
Import-Module ActiveDirectory, GroupPolicy, WebAdministration
$dir = Join-Path $PastaDados 'evidencias'
New-Item -ItemType Directory -Path $dir -Force | Out-Null
Titulo "Gerando evidências em $dir"

function Ev($arquivo, [scriptblock]$bloco) {
    $saida = try { & $bloco 2>&1 | Out-String -Width 200 } catch { $_ | Out-String }
    $cab = "### $arquivo — gerado em $(Get-Date -f 'dd/MM/yyyy HH:mm:ss') no servidor $env:COMPUTERNAME`r`n"
    Set-Content -Path (Join-Path $dir "$arquivo.txt") -Value ($cab + $saida) -Encoding UTF8
    Ok $arquivo
}

Ev '01-hostname-e-ip'        { hostname; ipconfig /all }
Ev '02-nslookup'             { nslookup "$NomeApp.$Dominio"; nslookup "$Hostname.$Dominio"; nslookup github.com }
Ev '03-test-netconnection'   { Test-NetConnection localhost -Port 80; Test-NetConnection "$NomeApp.$Dominio" -Port 80; Test-NetConnection oracle.fiap.com.br -Port 1521 }
Ev '04-dominio-ad'           { Get-ADDomain | Format-List Name, DNSRoot, NetBIOSName, DomainMode, PDCEmulator; Get-ADForest | Format-List Name, ForestMode, GlobalCatalogs }
Ev '05-ous-grupos-usuarios'  { Get-ADOrganizationalUnit -Filter * | Format-Table Name, DistinguishedName -AutoSize
                                Get-ADGroupMember GG_Operadores | Format-Table Name, SamAccountName; Get-ADGroupMember GG_TI | Format-Table Name, SamAccountName
                                Get-ADDefaultDomainPasswordPolicy }
Ev '06-funcoes-instaladas'   { Get-WindowsFeature | Where-Object Installed | Format-Table Name, DisplayName -AutoSize }
Ev '07-iis-site-e-pool'      { Get-Website | Format-Table Name, State, PhysicalPath, Bindings -AutoSize
                                Get-ItemProperty "IIS:\AppPools\$Pool" | Format-List name, state, managedRuntimeVersion, startMode, autoStart
                                Get-ItemProperty "IIS:\AppPools\$Pool" -Name failure | Format-List rapidFailProtection, rapidFailProtectionMaxCrashes, rapidFailProtectionInterval
                                & "$env:ProgramFiles\dotnet\dotnet.exe" --list-runtimes }
Ev '08-permissoes-ntfs'      { icacls $PastaSite; icacls (Join-Path $PastaSite 'uploads'); icacls (Join-Path $PastaDados 'Relatorios')
                                Get-SmbShare -Name Relatorios | Format-List; Get-SmbShareAccess -Name Relatorios | Format-Table -AutoSize }
Ev '09-gpo'                  { Get-GPO -All | Format-Table DisplayName, GpoStatus, ModificationTime -AutoSize
                                Get-GPInheritance -Target "OU=Operadores,OU=ZelaBairro,$((Get-ADDomain).DistinguishedName)" }
Ev '10-auditoria'            { auditpol /get /category:* }
Ev '11-monitoramento'        { logman query; logman query ZB-Alerta-CPU; logman query ZB-Alerta-Memoria; logman query ZB-Alerta-Disco
                                Get-ScheduledTask -TaskName 'ZB-Verificar-Saude' | Format-List TaskName, State, Description }
Ev '12-recuperacao-servicos' { sc.exe qfailure W3SVC; sc.exe qfailure WAS }
Ev '13-aplicacao-no-ar'      { Invoke-WebRequest 'http://localhost/api/saude' -UseBasicParsing | Select-Object StatusCode, Content | Format-List
                                Invoke-WebRequest 'http://localhost/api/ocorrencias/resumo' -UseBasicParsing | Select-Object StatusCode, Content | Format-List }
Ev '14-eventos'              { Get-WinEvent -FilterHashtable @{ LogName = 'Application'; ProviderName = 'ZelaBairro' } -MaxEvents 20 -ErrorAction SilentlyContinue | Format-Table TimeCreated, Id, LevelDisplayName, Message -Wrap
                                Get-WinEvent -LogName 'Microsoft-Windows-Diagnosis-PLA/Operational' -MaxEvents 10 -ErrorAction SilentlyContinue | Format-Table TimeCreated, Id, Message -Wrap
                                Get-WinEvent -FilterHashtable @{ LogName = 'Security'; Id = 4624, 4625, 4720, 4663 } -MaxEvents 20 -ErrorAction SilentlyContinue | Format-Table TimeCreated, Id, Message -Wrap }
Ev '15-logs-iis'             { $l = Get-ChildItem 'C:\inetpub\logs\LogFiles' -Recurse -Filter *.log | Sort-Object LastWriteTime -Descending | Select-Object -First 1
                                "Arquivo: $($l.FullName)"; Get-Content $l.FullName -Tail 25 }

Titulo 'Evidências geradas. Abra a pasta e tire prints:'
explorer.exe $dir
