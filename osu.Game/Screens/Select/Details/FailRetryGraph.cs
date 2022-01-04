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

        private APIFailTimes failTimes;

        public APIFailTimes FailTimes
        {
            get => failTimes;
            set
            {
                if (value == failTimes) return;

                failTimes = value;

                int[] retries = FailTimes?.Retries ?? Array.Empty<int>();
                int[] fails = FailTimes?.Fails ?? Array.Empty<int>();
                int[] retriesAndFails = sumRetriesAndFails(retries, fails);

                float maxValue = retriesAndFails.Any() ? retriesAndFails.Max() : 0;
                failGraph.MaxValue = maxValue;
                retryGraph.MaxValue = maxValue;

                failGraph.Values = fails.Select(v => (float)v);
                retryGraph.Values = retriesAndFails.Select(v => (float)v);
            }
        }

        private int[] sumRetriesAndFails(int[] retries, int[] fails)
        {
            int[] result = new int[Math.Max(retries.Length, fails.Length)];

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
