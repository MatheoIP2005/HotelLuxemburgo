$ErrorActionPreference = "Stop"
$url = "http://localhost:15672"

Write-Host "Verificando RabbitMQ Management en $url ..."
$response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 10

if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
    Write-Host "OK: RabbitMQ Management responde (HTTP $($response.StatusCode))"
    exit 0
}

Write-Host "ERROR: respuesta inesperada HTTP $($response.StatusCode)"
exit 1
