using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace FCG.Pagamentos.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private const string CorrelationHeader = "X-Correlation-Id";

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var correlationId = GetOrCreateCorrelationId(context);
        context.Response.Headers[CorrelationHeader] = correlationId;

        var (paymentId, userId) = ExtractRouteOrQueryIds(context);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("PaymentId", paymentId ?? Guid.Empty))
        using (LogContext.PushProperty("UserId", userId ?? Guid.Empty))
        {
            using (LogContext.PushProperty("ClassName", nameof(RequestLoggingMiddleware)))
            using (LogContext.PushProperty("MethodName", nameof(InitiateLog)))
            {
                _logger.LogInformation("Request started {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                using (LogContext.PushProperty("ClassName", nameof(RequestLoggingMiddleware)))
                using (LogContext.PushProperty("MethodName", nameof(Invoke)))
                {
                    _logger.LogInformation("Request completed {StatusCode} in {Elapsed:0.000} ms for {Method} {Path}",
                        context.Response?.StatusCode,
                        sw.Elapsed.TotalMilliseconds,
                        context.Request.Method,
                        context.Request.Path);
                }
            }
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationHeader, out var header) && !string.IsNullOrWhiteSpace(header))
        {
            return header.ToString();
        }

        var corr = context.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(corr))
        {
            corr = Guid.NewGuid().ToString();
        }
        context.Items[CorrelationHeader] = corr;
        return corr;
    }

    private static (Guid? paymentId, Guid? userId) ExtractRouteOrQueryIds(HttpContext context)
    {
        Guid? paymentId = null;
        Guid? userId = null;

        // From route values (case-insensitive)
        if (context.Request.RouteValues != null)
        {
            foreach (var kv in context.Request.RouteValues)
            {
                if (kv.Value is null) continue;
                var key = kv.Key.ToLowerInvariant();
                if ((key == "paymentid" || key == "id") && Guid.TryParse(kv.Value.ToString(), out var pid))
                    paymentId = pid;
                if ((key == "userid" || key == "user") && Guid.TryParse(kv.Value.ToString(), out var uid))
                    userId = uid;
            }
        }

        // From query string
        if (paymentId == null && context.Request.Query.TryGetValue("paymentId", out var qp) && Guid.TryParse(qp, out var qpId))
            paymentId = qpId;
        if (userId == null && context.Request.Query.TryGetValue("userId", out var qu) && Guid.TryParse(qu, out var quId))
            userId = quId;

        return (paymentId, userId);
    }

    // Dummy method name marker for start log
    private static void InitiateLog() { }
}
