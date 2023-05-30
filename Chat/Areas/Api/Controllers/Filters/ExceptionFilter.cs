namespace Chat.Areas.Api.Controllers.Filters;
using Microsoft.AspNetCore.Mvc.Filters;
using Chat.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

public class ExceptionFilter : IActionFilter
{

    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }


    public void OnActionExecuted(ActionExecutedContext context)
    {
        var exception = context.Exception;
        if (exception != null)
        {
            if (exception is NotFoundException)
            {
                context.Result = new NotFoundObjectResult(exception.Message);
                context.ExceptionHandled = true;
            }
            else if (exception is ValidationException validationException)
            {
                var problemDetails = new ValidationProblemDetails(validationException.ModelState)
                {
                    Title = "One or more validation errors occurred.",
                    Status = 400,
                    Detail = "See the errors property for details."
                };

                context.Result = new BadRequestObjectResult(problemDetails);
                context.ExceptionHandled = true;
            }
            else if (exception is ArgumentException)
            {
                context.Result = new BadRequestObjectResult(exception.Message);
                context.ExceptionHandled = true;
            }
            else if (exception is EnvironmentVariableException)
            {
                _logger.LogError(exception.Message);
                context.Result = new ObjectResult(exception.Message)
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
            }
            else
            {
                _logger.LogError(exception.Message);
                context.Result = new ObjectResult("Internal Server Error")
                {
                    StatusCode = 500
                };
                context.ExceptionHandled = true;
            }

        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}