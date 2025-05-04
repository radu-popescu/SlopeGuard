using Microsoft.Maui.Controls;
using SlopeGuard.Models;
using SlopeGuard.Services;
using System.Diagnostics;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Plugin.Maui.Audio;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;


namespace SlopeGuard;

public partial class MainPage : ContentPage
{
    bool isTracking = false;
    int ascents = 0, descents = 0;
    double? previousAltitude = null;
    //double maxAltitude = 0;
    double maxAltitude = double.MinValue;
    Stopwatch stopwatch = new Stopwatch();
    CancellationTokenSource? cts;
    double maxSessionSpeed = 0;
    Location? lastLocation = null;
    double totalDistanceKm = 0;
    Polyline routeLine = new Polyline { StrokeColor = Colors.Red, StrokeWidth = 5 }; // new

    DateTime lastAlertTime = DateTime.MinValue;
    TimeSpan alertCooldown = TimeSpan.FromSeconds(10);

    public MainPage()
    {
        InitializeComponent();
#if ANDROID || IOS
        MapBorder.IsVisible = true;
#else
        MapBorder.Content = null; // Prevent Windows crash
#endif
        LiveMap.MapElements.Add(routeLine); // add polyline to map
        _ = CenterMapOnCurrentLocationAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CenterMapOnCurrentLocationAsync();
    }

    private async Task CenterMapOnCurrentLocationAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                Console.WriteLine("Location permission not granted.");
                return;
            }

            Location? location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                location = await Geolocation.GetLocationAsync(request);
            }

            if (location != null)
            {
                var mapSpan = MapSpan.FromCenterAndRadius(
                    new Location(location.Latitude, location.Longitude),
                    Distance.FromKilometers(1));

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LiveMap.MoveToRegion(mapSpan);
                });
            }
            else
            {
                Console.WriteLine("Unable to obtain current location.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Map centering failed: {ex.Message}");
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Location permission is required to start tracking.", "OK");
            return;
        }

        if (isTracking) return;

        isTracking = true;
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;

        stopwatch.Restart();
        cts = new CancellationTokenSource();

        routeLine.Geopath.Clear(); // clear any old route

        await StartTrackingSpeed(cts.Token);
    }

    private async Task StartTrackingSpeed(CancellationToken token)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
            bool alertsEnabled = Preferences.Get("SpeedAlertEnabled", true);
            double maxAllowed = Preferences.Get("MaxSpeed", 50.0);

            await foreach (var location in Services.GeolocationExtensions.GetLocationUpdatesFallback(request, token))
            {
                if (location == null) continue;

                double speed = (location.Speed ?? 0) * 3.6;
                double? altitude = location.Altitude;

                if (altitude.HasValue && altitude.Value > maxAltitude)
                    maxAltitude = altitude.Value;

                if (lastLocation != null)
                {
                    double dist = Location.CalculateDistance(lastLocation, location, DistanceUnits.Kilometers);
                    totalDistanceKm += dist;
                }
                lastLocation = location;

                if (speed > maxSessionSpeed)
                    maxSessionSpeed = speed;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SpeedLabelValue.Text = $"{speed:F1}";
                    AltitudeLabelValue.Text = altitude.HasValue ? $"{altitude.Value:F0}" : "N/A";
                    DurationLabelValue.Text = $"{stopwatch.Elapsed:hh\\:mm\\:ss}";
                    AscentsLabelValue.Text = ascents.ToString();
                    DescentsLabelValue.Text = descents.ToString();
                    DistanceLabelValue.Text = $"{totalDistanceKm:F1}";
                });

                if (previousAltitude != null)
                {
                    if (altitude - previousAltitude > 1.0)
                        ascents++;
                    else if (previousAltitude - altitude > 1.0)
                        descents++;
                }
                previousAltitude = altitude;

                if (alertsEnabled && speed > maxAllowed && DateTime.Now - lastAlertTime > alertCooldown)
                {
                    lastAlertTime = DateTime.Now;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try
                        {
                            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(600));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Vibration error: " + ex.Message);
                        }

                        try
                        {
                            IAudioManager audioManager = AudioManager.Current;
                            var audioFile = await FileSystem.OpenAppPackageFileAsync("alert.mp3");
                            var player = audioManager?.CreatePlayer(audioFile);
                            player?.Play();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Audio error: " + ex.Message);
                        }
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LiveMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(location.Latitude, location.Longitude), Distance.FromKilometers(0.3)));
                    routeLine.Geopath.Add(new Location(location.Latitude, location.Longitude));
                });
            }
        }
        catch (TaskCanceledException)
        {
            // Expected on stop
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                DisplayAlert("Error", ex.Message, "OK"));
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
            Distance = totalDistanceKm,
            MaxSpeed = maxSessionSpeed,
            MaxAltitude = maxAltitude,
            Ascents = ascents,
            Descents = descents
        };

        await DatabaseService.InsertSessionAsync(session);

        string summary = $"Session Summary:\n" +
                         $"- Duration: {session.Duration}\n" +
                         $"- Distance: {session.Distance:F2} km\n" +
                         $"- Max Speed: {session.MaxSpeed:F1} km/h\n" +
                         $"- Max Altitude: {session.MaxAltitude}\n" +
                         $"- Ascents: {session.Ascents}\n" +
                         $"- Descents: {session.Descents}\n";

        await DisplayAlert("SlopeGuard", summary, "OK");

        ResetSessionData();
    }

    private void ResetSessionData()
    {
        maxSessionSpeed = 0;
        ascents = 0;
        descents = 0;
        maxAltitude = 0;
        previousAltitude = null;

        SpeedLabelValue.Text = "0.0";
        AltitudeLabelValue.Text = "0";
        DurationLabelValue.Text = "00:00:00";
        AscentsLabelValue.Text = "0";
        DescentsLabelValue.Text = "0";
        DistanceLabelValue.Text = "0.0";
        routeLine.Geopath.Clear();
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
