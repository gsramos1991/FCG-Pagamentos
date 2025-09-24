using System.Text;
using System.Text.Json;
using FCG.Pagamentos.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using FluentAssertions;

namespace FCG.Pagamentos.Tests.Tests;

public class MiddlewareTests
{
    [Fact]
    public async Task RequestLoggingMiddleware_Should_Set_Correlation_Header()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";

        var logger = NullLogger<RequestLoggingMiddleware>.Instance;
        var middleware = new RequestLoggingMiddleware(async ctx =>
        {
            // Simula próximo middleware/endpoint
            await Task.CompletedTask;
        }, logger);

        await middleware.Invoke(context);

        context.Response.Headers.TryGetValue("X-Correlation-Id", out var header).Should().BeTrue();
        header.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RequestLoggingMiddleware_Should_Respect_Existing_Correlation_Header()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test2";
        var provided = Guid.NewGuid().ToString("N");
        context.Request.Headers["X-Correlation-Id"] = provided;

        var logger = NullLogger<RequestLoggingMiddleware>.Instance;
        var middleware = new RequestLoggingMiddleware(async ctx => await Task.CompletedTask, logger);
        await middleware.Invoke(context);

        context.Response.Headers["X-Correlation-Id"].ToString().Should().Be(provided);
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_Should_Return_ProblemDetails_With_CorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/failure";
        var corr = Guid.NewGuid().ToString("N");
        context.Request.Headers["X-Correlation-Id"] = corr;
        context.Response.Body = new MemoryStream();

        var logger = NullLogger<ErrorHandlingMiddleware>.Instance;
        var middleware = new ErrorHandlingMiddleware(async ctx =>
        {
            throw new InvalidOperationException("boom");
        }, logger);

        await middleware.Invoke(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        var doc = JsonDocument.Parse(json);
        // Em ASP.NET Core, Extensions são promovidas para a raiz do objeto
        doc.RootElement.TryGetProperty("correlationId", out var corrProp).Should().BeTrue();
        corrProp.GetString().Should().Be(corr);
    }
}
