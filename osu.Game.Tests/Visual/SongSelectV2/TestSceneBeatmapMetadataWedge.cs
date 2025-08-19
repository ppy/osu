// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapMetadataWedge : SongSelectComponentsTestScene
    {
        private BeatmapMetadataWedge wedge = null!;

        [Cached(typeof(IBindable<RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult?>))]
        private Bindable<RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult?> onlineLookupResult = new Bindable<RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult?>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = wedge = new BeatmapMetadataWedge
            {
                State = { Value = Visibility.Visible },
            };
        }

        [Test]
        public void TestShowHide()
        {
            AddStep("all metrics", () => (Beatmap.Value, onlineLookupResult.Value) = createTestBeatmap());

            AddStep("hide wedge", () => wedge.Hide());
            AddStep("show wedge", () => wedge.Show());
        }

        [Test]
        public void TestVariousMetrics()
        {
            AddStep("all metrics", () => (Beatmap.Value, onlineLookupResult.Value) = createTestBeatmap());

            AddStep("null beatmap", () => Beatmap.SetDefault());
            AddStep("no source", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                working.Metadata.Source = string.Empty;

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
            AddStep("no success rate", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Beatmaps.Single().PlayCount = 0;
                lookupResult.Online.Beatmaps.Single().PassCount = 0;

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
            AddStep("no user ratings", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Ratings = Array.Empty<int>();

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
            AddStep("no fail times", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Beatmaps.Single().FailTimes = null;

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
            AddStep("no metrics", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Ratings = Array.Empty<int>();
                lookupResult.Online!.Beatmaps.Single().FailTimes = null;

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
            AddStep("local beatmap", () =>
            {
                var (working, _) = createTestBeatmap();

                working.BeatmapInfo.OnlineID = 0;

                onlineLookupResult.Value = new RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult(null, working.BeatmapSetInfo);
                Beatmap.Value = working;
            });
        }

        [Test]
        public void TestTruncation()
        {
            AddStep("long text", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                working.BeatmapInfo.Metadata.Author = new RealmUser { Username = "Verrrrryyyy llooonngggggg author" };
                working.BeatmapInfo.Metadata.Source = "Verrrrryyyy llooonngggggg source";
                working.BeatmapInfo.Metadata.Tags = string.Join(' ', Enumerable.Repeat(working.BeatmapInfo.Metadata.Tags, 3));
                lookupResult.Online!.Genre = new BeatmapSetOnlineGenre { Id = 12, Name = "Verrrrryyyy llooonngggggg genre" };
                lookupResult.Online.Language = new BeatmapSetOnlineLanguage { Id = 12, Name = "Verrrrryyyy llooonngggggg language" };
                lookupResult.Online.Beatmaps.Single().TopTags = Enumerable.Repeat(lookupResult.Online.Beatmaps.Single().TopTags, 3).SelectMany(t => t!).ToArray();

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
        }

        [Test]
        public void TestOnlineAvailability()
        {
            AddStep("online beatmapset", () => (Beatmap.Value, onlineLookupResult.Value) = createTestBeatmap());

            AddUntilStep("rating wedge visible", () => wedge.RatingsVisible);
            AddUntilStep("fail time wedge visible", () => wedge.FailRetryVisible);
            AddStep("online beatmapset with local diff", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                working.BeatmapInfo.ResetOnlineInfo();

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
            AddUntilStep("rating wedge hidden", () => !wedge.RatingsVisible);
            AddUntilStep("fail time wedge hidden", () => !wedge.FailRetryVisible);
            AddStep("local beatmap", () =>
            {
                var (working, _) = createTestBeatmap();

                onlineLookupResult.Value = new RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult(null, working.BeatmapSetInfo);
                Beatmap.Value = working;
            });
            AddAssert("rating wedge still hidden", () => !wedge.RatingsVisible);
            AddAssert("fail time wedge still hidden", () => !wedge.FailRetryVisible);
        }

        [Test]
        public void TestUserTags()
        {
            AddStep("user tags", () => (Beatmap.Value, onlineLookupResult.Value) = createTestBeatmap());

            AddStep("no user tags", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Beatmaps.Single().TopTags = null;
                lookupResult.Online.RelatedTags = null;
                lookupResult.Local.Beatmaps.Single().Metadata.UserTags.Clear();

                onlineLookupResult.Value = lookupResult;
                Beatmap.Value = working;
            });
        }

        [Test]
        public void TestLoading()
        {
            AddStep("set beatmap", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                onlineLookupResult.Value = null;
                Scheduler.AddDelayed(() => onlineLookupResult.Value = lookupResult, 500);
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);

            AddStep("set beatmap", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.RelatedTags![0].Name = "other/tag";
                lookupResult.Online.RelatedTags[1].Name = "another/tag";
                lookupResult.Online.RelatedTags[2].Name = "some/tag";

                onlineLookupResult.Value = null;
                Scheduler.AddDelayed(() => onlineLookupResult.Value = lookupResult, 500);
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);

            AddStep("no user tags", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Beatmaps.Single().TopTags = null;
                lookupResult.Online.RelatedTags = null;
                lookupResult.Local.Beatmaps.Single().Metadata.UserTags.Clear();

                onlineLookupResult.Value = null;
                Scheduler.AddDelayed(() => onlineLookupResult.Value = lookupResult, 500);
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);

            AddStep("no user tags", () =>
            {
                var (working, lookupResult) = createTestBeatmap();

                lookupResult.Online!.Beatmaps.Single().TopTags = null;
                lookupResult.Online.RelatedTags = null;
                lookupResult.Local.Beatmaps.Single().Metadata.UserTags.Clear();

                onlineLookupResult.Value = null;
                Scheduler.AddDelayed(() => onlineLookupResult.Value = lookupResult, 500);
                Beatmap.Value = working;
            });
            AddWaitStep("wait", 5);
        }

        private (WorkingBeatmap, RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult) createTestBeatmap()
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
            working.Metadata.UserTags.AddRange(onlineSet.RelatedTags.Select(t => t.Name));
            return (working, new RealmPopulatingOnlineLookupSource.BeatmapSetLookupResult(onlineSet, working.BeatmapSetInfo));
        }
    }
}
