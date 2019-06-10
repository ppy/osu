// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.App;
using Android.Content.PM;
using osu.Framework.Android;

namespace osu.Game.Tests.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorLandscape, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize, HardwareAccelerated = true)]
    public class MainActivity : AndroidGameActivity
    {
        protected override Framework.Game CreateGame() => new OsuTestBrowser();
    }
}
