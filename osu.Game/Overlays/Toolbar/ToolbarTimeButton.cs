// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarTimeButton : ToolbarButton
    {
        private readonly TimeSpan launchTick = new TimeSpan(DateTime.Now.Ticks);

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        public ToolbarTimeButton()
        {
            Width = 1;
            AutoSizeAxes = Axes.X;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateTime();
            updateBeatmaps();
        }

        private void updateTime()
        {
            var currentTime = DateTime.Now;
            DrawableText.Text = currentTime.ToString("HH:mm:ss tt");

            var currentTick = new TimeSpan(currentTime.Ticks);
            var xE = currentTick.Subtract(launchTick);
            string tooltipMain = "osu!已启动约";

            if (xE.Days > 0)
                tooltipMain += $"{xE.Days}天";

            if (xE.Hours > 0)
                tooltipMain += $"{xE.Hours}小时";

            if (xE.Minutes > 0)
                tooltipMain += $"{xE.Minutes}分";

            tooltipMain += $"{xE.Seconds}秒";

            TooltipMain = tooltipMain;

            this.Delay(500).Schedule(updateTime);
        }

        private void updateBeatmaps()
        {
            TooltipSub = $"共有{beatmapManager.GetAllUsableBeatmapSets(IncludedDetails.Minimal).Count}张谱面";

            this.Delay(60000).Schedule(updateBeatmaps);
        }
    }
}
