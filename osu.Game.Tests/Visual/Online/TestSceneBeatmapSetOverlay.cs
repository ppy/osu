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
        private readonly BeatmapSetOverlay overlay;

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
            typeof(DownloadButton),
            typeof(FavouriteButton),
            typeof(Header),
            typeof(HeaderButton),
            typeof(Info),
            typeof(PreviewButton),
            typeof(SuccessRate),
            typeof(BeatmapNotAvailable),
        };

        private RulesetInfo osuRuleset;
        private RulesetInfo taikoRuleset;
        private RulesetInfo maniaRuleset;

        public TestSceneBeatmapSetOverlay()
        {
            Add(overlay = new BeatmapSetOverlay());
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            osuRuleset = rulesets.GetRuleset(0);
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
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"Lachryma <Re:Queen’M>",
                        Artist = @"Kaneko Chiharu",
                        Source = @"SOUND VOLTEX III GRAVITY WARS",
                        Tags = @"sdvx grace the 5th kac original song contest konami bemani",
                        Author = new User
                        {
                            Username = @"Fresh Chicken",
                            Id = 3984370,
                        },
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Preview = @"https://b.ppy.sh/preview/415886.mp3",
                        PlayCount = 681380,
                        FavouriteCount = 356,
                        Submitted = new DateTime(2016, 2, 10),
                        Ranked = new DateTime(2016, 6, 19),
                        Status = BeatmapSetOnlineStatus.Ranked,
                        BPM = 236,
                        HasVideo = true,
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = @"https://assets.ppy.sh/beatmaps/415886/covers/cover.jpg?1465651778",
                        },
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 1.36,
                            Version = @"BASIC",
                            Ruleset = maniaRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 6.5f,
                                OverallDifficulty = 6.5f,
                                ApproachRate = 5,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 115000,
                                CircleCount = 265,
                                SliderCount = 71,
                                PlayCount = 47906,
                                PassCount = 19899,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.22,
                            Version = @"NOVICE",
                            Ruleset = maniaRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 7,
                                OverallDifficulty = 7,
                                ApproachRate = 5,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 118000,
                                CircleCount = 592,
                                SliderCount = 62,
                                PlayCount = 162021,
                                PassCount = 72116,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 3.49,
                            Version = @"ADVANCED",
                            Ruleset = maniaRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 7.5f,
                                OverallDifficulty = 7.5f,
                                ApproachRate = 5,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 118000,
                                CircleCount = 1042,
                                SliderCount = 79,
                                PlayCount = 225178,
                                PassCount = 73001,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 4.24,
                            Version = @"EXHAUST",
                            Ruleset = maniaRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 8,
                                OverallDifficulty = 8,
                                ApproachRate = 5,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 118000,
                                CircleCount = 1352,
                                SliderCount = 69,
                                PlayCount = 131545,
                                PassCount = 42703,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.26,
                            Version = @"GRAVITY",
                            Ruleset = maniaRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 8.5f,
                                OverallDifficulty = 8.5f,
                                ApproachRate = 5,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 118000,
                                CircleCount = 1730,
                                SliderCount = 115,
                                PlayCount = 117673,
                                PassCount = 24241,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                }, false);
            });

            downloadAssert(true);

            AddStep(@"show second", () =>
            {
                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"Soumatou Labyrinth",
                        Artist = @"Yunomi with Momobako&miko",
                        Tags = @"mmbk.com yuzu__rinrin charlotte",
                        Author = new User
                        {
                            Username = @"komasy",
                            Id = 1980256,
                        },
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Preview = @"https://b.ppy.sh/preview/625493.mp3",
                        PlayCount = 22996,
                        FavouriteCount = 58,
                        Submitted = new DateTime(2016, 6, 11),
                        Ranked = new DateTime(2016, 7, 12),
                        Status = BeatmapSetOnlineStatus.Pending,
                        BPM = 160,
                        HasVideo = false,
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = @"https://assets.ppy.sh/beatmaps/625493/covers/cover.jpg?1499167472",
                        },
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 1.40,
                            Version = @"yzrin's Kantan",
                            Ruleset = taikoRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 2,
                                DrainRate = 7,
                                OverallDifficulty = 3,
                                ApproachRate = 10,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 193000,
                                CircleCount = 262,
                                SliderCount = 0,
                                PlayCount = 3952,
                                PassCount = 1373,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.23,
                            Version = @"Futsuu",
                            Ruleset = taikoRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 2,
                                DrainRate = 6,
                                OverallDifficulty = 4,
                                ApproachRate = 10,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 193000,
                                CircleCount = 464,
                                SliderCount = 0,
                                PlayCount = 4833,
                                PassCount = 920,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 3.19,
                            Version = @"Muzukashii",
                            Ruleset = taikoRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 2,
                                DrainRate = 6,
                                OverallDifficulty = 5,
                                ApproachRate = 10,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 193000,
                                CircleCount = 712,
                                SliderCount = 0,
                                PlayCount = 4405,
                                PassCount = 854,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 3.97,
                            Version = @"Charlotte's Oni",
                            Ruleset = taikoRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 5,
                                DrainRate = 6,
                                OverallDifficulty = 5.5f,
                                ApproachRate = 10,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 193000,
                                CircleCount = 943,
                                SliderCount = 0,
                                PlayCount = 3950,
                                PassCount = 693,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.08,
                            Version = @"Labyrinth Oni",
                            Ruleset = taikoRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 5,
                                DrainRate = 5,
                                OverallDifficulty = 6,
                                ApproachRate = 10,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 193000,
                                CircleCount = 1068,
                                SliderCount = 0,
                                PlayCount = 5856,
                                PassCount = 1207,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                }, false);
            });

            downloadAssert(true);
        }

        [Test]
        public void TestUnavailable()
        {
            AddStep(@"show parts-removed", () =>
            {
                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"Sakura Kagetsu",
                        Artist = @"AKITO",
                        Source = @"DJMAX",
                        Tags = @"J-Trance Pasonia",
                        Author = new User
                        {
                            Username = @"Kharl",
                            Id = 452,
                        },
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Availability = new BeatmapSetOnlineAvailability
                        {
                            DownloadDisabled = false,
                            ExternalLink = @"https://gist.githubusercontent.com/peppy/079dc3f77e316f9cd40077d411319a72/raw",
                        },
                        Preview = @"https://b.ppy.sh/preview/119.mp3",
                        PlayCount = 626927,
                        FavouriteCount = 157,
                        Submitted = new DateTime(2007, 10, 24),
                        Ranked = new DateTime(2008, 4, 21),
                        Status = BeatmapSetOnlineStatus.Ranked,
                        BPM = 138,
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = @"https://assets.ppy.sh/beatmaps/119/covers/cover.jpg?1539847784",
                        },
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 1.51,
                            Version = "Easy",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 2,
                                OverallDifficulty = 1,
                                ApproachRate = 1,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 126000,
                                CircleCount = 371,
                                SliderCount = 35,
                                PlayCount = 84498,
                                PassCount = 37482,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.23,
                            Version = "Normal",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 5,
                                DrainRate = 4,
                                OverallDifficulty = 3,
                                ApproachRate = 3,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 126000,
                                CircleCount = 98,
                                SliderCount = 28,
                                PlayCount = 86427,
                                PassCount = 23273,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.83,
                            Version = "Hard",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 6,
                                DrainRate = 6,
                                OverallDifficulty = 6,
                                ApproachRate = 6,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 126000,
                                CircleCount = 139,
                                SliderCount = 37,
                                PlayCount = 206523,
                                PassCount = 44366,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 4.26,
                            Version = "Pasonia's Insane",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 6,
                                DrainRate = 6,
                                OverallDifficulty = 6,
                                ApproachRate = 6,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 126000,
                                CircleCount = 371,
                                SliderCount = 35,
                                PlayCount = 249479,
                                PassCount = 14042,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                }, false);
            });

            downloadAssert(true);

            AddStep(@"show undownloadable (no link)", () =>
            {
                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"China Express",
                        Artist = @"Ryu*",
                        Source = @"REFLEC BEAT",
                        Tags = @"konami bemani lincle link iidx iidx18 iidx19 resort anthem plus la cataline mmzz",
                        Author = new User
                        {
                            Username = @"yeahyeahyeahhh",
                            Id = 58042,
                        },
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Availability = new BeatmapSetOnlineAvailability
                        {
                            DownloadDisabled = true,
                        },
                        Preview = @"https://b.ppy.sh/preview/53853.mp3",
                        PlayCount = 436213,
                        FavouriteCount = 105,
                        Submitted = new DateTime(2012, 7, 1),
                        Ranked = new DateTime(2012, 7, 18),
                        Status = BeatmapSetOnlineStatus.Ranked,
                        BPM = 171,
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = @"https://assets.ppy.sh/beatmaps/53853/covers/cover.jpg?1456498562",
                        },
                    },
                    Metrics = new BeatmapSetMetrics { Ratings = Enumerable.Range(0, 11).ToArray() },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 1.85,
                            Version = "Easy",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 3,
                                DrainRate = 2,
                                OverallDifficulty = 2,
                                ApproachRate = 3,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 95000,
                                CircleCount = 49,
                                SliderCount = 60,
                                PlayCount = 20308,
                                PassCount = 10233,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.36,
                            Version = "Normal",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 3,
                                DrainRate = 2,
                                OverallDifficulty = 2,
                                ApproachRate = 5,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 96000,
                                CircleCount = 86,
                                SliderCount = 67,
                                PlayCount = 54015,
                                PassCount = 25603,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 4.42,
                            Version = "Hyper",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 7,
                                OverallDifficulty = 6,
                                ApproachRate = 8,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 96000,
                                CircleCount = 215,
                                SliderCount = 120,
                                PlayCount = 111400,
                                PassCount = 12583,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.05,
                            Version = "Another",
                            Ruleset = osuRuleset,
                            BaseDifficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 7,
                                OverallDifficulty = 9,
                                ApproachRate = 9,
                            },
                            OnlineInfo = new BeatmapOnlineInfo
                            {
                                Length = 96000,
                                CircleCount = 250,
                                SliderCount = 75,
                                PlayCount = 228253,
                                PassCount = 53037,
                            },
                            Metrics = new BeatmapMetrics
                            {
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6).ToArray(),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6).ToArray(),
                            },
                        },
                    },
                }, false);
            });

            downloadAssert(false);
        }

        private void downloadAssert(bool shown)
        {
            AddAssert($"is download button {(shown ? "shown" : "hidden")}", () => overlay.Header.DownloadButtonsContainer.Any() == shown);
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
    }
}
