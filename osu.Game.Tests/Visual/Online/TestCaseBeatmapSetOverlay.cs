// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Overlays.BeatmapSet.Scores;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestCaseBeatmapSetOverlay : OsuTestCase
    {
        private readonly BeatmapSetOverlay overlay;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Header),
            typeof(ClickableUsername),
            typeof(DrawableScore),
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
        };

        public TestCaseBeatmapSetOverlay()
        {
            Add(overlay = new BeatmapSetOverlay());
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var mania = rulesets.GetRuleset(3);
            var taiko = rulesets.GetRuleset(1);

            AddStep(@"show loading", () => overlay.ShowBeatmapSet(null));

            AddStep(@"show online", () => overlay.FetchAndShowBeatmapSet(55));

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
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 1.36,
                            Version = @"BASIC",
                            Ruleset = mania,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.22,
                            Version = @"NOVICE",
                            Ruleset = mania,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 3.49,
                            Version = @"ADVANCED",
                            Ruleset = mania,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 4.24,
                            Version = @"EXHAUST",
                            Ruleset = mania,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.26,
                            Version = @"GRAVITY",
                            Ruleset = mania,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                    },
                });
            });

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
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            StarDifficulty = 1.40,
                            Version = @"yzrin's Kantan",
                            Ruleset = taiko,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 2.23,
                            Version = @"Futsuu",
                            Ruleset = taiko,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 3.19,
                            Version = @"Muzukashii",
                            Ruleset = taiko,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 3.97,
                            Version = @"Charlotte's Oni",
                            Ruleset = taiko,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                        new BeatmapInfo
                        {
                            StarDifficulty = 5.08,
                            Version = @"Labyrinth Oni",
                            Ruleset = taiko,
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
                                Ratings = Enumerable.Range(0, 11),
                                Fails = Enumerable.Range(1, 100).Select(i => i % 12 - 6),
                                Retries = Enumerable.Range(-2, 100).Select(i => i % 12 - 6),
                            },
                        },
                    },
                });
            });

            AddStep(@"hide", overlay.Hide);
            AddStep(@"show without reload", overlay.Show);
        }
    }
}
