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
    double maxAltitude = double.MinValue;
    Stopwatch stopwatch = new Stopwatch();
    CancellationTokenSource? cts;
    double maxSessionSpeed = 0;
    Location? lastLocation = null;
    double totalDistanceKm = 0;
    Polyline currentRouteLine;
    Queue<double> altitudeHistory = new();
    const int SmoothingWindow = 5;
    List<Color> descentColors = new()
    {
        Color.FromArgb("#8B2EDB"),
        Color.FromArgb("#2E19DF"),
        Color.FromArgb("#F6E241"),
        Color.FromArgb("#8A9247"),
        Color.FromArgb("#951C0C"),
        Color.FromArgb("#8CF357"),
        Color.FromArgb("#F010BD"),
        Color.FromArgb("#89601F"),
        Color.FromArgb("#BE8DF8"),
        Color.FromArgb("#038473")
    };
    List<Polyline> allRouteLines = new(); // 🔄 to collect all descent lines


    DateTime lastAlertTime = DateTime.MinValue;
    TimeSpan alertCooldown = TimeSpan.FromSeconds(10);

    public MainPage()
    {
        InitializeComponent();
#if ANDROID || IOS
        MapBorder.IsVisible = true;
#else
        MapBorder.Content = null;
#endif
        currentRouteLine = CreateNewPolyline();
        LiveMap.MapElements.Add(currentRouteLine);
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
            if (status != PermissionStatus.Granted) return;

            Location? location = await Geolocation.GetLastKnownLocationAsync();
            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            }

            if (location != null)
            {
                var mapSpan = MapSpan.FromCenterAndRadius(new Location(location.Latitude, location.Longitude), Distance.FromKilometers(1));
                await MainThread.InvokeOnMainThreadAsync(() => LiveMap.MoveToRegion(mapSpan));
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

        currentRouteLine = CreateNewPolyline();
        LiveMap.MapElements.Add(currentRouteLine);
        allRouteLines.Add(currentRouteLine);

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

                bool isDescending = false;

                if (altitude.HasValue)
                {
                    altitudeHistory.Enqueue(altitude.Value);
                    if (altitudeHistory.Count > SmoothingWindow)
                        altitudeHistory.Dequeue();

                    if (altitudeHistory.Count == SmoothingWindow)
                    {
                        double trend = altitudeHistory.Last() - altitudeHistory.First();

                        if (trend > 10.0)
                        {
                            ascents++;
                            isDescending = false;
                        }
                        else if (trend < -10.0)
                        {
                            descents++;
                            isDescending = true;
                            currentRouteLine = CreateNewPolyline();
                            LiveMap.MapElements.Add(currentRouteLine);
                        }
                    }
                }

                previousAltitude = altitude;

                if (alertsEnabled && speed > maxAllowed && DateTime.Now - lastAlertTime > alertCooldown)
                {
                    lastAlertTime = DateTime.Now;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(600)); } catch { }
                        try
                        {
                            var audioFile = await FileSystem.OpenAppPackageFileAsync("alert.mp3");
                            AudioManager.Current?.CreatePlayer(audioFile)?.Play();
                        }
                        catch { }
                    });
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LiveMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(location.Latitude, location.Longitude), Distance.FromKilometers(0.3)));
                    if (isDescending)
                        currentRouteLine.Geopath.Add(new Location(location.Latitude, location.Longitude));
                });
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                DisplayAlert("Error", ex.Message, "OK"));
        }
    }

    private Polyline CreateNewPolyline()
    {
        var color = descentColors[descents % descentColors.Count];
        return new Polyline
        {
            StrokeColor = color,
            StrokeWidth = 5
        };
    }

    private async void OnStopClicked(object sender, EventArgs e)
    {
        if (!isTracking) return;

        isTracking = false;
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
        stopwatch.Stop();
        cts?.Cancel();

        string filename = $"SlopeSession_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, filename);
        //string filePath = System.IO.Path.Combine(FileSystem.AppDataDirectory, filename);


        await SaveMapSnapshotAsync(filePath);

        var session = new SkiSession
        {
            Date = DateTime.Now,
            Duration = stopwatch.Elapsed.ToString(@"hh\:mm\:ss"),
            Distance = totalDistanceKm,
            MaxSpeed = maxSessionSpeed,
            MaxAltitude = maxAltitude,
            Ascents = ascents,
            Descents = descents,
            MapImagePath = filePath
        };

        await DatabaseService.InsertSessionAsync(session);

        await DisplayAlert("SlopeGuard", $"Session Summary:\n" +
                         $"- Duration: {session.Duration}\n" +
                         $"- Distance: {session.Distance:F2} km\n" +
                         $"- Max Speed: {session.MaxSpeed:F1} km/h\n" +
                         $"- Max Altitude: {session.MaxAltitude}\n" +
                         $"- Ascents: {session.Ascents}\n" +
                         $"- Descents: {session.Descents}", "OK");

        ResetSessionData();
    }


    private async Task SaveMapSnapshotAsync(string path)
    {
        try
        {
            var allPoints = allRouteLines.SelectMany(l => l.Geopath).ToList();
            if (allPoints.Count < 2)
            {
                Console.WriteLine("Not enough points to generate map snapshot.");
                return;
            }

            var mapSpan = GetBoundingSpan(allPoints);
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                LiveMap.MoveToRegion(mapSpan);
            });

            await Task.Delay(1500);

            var snapshot = await LiveMap.CaptureAsync();
            if (snapshot != null)
            {
                using var stream = await snapshot.OpenReadAsync();
                using var file = File.Create(path);
                await stream.CopyToAsync(file);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Snapshot error: {ex.Message}");
        }
    }


    private MapSpan GetBoundingSpan(IEnumerable<Location> positions)
    {
        double minLat = positions.Min(p => p.Latitude);
        double maxLat = positions.Max(p => p.Latitude);
        double minLon = positions.Min(p => p.Longitude);
        double maxLon = positions.Max(p => p.Longitude);

        var center = new Location((minLat + maxLat) / 2, (minLon + maxLon) / 2);
        double radiusKm = Location.CalculateDistance(minLat, minLon, maxLat, maxLon, DistanceUnits.Kilometers) / 2;

        return MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(radiusKm + 0.1));
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

        LiveMap.MapElements.Clear();
        currentRouteLine = CreateNewPolyline();
        LiveMap.MapElements.Add(currentRouteLine);
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
