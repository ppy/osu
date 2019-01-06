// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Android.App;
using Android.Content.PM;
using osu.Framework.Android;
using osu.Game;

namespace osu.Android
{
    [Activity(Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorLandscape, SupportsPictureInPicture = false, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class OsuGameActivity : AndroidGameActivity
    {
        protected override Framework.Game CreateGame()
            => new OsuGame();
    }
}
