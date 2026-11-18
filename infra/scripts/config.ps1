# =====================================================================
#  ZelaBairro — configurações da infraestrutura (usado por todos os scripts)
# =====================================================================
$Hostname     = 'SRV-ZELABAIRRO'
$IP           = '10.0.2.15'          # IP fixo (rede NAT do VirtualBox)
$Prefixo      = 24
$Gateway      = '10.0.2.2'           # gateway da rede NAT do VirtualBox
$DnsInicial   = '10.0.2.3'           # DNS do VirtualBox (antes do AD)
$Dominio      = 'zelabairro.local'
$NetBios      = 'ZELABAIRRO'
$NomeApp      = 'app'                # registro DNS: app.zelabairro.local
$Site         = 'ZelaBairro'
$Pool         = 'ZelaBairroPool'
$PastaSite    = 'C:\inetpub\zelabairro'
$PastaDados   = 'C:\ZelaBairro'
$OracleUser   = 'rm560796'
$OracleFonte  = 'oracle.fiap.com.br:1521/ORCL'
$RepoZip      = 'https://github.com/jBarra15/zelabairro/archive/refs/heads/main.zip'

function Titulo($texto) { Write-Host ''; Write-Host ('=' * 70) -ForegroundColor DarkGreen; Write-Host "  $texto" -ForegroundColor Green; Write-Host ('=' * 70) -ForegroundColor DarkGreen }
function Ok($texto)     { Write-Host "  [OK] $texto" -ForegroundColor Green }
function Aviso($texto)  { Write-Host "  [!]  $texto" -ForegroundColor Yellow }
function Placa()        { Get-NetAdapter | Where-Object Status -eq 'Up' | Sort-Object ifIndex | Select-Object -First 1 }
