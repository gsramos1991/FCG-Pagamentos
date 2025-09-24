using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FCG.Pagamentos.Shared.Logging;

public static class LoggingScopes
{
    public static IDisposable BeginPaymentScope(this ILogger logger,
        string className,
        string methodName,
        Guid? paymentId = null,
        Guid? userId = null,
        string? correlationId = null,
        IDictionary<string, object>? extra = null)
    {
        var scope = new Dictionary<string, object>
        {
            ["ClassName"] = className,
            ["MethodName"] = methodName
        };

        if (paymentId.HasValue) scope["PaymentId"] = paymentId.Value;
        if (userId.HasValue) scope["UserId"] = userId.Value;
        if (!string.IsNullOrWhiteSpace(correlationId)) scope["CorrelationId"] = correlationId!;

        if (extra is not null)
        {
            foreach (var kv in extra)
            {
                scope[kv.Key] = kv.Value;
            }
        }

        return logger.BeginScope(scope);
    }
}

