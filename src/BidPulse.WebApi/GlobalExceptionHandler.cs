using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BidPulse.WebApi;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var statusCode = exception switch
        {
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            ArgumentNullException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = exception switch
            {
                OperationCanceledException => "Request cancelled",
                ArgumentNullException => "Bad request",
                _ => "Internal server error"
            },
            Detail = exception.Message
        };
        
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}