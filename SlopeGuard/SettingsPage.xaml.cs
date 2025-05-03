using System;
using Microsoft.Maui.Controls;

namespace SlopeGuard;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        SpeedEntry.Text = Preferences.Get("MaxSpeed", 50.0).ToString();
        AlertSwitch.IsToggled = Preferences.Get("SpeedAlertEnabled", true);

        ValidateSpeedInput(SpeedEntry.Text);
    }

    private void OnSpeedEntryChanged(object sender, TextChangedEventArgs e)
    {
        var newText = e.NewTextValue;

        // Reject non-digit input
        if (!string.IsNullOrEmpty(newText) && !newText.All(char.IsDigit))
        {
            ((Entry)sender).Text = e.OldTextValue;
            return;
        }

        ValidateSpeedInput(newText);
    }

    private void ValidateSpeedInput(string text)
    {
        if (int.TryParse(text, out int value) && value >= 10 && value <= 200)
        {
            SpeedEntry.TextColor = Colors.White;
            SaveButton.IsEnabled = true;
            ValidationLabel.IsVisible = false;
        }
        else
        {
            SpeedEntry.TextColor = Colors.Red;
            SaveButton.IsEnabled = false;
            ValidationLabel.Text = "Speed must be between 10 and 200 km/h.";
            ValidationLabel.IsVisible = true;
        }
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
