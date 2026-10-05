using FluentValidation;
using MohammedRaouf.Application.Common;

namespace MohammedRaouf.Api.Endpoints;

internal static class EndpointHttp
{
    public static async Task<IResult?> ValidateAsync<T>(IValidator<T> validator, T request)
    {
        var validation = await validator.ValidateAsync(request);
        if (validation.IsValid)
        {
            return null;
        }

        var errors = validation.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        return Results.ValidationProblem(errors);
    }

    public static IResult ToHttp(ActionResult result) =>
        result.Succeeded
            ? Results.Ok()
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);

    public static IResult ToHttp<T>(ActionResult<T> result)
    {
        if (!result.Succeeded)
        {
            return Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
        }

        return result.StatusCode switch
        {
            201 => Results.Created(string.Empty, result.Value),
            204 => Results.NoContent(),
            _ => Results.Ok(result.Value)
        };
    }
}
