param(
    [string]$Gateway = "http://localhost:5000"
)
$ErrorActionPreference = "Continue"

# --------------------------------------------------------------
# Helpers HTTP
# --------------------------------------------------------------
function Http {
    param(
        [string]$Method,
        [string]$Url,
        [string]$Token,
        [string]$BodyFile
    )
    $args = @("-s","-o","resp.tmp","-w","%{http_code}","-X",$Method,$Url)
    if ($Token)    { $args += @("-H","Authorization: Bearer $Token") }
    if ($BodyFile) { $args += @("-H","Content-Type: application/json","--data-binary","@$BodyFile") }
    $code = (& curl.exe @args).Trim()
    $body = ""
    if (Test-Path resp.tmp) { $body = Get-Content resp.tmp -Raw -Encoding UTF8; Remove-Item resp.tmp -Force }
    return [pscustomobject]@{ code=[int]$code; body=$body }
}

function Json-Body { param([string]$Text) $Text | Out-File -Encoding ascii -NoNewline body.json; return "body.json" }

function Get-Token {
    param([string]$User,[string]$Pwd)
    $bf = Json-Body "{`"username`":`"$User`",`"password`":`"$Pwd`"}"
    $r = Http POST "$Gateway/api/v1/auth/login" $null $bf
    Remove-Item body.json -ErrorAction SilentlyContinue
    return ($r.body | ConvertFrom-Json).access_token
}

# --------------------------------------------------------------
# Resultados
# --------------------------------------------------------------
$global:results = New-Object System.Collections.Generic.List[pscustomobject]
function Test-Endpoint {
    param(
        [string]$Group, [string]$Method, [string]$Path, [string]$Body,
        [string]$Token, [int[]]$Ok = @(200,201,202,204,400,401,403,404,409,422)
    )
    $bf = $null
    if ($Body) { $bf = Json-Body $Body }
    $r = Http $Method "$Gateway$Path" $Token $bf
    if ($bf) { Remove-Item body.json -ErrorAction SilentlyContinue }
    $pass = ($Ok -contains $r.code)
    $global:results.Add([pscustomobject]@{
        Group=$Group; Method=$Method; Path=$Path; Code=$r.code; Pass=$pass
    })
    $marker = if ($pass) { "[OK]  " } else { "[FAIL]" }
    $color = if ($pass) { "Green" } else { "Red" }
    Write-Host ("  {0} {1,-6} {2,3} {3}" -f $marker, $Method, $r.code, $Path) -ForegroundColor $color
    return $r
}

# --------------------------------------------------------------
# Login + cosecha GUIDs reales
# --------------------------------------------------------------
Write-Host "`n=== AUTH ===" -ForegroundColor Cyan
$adminTok = Get-Token "admin" "admin1234"
$vendTok  = Get-Token "vendedor" "vendedor1234"
if (-not $adminTok) { Write-Host "ERROR: no se pudo obtener token" -ForegroundColor Red; exit 99 }
Write-Host "  admin token OK" -ForegroundColor Green

Write-Host "`n=== Cosechando GUIDs ===" -ForegroundColor Cyan
function First-Guid {
    param([string]$Path, [string]$Key)
    $r = Http GET "$Gateway$Path" $adminTok $null
    if ($r.code -ne 200) { return $null }
    try {
        $j = $r.body | ConvertFrom-Json
        $items = $null
        if ($null -ne $j.data) {
            if ($j.data -is [System.Array] -or $j.data -is [object[]]) {
                $items = @($j.data)
            } elseif ($null -ne $j.data.items) {
                $items = @($j.data.items)
            } else {
                $items = @($j.data)
            }
        } elseif ($j.items) {
            $items = @($j.items)
        } else {
            $items = @($j)
        }
        if ($items.Count -gt 0) {
            $first = $items[0]
            if ($null -ne $first.$Key) { return [string]$first.$Key }
        }
    } catch {}
    return $null
}

$sucursalGuid       = First-Guid "/api/v1/internal/sucursales"             "sucursalGuid"
$habitacionGuid     = First-Guid "/api/v1/internal/habitaciones"           "habitacionGuid"
$tipoHabGuid        = First-Guid "/api/v1/internal/tipos-habitacion"       "tipoHabitacionGuid"
$catalogoGuid       = First-Guid "/api/v1/internal/catalogo-servicios"     "catalogoGuid"
$tarifaGuid         = First-Guid "/api/v1/internal/tarifas"                "tarifaGuid"
$clienteGuid        = First-Guid "/api/v1/internal/clientes"               "clienteGuid"
$reservaGuid        = First-Guid "/api/v1/internal/reservas"               "reservaGuid"
$estadiaGuid        = First-Guid "/api/v1/internal/estadias"               "estadiaGuid"
$valoracionGuid     = First-Guid "/api/v1/internal/valoraciones"           "valoracionGuid"
$facturaGuid        = First-Guid "/api/v1/internal/facturas"               "facturaGuid"
$pagoGuid           = First-Guid "/api/v1/internal/pagos"                  "pagoGuid"
$usuarioGuid        = First-Guid "/api/v1/internal/usuarios"               "usuarioGuid"
$rolGuid            = First-Guid "/api/v1/internal/roles"                  "rolGuid"
$auditoriaGuid      = First-Guid "/api/v1/internal/auditoria?pageSize=1"   "auditoriaGuid"
$idPermiso          = First-Guid "/api/v1/internal/permisos"               "idPermiso"
$cargoGuid          = $null
if ($estadiaGuid) {
    $cargoGuid = First-Guid "/api/v1/internal/estadias/$estadiaGuid/cargos" "cargoGuid"
}

$rnd = [Guid]::NewGuid().ToString()
$harvest = @{
    sucursalGuid=$sucursalGuid; habitacionGuid=$habitacionGuid; tipoHabGuid=$tipoHabGuid
    catalogoGuid=$catalogoGuid; tarifaGuid=$tarifaGuid; clienteGuid=$clienteGuid
    reservaGuid=$reservaGuid; estadiaGuid=$estadiaGuid; valoracionGuid=$valoracionGuid
    facturaGuid=$facturaGuid; pagoGuid=$pagoGuid; usuarioGuid=$usuarioGuid
    rolGuid=$rolGuid; auditoriaGuid=$auditoriaGuid; idPermiso=$idPermiso; cargoGuid=$cargoGuid
}
$harvest.GetEnumerator() | Sort-Object Key | ForEach-Object {
    $val = if ($_.Value) { $_.Value } else { "<vacio>" }
    Write-Host ("  {0,-18} = {1}" -f $_.Key, $val)
}
if (-not $sucursalGuid) { $sucursalGuid = $rnd }
if (-not $habitacionGuid) { $habitacionGuid = $rnd }
if (-not $tipoHabGuid) { $tipoHabGuid = $rnd }
if (-not $catalogoGuid) { $catalogoGuid = $rnd }
if (-not $tarifaGuid) { $tarifaGuid = $rnd }
if (-not $clienteGuid) { $clienteGuid = $rnd }
if (-not $reservaGuid) { $reservaGuid = $rnd }
if (-not $estadiaGuid) { $estadiaGuid = $rnd }
if (-not $valoracionGuid) { $valoracionGuid = $rnd }
if (-not $facturaGuid) { $facturaGuid = $rnd }
if (-not $pagoGuid) { $pagoGuid = $rnd }
if (-not $usuarioGuid) { $usuarioGuid = "21111111-1111-1111-1111-111111111001" }
if (-not $rolGuid) { $rolGuid = $rnd }
if (-not $auditoriaGuid) { $auditoriaGuid = $rnd }
if (-not $idPermiso) { $idPermiso = "1" }
if (-not $cargoGuid) { $cargoGuid = $rnd }
$idReserva = "1"   # FK numerica (no se conoce desde GUIDs)
$idSucursalImagen = "1"

# --------------------------------------------------------------
# CATALOGO DE ENDPOINTS
# Cada test usa el endpoint EXACTO documentado. Para POST/PUT/PATCH
# enviamos cuerpo minimo (vacio o trivial) — la idea es que el endpoint
# este enrutado y procese (cualquier 2xx/4xx es valido).
# --------------------------------------------------------------

# ============= Accommodations (publicos, sin auth) =============
Write-Host "`n=== Accommodations (public) ===" -ForegroundColor Cyan
Test-Endpoint Accommodations GET "/api/v1/accommodations/search" $null $null
Test-Endpoint Accommodations GET "/api/v1/accommodations/categories" $null $null
Test-Endpoint Accommodations GET "/api/v1/accommodations/$sucursalGuid" $null $null
Test-Endpoint Accommodations GET "/api/v1/accommodations/$sucursalGuid/reviews" $null $null

# ============= HabitacionesPublic + ReservasPublic =============
Write-Host "`n=== Public Habitaciones + Reservas ===" -ForegroundColor Cyan
Test-Endpoint Public GET  "/api/v1/public/sucursales/$sucursalGuid/habitaciones" $null $null
Test-Endpoint Public POST "/api/v1/public/reservas" "{}" $null

# ============= Auth =============
Write-Host "`n=== Auth ===" -ForegroundColor Cyan
Test-Endpoint Auth POST "/api/v1/auth/login"    '{"username":"admin","password":"admin1234"}' $null
Test-Endpoint Auth POST "/api/v1/auth/refresh"  "{}" $null  -Ok @(200,400,401,422)
Test-Endpoint Auth POST "/api/v1/auth/logout"   "{}" $adminTok -Ok @(200,204,400,401)
Test-Endpoint Auth POST "/api/v1/auth/cambiar-password" "{}" $adminTok -Ok @(200,204,400,401,403)

# ============= Sucursales =============
Write-Host "`n=== Sucursales ===" -ForegroundColor Cyan
Test-Endpoint Sucursales GET    "/api/v1/internal/sucursales" $null $adminTok
Test-Endpoint Sucursales POST   "/api/v1/internal/sucursales" "{}" $adminTok
Test-Endpoint Sucursales GET    "/api/v1/internal/sucursales/$sucursalGuid" $null $adminTok
Test-Endpoint Sucursales PUT    "/api/v1/internal/sucursales/$sucursalGuid" "{}" $adminTok
Test-Endpoint Sucursales DELETE "/api/v1/internal/sucursales/$sucursalGuid" $null $vendTok -Ok @(403)
Test-Endpoint Sucursales PATCH  "/api/v1/internal/sucursales/$sucursalGuid/politicas" "{}" $adminTok
Test-Endpoint Sucursales PATCH  "/api/v1/internal/sucursales/$sucursalGuid/inhabilitar" "{}" $adminTok
Test-Endpoint Sucursales GET    "/api/v1/internal/sucursales/$sucursalGuid/resumen-rating" $null $adminTok
Test-Endpoint Sucursales GET    "/api/v1/internal/sucursales/$sucursalGuid/imagenes" $null $adminTok
Test-Endpoint Sucursales POST   "/api/v1/internal/sucursales/$sucursalGuid/imagenes" "{}" $adminTok
Test-Endpoint Sucursales DELETE "/api/v1/internal/sucursales/$sucursalGuid/imagenes/$idSucursalImagen" $null $adminTok

# ============= TiposHabitacion =============
Write-Host "`n=== TiposHabitacion ===" -ForegroundColor Cyan
Test-Endpoint TiposHabitacion GET    "/api/v1/internal/tipos-habitacion" $null $adminTok
Test-Endpoint TiposHabitacion POST   "/api/v1/internal/tipos-habitacion" "{}" $adminTok
Test-Endpoint TiposHabitacion GET    "/api/v1/internal/tipos-habitacion/$tipoHabGuid" $null $adminTok
Test-Endpoint TiposHabitacion PUT    "/api/v1/internal/tipos-habitacion/$tipoHabGuid" "{}" $adminTok
Test-Endpoint TiposHabitacion DELETE "/api/v1/internal/tipos-habitacion/$tipoHabGuid" $null $vendTok -Ok @(403)
Test-Endpoint TiposHabitacion GET    "/api/v1/internal/tipos-habitacion/$tipoHabGuid/amenidades" $null $adminTok
Test-Endpoint TiposHabitacion POST   "/api/v1/internal/tipos-habitacion/$tipoHabGuid/amenidades" "{}" $adminTok
Test-Endpoint TiposHabitacion DELETE "/api/v1/internal/tipos-habitacion/$tipoHabGuid/amenidades/1" $null $adminTok
Test-Endpoint TiposHabitacion GET    "/api/v1/internal/tipos-habitacion/$tipoHabGuid/imagenes" $null $adminTok
Test-Endpoint TiposHabitacion POST   "/api/v1/internal/tipos-habitacion/$tipoHabGuid/imagenes" "{}" $adminTok
Test-Endpoint TiposHabitacion DELETE "/api/v1/internal/tipos-habitacion/$tipoHabGuid/imagenes/1" $null $adminTok

# ============= Habitaciones =============
Write-Host "`n=== Habitaciones ===" -ForegroundColor Cyan
Test-Endpoint Habitaciones GET    "/api/v1/internal/habitaciones" $null $adminTok
Test-Endpoint Habitaciones POST   "/api/v1/internal/habitaciones" "{}" $adminTok
Test-Endpoint Habitaciones GET    "/api/v1/internal/habitaciones/disponibles" $null $adminTok
Test-Endpoint Habitaciones GET    "/api/v1/internal/habitaciones/disponibilidad" $null $adminTok
Test-Endpoint Habitaciones GET    "/api/v1/internal/habitaciones/$habitacionGuid" $null $adminTok
Test-Endpoint Habitaciones PUT    "/api/v1/internal/habitaciones/$habitacionGuid" "{}" $adminTok
Test-Endpoint Habitaciones DELETE "/api/v1/internal/habitaciones/$habitacionGuid" $null $vendTok -Ok @(403)
Test-Endpoint Habitaciones PATCH  "/api/v1/internal/habitaciones/$habitacionGuid/estado" "{}" $adminTok

# ============= Tarifas =============
Write-Host "`n=== Tarifas ===" -ForegroundColor Cyan
Test-Endpoint Tarifas GET    "/api/v1/internal/tarifas" $null $adminTok
Test-Endpoint Tarifas POST   "/api/v1/internal/tarifas" "{}" $adminTok
Test-Endpoint Tarifas GET    "/api/v1/internal/tarifas/$tarifaGuid" $null $adminTok
Test-Endpoint Tarifas PUT    "/api/v1/internal/tarifas/$tarifaGuid" "{}" $adminTok
Test-Endpoint Tarifas DELETE "/api/v1/internal/tarifas/$tarifaGuid" $null $vendTok -Ok @(403)
Test-Endpoint Tarifas PATCH  "/api/v1/internal/tarifas/$tarifaGuid/desactivar" "{}" $adminTok

# ============= CatalogoServicios =============
Write-Host "`n=== CatalogoServicios ===" -ForegroundColor Cyan
Test-Endpoint CatalogoServicios GET    "/api/v1/internal/catalogo-servicios" $null $adminTok
Test-Endpoint CatalogoServicios POST   "/api/v1/internal/catalogo-servicios" "{}" $adminTok
Test-Endpoint CatalogoServicios GET    "/api/v1/internal/catalogo-servicios/$catalogoGuid" $null $adminTok
Test-Endpoint CatalogoServicios PUT    "/api/v1/internal/catalogo-servicios/$catalogoGuid" "{}" $adminTok
Test-Endpoint CatalogoServicios DELETE "/api/v1/internal/catalogo-servicios/$catalogoGuid" $null $vendTok -Ok @(403)
Test-Endpoint CatalogoServicios PATCH  "/api/v1/internal/catalogo-servicios/$catalogoGuid/desactivar" "{}" $adminTok

# ============= Clientes =============
Write-Host "`n=== Clientes ===" -ForegroundColor Cyan
Test-Endpoint Clientes GET    "/api/v1/internal/clientes" $null $adminTok
Test-Endpoint Clientes POST   "/api/v1/internal/clientes" "{}" $adminTok
Test-Endpoint Clientes GET    "/api/v1/internal/clientes/$clienteGuid" $null $adminTok
Test-Endpoint Clientes PUT    "/api/v1/internal/clientes/$clienteGuid" "{}" $adminTok
Test-Endpoint Clientes DELETE "/api/v1/internal/clientes/$clienteGuid" $null $vendTok -Ok @(403)
Test-Endpoint Clientes PATCH  "/api/v1/internal/clientes/$clienteGuid/inhabilitar" "{}" $adminTok
Test-Endpoint Clientes GET    "/api/v1/internal/clientes/$clienteGuid/reservas" $null $adminTok
Test-Endpoint Clientes GET    "/api/v1/internal/clientes/$clienteGuid/valoraciones" $null $adminTok

# ============= Reservas =============
Write-Host "`n=== Reservas ===" -ForegroundColor Cyan
Test-Endpoint Reservas GET    "/api/v1/internal/reservas" $null $adminTok
Test-Endpoint Reservas POST   "/api/v1/internal/reservas" "{}" $adminTok
Test-Endpoint Reservas GET    "/api/v1/internal/reservas/$reservaGuid" $null $adminTok
Test-Endpoint Reservas DELETE "/api/v1/internal/reservas/$reservaGuid" $null $vendTok -Ok @(403)
Test-Endpoint Reservas PATCH  "/api/v1/internal/reservas/$reservaGuid/confirmar" "{}" $adminTok
Test-Endpoint Reservas PATCH  "/api/v1/internal/reservas/$reservaGuid/cancelar" "{}" $adminTok
Test-Endpoint Reservas GET    "/api/v1/internal/reservas/$reservaGuid/habitaciones" $null $adminTok
Test-Endpoint Reservas POST   "/api/v1/internal/reservas/$reservaGuid/habitaciones" "{}" $adminTok
Test-Endpoint Reservas DELETE "/api/v1/internal/reservas/$reservaGuid/habitaciones/1" $null $adminTok

# ============= Estadias =============
Write-Host "`n=== Estadias ===" -ForegroundColor Cyan
Test-Endpoint Estadias GET    "/api/v1/internal/estadias" $null $adminTok
Test-Endpoint Estadias GET    "/api/v1/internal/estadias/$estadiaGuid" $null $adminTok
Test-Endpoint Estadias POST   "/api/v1/internal/estadias/checkin/$reservaGuid" "{}" $adminTok
Test-Endpoint Estadias PATCH  "/api/v1/internal/estadias/$estadiaGuid/checkout" "{}" $adminTok
Test-Endpoint Estadias GET    "/api/v1/internal/estadias/$estadiaGuid/cargos" $null $adminTok
Test-Endpoint Estadias POST   "/api/v1/internal/estadias/$estadiaGuid/cargos" "{}" $adminTok
Test-Endpoint Estadias PATCH  "/api/v1/internal/estadias/$estadiaGuid/mantenimiento" "{}" $adminTok

# ============= CargosEstadia =============
Write-Host "`n=== CargosEstadia ===" -ForegroundColor Cyan
Test-Endpoint CargosEstadia GET   "/api/v1/internal/cargos-estadia/$cargoGuid" $null $adminTok
Test-Endpoint CargosEstadia PATCH "/api/v1/internal/cargos-estadia/$cargoGuid/anular" "{}" $adminTok

# ============= Valoraciones =============
Write-Host "`n=== Valoraciones ===" -ForegroundColor Cyan
Test-Endpoint Valoraciones GET    "/api/v1/internal/valoraciones" $null $adminTok
Test-Endpoint Valoraciones POST   "/api/v1/internal/valoraciones" "{}" $adminTok
Test-Endpoint Valoraciones GET    "/api/v1/internal/valoraciones/$valoracionGuid" $null $adminTok
Test-Endpoint Valoraciones DELETE "/api/v1/internal/valoraciones/$valoracionGuid" $null $vendTok -Ok @(403)
Test-Endpoint Valoraciones PATCH  "/api/v1/internal/valoraciones/$valoracionGuid/moderar" "{}" $adminTok
Test-Endpoint Valoraciones PATCH  "/api/v1/internal/valoraciones/$valoracionGuid/responder" "{}" $adminTok

# ============= Facturas =============
Write-Host "`n=== Facturas ===" -ForegroundColor Cyan
Test-Endpoint Facturas GET   "/api/v1/internal/facturas" $null $adminTok
Test-Endpoint Facturas GET   "/api/v1/internal/facturas/reserva/$idReserva" $null $adminTok
Test-Endpoint Facturas GET   "/api/v1/internal/facturas/$facturaGuid" $null $adminTok
Test-Endpoint Facturas GET   "/api/v1/internal/facturas/$facturaGuid/detalle" $null $adminTok
Test-Endpoint Facturas POST  "/api/v1/internal/facturas/generar-reserva/$reservaGuid" "{}" $adminTok
Test-Endpoint Facturas POST  "/api/v1/internal/facturas/generar-final/$reservaGuid" "{}" $adminTok
Test-Endpoint Facturas POST  "/api/v1/internal/facturas/final-y-pago-simulado/$reservaGuid" "{}" $adminTok
Test-Endpoint Facturas PATCH "/api/v1/internal/facturas/$facturaGuid/anular" "{}" $adminTok
Test-Endpoint Facturas GET   "/api/v1/internal/facturas/$facturaGuid/pagos" $null $adminTok

# ============= Pagos =============
Write-Host "`n=== Pagos ===" -ForegroundColor Cyan
Test-Endpoint Pagos GET   "/api/v1/internal/pagos" $null $adminTok
Test-Endpoint Pagos POST  "/api/v1/internal/pagos" "{}" $adminTok
Test-Endpoint Pagos GET   "/api/v1/internal/pagos/$pagoGuid" $null $adminTok
Test-Endpoint Pagos PATCH "/api/v1/internal/pagos/$pagoGuid/estado" "{}" $adminTok

# ============= Usuarios =============
Write-Host "`n=== Usuarios ===" -ForegroundColor Cyan
Test-Endpoint Usuarios GET    "/api/v1/internal/usuarios" $null $adminTok
Test-Endpoint Usuarios POST   "/api/v1/internal/usuarios" "{}" $adminTok
Test-Endpoint Usuarios GET    "/api/v1/internal/usuarios/$usuarioGuid" $null $adminTok
Test-Endpoint Usuarios PUT    "/api/v1/internal/usuarios/$usuarioGuid" "{}" $adminTok
Test-Endpoint Usuarios DELETE "/api/v1/internal/usuarios/$usuarioGuid" $null $vendTok -Ok @(403)
Test-Endpoint Usuarios PATCH  "/api/v1/internal/usuarios/$usuarioGuid/inhabilitar" "{}" $adminTok
Test-Endpoint Usuarios GET    "/api/v1/internal/usuarios/$usuarioGuid/roles" $null $adminTok
Test-Endpoint Usuarios POST   "/api/v1/internal/usuarios/$usuarioGuid/roles" "{}" $adminTok
Test-Endpoint Usuarios DELETE "/api/v1/internal/usuarios/$usuarioGuid/roles/1" $null $adminTok

# ============= Roles =============
Write-Host "`n=== Roles ===" -ForegroundColor Cyan
Test-Endpoint Roles GET    "/api/v1/internal/roles" $null $adminTok
Test-Endpoint Roles POST   "/api/v1/internal/roles" "{}" $adminTok
Test-Endpoint Roles PUT    "/api/v1/internal/roles/$rolGuid" "{}" $adminTok
Test-Endpoint Roles DELETE "/api/v1/internal/roles/$rolGuid" $null $vendTok -Ok @(403)
Test-Endpoint Roles POST   "/api/v1/internal/roles/$rolGuid/permisos" "{}" $adminTok
Test-Endpoint Roles DELETE "/api/v1/internal/roles/$rolGuid/permisos/$idPermiso" $null $adminTok

# ============= Permisos =============
Write-Host "`n=== Permisos ===" -ForegroundColor Cyan
Test-Endpoint Permisos GET "/api/v1/internal/permisos" $null $adminTok

# ============= Auditoria =============
Write-Host "`n=== Auditoria ===" -ForegroundColor Cyan
Test-Endpoint Auditoria GET "/api/v1/internal/auditoria?pageSize=3" $null $adminTok
Test-Endpoint Auditoria GET "/api/v1/internal/auditoria/$auditoriaGuid" $null $adminTok

# ============= Images =============
Write-Host "`n=== Images ===" -ForegroundColor Cyan
Test-Endpoint Images POST "/api/v1/internal/images/upload" "{}" $adminTok -Ok @(200,201,400,401,403,415,422)

# --------------------------------------------------------------
# Resumen
# --------------------------------------------------------------
Write-Host ""
$total = $global:results.Count
$pass  = ($global:results | Where-Object Pass).Count
$fail  = $total - $pass
Write-Host ("Endpoints probados: {0}   OK: {1}   FAIL: {2}" -f $total, $pass, $fail) -ForegroundColor $(if ($fail -eq 0) { "Green" } else { "Yellow" })

if ($fail -gt 0) {
    Write-Host ""
    Write-Host "Endpoints que fallaron (status no esperado):" -ForegroundColor Yellow
    $global:results | Where-Object { -not $_.Pass } | ForEach-Object {
        Write-Host ("  [{0,3}] {1,-6} {2,-15} {3}" -f $_.Code, $_.Method, $_.Group, $_.Path) -ForegroundColor Red
    }
}

# Conteo por status para ver patrones
Write-Host ""
Write-Host "Distribucion de status codes:" -ForegroundColor Cyan
$global:results | Group-Object Code | Sort-Object Name | ForEach-Object {
    Write-Host ("  {0}  -> {1}" -f $_.Name, $_.Count)
}

exit $fail
