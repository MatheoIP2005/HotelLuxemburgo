param(
    [string]$Gateway = "http://localhost:5000",
    [switch]$NoColor
)

$ErrorActionPreference = "Continue"
$script:total   = 0
$script:passed  = 0
$script:failed  = 0
$script:details = New-Object System.Collections.Generic.List[string]

function _hue($c, $msg) { if ($NoColor) { Write-Host $msg } else { Write-Host $msg -ForegroundColor $c } }

function Assert-Eq {
    param([string]$Name, $Expected, $Actual)
    $script:total++
    if ("$Expected" -eq "$Actual") {
        $script:passed++
        _hue Green ("  [OK]   {0}  (esperado={1}, obtenido={2})" -f $Name, $Expected, $Actual)
    } else {
        $script:failed++
        _hue Red   ("  [FAIL] {0}  (esperado={1}, obtenido={2})" -f $Name, $Expected, $Actual)
        $script:details.Add("FAIL: $Name -> expected=$Expected actual=$Actual")
    }
}

function Assert-In {
    param([string]$Name, [int[]]$Expected, $Actual)
    $script:total++
    if ($Expected -contains [int]$Actual) {
        $script:passed++
        _hue Green ("  [OK]   {0}  (esperado IN [{1}], obtenido={2})" -f $Name, ($Expected -join ","), $Actual)
    } else {
        $script:failed++
        _hue Red   ("  [FAIL] {0}  (esperado IN [{1}], obtenido={2})" -f $Name, ($Expected -join ","), $Actual)
        $script:details.Add("FAIL: $Name -> expected IN [$($Expected -join ',')] actual=$Actual")
    }
}

function Http-Code {
    param([string]$Method, [string]$Url, [string]$Token, [string]$BodyFile)
    $args = @("-s","-o","NUL","-w","%{http_code}","-X",$Method,$Url)
    if ($Token) { $args += @("-H","Authorization: Bearer $Token") }
    if ($BodyFile) { $args += @("-H","Content-Type: application/json","--data-binary","@$BodyFile") }
    return (& curl.exe @args).Trim()
}

function Http-Json {
    param([string]$Method, [string]$Url, [string]$Token, [string]$BodyFile)
    $args = @("-s","-X",$Method,$Url)
    if ($Token) { $args += @("-H","Authorization: Bearer $Token") }
    if ($BodyFile) { $args += @("-H","Content-Type: application/json","--data-binary","@$BodyFile") }
    return (& curl.exe @args)
}

function Get-Token {
    param([string]$User, [string]$Pwd)
    "{`"username`":`"$User`",`"password`":`"$Pwd`"}" | Out-File -Encoding ascii -NoNewline body.json
    $j = Http-Json POST "$Gateway/api/v1/auth/login" $null "body.json" | ConvertFrom-Json
    Remove-Item body.json -ErrorAction SilentlyContinue
    return $j.access_token
}

function Decode-Jwt($jwt) {
    $parts = $jwt.Split('.')
    $b64 = $parts[1].Replace('-','+').Replace('_','/')
    switch ($b64.Length % 4) { 2 { $b64 += "==" } 3 { $b64 += "=" } }
    return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($b64)) | ConvertFrom-Json
}

# ================================================================
_hue Cyan "`n=== 1) Health checks de los 7 listeners ==="
$svcs = @(
    @{n="Gateway";       url="$Gateway/health"; alt="$Gateway/" },
    @{n="Auth REST";     url="http://127.0.0.1:5001/health"; alt="http://127.0.0.1:5001/swagger" },
    @{n="Accommodation"; url="http://127.0.0.1:5002/health"; alt="http://127.0.0.1:5002/swagger" },
    @{n="Reservation";   url="http://127.0.0.1:5003/health"; alt="http://127.0.0.1:5003/swagger" },
    @{n="Stay";          url="http://127.0.0.1:5004/health"; alt="http://127.0.0.1:5004/swagger" },
    @{n="Finance";       url="http://127.0.0.1:5005/health"; alt="http://127.0.0.1:5005/swagger" },
    @{n="Audit";         url="http://127.0.0.1:5008/health"; alt="http://127.0.0.1:5008/swagger" }
)
foreach ($s in $svcs) {
    $c1 = Http-Code GET $s.url   $null $null
    $c2 = Http-Code GET $s.alt   $null $null
    # cualquier respuesta != 000 (connection refused) -> el listener responde
    $alive = ($c1 -ne "000" -or $c2 -ne "000")
    Assert-Eq ("listener {0} responde" -f $s.n) $true $alive
}

