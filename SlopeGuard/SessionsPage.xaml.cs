using SlopeGuard.Services;
using SlopeGuard.Models;

namespace SlopeGuard;

public partial class SessionsPage : ContentPage
{
    private readonly ViewModels.SessionsViewModel viewModel = new();

    public SessionsPage()
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.LoadSessionsAsync();
        //await LoadSessions();
    }

    public async Task LoadSessions()
    {
        await Services.DatabaseService.InitAsync();
        var sessions = await Services.DatabaseService.GetSessionsAsync();

        viewModel.Sessions.Clear();
        foreach (var session in sessions)
            viewModel.Sessions.Add(session);
    }

    private void OnDeleteSession(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is int id)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                bool confirm = await DisplayAlert("Delete", "Are you sure you want to delete this session?", "Yes", "No");
                if (confirm)
                {
                    await Services.DatabaseService.DeleteSessionAsync(id);
                    await LoadSessions();
                }
            });
        }
    }
}

