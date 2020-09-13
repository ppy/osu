// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

                var retries = Metrics?.Retries ?? Array.Empty<int>();
                var fails = Metrics?.Fails ?? Array.Empty<int>();
                var retriesAndFails = sumRetriesAndFails(retries, fails);

                float maxValue = retriesAndFails.Any() ? retriesAndFails.Max() : 0;
                failGraph.MaxValue = maxValue;
                retryGraph.MaxValue = maxValue;

                failGraph.Values = fails.Select(v => (float)v);
                retryGraph.Values = retriesAndFails.Select(v => (float)v);
            }
        }

        private int[] sumRetriesAndFails(int[] retries, int[] fails)
        {
            var result = new int[Math.Max(retries.Length, fails.Length)];

            for (int i = 0; i < retries.Length; ++i)
                result[i] = retries[i];

            for (int i = 0; i < fails.Length; ++i)
                result[i] += fails[i];

            return result;
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
