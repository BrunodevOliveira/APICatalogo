using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters;

public class ApiLoggingFilter : IActionFilter
{
    //ILogger permite o registro de informações e ventos 
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    //Executa antes da Action
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation("### Executado -> OnActionExecuting");
        _logger.LogInformation("#################################################################");
        _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
        _logger.LogInformation($"ModelState : {context.ModelState.IsValid}");
        _logger.LogInformation("#################################################################"); 
    }

    //Executa depois da Action
    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("### Executado -> OnActionExecuted");
        _logger.LogInformation("#################################################################");
        _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
        _logger.LogInformation($"Status Code : {context.HttpContext.Response.StatusCode}");
        _logger.LogInformation("#################################################################");
    }
}
