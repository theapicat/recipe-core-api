using Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

// Oversetter Result fra Application til HTTP-svar. Feil returneres som ProblemDetails.
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result, Func<T, IActionResult> onSuccess)
        => result.Status switch
        {
            ResultStatus.Ok => onSuccess(result.Value!),
            _ => controller.ToFailure(result.Status, result.Error)
        };

    public static IActionResult ToActionResult(this ControllerBase controller, Result result, Func<IActionResult> onSuccess)
        => result.Status switch
        {
            ResultStatus.Ok => onSuccess(),
            _ => controller.ToFailure(result.Status, result.Error)
        };

    private static IActionResult ToFailure(this ControllerBase controller, ResultStatus status, string? error) => status switch
    {
        ResultStatus.NotFound => controller.NotFound(),
        ResultStatus.Conflict => controller.Problem(detail: error, statusCode: StatusCodes.Status409Conflict),
        ResultStatus.Invalid => controller.Problem(detail: error, statusCode: StatusCodes.Status400BadRequest),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Ukjent resultatstatus.")
    };
}
