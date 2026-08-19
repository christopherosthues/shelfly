using CommunityToolkit.Mvvm.ComponentModel;

namespace Shelfly.App.ViewModels;

public abstract class ShelflyViewModelBase : ObservableObject
{
    private CancellationTokenSource? _lifetimeCts;

    public async Task OnNavigatedToAsync()
    {
        if (_lifetimeCts != null)
        {
            await _lifetimeCts.CancelAsync();
            _lifetimeCts.Dispose();
        }

        _lifetimeCts = new CancellationTokenSource();

        try
        {
            await LoadAsync(_lifetimeCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Leaving the page
        }
    }

    protected abstract Task LoadAsync(CancellationToken cancellationToken);

    public virtual void OnNavigatingFrom()
    {
        _lifetimeCts?.Cancel();
    }
}