using System.Collections.ObjectModel;
using SlopeGuard.Models;

namespace SlopeGuard.ViewModels;

public class SessionsViewModel
{
    public ObservableCollection<SkiSession> Sessions { get; set; } = new();

    public async Task LoadSessionsAsync()
    {
        Sessions.Clear();
        var sessions = await Services.DatabaseService.GetSessionsAsync();
        foreach (var session in sessions)
        {
            Console.WriteLine($"[VM] Loading session image: {session.MapImagePath}, Exists: {File.Exists(session.MapImagePath)}");
            Sessions.Add(session);
        }
    }

}
