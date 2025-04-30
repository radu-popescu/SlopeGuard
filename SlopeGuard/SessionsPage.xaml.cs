using SlopeGuard.Services;
using SlopeGuard.Models;

namespace SlopeGuard;

public partial class SessionsPage : ContentPage
{
    public SessionsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSessions();
    }

    public async Task LoadSessions()
    {
        await DatabaseService.InitAsync();
        var sessions = await DatabaseService.GetSessionsAsync();
        SessionList.ItemsSource = sessions;
    }

    private void OnDeleteSession(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is int id)
        {
            // Use MainThread for UI work inside async
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool confirm = await DisplayAlert("Delete", "Are you sure you want to delete this session?", "Yes", "No");
                if (confirm)
                {
                    await DatabaseService.DeleteSessionAsync(id);
                    await LoadSessions(); // reload updated list
                }
            });
        }
    }


}
