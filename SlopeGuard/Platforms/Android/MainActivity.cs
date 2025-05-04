using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using System.IO;
using System.Linq;
using Android.Gms.Maps;

namespace SlopeGuard
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
              LaunchMode = LaunchMode.SingleTop,
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                                     ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                                     ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Platform.Init(this, savedInstanceState);

            // ✅ Load API key and set it at runtime
            string apiKey = LoadGoogleMapsApiKey();
            if (!string.IsNullOrEmpty(apiKey))
            {
                MapsInitializer.Initialize(this);
                //MapView.SetApiKey(apiKey);
            }
        }

        private string LoadGoogleMapsApiKey()
        {
            try
            {
                using var stream = Assets?.Open("Platforms/Android/secrets.env");
                if (stream == null)
                {
                    Android.Util.Log.Warn("SlopeGuard", "secrets.env file not found in Assets.");
                    return string.Empty;
                }

                using var reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line) && line.TrimStart().StartsWith("GOOGLE_MAPS_API_KEY="))
                    {
                        var parts = line.Split('=', 2);
                        if (parts.Length == 2)
                            return parts[1].Trim();
                    }
                }

                Android.Util.Log.Warn("SlopeGuard", "GOOGLE_MAPS_API_KEY not found in secrets.env.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("SlopeGuard", $"Failed to load API key: {ex.Message}");
                return string.Empty;
            }
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
    }
}
