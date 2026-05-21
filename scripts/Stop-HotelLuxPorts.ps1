#Requires -Version 5.1
<#
.SYNOPSIS
  Libera los puertos del stack HotelLux (5000-5008) cerrando procesos que escuchan en ellos.
#>
$ErrorActionPreference = "Continue"

$ports = @(5000, 5001, 5002, 5003, 5004, 5005, 5008, 5101, 5108)

$stopped = New-Object System.Collections.Generic.List[string]

foreach ($port in $ports) {
    $listeners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if (-not $listeners) { continue }

    foreach ($procId in ($listeners.OwningProcess | Sort-Object -Unique)) {
        if ($procId -le 0) { continue }
        try {
            $proc = Get-Process -Id $procId -ErrorAction Stop
            Write-Host "Deteniendo PID $procId ($($proc.ProcessName)) en puerto $port" -ForegroundColor Yellow
            Stop-Process -Id $procId -Force -ErrorAction Stop
            $stopped.Add("$($proc.ProcessName) ($procId) :$port")
        }
        catch {
            Write-Host "No se pudo detener PID $procId : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Start-Sleep -Seconds 2

Write-Host ""
Write-Host "Estado de puertos:" -ForegroundColor Cyan
foreach ($port in $ports) {
    $busy = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($busy) { Write-Host "  $port -> OCUPADO" -ForegroundColor Red }
    else { Write-Host "  $port -> libre" -ForegroundColor Green }
}

if ($stopped.Count -eq 0) {
    Write-Host "`nNo habia procesos HotelLux escuchando en esos puertos." -ForegroundColor Gray
}
else {
    Write-Host "`nProcesos detenidos: $($stopped.Count). Ya puedes ejecutar dotnet run de nuevo." -ForegroundColor Green
}
