$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $repoRoot "docker-compose.rabbitmq.yml"

Write-Host "Levantando RabbitMQ desde $composeFile ..."
docker compose -f $composeFile up -d

if ($LASTEXITCODE -ne 0) {
    throw "docker compose falló con código $LASTEXITCODE"
}

Write-Host "RabbitMQ iniciado. Management UI: http://localhost:15672 (guest/guest)"