# ================================================================
_hue Cyan "`n=== 2) JWT + login + middleware de autenticacion ==="

$adminTok = Get-Token "admin"    "admin1234"
$vendTok  = Get-Token "vendedor" "vendedor1234"

Assert-Eq "login admin -> token presente"    $true ($adminTok -and $adminTok.Length -gt 100)
Assert-Eq "login vendedor -> token presente" $true ($vendTok  -and $vendTok.Length  -gt 100)

'{"username":"admin","password":"WRONG"}' | Out-File -Encoding ascii -NoNewline body.json
$badCred = Http-Code POST "$Gateway/api/v1/auth/login" $null "body.json"
Remove-Item body.json -ErrorAction SilentlyContinue
Assert-In "login bad creds -> 4xx" @(400,401,403) $badCred

# 401 sin token sobre una ruta que SI existe en el Gateway
$no401 = Http-Code GET "$Gateway/api/v1/internal/sucursales" $null $null
Assert-Eq "sin token sobre /internal/sucursales -> 401" "401" $no401

# Token vencido / con basura
$noBad = Http-Code GET "$Gateway/api/v1/internal/sucursales" "abc.def.ghi" $null
Assert-Eq "token invalido -> 401" "401" $noBad

# Claims del JWT
$adminClaims = Decode-Jwt $adminTok
$vendClaims  = Decode-Jwt $vendTok
Assert-Eq "JWT iss admin"     "HotelLuxemburgo.Auth"      $adminClaims.iss
Assert-Eq "JWT aud admin"     "HotelLuxemburgo.Services"  $adminClaims.aud
Assert-Eq "JWT roles admin"   "ADMIN"                     $adminClaims.roles
Assert-Eq "JWT roles vendedor" "VENDEDOR"                 $vendClaims.roles

# ================================================================
_hue Cyan "`n=== 3) gRPC h2c sobre HTTP/2 (Auth + Audit) ==="
$smokeOut = dotnet run --project "grpc_smoke_test\GrpcSmoke.csproj" --no-build -- $adminTok 2>&1
$smokeText = $smokeOut -join "`n"
$smokeText -split "`n" | Where-Object { $_ -match "^\[OK\]|^\[FAIL\]" } | ForEach-Object { Write-Host $_ }
$okAuth  = $smokeText -match "\[OK\]\s+Auth\.ValidateToken \(gRPC 5101\)"
$okAudit = $smokeText -match "\[OK\]\s+Audit\.EmitAuditEvent \(gRPC 5108\)"
Assert-Eq "Auth.ValidateToken gRPC 5101 (h2c)"   $true $okAuth
Assert-Eq "Audit.EmitAuditEvent gRPC 5108 (h2c)" $true $okAudit

# Verificar que sobre el puerto REST 5001/5008 gRPC NO funciona (corretto)
$grpcFailRest = ($smokeText -match "\[FAIL\]\s+Auth\.ValidateToken \(REST 5001\)") -and `
                ($smokeText -match "\[FAIL\]\s+Audit\.EmitAuditEvent \(REST 5008\)") -and `
                ($smokeText -match "HTTP_1_1_REQUIRED")
Assert-Eq "REST 5001/5008 rechazan gRPC con HTTP_1_1_REQUIRED" $true $grpcFailRest

# ================================================================
_hue Cyan "`n=== 4) GET (lectura) en TODOS los servicios via Gateway ==="
# Endpoints reales (todos requieren JWT y se enrutan en YARP)
$gets = @(
    @{n="GET internal/sucursales";          url="/api/v1/internal/sucursales" },
    @{n="GET internal/habitaciones";        url="/api/v1/internal/habitaciones" },
    @{n="GET internal/tipos-habitacion";    url="/api/v1/internal/tipos-habitacion" },
    @{n="GET internal/catalogo-servicios";  url="/api/v1/internal/catalogo-servicios" },
    @{n="GET internal/tarifas";             url="/api/v1/internal/tarifas" },
    @{n="GET internal/clientes";            url="/api/v1/internal/clientes" },
    @{n="GET internal/reservas";            url="/api/v1/internal/reservas" },
    @{n="GET internal/estadias";            url="/api/v1/internal/estadias" },
    @{n="GET internal/valoraciones";        url="/api/v1/internal/valoraciones" },
    @{n="GET internal/facturas";            url="/api/v1/internal/facturas" },
    @{n="GET internal/pagos";               url="/api/v1/internal/pagos" },
    @{n="GET internal/usuarios";            url="/api/v1/internal/usuarios" },
    @{n="GET internal/roles";               url="/api/v1/internal/roles" },
    @{n="GET internal/permisos";            url="/api/v1/internal/permisos" },
    @{n="GET internal/auditoria";           url="/api/v1/internal/auditoria?pageSize=3" }
)
foreach ($g in $gets) { Assert-In $g.n @(200,204) (Http-Code GET ($Gateway + $g.url) $adminTok $null) }

