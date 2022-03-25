// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Screens.Select.Details;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapSetOverlayDetails : OsuTestScene
    {
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
            AddAssert("ratings set", () => details.Ratings.Ratings == firstSet.Ratings);

            AddStep("set second set", () => details.BeatmapSet = secondSet);
            AddAssert("ratings set", () => details.Ratings.Ratings == secondSet.Ratings);

            static APIBeatmapSet createSet() => new APIBeatmapSet
            {
                Beatmaps = new[]
                {
                    new APIBeatmap
                    {
                        FailTimes = new APIFailTimes
                        {
                            Fails = Enumerable.Range(1, 100).Select(_ => RNG.Next(10)).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(_ => RNG.Next(10)).ToArray(),
                        },
                    }
                },
                Ratings = Enumerable.Range(0, 11).Select(_ => RNG.Next(10)).ToArray(),
                Status = BeatmapOnlineStatus.Ranked
            };
        }

        private class RatingsExposingDetails : Details
        {
            public new UserRatings Ratings => base.Ratings;
        }
    }
}
