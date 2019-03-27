// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using System.Linq;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select.Details
{
    public class FailRetryGraph : Container
    {
        private readonly BarGraph retryGraph, failGraph;

        private BeatmapMetrics metrics;

        public BeatmapMetrics Metrics
        {
            get => metrics;
            set
            {
                if (value == metrics) return;

                metrics = value;

                var retries = Metrics?.Retries ?? new int[0];
                var fails = Metrics?.Fails ?? new int[0];

                float maxValue = fails.Any() ? fails.Zip(retries, (fail, retry) => fail + retry).Max() : 0;
                failGraph.MaxValue = maxValue;
                retryGraph.MaxValue = maxValue;

                failGraph.Values = fails.Select(f => (float)f);
                retryGraph.Values = retries.Zip(fails, (retry, fail) => retry + MathHelper.Clamp(fail, 0, maxValue));
            }
        }

        public FailRetryGraph()
        {
            Children = new[]
            {
                retryGraph = new BarGraph
                {
                    RelativeSizeAxes = Axes.Both,
                },
                failGraph = new BarGraph
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            retryGraph.Colour = colours.Yellow;
            failGraph.Colour = colours.YellowDarker;
        }
    }
}
