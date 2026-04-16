// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneRatingDistributionGraph : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private RatingDistributionGraph graph = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = graph = new RatingDistributionGraph
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.5f, 0.25f)
            };
        });

        [Test]
        public void TestRandomData()
        {
            AddStep("set random data", () =>
            {
                List<(int x, int y)> values = new List<(int x, int y)>();
                for (int i = 400; i <= 2800; i += 100)
                    values.Add((i, (int)Math.Round(generateCount(i, 1600, 400, 7200))));
                graph.SetData(values.ToArray(), Random.Shared.Next(400, 2800));
            });
        }

        [Test]
        public void TestNoUserRating()
        {
            AddStep("set data", () =>
            {
                List<(int x, int y)> values = new List<(int x, int y)>();
                for (int i = 400; i <= 2800; i += 100)
                    values.Add((i, (int)Math.Round(generateCount(i, 1600, 400, 7200))));
                graph.SetData(values.ToArray(), null);
            });
        }

        [Test]
        public void TestNoData()
        {
            AddStep("set empty data", () => graph.SetData([], null));
        }

        [Test]
        public void TestOutOfBoundsUserRating()
        {
            AddStep("set data with max user rating", () =>
            {
                List<(int x, int y)> values = new List<(int x, int y)>();
                for (int i = 400; i <= 2800; i += 100)
                    values.Add((i, (int)Math.Round(generateCount(i, 1600, 400, 7200))));

                graph.SetData(values.ToArray(), 4000);
            });

            AddStep("set data with min user rating", () =>
            {
                List<(int x, int y)> values = new List<(int x, int y)>();
                for (int i = 400; i <= 2800; i += 100)
                    values.Add((i, (int)Math.Round(generateCount(i, 1600, 400, 7200))));

                graph.SetData(values.ToArray(), 0);
            });

            AddStep("set data with only user rating", () =>
            {
                List<(int x, int y)> values = new List<(int x, int y)>();
                for (int i = 400; i <= 2800; i += 100)
                    values.Add((i, (int)Math.Round(generateCount(i, 1600, 400, 7200))));

                graph.SetData([], 1500);
            });
        }

        private static double generateCount(double x, double mean, double stdDev, double amplitude)
        {
            return amplitude * Math.Exp(-Math.Pow(x - mean, 2) / (2 * Math.Pow(stdDev, 2))) + Random.Shared.Next(300);
        }
    }
}
