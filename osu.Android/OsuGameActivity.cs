// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using osu.Framework.Android;
using osu.Game;

namespace osu.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorLandscape, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = true)]
    public class OsuGameActivity : AndroidGameActivity
    {
        protected override Framework.Game CreateGame()
            => new OsuGame();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }
    }
}
