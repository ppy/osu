// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Views;
using osu.Framework.Android;

namespace osu.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.FullUser, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = false)]
    [IntentFilter(new[] { Intent.ActionDefault }, Categories = new[] { Intent.CategoryDefault }, DataScheme = "content", DataPathPatterns = new[] { ".*\\.osz", ".*\\.osk" }, DataMimeType = "application/*")]
    public class OsuGameActivity : AndroidGameActivity
    {
        private OsuGameAndroid game;

        protected override Framework.Game CreateGame() => game = new OsuGameAndroid(this);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // The default current directory on android is '/'.
            // On some devices '/' maps to the app data directory. On others it maps to the root of the internal storage.
            // In order to have a consistent current directory on all devices the full path of the app data directory is set as the current directory.
            System.Environment.CurrentDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            base.OnCreate(savedInstanceState);

            OnNewIntent(Intent);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent.Action == Intent.ActionView)
            {
                var cursor = ContentResolver.Query(intent.Data, null, null, null);
                var filename_column = cursor.GetColumnIndex(OpenableColumns.DisplayName);
                cursor.MoveToFirst();

                var stream = ContentResolver.OpenInputStream(intent.Data);
                if (stream != null)
                    // intent handler may run before the game has even loaded so we need to wait for the file importers to load before launching import
                    game.WaitForReady(() => game, _ => Task.Run(() => game.Import(stream, cursor.GetString(filename_column))));
            }

            base.OnNewIntent(intent);
        }
    }
}
