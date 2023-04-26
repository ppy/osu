// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelect
{
    [System.ComponentModel.Description("PlaySongSelect beatmap details")]
    public partial class TestSceneBeatmapDetails : OsuTestScene
    {
        private BeatmapDetails details;

        private DummyAPIAccess api => (DummyAPIAccess)API;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = details = new BeatmapDetails
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(150),
            };
        });

        [Test]
        public void TestAllMetrics()
        {
            AddStep("all metrics", () => details.BeatmapInfo = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet
                {
                    Source = "osu!",
                    Tags = "this beatmap has all the metrics",
                    Ratings = Enumerable.Range(0, 11).ToArray(),
                },
                DifficultyName = "All Metrics",
                CircleSize = 7,
                DrainRate = 1,
                OverallDifficulty = 5.7f,
                ApproachRate = 3.5f,
                StarRating = 5.3f,
                FailTimes = new APIFailTimes
                {
                    Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                },
            });
        }

        [Test]
        public void TestAllMetricsExceptSource()
        {
            AddStep("all except source", () => details.BeatmapInfo = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet
                {
                    Tags = "this beatmap has all the metrics",
                    Ratings = Enumerable.Range(0, 11).ToArray(),
                },
                DifficultyName = "All Metrics",
                CircleSize = 7,
                DrainRate = 1,
                OverallDifficulty = 5.7f,
                ApproachRate = 3.5f,
                StarRating = 5.3f,
                FailTimes = new APIFailTimes
                {
                    Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                },
            });
        }

        [Test]
        public void TestOnlyRatings()
        {
            AddStep("ratings", () => details.BeatmapInfo = new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet
                {
                    Ratings = Enumerable.Range(0, 11).ToArray(),
                    Source = "osu!",
                    Tags = "this beatmap has ratings metrics but not retries or fails",
                },
                DifficultyName = "Only Ratings",
                CircleSize = 6,
                DrainRate = 9,
                OverallDifficulty = 6,
                ApproachRate = 6,
                StarRating = 4.8f,
            });
        }

        [Test]
        public void TestOnlyFailsAndRetries()
        {
            AddStep("fails retries", () => details.BeatmapInfo = new APIBeatmap
            {
                DifficultyName = "Only Retries and Fails",
                BeatmapSet = new APIBeatmapSet
                {
                    Source = "osu!",
                    Tags = "this beatmap has retries and fails but no ratings",
                },
                CircleSize = 3.7f,
                DrainRate = 6,
                OverallDifficulty = 6,
                ApproachRate = 7,
                StarRating = 2.91f,
                FailTimes = new APIFailTimes
                {
                    Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                },
            });
        }

        [Test]
        public void TestNoMetrics()
        {
            AddStep("no metrics", () => details.BeatmapInfo = new APIBeatmap
            {
                DifficultyName = "No Metrics",
                BeatmapSet = new APIBeatmapSet
                {
                    Source = "osu!",
                    Tags = "this beatmap has no metrics",
                },
                CircleSize = 5,
                DrainRate = 5,
                OverallDifficulty = 5.5f,
                ApproachRate = 6.5f,
                StarRating = 1.97f,
            });
        }

        [Test]
        public void TestNullBeatmap()
        {
            AddStep("null beatmap", () => details.BeatmapInfo = null);
        }

        [Test]
        public void TestOnlineMetrics()
        {
            AddStep("online ratings/retries/fails", () => details.BeatmapInfo = new APIBeatmap
            {
                OnlineID = 162,
            });
            AddStep("set online", () => api.SetState(APIState.Online));
            AddStep("set offline", () => api.SetState(APIState.Offline));
        }
    }
}
