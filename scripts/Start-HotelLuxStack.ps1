#Requires -Version 5.1
<#
.SYNOPSIS
  Arranca Accommodation, Reservation, Stay y Gateway en ventanas separadas (orden: servicios primero, gateway al final).

.NOTES
  Requiere PostgreSQL local según appsettings de cada API. Espera ~3 s entre servicios y el gateway.
#>
$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

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
           "dotnet run --project `"$proj`"`r`n" +
           "Write-Host 'Proceso terminado. Pulsa Enter.' -ForegroundColor Yellow`r`n" +
           "Read-Host"
    Start-Process powershell.exe -ArgumentList @("-NoExit", "-Command", $cmd) | Out-Null
}

Write-Host "HotelLux stack: raiz = $RepoRoot" -ForegroundColor Green

dotnet build (Join-Path $RepoRoot "HotelLux.Stack.slnx") -v minimal
if ($LASTEXITCODE -ne 0) { throw "dotnet build fallo." }

Start-ServiceWindow "HotelLux.Auth (5101)" "HotelLux.Auth\HotelLux.Auth.API\HotelLux.Auth.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Accommodation (5002)" "HotelLux.Accommodation\HotelLux.Accommodation.API\HotelLux.Accommodation.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Reservation (5003)" "HotelLux.Reservation\HotelLux.Reservation.API\HotelLux.Reservation.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Stay (5004)" "HotelLux.Stay\HotelLux.Stay.API\HotelLux.Stay.API.csproj"
Start-Sleep -Seconds 2
Start-ServiceWindow "HotelLux.Gateway (5000)" "HotelLux.Gateway\HotelLux.Gateway.csproj"

Write-Host ""
Write-Host "Gateway publico: http://127.0.0.1:5000/health" -ForegroundColor Green
Write-Host "Ejemplos (con servicios y BD en marcha):" -ForegroundColor Green
Write-Host "  GET  http://127.0.0.1:5000/api/v1/accommodations/search?destino=...&fecha_entrada=...&fecha_salida=...&num_adultos=1&num_habitaciones=1"
Write-Host "  GET  http://127.0.0.1:5000/api/v1/accommodations/{sucursalGuid}"
Write-Host ""
