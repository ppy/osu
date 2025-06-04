// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapMetadataWedge : SongSelectComponentsTestScene
    {
        private APIBeatmapSet? currentOnlineSet;

        private BeatmapMetadataWedge wedge = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = wedge = new BeatmapMetadataWedge
            {
                State = { Value = Visibility.Visible },
            };
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            AddStep("register request handling", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    switch (request)
                    {
                        case GetBeatmapSetRequest set:
                            if (set.ID == currentOnlineSet?.OnlineID)
                            {
                                set.TriggerSuccess(currentOnlineSet);
                                return true;
                            }

                            return false;

                        default:
                            return false;
                    }
                };
            });
        }

        [Test]
        public void TestShowHide()
        {
            AddStep("all metrics", () =>
            {
                var (working, onlineSet) = createTestBeatmap();
                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });

            AddStep("hide wedge", () => wedge.Hide());
            AddStep("show wedge", () => wedge.Show());
        }

        [Test]
        public void TestVariousMetrics()
        {
            AddStep("all metrics", () =>
            {
                var (working, onlineSet) = createTestBeatmap();
                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("null beatmap", () => Beatmap.SetDefault());
            AddStep("no source", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                working.Metadata.Source = string.Empty;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("no success rate", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Beatmaps.Single().PlayCount = 0;
                onlineSet.Beatmaps.Single().PassCount = 0;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("no user ratings", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Ratings = Array.Empty<int>();

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("no fail times", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Beatmaps.Single().FailTimes = null;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("no metrics", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Ratings = Array.Empty<int>();
                onlineSet.Beatmaps.Single().FailTimes = null;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("local beatmap", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                working.BeatmapInfo.OnlineID = 0;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
        }

        [Test]
        public void TestTruncation()
        {
            AddStep("long text", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                working.BeatmapInfo.Metadata.Author = new RealmUser { Username = "Verrrrryyyy llooonngggggg author" };
                working.BeatmapInfo.Metadata.Source = "Verrrrryyyy llooonngggggg source";
                working.BeatmapInfo.Metadata.Tags = string.Join(' ', Enumerable.Repeat(working.BeatmapInfo.Metadata.Tags, 3));
                onlineSet.Genre = new BeatmapSetOnlineGenre { Id = 12, Name = "Verrrrryyyy llooonngggggg genre" };
                onlineSet.Language = new BeatmapSetOnlineLanguage { Id = 12, Name = "Verrrrryyyy llooonngggggg language" };
                onlineSet.Beatmaps.Single().TopTags = Enumerable.Repeat(onlineSet.Beatmaps.Single().TopTags, 3).SelectMany(t => t!).ToArray();

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
        }

        [Test]
        public void TestOnlineAvailability()
        {
            AddStep("online beatmapset", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddUntilStep("rating wedge visible", () => wedge.RatingsVisible);
            AddUntilStep("fail time wedge visible", () => wedge.FailRetryVisible);
            AddStep("online beatmapset with local diff", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                working.BeatmapInfo.ResetOnlineInfo();

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddUntilStep("rating wedge hidden", () => !wedge.RatingsVisible);
            AddUntilStep("fail time wedge hidden", () => !wedge.FailRetryVisible);
            AddStep("local beatmap", () =>
            {
                var (working, _) = createTestBeatmap();

                currentOnlineSet = null;
                Beatmap.Value = working;
            });
            AddAssert("rating wedge still hidden", () => !wedge.RatingsVisible);
            AddAssert("fail time wedge still hidden", () => !wedge.FailRetryVisible);
        }

        [Test]
        public void TestUserTags()
        {
            AddStep("user tags", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddStep("no user tags", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Beatmaps.Single().TopTags = null;
                onlineSet.RelatedTags = null;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
        }

        [Test]
        public void TestLoading()
        {
            AddStep("override request handling", () =>
            {
                currentOnlineSet = null;

                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    switch (request)
                    {
                        case GetBeatmapSetRequest set:
                            Scheduler.AddDelayed(() => set.TriggerSuccess(currentOnlineSet!), 500);
                            return true;

                        default:
                            return false;
                    }
                };
            });

            AddStep("set beatmap", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);

            AddStep("set beatmap", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.RelatedTags![0].Name = "other/tag";
                onlineSet.RelatedTags[1].Name = "another/tag";
                onlineSet.RelatedTags[2].Name = "some/tag";

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);

            AddStep("no user tags", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Beatmaps.Single().TopTags = null;
                onlineSet.RelatedTags = null;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);

            AddStep("no user tags", () =>
            {
                var (working, onlineSet) = createTestBeatmap();

                onlineSet.Beatmaps.Single().TopTags = null;
                onlineSet.RelatedTags = null;

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);
        }

        private (WorkingBeatmap, APIBeatmapSet) createTestBeatmap()
        {
            var working = CreateWorkingBeatmap(Ruleset.Value);
            var onlineSet = new APIBeatmapSet
            {
                OnlineID = working.BeatmapSetInfo.OnlineID,
                Genre = new BeatmapSetOnlineGenre { Id = 15, Name = "Pop" },
                Language = new BeatmapSetOnlineLanguage { Id = 15, Name = "English" },
                Ratings = Enumerable.Range(0, 11).ToArray(),
                Beatmaps = new[]
                {
                    new APIBeatmap
                    {
                        OnlineID = working.BeatmapInfo.OnlineID,
                        PlayCount = 10000,
                        PassCount = 4567,
                        TopTags =
                        [
                            new APIBeatmapTag { TagId = 4, VoteCount = 1 },
                            new APIBeatmapTag { TagId = 2, VoteCount = 1 },
                            new APIBeatmapTag { TagId = 23, VoteCount = 5 },
                        ],
                        FailTimes = new APIFailTimes
                        {
                            Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                        },
                    },
                },
                RelatedTags =
                [
                    new APITag
                    {
                        Id = 2,
                        Name = "song representation/simple",
                        Description = "Accessible and straightforward map design."
                    },
                    new APITag
                    {
                        Id = 4,
                        Name = "style/clean",
                        Description = "Visually uncluttered and organised patterns, often involving few overlaps and equal visual spacing between objects."
                    },
                    new APITag
                    {
                        Id = 23,
                        Name = "aim/aim control",
                        Description = "Patterns with velocity or direction changes which strongly go against a player's natural movement pattern."
                    }
                ]
            };

            working.BeatmapSetInfo.DateSubmitted = DateTimeOffset.Now;
            working.BeatmapSetInfo.DateRanked = DateTimeOffset.Now;
            return (working, onlineSet);
        }
    }
}
