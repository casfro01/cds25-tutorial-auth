using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Etc;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(exception, exception.Message);

        var status = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            AuthenticationError => StatusCodes.Status401Unauthorized,
            ForbiddenError => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
        httpContext.Response.StatusCode = status;
        if (status != StatusCodes.Status500InternalServerError)
        {
            var problemDetails = new ProblemDetails
            {
                Title = exception.Message,
                Detail = exception.InnerException?.Message,
                Status = status,
            };
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        }

        return true;
    }
}
