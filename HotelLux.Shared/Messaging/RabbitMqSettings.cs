namespace HotelLux.Shared.Messaging;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string? Uri { get; set; }
    public string Host { get; set; } = "localhost";
    public int? Port { get; set; }
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public bool UseSsl { get; set; }
    public string AuditQueue { get; set; } = "hotellux.audit.events";
}
