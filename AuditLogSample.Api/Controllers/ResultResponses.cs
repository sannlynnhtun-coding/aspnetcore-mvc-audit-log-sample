namespace AuditLogSample.Api.Controllers;

internal static class ResultResponses
{
    internal static IActionResult ToActionResult(this ControllerBase controller, Result result)
    {
        if (result.IsFailure)
            return controller.BadRequest(result);

        return controller.Ok(result);
    }
}
