using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Gms.Maps;
using Plugin.Firebase.RemoteConfig;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;

namespace SlopeGuard
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
              ConfigurationChanges = ConfigChanges.ScreenSize |
                                     ConfigChanges.Orientation |
                                     ConfigChanges.UiMode |
                                     ConfigChanges.ScreenLayout |
                                     ConfigChanges.SmallestScreenSize |
                                     ConfigChanges.Density)]

    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            Console.WriteLine("[MainActivity] OnCreate entered");

            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            // 1) Initialize default FirebaseApp (via google-services.json)
            try
            {
                FirebaseApp.InitializeApp(this);
                Console.WriteLine($"[MainActivity] FirebaseApp.Instance.Name = '{FirebaseApp.Instance?.Name}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainActivity] ❌ FirebaseApp.InitializeApp failed: {ex.Message}");
            }

            // 2) Verify that the plugin injected google_app_id
            try
            {
                int resId = Resources.GetIdentifier("google_app_id", "string", PackageName);
                if (resId != 0)
                {
                    var googleAppId = Resources.GetString(resId);
                    Console.WriteLine($"[MainActivity] Resource google_app_id = '{googleAppId}'");
                }
                else
                {
                    Console.WriteLine("[MainActivity] Resource google_app_id not found (resId=0)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainActivity] ❌ reading google_app_id: {ex.Message}");
            }

            // 3) Kick off Remote Config fetch
            _ = InitRemoteConfigAsync();

            // 4) If RC has provided a maps key, initialize Maps
            var mapsKey = AppConfig.MapsKey;
            Console.WriteLine($"[MainActivity] AppConfig.MapsKey = '{mapsKey}'");
            if (!string.IsNullOrWhiteSpace(mapsKey))
            {
                MapsInitializer.Initialize(this);
                Console.WriteLine("[MainActivity] MapsInitializer.Initialize called");
            }
        }


        async Task InitRemoteConfigAsync()
        {
            Console.WriteLine("[MainActivity] InitRemoteConfigAsync starting");
            try
            {
                var rc = CrossFirebaseRemoteConfig.Current;
                Console.WriteLine("[MainActivity] Obtained CrossFirebaseRemoteConfig.Current");

                await rc.SetDefaultsAsync(new Dictionary<string, object>
                {
                    ["maps_api_key"] = "",
                    ["firebase_api_key_android"] = "",
                    ["firebase_api_key_ios"] = ""
                });
                Console.WriteLine("[MainActivity] SetDefaultsAsync done");

                await rc.FetchAndActivateAsync();
                Console.WriteLine("[MainActivity] FetchAndActivateAsync done");

                AppConfig.MapsKey = rc.GetString("maps_api_key");
                AppConfig.FirebaseKey = DeviceInfo.Platform == DevicePlatform.Android
                    ? rc.GetString("firebase_api_key_android")
                    : rc.GetString("firebase_api_key_ios");

                Console.WriteLine($"[MainActivity] RC → MapsKey = '{AppConfig.MapsKey}'");
                Console.WriteLine($"[MainActivity] RC → FirebaseKey = '{AppConfig.FirebaseKey}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MainActivity] ❌ Remote Config init FAILED: {ex}");
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
