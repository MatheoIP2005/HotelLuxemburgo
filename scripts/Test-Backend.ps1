#Requires -Version 5.1
<#
.SYNOPSIS
  Verificacion estandar del backend: build del stack, tests Shared/Reservation/Finance y smoke opcional de Gateway.

.PARAMETER NoRestore
  Pasa --no-restore a los comandos dotnet test.

.PARAMETER GatewaySmoke
  Ejecuta scripts/Test-GatewayHealth.ps1 al final (requiere Gateway levantado).

.PARAMETER GatewayBaseUrl
  URL base del Gateway para el smoke opcional.
#>
param(
    [switch]$NoRestore,
    [switch]$GatewaySmoke,
    [string]$GatewayBaseUrl = "http://127.0.0.1:5000"
)

$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$Slnx = Join-Path $RepoRoot "HotelLux.Stack.slnx"

$results = [ordered]@{
    Build = "FAIL"
    Shared = "FAIL"
    Reservation = "FAIL"
    Finance = "FAIL"
    GatewaySmoke = if ($GatewaySmoke) { "FAIL" } else { "skipped" }
}

function Invoke-DotNetStep {
    param(
        [Parameter(Mandatory)][string]$Label,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    Write-Host ""
    Write-Host "=== $Label ===" -ForegroundColor Cyan
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Label fallo con codigo $LASTEXITCODE"
    }
}

try {
    Push-Location $RepoRoot

    Invoke-DotNetStep -Label "Build" -Arguments @("build", $Slnx, "-v", "minimal")
    $results.Build = "OK"

    $testArgs = @("test", "-v", "minimal")
    if ($NoRestore) { $testArgs += "--no-restore" }

    Invoke-DotNetStep -Label "Shared tests" -Arguments ($testArgs + @("tests\HotelLux.Shared.Tests\HotelLux.Shared.Tests.csproj"))
    $results.Shared = "OK"

    Invoke-DotNetStep -Label "Reservation tests" -Arguments ($testArgs + @("tests\HotelLux.Reservation.Business.Tests\HotelLux.Reservation.Business.Tests.csproj"))
    $results.Reservation = "OK"

    Invoke-DotNetStep -Label "Finance tests" -Arguments ($testArgs + @("tests\HotelLux.Finance.Business.Tests\HotelLux.Finance.Business.Tests.csproj"))
    $results.Finance = "OK"

    if ($GatewaySmoke) {
        Write-Host ""
        Write-Host "=== Gateway smoke ===" -ForegroundColor Cyan
        & powershell -ExecutionPolicy Bypass -File (Join-Path $RepoRoot "scripts\Test-GatewayHealth.ps1") -GatewayBaseUrl $GatewayBaseUrl
        if ($LASTEXITCODE -ne 0) {
            throw "Gateway smoke fallo con codigo $LASTEXITCODE"
        }
        $results.GatewaySmoke = "OK"
    }

    Write-Host ""
    Write-Host "Resumen:" -ForegroundColor Green
    Write-Host "Build: $($results.Build)"
    Write-Host "Shared tests: $($results.Shared)"
    Write-Host "Reservation tests: $($results.Reservation)"
    Write-Host "Finance tests: $($results.Finance)"
    Write-Host "Gateway smoke: $($results.GatewaySmoke)"
}
catch {
    Write-Host ""
    Write-Host "Resumen parcial:" -ForegroundColor Red
    Write-Host "Build: $($results.Build)"
    Write-Host "Shared tests: $($results.Shared)"
    Write-Host "Reservation tests: $($results.Reservation)"
    Write-Host "Finance tests: $($results.Finance)"
    Write-Host "Gateway smoke: $($results.GatewaySmoke)"
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
}
