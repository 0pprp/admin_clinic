using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MohammedRaouf.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = CorrelationIdMiddleware.Read(httpContext);
        logger.LogError(exception, "Unhandled exception for {RequestPath} ({CorrelationId})", httpContext.Request.Path, correlationId);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "حدث خطأ غير متوقع.",
            Detail = "تعذر إكمال الطلب. استخدم رقم التتبع عند التواصل مع الدعم.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };
        problemDetails.Extensions["correlationId"] = correlationId;
        if (environment.IsDevelopment())
        {
            problemDetails.Extensions["debug"] = exception.GetType().Name;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
