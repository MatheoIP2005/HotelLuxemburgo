#Requires -Version 5.1
param(
    [string]$GatewayBaseUrl = "http://127.0.0.1:5000"
)
$ErrorActionPreference = "Stop"
$base = $GatewayBaseUrl.TrimEnd('/')
$health = "$base/health"
Write-Host "GET $health"
$r = Invoke-WebRequest -Uri $health -UseBasicParsing -Method GET
if ($r.StatusCode -ne 200) { throw "Health esperaba 200, obtuvo $($r.StatusCode)" }
Write-Host "OK: gateway responde $($r.StatusCode)" -ForegroundColor Green
