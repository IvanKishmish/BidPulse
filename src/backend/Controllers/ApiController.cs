using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BidPulse.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    /// <summary>
    /// Converts a list of ErrorOr errors into the appropriate ProblemDetails response.
    /// If all errors are validation errors they are aggregated into a 422 ValidationProblem.
    /// Otherwise the first error determines the HTTP status code.
    /// </summary>
    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count == 0)
            return Problem();
 
        if (errors.All(e => e.Type == ErrorType.Validation))
            return ValidationProblem(errors);
 
        return Problem(errors[0]);
    }
 
    private IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict     => StatusCodes.Status409Conflict,
            ErrorType.Validation   => StatusCodes.Status400BadRequest,
            ErrorType.NotFound     => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden    => StatusCodes.Status403Forbidden,
            _                      => StatusCodes.Status500InternalServerError
        };
 
        return Problem(statusCode: statusCode, title: error.Description);
    }
 
    private IActionResult ValidationProblem(List<Error> errors)
    {
        var modelState = new ModelStateDictionary();
 
        foreach (var error in errors)
            modelState.AddModelError(error.Code, error.Description);
 
        return ValidationProblem(modelState);
    }
}
