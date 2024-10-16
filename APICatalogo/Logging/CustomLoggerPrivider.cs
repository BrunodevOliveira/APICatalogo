using System.Collections.Concurrent;

namespace APICatalogo.Logging;

public class CustomLoggerPrivider : ILoggerProvider //Interface para criação de Loggs personalizados
{
    readonly CustomLoggerProviderConfiguration loggerConfig;

    //dicionário de Loggers onde a chave é o nome da categoria (string, nomalmente o nome da classe ou componente) e o valor é uma instância de CustomerLogger  
    readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();

    public CustomLoggerPrivider(CustomLoggerProviderConfiguration config) 
    {
        //Define a configuração dos Loggers
        loggerConfig = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, loggerConfig));
    }

    public void Dispose()
    {
       loggers.Clear();
    }
}
