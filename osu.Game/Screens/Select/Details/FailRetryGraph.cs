﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
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
            get { return metrics; }
            set
            {
                if (value == metrics) return;
                metrics = value;

                var retries = Metrics.Retries;
                var fails = Metrics.Fails;

                float maxValue = fails.Zip(retries, (fail, retry) => fail + retry).Max();
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
