// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Screens.Select.Details;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapSetOverlayDetails : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Details)
        };

        private RatingsExposingDetails details;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = details = new RatingsExposingDetails
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        });

        [Test]
        public void TestMetrics()
        {
            var firstSet = createSet();
            var secondSet = createSet();

            AddStep("set first set", () => details.BeatmapSet = firstSet);
            AddAssert("ratings set", () => details.Ratings.Metrics == firstSet.Metrics);

            AddStep("set second set", () => details.BeatmapSet = secondSet);
            AddAssert("ratings set", () => details.Ratings.Metrics == secondSet.Metrics);

            static BeatmapSetInfo createSet() => new BeatmapSetInfo
            {
                Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).Select(_ => RNG.Next(10)).ToArray() },
                Beatmaps = new List<BeatmapInfo>
                {
                    new BeatmapInfo
                    {
                        Metrics = new BeatmapMetrics
                        {
                            Fails = Enumerable.Range(1, 100).Select(_ => RNG.Next(10)).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(_ => RNG.Next(10)).ToArray(),
                        },
                    }
                },
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Status = BeatmapSetOnlineStatus.Ranked
                }
            };
        }

        private class RatingsExposingDetails : Details
        {
            public new UserRatings Ratings => base.Ratings;
        }
    }
}
