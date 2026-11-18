#Requires -RunAsAdministrator
# ETAPA 3 — DNS, OUs, grupos, usuários, política de senha, acesso remoto,
#           File Server com permissões NTFS, GPOs e auditoria
. "$PSScriptRoot\config.ps1"
Import-Module ActiveDirectory, GroupPolicy, DnsServer
Titulo 'ETAPA 3 — Identidade, pastas, GPO e auditoria'
$base = (Get-ADDomain).DistinguishedName

# ---------- DNS ----------
$nic = Placa
Set-DnsClientServerAddress -InterfaceIndex $nic.ifIndex -ServerAddresses 127.0.0.1
Set-DnsServerForwarder -IPAddress 8.8.8.8, 1.1.1.1
if (-not (Get-DnsServerResourceRecord -ZoneName $Dominio -Name $NomeApp -ErrorAction SilentlyContinue)) {
    Add-DnsServerResourceRecordA -ZoneName $Dominio -Name $NomeApp -IPv4Address $IP
}
Ok "DNS: encaminhadores 8.8.8.8/1.1.1.1 e registro $NomeApp.$Dominio -> $IP"

# ---------- OUs e grupos ----------
function OU($nome, $caminho) {
    if (-not (Get-ADOrganizationalUnit -Filter "Name -eq '$nome'" -SearchBase $caminho -SearchScope OneLevel -ErrorAction SilentlyContinue)) {
        New-ADOrganizationalUnit -Name $nome -Path $caminho -ProtectedFromAccidentalDeletion $true
    }
    "OU=$nome,$caminho"
}
$ouRaiz  = OU 'ZelaBairro' $base
$ouOper  = OU 'Operadores' $ouRaiz
$ouTI    = OU 'TI' $ouRaiz
$ouGrupo = OU 'Grupos' $ouRaiz
foreach ($g in 'GG_Operadores', 'GG_TI') {
    if (-not (Get-ADGroup -Filter "Name -eq '$g'" -ErrorAction SilentlyContinue)) {
        New-ADGroup -Name $g -GroupScope Global -GroupCategory Security -Path $ouGrupo
    }
}
Ok 'OUs ZelaBairro/Operadores, TI, Grupos e grupos GG_Operadores, GG_TI'

# ---------- Política de senha do domínio ----------
Set-ADDefaultDomainPasswordPolicy -Identity $Dominio -MinPasswordLength 10 -ComplexityEnabled $true `
    -LockoutThreshold 5 -LockoutDuration '00:15:00' -LockoutObservationWindow '00:15:00'
Ok 'Política de senha: mínimo 10 caracteres, complexa, bloqueio após 5 tentativas'

# ---------- Usuários ----------
Aviso 'Senha inicial para os usuários de teste (mínimo 10 caracteres, maiúscula, minúscula, número e símbolo):'
$senhaUsuarios = Read-Host 'Senha' -AsSecureString
$usuarios = @(
    @{ Sam='op.ana';   Nome='Ana Operadora';    OU=$ouOper; Grupo='GG_Operadores' },
    @{ Sam='op.bruno'; Nome='Bruno Operador';   OU=$ouOper; Grupo='GG_Operadores' },
    @{ Sam='ti.admin'; Nome='Administrador TI'; OU=$ouTI;   Grupo='GG_TI' }
)
foreach ($u in $usuarios) {
    if (-not (Get-ADUser -Filter "SamAccountName -eq '$($u.Sam)'" -ErrorAction SilentlyContinue)) {
        New-ADUser -Name $u.Nome -SamAccountName $u.Sam -UserPrincipalName "$($u.Sam)@$Dominio" `
            -Path $u.OU -AccountPassword $senhaUsuarios -Enabled $true -PasswordNeverExpires $true
    }
    Add-ADGroupMember -Identity $u.Grupo -Members $u.Sam -ErrorAction SilentlyContinue
}
Ok 'Usuários op.ana, op.bruno (Operadores) e ti.admin (TI)'

# ---------- Acesso remoto (RDP) ----------
Set-ItemProperty 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name fDenyTSConnections -Value 0
Set-ItemProperty 'HKLM:\System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name UserAuthentication -Value 1
Enable-NetFirewallRule -Name 'RemoteDesktop-UserMode-In-TCP', 'RemoteDesktop-UserMode-In-UDP' -ErrorAction SilentlyContinue
$rdpUsers = Get-ADGroup -Identity 'S-1-5-32-555'     # Remote Desktop Users (por SID: funciona em qualquer idioma)
Add-ADGroupMember -Identity $rdpUsers -Members 'GG_TI' -ErrorAction SilentlyContinue
Ok 'Área de Trabalho Remota ativada com NLA (somente GG_TI)'

