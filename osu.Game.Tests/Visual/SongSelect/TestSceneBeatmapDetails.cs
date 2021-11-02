// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.SongSelect
{
    [System.ComponentModel.Description("PlaySongSelect beatmap details")]
    public class TestSceneBeatmapDetails : OsuTestScene
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
            AddStep("all metrics", () => details.BeatmapInfo = new BeatmapInfo
            {
                BeatmapSet = new BeatmapSetInfo
                {
                    OnlineInfo = new APIBeatmapSet
                    {
                        Ratings = Enumerable.Range(0, 11).ToArray(),
                    }
                },
                Version = "All Metrics",
                Metadata = new BeatmapMetadata
                {
                    Source = "osu!",
                    Tags = "this beatmap has all the metrics",
                },
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 7,
                    DrainRate = 1,
                    OverallDifficulty = 5.7f,
                    ApproachRate = 3.5f,
                },
                StarDifficulty = 5.3f,
                Ruleset = new OsuRuleset().RulesetInfo,
                OnlineInfo = new APIBeatmap
                {
                    FailTimes = new APIFailTimes
                    {
                        Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                        Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                    },
                }
            });
        }

        [Test]
        public void TestAllMetricsExceptSource()
        {
            AddStep("all except source", () => details.BeatmapInfo = new BeatmapInfo
            {
                BeatmapSet = new BeatmapSetInfo
                {
                    OnlineInfo = new APIBeatmapSet
                    {
                        Ratings = Enumerable.Range(0, 11).ToArray(),
                    }
                },
                Version = "All Metrics",
                Metadata = new BeatmapMetadata
                {
                    Tags = "this beatmap has all the metrics",
                },
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 7,
                    DrainRate = 1,
                    OverallDifficulty = 5.7f,
                    ApproachRate = 3.5f,
                },
                StarDifficulty = 5.3f,
                Ruleset = new OsuRuleset().RulesetInfo,
                OnlineInfo = new APIBeatmap
                {
                    FailTimes = new APIFailTimes
                    {
                        Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                        Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                    },
                }
            });
        }

        [Test]
        public void TestOnlyRatings()
        {
            AddStep("ratings", () => details.BeatmapInfo = new BeatmapInfo
            {
                BeatmapSet = new BeatmapSetInfo
                {
                    OnlineInfo = new APIBeatmapSet
                    {
                        Ratings = Enumerable.Range(0, 11).ToArray(),
                    }
                },
                Version = "Only Ratings",
                Metadata = new BeatmapMetadata
                {
                    Source = "osu!",
                    Tags = "this beatmap has ratings metrics but not retries or fails",
                },
                Ruleset = new OsuRuleset().RulesetInfo,
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 6,
                    DrainRate = 9,
                    OverallDifficulty = 6,
                    ApproachRate = 6,
                },
                StarDifficulty = 4.8f,
            });
        }

        [Test]
        public void TestOnlyFailsAndRetries()
        {
            AddStep("fails retries", () => details.BeatmapInfo = new BeatmapInfo
            {
                Version = "Only Retries and Fails",
                Metadata = new BeatmapMetadata
                {
                    Source = "osu!",
                    Tags = "this beatmap has retries and fails but no ratings",
                },
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 3.7f,
                    DrainRate = 6,
                    OverallDifficulty = 6,
                    ApproachRate = 7,
                },
                Ruleset = new OsuRuleset().RulesetInfo,
                StarDifficulty = 2.91f,
                OnlineInfo = new APIBeatmap
                {
                    FailTimes = new APIFailTimes
                    {
                        Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                        Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                    },
                }
            });
        }

        [Test]
        public void TestNoMetrics()
        {
            AddStep("no metrics", () => details.BeatmapInfo = new BeatmapInfo
            {
                Version = "No Metrics",
                Metadata = new BeatmapMetadata
                {
                    Source = "osu!",
                    Tags = "this beatmap has no metrics",
                },
                Ruleset = new OsuRuleset().RulesetInfo,
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 5,
                    DrainRate = 5,
                    OverallDifficulty = 5.5f,
                    ApproachRate = 6.5f,
                },
                StarDifficulty = 1.97f,
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
            AddStep("online ratings/retries/fails", () => details.BeatmapInfo = new BeatmapInfo
            {
                OnlineBeatmapID = 162,
                Ruleset = new OsuRuleset().RulesetInfo
            });
            AddStep("set online", () => api.SetState(APIState.Online));
            AddStep("set offline", () => api.SetState(APIState.Offline));
        }
    }
}
