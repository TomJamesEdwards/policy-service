using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PolicyService.Api.ExceptionHandling;

internal sealed partial class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(problemDetailsService);

        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        LogUnhandledException(
            _logger,
            httpContext.Request.Method,
            httpContext.Request.Path.Value ?? string.Empty,
            httpContext.TraceIdentifier,
            exception);

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await _problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status =
                        StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred",
                    Extensions =
                    {
                        ["code"] = "server.unexpected_error",
                        ["traceId"] =
                            httpContext.TraceIdentifier
                    }
                }
            })
            .ConfigureAwait(false);

        return true;
    }

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message =
            "Unhandled exception while processing {Method} {Path}. "
            + "Trace identifier: {TraceId}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        string method,
        string path,
        string traceId,
        Exception exception);
}