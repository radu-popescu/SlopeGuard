using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Gms.Maps;
using Firebase;
using Java.Lang;
using Java.Lang.Reflect;
using Microsoft.Maui.Devices;
using Plugin.Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using Com.Google.Android.Libraries.Maps;
//using Com.Google.Android.Libraries.Maps.Ktx;         // if you also included maps-ktx
//using Com.Google.Android.Libraries.Maps.MapsSdkInitializedCallback;


namespace SlopeGuard
{
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize
                             | ConfigChanges.Orientation
                             | ConfigChanges.UiMode
                             | ConfigChanges.ScreenLayout
                             | ConfigChanges.SmallestScreenSize
                             | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);

            Console.WriteLine("[MainActivity] ▶️ OnCreate entered");

            // 1) Init FirebaseApp from google-services.json
            try
            {
                FirebaseApp.InitializeApp(this);
                Console.WriteLine($"[MainActivity] ✅ FirebaseApp initialized: '{FirebaseApp.Instance?.Name}'");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[MainActivity] ❌ FirebaseApp.InitializeApp failed: {ex.Message}");
            }

            // 2) Kick off RC + MapsKey override
            _ = InitRemoteConfigAndMapsKeyAsync();
        }

        async Task InitRemoteConfigAndMapsKeyAsync()
        {
            Console.WriteLine("[MainActivity] ▶️  Remote Config fetch starting");
            var rc = CrossFirebaseRemoteConfig.Current;

            try
            {
                // 1) Safe defaults
                await rc.SetDefaultsAsync(new Dictionary<string, object>
                {
                    ["maps_api_key"] = ""
                    // …other defaults
                });
                Console.WriteLine("[MainActivity] ✅ SetDefaultsAsync done");

                // 2) Fetch & activate
                await rc.FetchAndActivateAsync();
                Console.WriteLine($"[MainActivity] ✅ FetchAndActivateAsync done");

                // 3) Pull into your static holder
                AppConfig.MapsKey = rc.GetString("maps_api_key");
                Console.WriteLine($"[MainActivity] 🔑 RC → maps_api_key = '{AppConfig.MapsKey}'");

                if (!string.IsNullOrWhiteSpace(AppConfig.MapsKey))
                {
                    // 4) Override the manifest <meta-data> at runtime
                    try
                    {
                        var pm = PackageManager;
                        var ai = pm.GetApplicationInfo(PackageName, PackageInfoFlags.MetaData);
                        ai.MetaData.PutString("com.google.android.geo.API_KEY", AppConfig.MapsKey);
                        Console.WriteLine("[MainActivity] ✅ Overrode manifest meta-data API_KEY");
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"[MainActivity] ❌ Manifest override failed: {ex}");
                    }

                    // 5) Initialize the Maps SDK (will pick up the new key)
                    Console.WriteLine("[MainActivity] 📍 Calling MapsInitializer.Initialize(context)");
                    MapsInitializer.Initialize(this);
                }
                else
                {
                    Console.WriteLine("[MainActivity] ⚠️ No maps_api_key to apply");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[MainActivity] ❌ Remote Config + Maps init failed: {ex}");
            }
        }




        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
