// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Views;
using osu.Framework.Android;

namespace osu.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.FullUser, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = false)]
    [IntentFilter(new[] { Intent.ActionDefault, Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataPathPatterns = new[] { ".*\\.osz", ".*\\.osk", ".*\\.osr" }, DataMimeType = "application/*")]
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
            switch (intent.Action)
            {
                case Intent.ActionDefault:
                    if (intent.Scheme == ContentResolver.SchemeContent)
                        handleImportFromUri(intent.Data);
                    break;

                case Intent.ActionSend:
                {
                    var content = intent.ClipData?.GetItemAt(0);
                    if (content != null)
                        handleImportFromUri(content.Uri);
                    break;
                }
            }
        }

        private void handleImportFromUri(Uri uri)
        {
            var cursor = ContentResolver?.Query(uri, new[] { OpenableColumns.DisplayName }, null, null);

            if (cursor == null)
                return;

            cursor.MoveToFirst();

            var filenameColumn = cursor.GetColumnIndex(OpenableColumns.DisplayName);

            var stream = ContentResolver.OpenInputStream(uri);

            if (stream != null)
                Task.Factory.StartNew(() => game.Import(stream, cursor.GetString(filenameColumn)), TaskCreationOptions.LongRunning);
        }
    }
}
