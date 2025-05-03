using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.ApplicationModel;

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

            // ✅ Essential for permissions and maps
            Platform.Init(this, savedInstanceState);
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
