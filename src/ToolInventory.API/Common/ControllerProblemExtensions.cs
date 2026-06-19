using Microsoft.AspNetCore.Mvc;

namespace ToolInventory.API.Common;

public static class ControllerProblemExtensions
{
    public static ObjectResult BadRequestProblem(this ControllerBase controller, string title, string detail)
        => controller.ProblemResult(StatusCodes.Status400BadRequest, title, detail);

    public static ObjectResult ConflictProblem(this ControllerBase controller, string title, string detail)
        => controller.ProblemResult(StatusCodes.Status409Conflict, title, detail);

    public static ObjectResult NotFoundProblem(this ControllerBase controller, string title, string detail)
        => controller.ProblemResult(StatusCodes.Status404NotFound, title, detail);

    private static ObjectResult ProblemResult(this ControllerBase controller, int statusCode, string title, string detail)
        => new(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        })
        {
            StatusCode = statusCode
        };
}
