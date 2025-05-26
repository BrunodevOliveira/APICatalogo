using Microsoft.Extensions.Logging; // For ILogger, LogLevel, EventId
using System;                       // For IDisposable, Func, Exception, String
using System.IO;                    // For StreamWriter, Path related operations (though path is hardcoded)

namespace APICatalogo.Infrastructure.Logging; // Updated namespace

//Interface que difine os métodos necessários para implementar as classes de log
public class CustomerLogger : ILogger // Assuming CustomerLogger is the intended name, not CustomLogger
{
    readonly string loggerName;
    readonly CustomLoggerProviderConfiguration loggerConfig;

    public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
    {
        loggerName = name;
        loggerConfig = config;
    }
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == loggerConfig.LogLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Construct the log message using the provided formatter
        string mensagem = $"{logLevel.ToString()}: {eventId.Id} - {formatter(state, exception)}";
        
        // Append the logger name to the message for context
        string logMessageWithContext = $"[{loggerName}] {mensagem}";
        
        EscreverTextoNoArquivo(logMessageWithContext);
    }

    private void EscreverTextoNoArquivo(string mensagem)
    {
        // It's better to make the log path configurable or relative to the application.
        // For now, using the hardcoded path as in the original code.
        // Consider making this path safer and more configurable.
        string caminhoArquivoLog = @"c:\dados-log\curso_ASP_DOT_Net.txt"; 
        // Ensure the directory exists before writing.
        // string directoryPath = Path.GetDirectoryName(caminhoArquivoLog);
        // if (directoryPath != null && !Directory.Exists(directoryPath))
        // {
        //    Directory.CreateDirectory(directoryPath);
        // }

        try
        {
            // Using 'using' statement ensures the StreamWriter is disposed of correctly.
            // Appending to the file.
            using (StreamWriter streamWriter = new StreamWriter(caminhoArquivoLog, true))
            {
                streamWriter.WriteLine(mensagem);
                // streamWriter.Close(); // Not needed with 'using'
            }
        }
        catch (Exception ex) // Catch specific exceptions if possible, e.g., IOException
        {
            // Handle the exception (e.g., log to console, debug output, or a fallback logger)
            // For now, rethrowing as per original code, but this might hide the actual logging failure.
            Console.WriteLine($"Error writing to log file: {ex.Message}"); // Added a console log for visibility
            throw; 
        }
    }
}
