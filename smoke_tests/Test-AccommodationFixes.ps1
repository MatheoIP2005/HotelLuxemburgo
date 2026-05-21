param([string]$Gateway = "http://127.0.0.1:5000")

$ErrorActionPreference = "Continue"
$script:total = 0; $script:passed = 0; $script:failed = 0
$script:details = @()

function Assert-Eq($Name, $Expected, $Actual) {
    $script:total++
    if ("$Expected" -eq "$Actual") {
        $script:passed++
        Write-Host "  [OK]   $Name (esperado=$Expected)" -ForegroundColor Green
    } else {
        $script:failed++
        Write-Host "  [FAIL] $Name (esperado=$Expected, obtenido=$Actual)" -ForegroundColor Red
        $script:details += "FAIL: $Name -> expected=$Expected actual=$Actual"
    }
}

function Http-Code($Method, $Url, $Token) {
    $args = @("-s", "-o", "NUL", "-w", "%{http_code}", "-X", $Method, "--url", $Url)
    if ($Token) { $args += @("-H", "Authorization: Bearer $Token") }
    return (& curl.exe @args).Trim()
}

function Service-Alive($Port) {
    foreach ($path in @("/health", "/swagger/index.html")) {
        $c = Http-Code GET "http://127.0.0.1:$Port$path" $null
        if ($c -ne "000" -and $c -ne "404") { return $true }
        if ($c -eq "200") { return $true }
    }
    return $false
}

function Get-Token($User, $Pwd) {
    $body = "{`"username`":`"$User`",`"password`":`"$Pwd`"}"
    $body | Out-File -Encoding ascii -NoNewline body_login.json
    $args = @("-s", "-X", "POST", "$Gateway/api/v1/auth/login", "-H", "Content-Type: application/json", "--data-binary", "@body_login.json")
    $j = (& curl.exe @args) | ConvertFrom-Json
    Remove-Item body_login.json -ErrorAction SilentlyContinue
    return $j.access_token
}

$sucursal = "44444444-4444-4444-4444-444444444001"
$reserva  = "99999999-9999-9999-9999-999999999001"

Write-Host "`n=== Health ===" -ForegroundColor Cyan
foreach ($p in @("5000","5001","5002","5003")) {
    Assert-Eq "listener :$p responde" $true (Service-Alive $p)
}

Write-Host "`n=== Search (params opcionales) ===" -ForegroundColor Cyan
Assert-Eq "Search sin params -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/search" $null)
Assert-Eq "Search destino=Quito -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/search?destino=Quito" $null)
Assert-Eq "Search num_adultos=2 -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/search?num_adultos=2&num_habitaciones=1" $null)
Assert-Eq "Search solo fechaInicio -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/search?fechaInicio=2026-06-10T00:00:00Z" $null)
Assert-Eq "Search num_adultos=0 -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/search?num_adultos=0" $null)
Assert-Eq "Search fechas invertidas -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/search?fechaInicio=2026-06-15T00:00:00Z&fechaFin=2026-06-10T00:00:00Z" $null)

Write-Host "`n=== Typo accomodations (una M) ===" -ForegroundColor Cyan
Assert-Eq "Typo /accomodations/search -> 404" "404" (Http-Code GET "$Gateway/api/v1/accomodations/search" $null)
Assert-Eq "Typo /accomodations/reservas -> 404" "404" (Http-Code GET "$Gateway/api/v1/accomodations/reservas" $null)

Write-Host "`n=== Gateway routing (doble M) ===" -ForegroundColor Cyan
Assert-Eq "GET /accommodations/search -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/search" $null)
Assert-Eq "GET /accommodations/{guid} -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/$sucursal" $null)
Assert-Eq "GET sucursales/.../habitaciones eliminado -> 404" "404" (Http-Code GET "$Gateway/api/v1/accommodations/sucursales/$sucursal/habitaciones" $null)

Write-Host "`n=== GET reservas requiere JWT ===" -ForegroundColor Cyan
Assert-Eq "GET reservas sin token -> 401" "401" (Http-Code GET "$Gateway/api/v1/accommodations/reservas/$reserva" $null)
$adminTok = Get-Token "admin" "admin1234"
if ($adminTok) {
    Assert-Eq "GET reservas con JWT -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/reservas/$reserva" $adminTok)
} else {
    Assert-Eq "login admin para JWT" "token" "missing"
}

Write-Host "`n=== GetById validacion fechas (fechaInicio/fechaFin) ===" -ForegroundColor Cyan
Assert-Eq "GetById solo fechaInicio -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/$sucursal?fechaInicio=2026-06-10T00:00:00Z" $null)
Assert-Eq "GetById solo fechaFin -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/$sucursal?fechaFin=2026-06-15T00:00:00Z" $null)
Assert-Eq "GetById fechas invertidas -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/$sucursal?fechaInicio=2026-06-15T00:00:00Z&fechaFin=2026-06-10T00:00:00Z" $null)
Assert-Eq "GetById sin fechas -> 200" "200" (Http-Code GET "$Gateway/api/v1/accommodations/$sucursal" $null)
$validDatesUrl = "$Gateway/api/v1/accommodations/$sucursal" + "?fechaInicio=2026-06-10T00:00:00Z&fechaFin=2026-06-15T00:00:00Z"
Assert-Eq "GetById fechas validas -> 200" "200" (Http-Code GET $validDatesUrl $null)

Write-Host "`n=== UUID invalido -> 400 ===" -ForegroundColor Cyan
Assert-Eq "GetById uuid invalido -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/id-no-valido" $null)
Assert-Eq "GetReviews uuid invalido -> 400" "400" (Http-Code GET "$Gateway/api/v1/accommodations/abc123/reviews" $null)

Write-Host "`n=== Resumen ===" -ForegroundColor Cyan
$color = if ($script:failed -eq 0) { "Green" } else { "Red" }
Write-Host ("Pruebas: {0}  PASSED: {1}  FAILED: {2}" -f $script:total, $script:passed, $script:failed) -ForegroundColor $color
if ($script:failed -gt 0) { foreach ($d in $script:details) { Write-Host "  - $d" -ForegroundColor Yellow } }
exit $script:failed
