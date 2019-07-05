// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Content;
using osu.Framework.Android;

namespace osu.Android
{
    public class OsuGameActivity : AndroidGameActivity
    {
        protected override Framework.Game CreateGame() => new OsuGameAndroid(getImportFilePath());

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        private string getImportFilePath()
        {
            if (Intent.Action != null
                && Intent.Action.Equals("android.intent.action.VIEW"))
            {
                return Intent.Data.Path;
            }
            return null;
        }
    }
}
