using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgentSaaS.Web.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
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
        _logger.LogError(exception, "Erro não tratado na requisição: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno no servidor",
            Detail = "Ocorreu um problema ao processar sua solicitação. Tente novamente mais tarde."
        };

        // Customização baseada no tipo de erro
        if (exception is PlanLimitReachedException) // Sua exceção customizada
        {
            problemDetails.Status = StatusCodes.Status403Forbidden;
            problemDetails.Title = "Limite do Plano Atingido";
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["upgradeUrl"] = "/Subscription/Upgrade";
        }
        else if (exception is UnauthorizedAccessException)
        {
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Title = "Acesso Negado";
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}