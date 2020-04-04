// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.App;
using Android.Content.PM;
using osu.Framework.Android;
using osu.Game.Tests;

namespace osu.Game.Rulesets.Mania.Tests.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorLandscape, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AndroidGameActivity
    {
        protected override Framework.Game CreateGame() => new OsuTestBrowser();
    }
}
