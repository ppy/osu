// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Rulesets;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneBeatmapSetOverlay : OsuTestScene
    {
        private readonly TestBeatmapSetOverlay overlay;

        protected override bool UseOnlineAPI => true;

        private int nextBeatmapSetId = 1;

        public TestSceneBeatmapSetOverlay()
        {
            Add(overlay = new TestBeatmapSetOverlay());
        }

        [Resolved]
        private IRulesetStore rulesets { get; set; }

        [Test]
        public void TestLoading()
        {
            AddStep(@"show loading", () => overlay.ShowBeatmapSet(null));
        }

        [Test]
        public void TestOnline()
        {
            AddStep(@"show online", () => overlay.FetchAndShowBeatmapSet(55));
        }

        [Test]
        public void TestLocalBeatmaps()
        {
            AddStep(@"show first", () =>
            {
                overlay.ShowBeatmapSet(new APIBeatmapSet
                {
                    OnlineID = 1235,
                    Title = @"an awesome beatmap",
                    Artist = @"naru narusegawa",
                    Source = @"hinata sou",
                    Tags = @"test tag tag more tag",
                    Author = new APIUser
                    {
                        Username = @"BanchoBot",
                        Id = 3,
                    },
                    Preview = @"https://b.ppy.sh/preview/12345.mp3",
                    PlayCount = 123,
                    FavouriteCount = 456,
                    Submitted = DateTime.Now,
                    Ranked = DateTime.Now,
                    BPM = 111,
                    HasVideo = true,
                    Ratings = Enumerable.Range(0, 11).ToArray(),
                    HasStoryboard = true,
                    Covers = new BeatmapSetOnlineCovers(),
                    Beatmaps = new[]
                    {
                        new APIBeatmap
                        {
                            StarRating = 9.99,
                            DifficultyName = @"TEST",
                            Length = 456000,
                            RulesetID = 3,
                            CircleSize = 1,
                            DrainRate = 2.3f,
                            OverallDifficulty = 4.5f,
                            ApproachRate = 6,
                            CircleCount = 111,
                            SliderCount = 12,
                            PlayCount = 222,
                            PassCount = 21,
                            FailTimes = new APIFailTimes
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                });
            });

            downloadAssert(true);

            AddStep("show many difficulties", () => overlay.ShowBeatmapSet(createManyDifficultiesBeatmapSet()));
            downloadAssert(true);
        }

        [Test]
        public void TestAvailability()
        {
            AddStep(@"show undownloadable", () =>
            {
                var set = getBeatmapSet();

                set.Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = true,
                    ExternalLink = "https://osu.ppy.sh",
                };

                overlay.ShowBeatmapSet(set);
            });

            downloadAssert(false);
        }

        [Test]
        public void TestMultipleRulesets()
        {
            AddStep("show multiple rulesets beatmap", () =>
            {
                var beatmaps = new List<APIBeatmap>();

                foreach (var ruleset in rulesets.AvailableRulesets.Skip(1))
                {
                    beatmaps.Add(new APIBeatmap
                    {
                        DifficultyName = ruleset.Name,
                        RulesetID = ruleset.OnlineID,
                        FailTimes = new APIFailTimes
                        {
                            Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                        },
                    });
                }

                var set = getBeatmapSet();

                set.Beatmaps = beatmaps.ToArray();

                overlay.ShowBeatmapSet(set);
            });

            AddAssert("shown beatmaps of current ruleset", () => overlay.Header.HeaderContent.Picker.Difficulties.All(b => b.Beatmap.Ruleset.OnlineID == overlay.Header.RulesetSelector.Current.Value.OnlineID));
            AddAssert("left-most beatmap selected", () => overlay.Header.HeaderContent.Picker.Difficulties.First().State == BeatmapPicker.DifficultySelectorState.Selected);
        }

        [Test]
        public void TestExplicitBeatmap()
        {
            AddStep("show explicit map", () =>
            {
                var beatmapSet = getBeatmapSet();
                beatmapSet.HasExplicitContent = true;
                overlay.ShowBeatmapSet(beatmapSet);
            });
        }

        [Test]
        public void TestFeaturedBeatmap()
        {
            AddStep("show featured map", () =>
            {
                var beatmapSet = getBeatmapSet();
                beatmapSet.TrackId = 1;
                overlay.ShowBeatmapSet(beatmapSet);
            });
        }

        [Test]
        public void TestHide()
        {
            AddStep(@"hide", overlay.Hide);
        }

        [Test]
        public void TestShowWithNoReload()
        {
            AddStep(@"show without reload", overlay.Show);
        }

        private APIBeatmapSet createManyDifficultiesBeatmapSet()
        {
            var set = getBeatmapSet();

            var beatmaps = new List<APIBeatmap>();

            for (int i = 1; i < 41; i++)
            {
                beatmaps.Add(new APIBeatmap
                {
                    OnlineID = i * 10,
                    DifficultyName = $"Test #{i}",
                    RulesetID = Ruleset.Value.OnlineID,
                    StarRating = 2 + i * 0.1,
                    OverallDifficulty = 3.5f,
                    FailTimes = new APIFailTimes
                    {
                        Fails = Enumerable.Range(1, 100).Select(j => j % 12 - 6).ToArray(),
                        Retries = Enumerable.Range(-2, 100).Select(j => j % 12 - 6).ToArray(),
                    },
                });
            }

            set.Beatmaps = beatmaps.ToArray();

            return set;
        }

        private APIBeatmapSet getBeatmapSet()
        {
            var beatmapSet = CreateAPIBeatmapSet(Ruleset.Value);

            // Make sure the overlay is reloaded (see `BeatmapSetInfo.Equals`).
            beatmapSet.OnlineID = nextBeatmapSetId++;

            return beatmapSet;
        }

        private void downloadAssert(bool shown)
        {
            AddAssert($"is download button {(shown ? "shown" : "hidden")}", () => overlay.Header.HeaderContent.DownloadButtonsVisible == shown);
        }

        private class TestBeatmapSetOverlay : BeatmapSetOverlay
        {
            public new BeatmapSetHeader Header => base.Header;
        }
    }
}
