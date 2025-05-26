using Microsoft.Extensions.Logging; // For ILogger, ILoggerProvider
using System.Collections.Concurrent;

namespace APICatalogo.Infrastructure.Logging; // Updated namespace

public class CustomLoggerPrivider : ILoggerProvider // Typo "Privider" kept as is from original filename
{
    readonly CustomLoggerProviderConfiguration loggerConfig;
    
    // Assuming CustomerLogger is the correct class name for the logger instance
    readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();

    public CustomLoggerPrivider(CustomLoggerProviderConfiguration config) 
    {
        loggerConfig = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        // Assuming CustomerLogger is the correct class name for the logger instance
        return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, loggerConfig));
    }

    public void Dispose()
    {
       loggers.Clear();
    }
}
