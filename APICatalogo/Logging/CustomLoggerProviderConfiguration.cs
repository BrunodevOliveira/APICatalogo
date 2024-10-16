namespace APICatalogo.Logging;

public class CustomLoggerProviderConfiguration
{
    //Define o nível mínimo de log a ser registrado, com o padrão sendo LogLevel.Warning
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;

    //Define o ID do evento de log, com o padrão sendo zero
    public int EventId { get; set; } = 0;
}
