using System.Collections.ObjectModel;
using SlopeGuard.Models;

namespace SlopeGuard.ViewModels;

public class SessionsViewModel
{
    public ObservableCollection<SkiSession> Sessions { get; set; } = new();
}
