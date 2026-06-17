# Publicar Okomos en GitHub
# Requisitos: Git y GitHub CLI (gh) instalados y autenticados
#   winget install Git.Git
#   winget install GitHub.cli
#   gh auth login

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

Write-Host "Inicializando repositorio..." -ForegroundColor Cyan
if (-not (Test-Path ".git")) {
    git init
    git branch -M main
}

Write-Host "Agregando archivos..." -ForegroundColor Cyan
git add .

$commitMessage = @"
Add Okomos modular monolith with vertical slices, CQRS, and test suite.

Includes Identity, Billing, Accounting, and Inventory modules with multitenancy, outbox pattern, and Azure-ready API host.
"@

Write-Host "Creando commit..." -ForegroundColor Cyan
git commit -m $commitMessage
if ($LASTEXITCODE -ne 0) {
    Write-Host "Sin cambios nuevos para commitear, o commit ya existe." -ForegroundColor Yellow
}

$repoName = "Okomos"
$owner = gh api user -q .login

Write-Host "Creando repositorio en GitHub: $owner/$repoName" -ForegroundColor Cyan
gh repo create $repoName --public --source=. --remote=origin --push --description "Modular monolith .NET 10 - Okomos ERP"

Write-Host ""
Write-Host "Listo: https://github.com/$owner/$repoName" -ForegroundColor Green
