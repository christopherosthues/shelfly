namespace Shelfly.App;

public partial class LoadingPage : ContentPage
{
    public event EventHandler? RetryRequested;

    public void ShowError()
    {
        LoadingContent.IsVisible = false;
        ErrorContent.IsVisible = true;
    }

    private void OnRetryClicked(object? sender, EventArgs e) => RetryRequested?.Invoke(this, EventArgs.Empty);
}
