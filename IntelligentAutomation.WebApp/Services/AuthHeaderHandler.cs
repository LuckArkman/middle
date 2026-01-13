using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace IntelligentAutomation.WebApp.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthHeaderHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Tenta buscar o token do sessionStorage do navegador
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "authToken", cancellationToken);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch(Exception)
        {
            // Ignorar erros de JS se a renderização for estática (pré-renderização)
        }

        return await base.SendAsync(request, cancellationToken);
    }
}