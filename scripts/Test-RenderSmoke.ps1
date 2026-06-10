#Requires -Version 5.1
<#
.SYNOPSIS
  Smoke HTTP remoto para servicios desplegados en Render (o local apuntando a URLs publicas).

.PARAMETER RequireServiceReady
  Si esta activo, falla si algun servicio enviado devuelve distinto de 200 en /health/ready.
  Gateway siempre debe responder bien en health/live/ready/graphql/swagger.
#>
param(
    [Parameter(Mandatory)][string]$GatewayBaseUrl,
    [string]$AuditBaseUrl,
    [string]$AuthBaseUrl,
    [string]$AccommodationBaseUrl,
    [string]$ReservationBaseUrl,
    [string]$StayBaseUrl,
    [string]$FinanceBaseUrl,
    [switch]$RequireServiceReady
)

$ErrorActionPreference = "Stop"

function Normalize-Url([string]$Url) {
    if ([string]::IsNullOrWhiteSpace($Url)) { return $null }
    return $Url.Trim().TrimEnd('/')
}

function Get-ResponseText {
    param([Parameter(Mandatory)]$Response)

    if ($Response.Content -is [byte[]]) {
        return [System.Text.Encoding]::UTF8.GetString($Response.Content)
    }

    return [string]$Response.Content
}

function Invoke-StatusGet {
    param([Parameter(Mandatory)][string]$Uri)

    try {
        $r = Invoke-WebRequest -Uri $Uri -UseBasicParsing -Method GET -TimeoutSec 60
        return [int]$r.StatusCode
    } catch {
        $resp = $_.Exception.Response
        if ($resp) { return [int]$resp.StatusCode }
        return -1
    }
}

function Test-GetEndpoint {
    param(
        [Parameter(Mandatory)][string]$Label,
        [Parameter(Mandatory)][string]$Uri,
        [int]$ExpectedStatus = 200,
        [switch]$WarnOnly
    )

    Write-Host "GET $Uri"
    $code = Invoke-StatusGet -Uri $Uri

    if ($code -eq $ExpectedStatus) {
        Write-Host "OK: $Label -> $code" -ForegroundColor Green
        return "OK"
    }

    $msg = "$Label esperaba $ExpectedStatus, obtuvo $code"
    if ($WarnOnly) {
        Write-Host "WARNING: $msg" -ForegroundColor Yellow
        return "WARN"
    }

    throw $msg
}

function Test-ServiceReadiness {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$BaseUrl
    )

    Write-Host ""
    Write-Host "=== $Name ===" -ForegroundColor Cyan
    Test-GetEndpoint -Label "$Name health" -Uri "$BaseUrl/health" | Out-Null
    Test-GetEndpoint -Label "$Name health/live" -Uri "$BaseUrl/health/live" | Out-Null
    $ready = Test-GetEndpoint -Label "$Name health/ready" -Uri "$BaseUrl/health/ready" -WarnOnly:(-not $RequireServiceReady)
    if ($RequireServiceReady -and $ready -eq "WARN") {
        throw "$Name /health/ready no devolvio 200 y RequireServiceReady esta activo."
    }
    return $ready
}

$gateway = Normalize-Url $GatewayBaseUrl
$summary = [ordered]@{
    GatewayHealth = "FAIL"
    GatewayGraphQL = "FAIL"
    GatewaySwagger = "FAIL"
    AuditReady = "skipped"
    AuthReady = "skipped"
    AccommodationReady = "skipped"
    ReservationReady = "skipped"
    StayReady = "skipped"
    FinanceReady = "skipped"
}

try {
    Write-Host "=== Gateway ===" -ForegroundColor Cyan
    Test-GetEndpoint -Label "Gateway health" -Uri "$gateway/health" | Out-Null
    Test-GetEndpoint -Label "Gateway health/live" -Uri "$gateway/health/live" | Out-Null
    Test-GetEndpoint -Label "Gateway health/ready" -Uri "$gateway/health/ready" | Out-Null
    $summary.GatewayHealth = "OK"

    Write-Host "POST $gateway/graphql"
    $body = @{ query = "{ __typename }" } | ConvertTo-Json
    $gql = Invoke-WebRequest -Uri "$gateway/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing -TimeoutSec 60
    if ($gql.StatusCode -ne 200) {
        throw "GraphQL esperaba 200, obtuvo $($gql.StatusCode)"
    }
    $content = Get-ResponseText -Response $gql
    try {
        $json = $content | ConvertFrom-Json
    } catch {
        throw "GraphQL devolvio contenido no JSON: $content"
    }
    if ($json.data.__typename -ne "Query") {
        throw "GraphQL no devolvio __typename Query: $content"
    }
    Write-Host "OK: Gateway GraphQL -> 200 ($content)" -ForegroundColor Green
    $summary.GatewayGraphQL = "OK"

    Test-GetEndpoint -Label "Gateway Swagger" -Uri "$gateway/swagger/v1/swagger.json" | Out-Null
    $summary.GatewaySwagger = "OK"

    $serviceMap = [ordered]@{
        AuditReady = Normalize-Url $AuditBaseUrl
        AuthReady = Normalize-Url $AuthBaseUrl
        AccommodationReady = Normalize-Url $AccommodationBaseUrl
        ReservationReady = Normalize-Url $ReservationBaseUrl
        StayReady = Normalize-Url $StayBaseUrl
        FinanceReady = Normalize-Url $FinanceBaseUrl
    }

    foreach ($entry in $serviceMap.GetEnumerator()) {
        $key = $entry.Key
        $url = $entry.Value
        if (-not $url) { continue }
        $name = $key -replace 'Ready$',''
        $summary[$key] = Test-ServiceReadiness -Name $name -BaseUrl $url
    }

    Write-Host ""
    Write-Host "Resumen:" -ForegroundColor Green
    Write-Host "Gateway health: $($summary.GatewayHealth)"
    Write-Host "Gateway GraphQL: $($summary.GatewayGraphQL)"
    Write-Host "Gateway Swagger: $($summary.GatewaySwagger)"
    Write-Host "Audit ready: $($summary.AuditReady)"
    Write-Host "Auth ready: $($summary.AuthReady)"
    Write-Host "Accommodation ready: $($summary.AccommodationReady)"
    Write-Host "Reservation ready: $($summary.ReservationReady)"
    Write-Host "Stay ready: $($summary.StayReady)"
    Write-Host "Finance ready: $($summary.FinanceReady)"
}
catch {
    Write-Host ""
    Write-Host "Resumen parcial:" -ForegroundColor Red
    foreach ($k in $summary.Keys) { Write-Host "${k}: $($summary[$k])" }
    Write-Host ""
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
