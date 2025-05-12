using System;
using Firebase;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.Audio;
using SlopeGuard.Services;

namespace SlopeGuard
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiMaps() // ✅ REQUIRED for maps
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android => android
                        .OnCreate((activity, bundle) =>
                        {
                            // Initialize Firebase from google-services.json
                            var firebaseApp = FirebaseApp.InitializeApp(activity);
                            Console.WriteLine($"[Lifecycle] FirebaseApp default-instance = '{firebaseApp?.Name}'");

                            // Verify that google_app_id was injected
                            try
                            {
                                var resId = activity.Resources.GetIdentifier("google_app_id", "string", activity.PackageName);
                                var googleAppId = activity.Resources.GetString(resId);
                                Console.WriteLine($"[Lifecycle] google_app_id resource = {googleAppId}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[Lifecycle] FAILED to read google_app_id resource: {ex.Message}");
                            }
                        }));
#endif
                });

            // Your app services
            builder.Services.AddSingleton<FirebaseService>();
            builder.Services.AddSingleton<IAudioManager>(_ => AudioManager.Current);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
