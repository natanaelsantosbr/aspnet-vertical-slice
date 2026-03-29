using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Imoveis.API.Middleware;

/// <summary>
/// Captura exceções não tratadas e retorna ProblemDetails padronizado.
/// Exceções de negócio esperadas são tratadas via Result&lt;T&gt; nos handlers —
/// este middleware cobre apenas falhas inesperadas de infraestrutura.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, titulo) = exception switch
        {
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Requisição cancelada"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor")
        };

        LogException(exception, httpContext, statusCode);

        httpContext.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = statusCode == 500 ? "Ocorreu um erro inesperado. Tente novamente em instantes." : null,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private void LogException(Exception exception, HttpContext ctx, int statusCode)
    {
        if (exception is OperationCanceledException)
        {
            _logger.LogInformation(
                "Requisição cancelada pelo cliente: {Method} {Path}",
                ctx.Request.Method, ctx.Request.Path);
            return;
        }

        _logger.LogError(
            exception,
            "Exceção não tratada [{StatusCode}]: {Method} {Path}",
            statusCode, ctx.Request.Method, ctx.Request.Path);
    }
}
