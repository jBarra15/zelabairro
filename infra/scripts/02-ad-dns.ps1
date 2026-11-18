#Requires -RunAsAdministrator
# ETAPA 2 — Active Directory + DNS: cria o domínio zelabairro.local (reinicia sozinho no final)
. "$PSScriptRoot\config.ps1"
Titulo "ETAPA 2 — Active Directory e DNS ($Dominio)"

Install-WindowsFeature AD-Domain-Services, DNS -IncludeManagementTools | Out-Null
Ok 'Funções AD DS e DNS instaladas'

Aviso 'Escolha a senha do modo de restauração do AD (DSRM). Use letras maiúsculas, minúsculas, números e símbolo.'
$dsrm = Read-Host 'Senha DSRM' -AsSecureString

Aviso 'Criando a floresta e o domínio. O servidor reinicia sozinho (leva alguns minutos).'
Aviso "Depois, entre como $NetBios\Administrator e rode a ETAPA 3 (03-identidade-pastas-gpo.ps1)."
Install-ADDSForest -DomainName $Dominio -DomainNetbiosName $NetBios -InstallDns `
    -SafeModeAdministratorPassword $dsrm -Force -NoRebootOnCompletion:$false
