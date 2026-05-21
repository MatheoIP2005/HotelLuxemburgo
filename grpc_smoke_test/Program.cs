using System.Net;
using Grpc.Net.Client;
using HotelLux.Protos.Auth;
using HotelLux.Protos.Audit;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

if (args.Length < 1)
{
    Console.Error.WriteLine("Uso: GrpcSmoke <token>");
    return 1;
}
var token = args[0];

async Task Probe(string label, string address, Func<GrpcChannel, Task<string>> action)
{
    try
    {
        using var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true }
        });
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var resp = await action(channel);
        sw.Stop();
        Console.WriteLine($"[OK]  {label,-35} {address,-30}  {sw.ElapsedMilliseconds,4} ms  -> {resp}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FAIL] {label,-34} {address,-30}  {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
    }
}

await Probe("Auth.ValidateToken (gRPC 5101)", "http://127.0.0.1:5101", async ch =>
{
    var client = new AuthService.AuthServiceClient(ch);
    var r = await client.ValidateTokenAsync(new ValidateTokenRequest { Token = token });
    return $"valid={r.Valid} user={r.Username} roles=[{string.Join(",", r.Roles)}]";
});

await Probe("Auth.ValidateToken (REST 5001)", "http://127.0.0.1:5001", async ch =>
{
    var client = new AuthService.AuthServiceClient(ch);
    var r = await client.ValidateTokenAsync(new ValidateTokenRequest { Token = token });
    return $"valid={r.Valid} user={r.Username} roles=[{string.Join(",", r.Roles)}]";
});

await Probe("Audit.EmitAuditEvent (gRPC 5108)", "http://127.0.0.1:5108", async ch =>
{
    var client = new AuditService.AuditServiceClient(ch);
    await client.EmitAuditEventAsync(new EmitAuditEventRequest
    {
        ServicioOrigen = "audit-service",
        TablaAfectada = "test",
        Operacion = "INSERT",
        EntidadGuid = Guid.NewGuid().ToString(),
        IdRegistro = "1",
        UsuarioGuid = Guid.NewGuid().ToString(),
        UsuarioEjecutor = "smoke",
        IpOrigen = "127.0.0.1",
        DatosAnterioresJson = string.Empty,
        DatosNuevosJson = "{\"k\":\"v\"}",
        FechaEventoIso = string.Empty
    });
    return "emitted";
});

await Probe("Audit.EmitAuditEvent (REST 5008)", "http://127.0.0.1:5008", async ch =>
{
    var client = new AuditService.AuditServiceClient(ch);
    await client.EmitAuditEventAsync(new EmitAuditEventRequest
    {
        ServicioOrigen = "audit-service",
        TablaAfectada = "test",
        Operacion = "INSERT",
        EntidadGuid = Guid.NewGuid().ToString(),
        UsuarioGuid = Guid.NewGuid().ToString(),
        UsuarioEjecutor = "smoke"
    });
    return "emitted";
});

return 0;
