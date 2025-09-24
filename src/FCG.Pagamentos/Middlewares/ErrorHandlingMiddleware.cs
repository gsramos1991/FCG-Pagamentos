using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace FCG.Pagamentos.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = GetCorrelationId(context);
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("ClassName", nameof(ErrorHandlingMiddleware)))
            using (LogContext.PushProperty("MethodName", nameof(Invoke)))
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            await WriteProblemDetailsAsync(context, ex, correlationId);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        if (context.Response.Headers.TryGetValue("X-Correlation-Id", out var h) && !string.IsNullOrWhiteSpace(h))
            return h.ToString();
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var hr) && !string.IsNullOrWhiteSpace(hr))
            return hr.ToString();
        return context.TraceIdentifier ?? Guid.NewGuid().ToString();
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception ex, string correlationId)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problem = new ProblemDetails
        {
            Title = "Erro interno no servidor",
            Status = context.Response.StatusCode,
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        problem.Extensions["correlationId"] = correlationId;

        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}
