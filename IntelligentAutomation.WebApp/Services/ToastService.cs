using Microsoft.JSInterop;

namespace IntelligentAutomation.WebApp.Services;

public class ToastService
{
    private readonly IJSRuntime _jsRuntime;
    public ToastService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

    public async Task ShowSuccess(string message)
    {
        await _jsRuntime.InvokeVoidAsync("showToast", "success", message);
    }

    public async Task ShowError(string message)
    {
        await _jsRuntime.InvokeVoidAsync("showToast", "error", message);
    }
}