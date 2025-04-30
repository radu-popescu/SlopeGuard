using Microsoft.Maui.Controls;
using SlopeGuard.Models;
using SlopeGuard.Services;
using System.Diagnostics;

namespace SlopeGuard;

public partial class MainPage : ContentPage
{
    bool isTracking = false;
    int ascents = 0, descents = 0;
    double? previousAltitude = null;
    Stopwatch stopwatch = new Stopwatch();
    CancellationTokenSource? cts;
    double maxSessionSpeed = 0;
    Location? lastLocation = null;
    double totalDistanceKm = 0;


    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (isTracking) return;

        isTracking = true;
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;

        stopwatch.Restart();
        cts = new CancellationTokenSource();

        await StartTrackingSpeed(cts.Token);
    }


    private async Task StartTrackingSpeed(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
                var location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    double speed = (location.Speed ?? 0) * 3.6; // Convert m/s to km/h
                    double altitude = location.Altitude ?? 0;

                    // ✅ Distance tracking
                    if (lastLocation != null)
                    {
                        double dist = Location.CalculateDistance(lastLocation, location, DistanceUnits.Kilometers);
                        totalDistanceKm += dist;
                    }
                    lastLocation = location;

                    // ✅ Max speed
                    if (speed > maxSessionSpeed)
                        maxSessionSpeed = speed;

                    // ✅ UI Updates
                    SpeedLabel.Text = $"Speed: {speed:F1} km/h";
                    AltitudeLabel.Text = $"Altitude: {altitude:F1} m";
                    DurationLabel.Text = $"Duration: {stopwatch.Elapsed:hh\\:mm\\:ss}";
                    AscentsLabel.Text = $"Ascents: {ascents}";
                    DescentsLabel.Text = $"Descents: {descents}";

                    // ✅ Ascents/descents
                    if (previousAltitude != null)
                    {
                        if (altitude - previousAltitude > 1.0)
                            ascents++;
                        else if (previousAltitude - altitude > 1.0)
                            descents++;
                    }
                    previousAltitude = altitude;

                    // ✅ Overspeed alert
                    double maxAllowed = Preferences.Get("MaxSpeed", 50.0);
                    if (speed > maxAllowed)
                    {
                        await DisplayAlert("Slow Down!", "You're exceeding your set speed limit.", "OK");
                    }
                }

                await Task.Delay(1000, token); // update every second
            }
        }
        catch (TaskCanceledException)
        {
            // expected when session stops
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }





    private async void OnStopClicked(object sender, EventArgs e)
    {
        if (!isTracking) return;

        isTracking = false;
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        stopwatch.Stop();
        cts?.Cancel();

        var session = new SkiSession
        {
            Date = DateTime.Now,
            Duration = stopwatch.Elapsed.ToString(@"hh\:mm\:ss"),
            MaxSpeed = maxSessionSpeed,
            Ascents = ascents,
            Descents = descents,
            Distance = totalDistanceKm // we’ll track this next!
        };

        await DatabaseService.InsertSessionAsync(session);

        string summary = $"Session Summary:\n" +
                         $"- Duration: {session.Duration}\n" +
                         $"- Max Speed: {session.MaxSpeed:F1} km/h\n" +
                         $"- Ascents: {session.Ascents}\n" +
                         $"- Descents: {session.Descents}\n" +
                         $"- Distance: {session.Distance:F2} km";

        await DisplayAlert("SlopeGuard", summary, "OK");

        ResetSessionData();
    }



    private void ResetSessionData()
    {
        maxSessionSpeed = 0;
        ascents = 0;
        descents = 0;
        previousAltitude = null;

        SpeedLabel.Text = "Speed: 0 km/h";
        AltitudeLabel.Text = "Altitude: 0 m";
        DurationLabel.Text = "Duration: 00:00:00";
        AscentsLabel.Text = "Ascents: 0";
        DescentsLabel.Text = "Descents: 0";
    }

    private async void OnViewSessionsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/sessions");
    }


    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("/settings");
    }
}
