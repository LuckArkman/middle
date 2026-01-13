namespace IntelligentAutomation.WebApp.Services;

public class LoadingHandler : DelegatingHandler
{
    private readonly LoadingService _loadingService;

    public LoadingHandler(LoadingService loadingService)
    {
        _loadingService = loadingService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _loadingService.Show();
        try
        {
            // Continua com a requisição original
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            // Garante que o indicador seja escondido, mesmo que a requisição falhe
            _loadingService.Hide();
        }
    }
}