namespace IntelligentAutomation.BlazorApp.Services;

public class LoadingService
{
    public bool IsLoading { get; private set; }
    public event Action? OnChange;

    public void Show()
    {
        IsLoading = true;
        OnChange?.Invoke();
    }

    public void Hide()
    {
        IsLoading = false;
        OnChange?.Invoke();
    }
}