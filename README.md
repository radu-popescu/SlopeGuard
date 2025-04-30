# SlopeGuard 🏔️

**SlopeGuard** is a cross-platform ski tracking app built with .NET MAUI.  
It tracks your speed using GPS and alerts you if you go over a safe limit.
- Cross-platform (Android, iOS, Windows)
- Built with .NET MAUI

## Features
🧭 Live Ski Session Tracking

    Real-time GPS tracking using Geolocation.GetLocationAsync

    Speed calculation from GPS (converted from m/s to km/h)

    Altitude tracking (from GPS)

    Ascents and descents counted based on altitude change

    Session duration tracked using Stopwatch

    Distance traveled calculated using Haversine formula (Location.CalculateDistance)

    Max speed tracked for each session

⚠️ Safety Alert

    User-defined max speed limit (saved using Preferences)

    Alert triggered if current speed exceeds that limit

## License
[Apache 2.0](LICENSE)
