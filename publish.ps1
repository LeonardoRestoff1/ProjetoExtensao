# ============================================================
# publish.ps1 — Gera os artefatos de produção da BibliotecaOnline
#
# USO:  .\publish.ps1
# SAÍDA: pasta "publish/" pronta para copiar para o servidor
# ============================================================

$ProjectDir = Join-Path $PSScriptRoot "BibliotecaOnline"
$OutputDir  = Join-Path $PSScriptRoot "publish"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  BibliotecaOnline — Publicação de produção"      -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Limpar pasta anterior
if (Test-Path $OutputDir) {
    Write-Host "→ Limpando publicação anterior..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}

# 2. Restaurar dependências
Write-Host "→ Restaurando pacotes NuGet..." -ForegroundColor Yellow
dotnet restore $ProjectDir
if ($LASTEXITCODE -ne 0) { Write-Host "ERRO: falha no restore." -ForegroundColor Red; exit 1 }

# 3. Publicar em Release
Write-Host "→ Publicando em modo Release..." -ForegroundColor Yellow
dotnet publish $ProjectDir `
    --configuration Release `
    --output $OutputDir `
    --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "ERRO: falha na publicação." -ForegroundColor Red; exit 1 }

# 4. Confirmar resultado
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Publicação concluída com sucesso!"              -ForegroundColor Green
Write-Host "  Arquivos em: $OutputDir"                        -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "  • Para testar localmente:"
Write-Host "      cd publish"
Write-Host "      dotnet BibliotecaOnline.dll"
Write-Host ""
Write-Host "  • Para deploy via Docker:"
Write-Host "      docker build -t biblioteca-online ."
Write-Host "      docker run -p 8080:8080 -v biblioteca-data:/data biblioteca-online"
Write-Host ""
Write-Host "  • Para deploy no Railway: veja DEPLOY.md"
Write-Host ""
