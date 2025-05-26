using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters;

public class APIExceptionFilter : IExceptionFilter
{
    private readonly ILogger<APIExceptionFilter> _logger;

    public APIExceptionFilter(ILogger<APIExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        //Informação que será registrada no arquivo TXT de logger:
        _logger.LogError(context.Exception, "Ocorreu uma exceção não tratada: Status code 500");

        //Informação que será exibida na API como resposta:
        context.Result = new ObjectResult("Ocorreu um problema ao tratar a sua solicitação: Status Code 500")
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
    }
}
