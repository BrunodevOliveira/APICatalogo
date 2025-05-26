using Microsoft.Extensions.Logging; // For LogLevel

namespace APICatalogo.Infrastructure.Logging; // Updated namespace

public class CustomLoggerProviderConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
    public int EventId { get; set; } = 0;
}
