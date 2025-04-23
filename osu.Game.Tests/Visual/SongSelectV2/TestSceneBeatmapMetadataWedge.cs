// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
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

            Child = wedge = new BeatmapMetadataWedge
            {
                State = { Value = Visibility.Visible },
            };
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

                currentOnlineSet = onlineSet;
                Beatmap.Value = working;
            });
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
                        FailTimes = new APIFailTimes
                        {
                            Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                        },
                    },
                }
            };

            working.BeatmapSetInfo.DateSubmitted = DateTimeOffset.Now;
            working.BeatmapSetInfo.DateRanked = DateTimeOffset.Now;
            return (working, onlineSet);
        }
    }
}
