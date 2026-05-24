using Grpc.Net.Client;
using HotelLux.Protos.Audit;
using HotelLux.Protos.Auth;
using HotelLux.Shared.Grpc;

if (args.Length < 1)
{
    Console.Error.WriteLine("Uso: dotnet run --project grpc_smoke_test -- <token>");
  Console.Error.WriteLine("Requiere Auth (5001/5101) y Audit (5008/5108) en ejecución local.");
    return 1;
}

var token = args[0];

async Task Probe(string label, string address, bool forceGrpcWeb, Func<GrpcChannel, Task<string>> action)
{
    try
    {
        var previousRender = Environment.GetEnvironmentVariable("RENDER");
        var previousWeb = Environment.GetEnvironmentVariable("GRPC_USE_WEB");
        try
        {
            if (forceGrpcWeb)
            {
                Environment.SetEnvironmentVariable("RENDER", "true");
                Environment.SetEnvironmentVariable("GRPC_USE_WEB", "true");
            }
            else
            {
                Environment.SetEnvironmentVariable("RENDER", null);
                Environment.SetEnvironmentVariable("GRPC_USE_WEB", "false");
            }

            using var channel = GrpcChannelFactory.Create(address);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var resp = await action(channel);
            sw.Stop();
            var mode = forceGrpcWeb ? "GrpcWeb" : "H2c";
            Console.WriteLine($"[OK]  {label,-42} {address,-28} {mode,-6} {sw.ElapsedMilliseconds,4} ms -> {resp}");
        }
        finally
        {
            Environment.SetEnvironmentVariable("RENDER", previousRender);
            Environment.SetEnvironmentVariable("GRPC_USE_WEB", previousWeb);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FAIL] {label,-41} {address,-28} {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
    }
}

Console.WriteLine("=== gRPC smoke (HotelLux.Shared.GrpcChannelFactory) ===\n");

// Render / puerto único (GrpcWeb sobre HTTP/1.1)
await Probe("Auth.ValidateToken (GrpcWeb on 5001)", "http://127.0.0.1:5001", true, async ch =>
{
    var client = new AuthService.AuthServiceClient(ch);
    var r = await client.ValidateTokenAsync(new ValidateTokenRequest { Token = token });
    return $"valid={r.Valid} user={r.Username}";
});

await Probe("Audit.EmitAuditEvent (GrpcWeb on 5008)", "http://127.0.0.1:5008", true, async ch =>
{
    var client = new AuditService.AuditServiceClient(ch);
    await client.EmitAuditEventAsync(MakeAuditRequest());
    return "emitted";
});

// Desarrollo local (h2c HTTP/2 dedicado)
await Probe("Auth.ValidateToken", "http://127.0.0.1:5101", false, async ch =>
{
    var client = new AuthService.AuthServiceClient(ch);
    var r = await client.ValidateTokenAsync(new ValidateTokenRequest { Token = token });
    return $"valid={r.Valid} user={r.Username}";
});

await Probe("Audit.EmitAuditEvent", "http://127.0.0.1:5108", false, async ch =>
{
    var client = new AuditService.AuditServiceClient(ch);
    await client.EmitAuditEventAsync(MakeAuditRequest());
    return "emitted";
});

return 0;

static EmitAuditEventRequest MakeAuditRequest() => new()
{
    ServicioOrigen = "grpc-smoke",
    TablaAfectada = "test",
    Operacion = "INSERT",
    EntidadGuid = Guid.NewGuid().ToString(),
    IdRegistro = "1",
    UsuarioGuid = Guid.NewGuid().ToString(),
    UsuarioEjecutor = "smoke",
    IpOrigen = "127.0.0.1",
    DatosAnterioresJson = string.Empty,
    DatosNuevosJson = "{\"k\":\"v\"}",
    FechaEventoIso = DateTimeOffset.UtcNow.ToString("o")
};
