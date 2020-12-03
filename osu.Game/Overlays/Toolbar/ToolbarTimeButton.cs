// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarTimeButton : ToolbarButton
    {
        private readonly TimeSpan launchTick = new TimeSpan(DateTime.Now.Ticks);
        private IBindable<WeakReference<BeatmapSetInfo>> beatmapUpdated;
        private IBindable<WeakReference<BeatmapSetInfo>> beatmapRemoved;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        public ToolbarTimeButton()
        {
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapUpdated = beatmapManager.ItemUpdated.GetBoundCopy();
            beatmapRemoved = beatmapManager.ItemRemoved.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateTime();
            beatmapUpdated.BindValueChanged(_ => updateBeatmapTooltip(), true);
            beatmapRemoved.BindValueChanged(_ => updateBeatmapTooltip());
        }

        private void updateTime()
        {
            var currentTime = DateTime.Now;
            DrawableText.Text = currentTime.ToString("HH:mm:ss tt");

            var currentTick = new TimeSpan(currentTime.Ticks);
            var xE = currentTick.Subtract(launchTick);
            string tooltipMain = "osu!已经运行了";

            if (xE.Days > 0)
                tooltipMain += $"{xE.Days}天";

            if (xE.Hours > 0)
                tooltipMain += $"{xE.Hours:00}:";

            tooltipMain += $"{xE.Minutes:00}:{xE.Seconds:00}。";

            TooltipMain = tooltipMain;

            this.Delay(500).Schedule(updateTime);
        }

        private void updateBeatmapTooltip() =>
            TooltipSub = $"你共有{beatmapManager.QueryBeatmapsMinimal(_ => true).ToList().Count}张谱面!";

        protected override bool OnClick(ClickEvent e)
        {
            return true;
        }
    }
}