# ---------- File Server + NTFS ----------
Install-WindowsFeature FS-FileServer | Out-Null
$pastaRel = Join-Path $PastaDados 'Relatorios'
New-Item -ItemType Directory -Path $pastaRel -Force | Out-Null
icacls $pastaRel /inheritance:r /grant:r '*S-1-5-18:(OI)(CI)F' '*S-1-5-32-544:(OI)(CI)F' "${NetBios}\GG_TI:(OI)(CI)F" "${NetBios}\GG_Operadores:(OI)(CI)M" | Out-Null
if (-not (Get-SmbShare -Name 'Relatorios' -ErrorAction SilentlyContinue)) {
    New-SmbShare -Name 'Relatorios' -Path $pastaRel -FullAccess "${NetBios}\GG_TI" -ChangeAccess "${NetBios}\GG_Operadores" | Out-Null
}
Set-Content -Path (Join-Path $pastaRel 'LEIA-ME.txt') -Value 'Relatórios de atendimento do ZelaBairro. Acesso: GG_Operadores (modificar) e GG_TI (controle total).' -Encoding UTF8
Ok "Compartilhamento \\$Hostname\Relatorios com NTFS: SYSTEM/Administradores/GG_TI total, GG_Operadores modificar"

# ---------- Auditoria ----------
# Força o uso das subcategorias avançadas (evita que a política legada sobrescreva)
Set-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa' -Name SCENoApplyLegacyAuditPolicy -Value 1 -Type DWord
$subcategorias = @{
    'Logon'                      = '{0CCE9215-69AE-11D9-BED3-505054503030}'
    'Sistema de arquivos'        = '{0CCE921D-69AE-11D9-BED3-505054503030}'
    'Gerenciamento de contas'    = '{0CCE9235-69AE-11D9-BED3-505054503030}'
    'Alteração de política'      = '{0CCE922F-69AE-11D9-BED3-505054503030}'
}
foreach ($s in $subcategorias.GetEnumerator()) { auditpol /set /subcategory:"$($s.Value)" /success:enable /failure:enable | Out-Null }
$acl = Get-Acl -Path $pastaRel -Audit
$todos = New-Object System.Security.Principal.SecurityIdentifier 'S-1-1-0'
$regra = New-Object System.Security.AccessControl.FileSystemAuditRule($todos, 'Write, Delete, DeleteSubdirectoriesAndFiles', 'ContainerInherit, ObjectInherit', 'None', 'Success, Failure')
$acl.AddAuditRule($regra); Set-Acl -Path $pastaRel -AclObject $acl
Ok 'Auditoria: logon, sistema de arquivos (pasta Relatorios), contas e políticas'

# ---------- GPOs ----------
$gpoOper = 'ZB - Politica dos Operadores'
if (-not (Get-GPO -Name $gpoOper -ErrorAction SilentlyContinue)) { New-GPO -Name $gpoOper -Comment 'Bloqueio de tela e restrições para operadores do ZelaBairro' | Out-Null }
$chaveDesk = 'HKCU\Software\Policies\Microsoft\Windows\Control Panel\Desktop'
Set-GPRegistryValue -Name $gpoOper -Key $chaveDesk -ValueName 'ScreenSaveActive'    -Type String -Value '1' | Out-Null
Set-GPRegistryValue -Name $gpoOper -Key $chaveDesk -ValueName 'ScreenSaverIsSecure' -Type String -Value '1' | Out-Null
Set-GPRegistryValue -Name $gpoOper -Key $chaveDesk -ValueName 'ScreenSaveTimeOut'   -Type String -Value '600' | Out-Null
Set-GPRegistryValue -Name $gpoOper -Key 'HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer' -ValueName 'NoControlPanel' -Type DWord -Value 1 | Out-Null
if ((Get-GPInheritance -Target $ouOper).GpoLinks.DisplayName -notcontains $gpoOper) { New-GPLink -Name $gpoOper -Target $ouOper | Out-Null }

$gpoAviso = 'ZB - Aviso de Logon'
if (-not (Get-GPO -Name $gpoAviso -ErrorAction SilentlyContinue)) { New-GPO -Name $gpoAviso -Comment 'Aviso legal exibido antes do logon' | Out-Null }
$chaveSys = 'HKLM\Software\Microsoft\Windows\CurrentVersion\Policies\System'
Set-GPRegistryValue -Name $gpoAviso -Key $chaveSys -ValueName 'legalnoticecaption' -Type String -Value 'ZelaBairro — Sistema da Subprefeitura' | Out-Null
Set-GPRegistryValue -Name $gpoAviso -Key $chaveSys -ValueName 'legalnoticetext' -Type String -Value 'Acesso restrito a pessoas autorizadas. As ações neste servidor são registradas e auditadas.' | Out-Null
if ((Get-GPInheritance -Target $base).GpoLinks.DisplayName -notcontains $gpoAviso) { New-GPLink -Name $gpoAviso -Target $base | Out-Null }
gpupdate /force | Out-Null
Ok "GPOs '$gpoOper' (OU Operadores) e '$gpoAviso' (domínio) criadas e vinculadas"

Titulo 'ETAPA 3 concluída. Próximo: 04-iis-aplicacao.ps1'
