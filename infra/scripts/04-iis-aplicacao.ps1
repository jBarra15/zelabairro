#Requires -RunAsAdministrator
# ETAPA 4 — IIS + .NET 8 Hosting Bundle + publicação da API ZelaBairro, pool, site,
#           permissões NTFS, logs e recuperação automática de serviços
. "$PSScriptRoot\config.ps1"
Titulo 'ETAPA 4 — IIS e aplicação ZelaBairro'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$ProgressPreference = 'SilentlyContinue'
$inst = Join-Path $PastaDados 'instaladores'
New-Item -ItemType Directory -Path $inst -Force | Out-Null

# ---------- IIS ----------
Install-WindowsFeature Web-Server, Web-Common-Http, Web-Http-Logging, Web-Log-Libraries, Web-Request-Monitor, `
    Web-Http-Tracing, Web-Stat-Compression, Web-Filtering, Web-Mgmt-Console, Web-Scripting-Tools -IncludeManagementTools | Out-Null
Import-Module WebAdministration
Ok 'IIS instalado (logs, rastreamento de requisições, filtragem, console de gerenciamento)'

# ---------- .NET 8: Hosting Bundle (runtime + módulo do IIS) e SDK (para publicar) ----------
$bundle = Join-Path $inst 'dotnet-hosting-8.exe'
if (-not (Test-Path $bundle)) { Invoke-WebRequest 'https://aka.ms/dotnet/8.0/dotnet-hosting-win.exe' -OutFile $bundle -UseBasicParsing }
Start-Process $bundle -ArgumentList '/install', '/quiet', '/norestart' -Wait
Ok '.NET 8 Hosting Bundle instalado (ASP.NET Core Module V2)'

$sdkDir = 'C:\dotnet-sdk'
if (-not (Test-Path "$sdkDir\dotnet.exe")) {
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile "$inst\dotnet-install.ps1" -UseBasicParsing
    & "$inst\dotnet-install.ps1" -Channel 8.0 -InstallDir $sdkDir | Out-Null
}
Ok '.NET 8 SDK disponível para publicar a aplicação'
net stop was /y | Out-Null; net start w3svc | Out-Null

# ---------- Código-fonte ----------
$fonte = Resolve-Path (Join-Path $PSScriptRoot '..\..\api\ZelaBairro.Api') -ErrorAction SilentlyContinue
if (-not $fonte) {
    Invoke-WebRequest $RepoZip -OutFile "$inst\repo.zip" -UseBasicParsing
    Expand-Archive "$inst\repo.zip" -DestinationPath $inst -Force
    $fonte = Resolve-Path "$inst\zelabairro-main\api\ZelaBairro.Api"
}
Ok "Código-fonte: $fonte"

# ---------- Pool de aplicativos ----------
if (-not (Test-Path "IIS:\AppPools\$Pool")) { New-WebAppPool -Name $Pool | Out-Null }
$p = "IIS:\AppPools\$Pool"
Set-ItemProperty $p -Name managedRuntimeVersion -Value ''                     # .NET Core: sem código gerenciado
Set-ItemProperty $p -Name startMode -Value 'AlwaysRunning'
Set-ItemProperty $p -Name autoStart -Value $true
Set-ItemProperty $p -Name processModel.idleTimeout -Value '00:00:00'
Set-ItemProperty $p -Name failure.rapidFailProtection -Value $true
Set-ItemProperty $p -Name failure.rapidFailProtectionMaxCrashes -Value 10
Set-ItemProperty $p -Name failure.rapidFailProtectionInterval -Value '00:05:00'
Set-ItemProperty $p -Name recycling.periodicRestart.time -Value '00:00:00'
Clear-ItemProperty $p -Name recycling.periodicRestart.schedule -ErrorAction SilentlyContinue
Set-ItemProperty $p -Name recycling.periodicRestart.schedule -Value @{ value = '03:00:00' }
Set-ItemProperty $p -Name recycling.logEventOnRecycle -Value 'Time, Requests, Schedule, Memory, IsapiUnhealthy, OnDemand, ConfigChange, PrivateMemory'
Ok "Pool ${Pool}: sem código gerenciado, sempre ativo, proteção contra falhas rápidas, reciclagem diária às 03:00"

# ---------- Senha do banco (variável de ambiente só deste pool, fora do código) ----------
Aviso "Digite a senha do Oracle da FIAP do usuário ${OracleUser}:"
$sec = Read-Host 'Senha Oracle' -AsSecureString
$plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec))
$conn = "User Id=$OracleUser;Password=$plain;Data Source=$OracleFonte"
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"
& $appcmd set config -section:system.applicationHost/applicationPools "/-[name='$Pool'].environmentVariables.[name='ConnectionStrings__Oracle']" /commit:apphost 2>$null | Out-Null
& $appcmd set config -section:system.applicationHost/applicationPools "/+[name='$Pool'].environmentVariables.[name='ConnectionStrings__Oracle',value='$conn']" /commit:apphost | Out-Null
$plain = $null; $conn = $null
Ok 'Connection string guardada como variável de ambiente do pool (applicationHost.config, só administradores leem)'

# ---------- Publicação ----------
Stop-WebAppPool -Name $Pool -ErrorAction SilentlyContinue
Start-Sleep 3
& "$sdkDir\dotnet.exe" publish "$fonte\ZelaBairro.Api.csproj" -c Release -o $PastaSite --nologo
if ($LASTEXITCODE -ne 0) { throw 'Falha ao publicar a aplicação.' }
New-Item -ItemType Directory -Path (Join-Path $PastaSite 'uploads') -Force | Out-Null
Ok "Aplicação publicada em $PastaSite"

# ---------- Permissões NTFS da aplicação ----------
icacls $PastaSite /inheritance:r /grant:r '*S-1-5-18:(OI)(CI)F' '*S-1-5-32-544:(OI)(CI)F' "IIS AppPool\${Pool}:(OI)(CI)RX" "${NetBios}\GG_TI:(OI)(CI)RX" | Out-Null
icacls (Join-Path $PastaSite 'uploads') /grant "IIS AppPool\${Pool}:(OI)(CI)M" | Out-Null
Ok "NTFS: pool com leitura/execução no site e modificação apenas em uploads"

# ---------- Site ----------
Get-Website | Where-Object Name -ne $Site | ForEach-Object { Stop-Website -Name $_.Name; Remove-Website -Name $_.Name }
if (-not (Get-Website -Name $Site)) {
    New-Website -Name $Site -PhysicalPath $PastaSite -ApplicationPool $Pool -Port 80 -Force | Out-Null
}
Set-ItemProperty "IIS:\Sites\$Site" -Name applicationDefaults.preloadEnabled -Value $true
Set-ItemProperty "IIS:\Sites\$Site" -Name logFile.logFormat -Value 'W3C'
Set-ItemProperty "IIS:\Sites\$Site" -Name logFile.period -Value 'Daily'
Ok "Site $Site na porta 80 (http://$NomeApp.$Dominio), logs W3C diários"

# ---------- Recuperação automática dos serviços do IIS ----------
foreach ($svc in 'W3SVC', 'WAS') {
    sc.exe failure $svc reset= 86400 actions= restart/5000/restart/10000/restart/30000 | Out-Null
    sc.exe failureflag $svc 1 | Out-Null
}
Ok 'Serviços W3SVC e WAS reiniciam sozinhos em caso de falha (5 s, 10 s, 30 s)'

Start-WebAppPool -Name $Pool -ErrorAction SilentlyContinue
Start-Website -Name $Site -ErrorAction SilentlyContinue
Start-Sleep 5
try {
    $r = Invoke-WebRequest 'http://localhost/api/saude' -UseBasicParsing -TimeoutSec 60
    Ok "Aplicação respondeu: $($r.Content)"
} catch {
    Aviso "A aplicação ainda não respondeu: $($_.Exception.Message)"
    Aviso 'Confira a senha do Oracle e o acesso à internet da VM; veja os logs em Visualizador de Eventos > Application.'
}
Titulo "ETAPA 4 concluída. Abra http://$NomeApp.$Dominio no navegador da VM. Próximo: 05-monitoramento.ps1"
