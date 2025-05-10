using System;
using Microsoft.Maui.Controls;
using SlopeGuard.Services;
using Microsoft.Maui.Graphics;

namespace SlopeGuard;

public partial class SettingsPage : ContentPage
{
    private readonly FirebaseService _firebaseService;
    private string generatedGuid = string.Empty;

    public SettingsPage(FirebaseService firebaseService)
    {
        InitializeComponent();
        _firebaseService = firebaseService;

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
        if (int.TryParse(text, out int value) && value >= 10 && value <= 100)
        {
            SpeedEntry.TextColor = Colors.White;
            SaveButton.IsEnabled = true;
            ValidationLabel.IsVisible = false;
        }
        else
        {
            SpeedEntry.TextColor = Colors.Red;
            SaveButton.IsEnabled = false;
            ValidationLabel.Text = "Speed must be between 10 and 100 km/h.";
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
            //await Shell.Current.GoToAsync("..");
        }
        else
        {
            await DisplayAlert("Error", "Please enter a valid number", "OK");
        }
    }

    // Generate a new GUID when the button is clicked
    private void OnGenerateGuidButtonClicked(object sender, EventArgs e)
    {
        generatedGuid = Guid.NewGuid().ToString(); // Generate a new GUID
        GuidLabel.Text = generatedGuid; // Update the GUID label

        // Enable the "Start Pairing" button
        StartPairingButton.IsEnabled = true;

        // Clear pairing feedback message
        PairingFeedbackLabel.IsVisible = false;
    }

    // Copy the generated GUID to the clipboard
    private void OnCopyGuidButtonClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(generatedGuid))
        {
            Clipboard.SetTextAsync(generatedGuid); // Copy GUID to clipboard
            DisplayAlert("Success", "GUID copied to clipboard", "OK");
        }
        else
        {
            DisplayAlert("Error", "Please generate a GUID first", "OK");
        }
    }

    // Start pairing with the generated GUID
    private async void OnStartPairingButtonClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(generatedGuid))
        {
            PairingFeedbackLabel.Text = "Please generate a GUID first.";
            PairingFeedbackLabel.IsVisible = true;
            return;
        }

        // Simulate pairing (e.g., check Firebase, etc.)
        bool pairingSuccess = await TryPairWithGUID(generatedGuid);

        if (pairingSuccess)
        {
            PairingFeedbackLabel.Text = "Pairing successful!";
            PairingFeedbackLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#00FF00");
        }
        else
        {
            PairingFeedbackLabel.Text = "Pairing failed. Try again.";
            PairingFeedbackLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF0000");
        }

        PairingFeedbackLabel.IsVisible = true;
    }

    // Simulate pairing logic (in reality, connect devices via Firebase or another method)
    private Task<bool> TryPairWithGUID(string guid)
    {
        // For now, simulate pairing success after a brief delay
        return Task.FromResult(true); // This should be replaced with actual pairing logic
    }
}
