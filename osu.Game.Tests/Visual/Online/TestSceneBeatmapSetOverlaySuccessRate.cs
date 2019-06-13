// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Screens.Select.Details;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapSetOverlaySuccessRate : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Details)
        };

        private GraphExposingSuccessRate successRate;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(275, 220),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    successRate = new GraphExposingSuccessRate
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(275, 220),
                        Padding = new MarginPadding(20)
                    }
                }
            };
        });

        [Test]
        public void TestMetrics()
        {
            var firstBeatmap = createBeatmap();
            var secondBeatmap = createBeatmap();

            AddStep("set first set", () => successRate.Beatmap = firstBeatmap);
            AddAssert("ratings set", () => successRate.Graph.Metrics == firstBeatmap.Metrics);

            AddStep("set second set", () => successRate.Beatmap = secondBeatmap);
            AddAssert("ratings set", () => successRate.Graph.Metrics == secondBeatmap.Metrics);

            BeatmapInfo createBeatmap() => new BeatmapInfo
            {
                Metrics = new BeatmapMetrics
                {
                    Fails = Enumerable.Range(1, 100).Select(_ => RNG.Next(10)).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(_ => RNG.Next(10)).ToArray(),
                }
            };
        }

        private class GraphExposingSuccessRate : SuccessRate
        {
            public new FailRetryGraph Graph => base.Graph;
        }
    }
}
