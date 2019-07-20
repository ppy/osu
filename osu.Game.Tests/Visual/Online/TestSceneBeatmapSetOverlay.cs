﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        };

        private RulesetInfo taikoRuleset;
        private RulesetInfo maniaRuleset;

        public TestSceneBeatmapSetOverlay()
        {
            Add(overlay = new TestBeatmapSetOverlay());
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            taikoRuleset = rulesets.GetRuleset(1);
            maniaRuleset = rulesets.GetRuleset(3);
        }

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
                            Ruleset = maniaRuleset,
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
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.67,
                            Version = @"ANOTHER TEST",
                            Length = 123000,
                            Ruleset = taikoRuleset,
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
        public void TestHide()
        {
            AddStep(@"hide", overlay.Hide);
        }

        [Test]
        public void TestShowWithNoReload()
        {
            AddStep(@"show without reload", overlay.Show);
        }

        private void downloadAssert(bool shown)
        {
            AddAssert($"is download button {(shown ? "shown" : "hidden")}", () => overlay.DownloadButtonsVisible == shown);
        }

        private class TestBeatmapSetOverlay : BeatmapSetOverlay
        {
            public bool DownloadButtonsVisible => Header.DownloadButtonsVisible;
        }
    }
}
