// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.Content.PM;
using Android.Content.Res;
using osu.Framework.Allocation;
using osu.Game.Mobile;

namespace osu.Android
{
    public partial class AndroidOrientationManager : OrientationManager
    {
        [Resolved]
        private OsuGameActivity gameActivity { get; set; } = null!;

        protected override bool IsCurrentOrientationPortrait => gameActivity.Resources!.Configuration!.Orientation == Orientation.Portrait;
        protected override bool IsTablet => gameActivity.IsTablet;

        protected override void SetAllowedOrientations(GameOrientation? orientation)
            => gameActivity.RequestedOrientation = orientation == null ? gameActivity.DefaultOrientation : toScreenOrientation(orientation.Value);

        private static ScreenOrientation toScreenOrientation(GameOrientation orientation)
        {
            if (orientation == GameOrientation.Locked)
                return ScreenOrientation.Locked;

            if (orientation == GameOrientation.Portrait)
                return ScreenOrientation.Portrait;

            if (orientation == GameOrientation.Landscape)
                return ScreenOrientation.Landscape;

            if (orientation == GameOrientation.FullPortrait)
                return ScreenOrientation.SensorPortrait;

            return ScreenOrientation.SensorLandscape;
        }
    }
}
