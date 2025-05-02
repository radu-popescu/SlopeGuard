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

        // Load existing values
        SpeedEntry.Text = Preferences.Get("MaxSpeed", 50.0).ToString();
        AlertSwitch.IsToggled = Preferences.Get("SpeedAlertEnabled", true);
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (double.TryParse(SpeedEntry.Text, out double speed))
        {
            Preferences.Set("MaxSpeed", speed);
            Preferences.Set("SpeedAlertEnabled", AlertSwitch.IsToggled);

            await DisplayAlert("Saved", $"Max speed set to {speed} km/h", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid number", "OK");
        }
    }

}
