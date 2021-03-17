// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Dashboard.Home
{
    public class DashboardNewBeatmapPanel : DashboardBeatmapPanel
    {
        public DashboardNewBeatmapPanel(BeatmapSetInfo setInfo)
            : base(setInfo)
        {
        }

        protected override Drawable CreateInfo() => new DrawableDate(SetInfo.OnlineInfo.Ranked ?? DateTimeOffset.Now, 10, false)
        {
            Colour = ColourProvider.Foreground1
        };
    }
}
