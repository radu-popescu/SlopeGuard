using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlopeGuard;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (double.TryParse(SpeedEntry.Text, out double speed))
        {
            Preferences.Set("MaxSpeed", speed);
            await DisplayAlert("Saved", $"Max speed set to {speed} km/h", "OK");

            // This will now work since we’re not using ShellContent
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid number", "OK");
        }
    }

}
