#Requires -Version 5.1
param(
    [string]$GatewayBaseUrl = "http://127.0.0.1:5000",
    [switch]$RequireReady
)

$ErrorActionPreference = "Stop"
$base = $GatewayBaseUrl.TrimEnd('/')

function Get-ResponseText {
    param([Parameter(Mandatory)]$Response)

    if ($Response.Content -is [byte[]]) {
        return [System.Text.Encoding]::UTF8.GetString($Response.Content)
    }

    return [string]$Response.Content
}

function Test-Endpoint {
    param(
        [Parameter(Mandatory)][string]$Label,
        [Parameter(Mandatory)][string]$Uri,
        [int]$ExpectedStatus = 200,
        [switch]$WarnOnly
    )
    Write-Host "GET $Uri"
    try {
        $r = Invoke-WebRequest -Uri $Uri -UseBasicParsing -Method GET
        $code = [int]$r.StatusCode
    } catch {
        $resp = $_.Exception.Response
        $code = if ($resp) { [int]$resp.StatusCode } else { -1 }
    }

    if ($code -eq $ExpectedStatus) {
        Write-Host "OK: $Label -> $code" -ForegroundColor Green
        return $true
    }

    $msg = "$Label esperaba $ExpectedStatus, obtuvo $code"
    if ($WarnOnly) {
        Write-Host "WARNING: $msg" -ForegroundColor Yellow
        return $false
    }
    throw $msg
}

Test-Endpoint -Label "health" -Uri "$base/health" | Out-Null
Test-Endpoint -Label "health/live" -Uri "$base/health/live" | Out-Null

$readyOk = Test-Endpoint -Label "health/ready" -Uri "$base/health/ready" -WarnOnly:(-not $RequireReady)
if ($RequireReady -and -not $readyOk) {
    throw "health/ready no devolvio 200 y RequireReady esta activo."
}

Write-Host "POST $base/graphql"
$body = @{ query = "{ __typename }" } | ConvertTo-Json
$gql = Invoke-WebRequest -Uri "$base/graphql" -Method POST -ContentType "application/json" -Body $body -UseBasicParsing
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
Write-Host "OK: GraphQL -> 200 ($content)" -ForegroundColor Green
