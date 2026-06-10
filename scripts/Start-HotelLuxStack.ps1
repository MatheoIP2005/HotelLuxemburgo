#Requires -Version 5.1
<#
.SYNOPSIS
  Arranca RabbitMQ (opcional), Audit, Auth, Finance, Accommodation, Reservation, Stay y Gateway.

.PARAMETER SkipRabbitMq
  No intenta levantar RabbitMQ con Docker. Los servicios arrancan igual; /health/ready puede devolver 503.

.PARAMETER RequireRabbitMq
  Falla si Docker no esta disponible o RabbitMQ no puede levantarse.

.NOTES
  Requiere PostgreSQL local segun appsettings de cada API.
  Puertos: 5000 Gateway, 5001 Auth, 5002 Accommodation, 5003 Reservation, 5004 Stay, 5005 Finance, 5008 Audit.
  RabbitMQ local: 5672 AMQP, 15672 Management UI.
#>
param(
    [switch]$SkipRabbitMq,
    [switch]$RequireRabbitMq
)

$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$ComposeFile = Join-Path $RepoRoot "docker-compose.rabbitmq.yml"

$ServicePorts = @(5000, 5001, 5002, 5003, 5004, 5005, 5008)
$RabbitPorts = @(5672, 15672)

function Test-DockerAvailable {
    try {
        docker info 2>$null | Out-Null
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

function Start-ServiceWindow {
    param(
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][string]$ProjectRelativePath
    )
    $proj = Join-Path $RepoRoot $ProjectRelativePath
    if (-not (Test-Path $proj)) {
        throw "No se encuentra el proyecto: $proj"
    }
    $cmd = "title $Title`r`n" +
           "Write-Host '=== $Title ===' -ForegroundColor Cyan`r`n" +
           "Set-Location `"$RepoRoot`"`r`n" +
           "dotnet run --no-build --no-launch-profile --project `"$proj`"`r`n" +
           "Write-Host 'Proceso terminado. Pulsa Enter.' -ForegroundColor Yellow`r`n" +
           "Read-Host"
    Start-Process powershell.exe -ArgumentList @("-NoExit", "-Command", $cmd) | Out-Null
}

function Start-RabbitMqLocal {
    if (-not (Test-Path $ComposeFile)) {
        throw "No se encuentra $ComposeFile"
    }
    Write-Host "Levantando RabbitMQ con docker compose..." -ForegroundColor Cyan
    docker compose -f $ComposeFile up -d
    if ($LASTEXITCODE -ne 0) {
        throw "docker compose fallo con codigo $LASTEXITCODE"
    }

    $ready = $false
    for ($i = 1; $i -le 12; $i++) {
        try {
            $r = Invoke-WebRequest -Uri "http://localhost:15672" -UseBasicParsing -TimeoutSec 5
            if ($r.StatusCode -eq 200) {
                $ready = $true
                break
            }
        } catch {
            Write-Host "Esperando RabbitMQ Management UI... ($i/12)" -ForegroundColor DarkYellow
            Start-Sleep -Seconds 5
        }
    }
    if (-not $ready) {
        throw "RabbitMQ no respondio en http://localhost:15672"
    }
    Write-Host "RabbitMQ listo (AMQP :5672, UI http://localhost:15672)." -ForegroundColor Green
}

Write-Host "HotelLux stack: raiz = $RepoRoot" -ForegroundColor Green

$portsToCheck = $ServicePorts
if (-not $SkipRabbitMq) { $portsToCheck += $RabbitPorts }
$busy = $portsToCheck | Where-Object {
    Get-NetTCPConnection -LocalPort $_ -State Listen -ErrorAction SilentlyContinue
}
if ($busy.Count -gt 0) {
    Write-Host "Puertos en uso: $($busy -join ', '). Los servicios YA pueden estar corriendo." -ForegroundColor Yellow
    Write-Host "  - Gateway: http://127.0.0.1:5000/health" -ForegroundColor Yellow
    Write-Host "  - Para reiniciar limpio: .\scripts\Stop-HotelLuxPorts.ps1 y vuelve a ejecutar este script." -ForegroundColor Yellow
    exit 0
}

if (-not $SkipRabbitMq) {
    if (Test-DockerAvailable) {
        try {
            Start-RabbitMqLocal
        } catch {
            $msg = $_.Exception.Message
            if ($RequireRabbitMq) {
                throw "RequireRabbitMq activo y RabbitMQ no pudo levantarse: $msg"
            }
            Write-Host "WARNING: RabbitMQ no se levanto ($msg). /health/ready puede devolver 503." -ForegroundColor Yellow
        }
    } else {
        if ($RequireRabbitMq) {
            throw "RequireRabbitMq activo pero Docker Desktop no esta corriendo."
        }
        Write-Host "WARNING: Docker Desktop no esta corriendo. Se omitio RabbitMQ; /health/ready puede devolver 503." -ForegroundColor Yellow
    }
} else {
    Write-Host "SkipRabbitMq activo: no se levanta RabbitMQ." -ForegroundColor DarkYellow
}

Write-Host "Compilando solucion..." -ForegroundColor Cyan
dotnet build (Join-Path $RepoRoot "HotelLux.Stack.slnx") -v minimal
if ($LASTEXITCODE -ne 0) { throw "dotnet build fallo." }

Start-ServiceWindow "HotelLux.Audit (5008)" "HotelLux.Audit\HotelLux.Audit.API\HotelLux.Audit.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Auth (5001)" "HotelLux.Auth\HotelLux.Auth.API\HotelLux.Auth.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Finance (5005)" "HotelLux.Finance\HotelLux.Finance.API\HotelLux.Finance.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Accommodation (5002)" "HotelLux.Accommodation\HotelLux.Accommodation.API\HotelLux.Accommodation.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Reservation (5003)" "HotelLux.Reservation\HotelLux.Reservation.API\HotelLux.Reservation.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Stay (5004)" "HotelLux.Stay\HotelLux.Stay.API\HotelLux.Stay.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Gateway (5000)" "HotelLux.Gateway\HotelLux.Gateway.csproj"

Write-Host ""
Write-Host "Stack iniciado. URLs utiles:" -ForegroundColor Green
Write-Host "  Gateway health:  http://127.0.0.1:5000/health"
Write-Host "  Gateway ready:   http://127.0.0.1:5000/health/ready"
Write-Host "  Gateway Swagger: http://127.0.0.1:5000/swagger"
Write-Host "  Gateway GraphQL: http://127.0.0.1:5000/graphql"
if (-not $SkipRabbitMq) {
    Write-Host "  RabbitMQ UI:     http://localhost:15672 (guest/guest)"
}
Write-Host "  Audit ready:     http://127.0.0.1:5008/health/ready"
Write-Host "  Accommodation ready: http://127.0.0.1:5002/health/ready"
Write-Host "  Reservation ready:   http://127.0.0.1:5003/health/ready"
Write-Host ""