# ================================================================
_hue Cyan "`n=== 5) RBAC: vendedor NO puede DELETE; admin si ==="
$rndGuid = "00000000-0000-0000-0000-000000000000"
$deletes = @(
    @{n="DELETE sucursal";    url="/api/v1/internal/sucursales/$rndGuid" },
    @{n="DELETE habitacion";  url="/api/v1/internal/habitaciones/$rndGuid" },
    @{n="DELETE cliente";     url="/api/v1/internal/clientes/$rndGuid" },
    @{n="DELETE reserva";     url="/api/v1/internal/reservas/$rndGuid" },
    @{n="DELETE valoracion";  url="/api/v1/internal/valoraciones/$rndGuid" },
    @{n="DELETE usuario";     url="/api/v1/internal/usuarios/$rndGuid" },
    @{n="DELETE rol";         url="/api/v1/internal/roles/$rndGuid" }
)
foreach ($d in $deletes) {
    $cV = Http-Code DELETE ($Gateway + $d.url) $vendTok  $null
    Assert-Eq ("vendedor " + $d.n + " -> 403") "403" $cV
    $cA = Http-Code DELETE ($Gateway + $d.url) $adminTok $null
    Assert-Eq ("admin "    + $d.n + " no-403")   $false ($cA -eq "403")
}

# ================================================================
_hue Cyan "`n=== 6) POST/PUT permitidos para vendedor (CRU) ==="
# El vendedor crea un cliente
$rndDoc = "9" + (Get-Random -Minimum 100000000 -Maximum 999999999).ToString()
@"
{"tipoIdentificacion":"CED","numeroIdentificacion":"$rndDoc","nombres":"Smoke","apellidos":"Tester","correo":"smoke$rndDoc@test.com","telefono":"+593900000000","direccion":"x","ciudad":"Quito","pais":"EC"}
"@ | Out-File -Encoding ascii -NoNewline body.json

$postCliente = Http-Code POST "$Gateway/api/v1/internal/clientes" $vendTok "body.json"
Assert-In "vendedor POST cliente -> 201/200/409" @(200,201,409) $postCliente
Remove-Item body.json -ErrorAction SilentlyContinue

# ================================================================
_hue Cyan "`n=== 7) Bus de eventos (auditoria via gRPC) ==="
# Esperamos al menos un evento mas que al iniciar (login + opcionalmente POST cliente)
Start-Sleep -Seconds 2
$listResp = Http-Json GET "$Gateway/api/v1/internal/auditoria?pageSize=5" $adminTok $null
$listJson = $listResp | ConvertFrom-Json
$ultimos  = @($listJson.data.items)
$hayLoginRecent = ($ultimos | Where-Object {
    ($_.servicioOrigen -eq "auth-service") -and
    ($_.tablaAfectada  -eq "seguridad.usuario_app") -and
    ($_.datosNuevos -like "*LOGIN*")
}).Count -gt 0
Assert-Eq "Audit contiene LOGIN reciente de auth-service" $true $hayLoginRecent

_hue Yellow "Top 5 ultimos eventos:"
$ultimos | Select-Object servicioOrigen, tablaAfectada, operacion, usuarioEjecutor, fechaEventoUtc | Format-Table | Out-String | Write-Host

# ================================================================
_hue Cyan "`n=== 8) Resumen ==="
$color = if ($script:failed -eq 0) { "Green" } else { "Red" }
_hue $color ("Pruebas: {0}  PASSED: {1}  FAILED: {2}" -f $script:total, $script:passed, $script:failed)
if ($script:failed -gt 0) { Write-Host ""; _hue Yellow "Detalles de fallos:"; foreach ($d in $script:details) { Write-Host "  - $d" } }

exit $script:failed
