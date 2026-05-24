using HotelLux.Auth.DataAccess.Context;

namespace HotelLux.Auth.API.Seeders;

public sealed class AuthStartupSeeder : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromSeconds(60)
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuthStartupSeeder> _logger;

    public AuthStartupSeeder(IServiceScopeFactory scopeFactory, ILogger<AuthStartupSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var attempt = 0; !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

                await PasswordSeeder.RegenerarHashesPlaceholderAsync(db, stoppingToken);
                await DevUsersSeeder.EnsureDevCredentialsAsync(db, stoppingToken);

                SafeLogInformation("Auth startup seed completed.");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                if (attempt >= RetryDelays.Length)
                {
                    SafeLogWarning(ex, "Auth startup seed could not complete. The service will keep running.");
                    return;
                }

                var delay = RetryDelays[attempt];
                SafeLogWarning(ex, "Auth startup seed failed. Retrying in {DelaySeconds} seconds.", delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private void SafeLogInformation(string message)
    {
        try
        {
            _logger.LogInformation(message);
        }
        catch
        {
            // Logging providers should not decide whether the service stays alive.
        }
    }

    private void SafeLogWarning(Exception exception, string message, params object[] args)
    {
        try
        {
            _logger.LogWarning(exception, message, args);
        }
        catch
        {
            // Logging providers should not decide whether the service stays alive.
        }
    }
}
