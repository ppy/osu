// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneBeatmapSetOverlay : OsuTestScene
    {
        private readonly TestBeatmapSetOverlay overlay;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Header),
            typeof(ScoreTable),
            typeof(ScoreTableRowBackground),
            typeof(DrawableTopScore),
            typeof(ScoresContainer),
            typeof(AuthorInfo),
            typeof(BasicStats),
            typeof(BeatmapPicker),
            typeof(Details),
            typeof(HeaderDownloadButton),
            typeof(FavouriteButton),
            typeof(Header),
            typeof(HeaderButton),
            typeof(Info),
            typeof(PreviewButton),
            typeof(SuccessRate),
            typeof(BeatmapAvailability),
            typeof(BeatmapRulesetSelector),
            typeof(BeatmapRulesetTabItem),
        };

        protected override bool UseOnlineAPI => true;

        public TestSceneBeatmapSetOverlay()
        {
            Add(overlay = new TestBeatmapSetOverlay());
        }

        [Resolved]
        private RulesetStore rulesets { get; set; }

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
                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 1235,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"an awesome beatmap",
                        Artist = @"naru narusegawa",
                        Source = @"hinata sou",
                        Tags = @"test tag tag more tag",
                        Author = new User
                        {
                            Username = @"BanchoBot",
                            Id = 3,
                        },
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Preview = @"https://b.ppy.sh/preview/12345.mp3",
                        PlayCount = 123,
                        FavouriteCount = 456,
                        Submitted = DateTime.Now,
                        Ranked = DateTime.Now,
                        BPM = 111,
                        HasVideo = true,
                        HasStoryboard = true,
                        Covers = new BeatmapSetOnlineCovers(),
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 9.99,
                            Version = @"TEST",
                            Length = 456000,
                            Ruleset = rulesets.GetRuleset(3),
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 1,
                                DrainRate = 2.3f,
                                OverallDifficulty = 4.5f,
                                ApproachRate = 6,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                CircleCount = 111,
                                SliderCount = 12,
                                PlayCount = 222,
                                PassCount = 21,
                            },
                            Metrics = new BeatmapMetrics
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
                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 1234,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"undownloadable beatmap",
                        Artist = @"no one",
                        Source = @"some source",
                        Tags = @"another test tag tag more test tags",
                        Author = new User
                        {
                            Username = @"BanchoBot",
                            Id = 3,
                        },
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Availability = new BeatmapSetOnlineAvailability
                        {
                            DownloadDisabled = true,
                            ExternalLink = "https://osu.ppy.sh",
                        },
                        Preview = @"https://b.ppy.sh/preview/1234.mp3",
                        PlayCount = 123,
                        FavouriteCount = 456,
                        Submitted = DateTime.Now,
                        Ranked = DateTime.Now,
                        BPM = 111,
                        HasVideo = true,
                        HasStoryboard = true,
                        Covers = new BeatmapSetOnlineCovers(),
                        Language = new BeatmapSetOnlineLanguage { Id = 3, Name = "English" },
                        Genre = new BeatmapSetOnlineGenre { Id = 4, Name = "Rock" },
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.67,
                            Version = @"ANOTHER TEST",
                            Length = 123000,
                            Ruleset = rulesets.GetRuleset(1),
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 9,
                                DrainRate = 8,
                                OverallDifficulty = 7,
                                ApproachRate = 6,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                CircleCount = 123,
                                SliderCount = 45,
                                PlayCount = 567,
                                PassCount = 89,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                });
            });

            downloadAssert(false);
        }

        [Test]
        public void TestMultipleRulesets()
        {
            AddStep("show multiple rulesets beatmap", () =>
            {
                var beatmaps = new List<BeatmapInfo>();

                foreach (var ruleset in rulesets.AvailableRulesets.Skip(1))
                {
                    beatmaps.Add(new BeatmapInfo
                    {
                        Version = ruleset.Name,
                        Ruleset = ruleset,
                        BaseDifficulty = new BeatmapDifficulty(),
                        OnlineInfo = new BeatmapOnlineInfo(),
                        Metrics = new BeatmapMetrics
                        {
                            Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                            Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                        },
                    });
                }

                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"multiple rulesets beatmap",
                        Artist = @"none",
                        Author = new User
                        {
                            Username = "BanchoBot",
                            Id = 3,
                        }
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Covers = new BeatmapSetOnlineCovers(),
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = beatmaps
                });
            });

            AddAssert("shown beatmaps of current ruleset", () => overlay.Header.Picker.Difficulties.All(b => b.Beatmap.Ruleset.Equals(overlay.Header.RulesetSelector.Current.Value)));
            AddAssert("left-most beatmap selected", () => overlay.Header.Picker.Difficulties.First().State == BeatmapPicker.DifficultySelectorState.Selected);
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

        private BeatmapSetInfo createManyDifficultiesBeatmapSet()
        {
            var beatmaps = new List<BeatmapInfo>();

            for (int i = 1; i < 41; i++)
            {
                beatmaps.Add(new BeatmapInfo
                {
                    OnlineBeatmapID = i * 10,
                    Version = $"Test #{i}",
                    Ruleset = Ruleset.Value,
                    StarDifficulty = 2 + i * 0.1,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    },
                    OnlineInfo = new BeatmapOnlineInfo(),
                    Metrics = new BeatmapMetrics
                    {
                        Fails = Enumerable.Range(1, 100).Select(j => j % 12 - 6).ToArray(),
                        Retries = Enumerable.Range(-2, 100).Select(j => j % 12 - 6).ToArray(),
                    },
                });
            }

            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 123,
                Metadata = new BeatmapMetadata
                {
                    Title = @"many difficulties beatmap",
                    Artist = @"none",
                    Author = new User
                    {
                        Username = @"BanchoBot",
                        Id = 3,
                    },
                },
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Preview = @"https://b.ppy.sh/preview/123.mp3",
                    HasVideo = true,
                    HasStoryboard = true,
                    Covers = new BeatmapSetOnlineCovers(),
                },
                Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                Beatmaps = beatmaps,
            };
        }

        private void downloadAssert(bool shown)
        {
            AddAssert($"is download button {(shown ? "shown" : "hidden")}", () => overlay.Header.DownloadButtonsVisible == shown);
        }

        private class TestBeatmapSetOverlay : BeatmapSetOverlay
        {
            public new Header Header => base.Header;
        }
    }
}
