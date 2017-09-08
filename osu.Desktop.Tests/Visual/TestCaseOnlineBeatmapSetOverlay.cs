using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseOnlineBeatmapSetOverlay : TestCase
    {
        public override string Description => @"view online beatmap sets";

        private readonly OnlineBeatmapSetOverlay overlay;

        public TestCaseOnlineBeatmapSetOverlay()
        {
            Add(overlay = new OnlineBeatmapSetOverlay());
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var r = rulesets.GetRuleset(3);

            AddStep(@"show lachryma", () =>
            {
                overlay.ShowBeatmapSet(new BeatmapSetInfo
                {
                    OnlineBeatmapSetID = 415886,
                    Metadata = new BeatmapMetadata
                    {
                        Title = @"Lachryma <Re:Queen’M>",
                        Artist = @"Kaneko Chiharu",
                        Author = @"Fresh Chicken",
                        Source = @"SOUND VOLTEX III GRAVITY WARS",
                        Tags = @"sdvx grace the 5th kac original song contest konami bemani",
                    },
                    OnlineInfo = new BeatmapSetOnlineInfo
                    {
                        Preview = @"https://b.ppy.sh/preview/415886.mp3",
                        PlayCount = 681380,
                        FavouriteCount = 356,
                        Covers = new BeatmapSetOnlineCovers
                        {
                            Cover = @"https://assets.ppy.sh/beatmaps/415886/covers/cover.jpg?1465651778",
                        },
                    },
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 901048,
                            StarDifficulty = 1.36,
                            Version = @"BASIC",
                            Ruleset = r,
                            Difficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 6.5f,
                                OverallDifficulty = 6.5f,
                                ApproachRate = 5,
                            },
                        },
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 901051,
                            StarDifficulty = 2.22,
                            Version = @"NOVICE",
                            Ruleset = r,
                            Difficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 7,
                                OverallDifficulty = 7,
                                ApproachRate = 5,
                            },
                        },
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 901047,
                            StarDifficulty = 3.49,
                            Version = @"ADVANCED",
                            Ruleset = r,
                            Difficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 7.5f,
                                OverallDifficulty = 7.5f,
                                ApproachRate = 5,
                            },
                        },
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 901049,
                            StarDifficulty = 4.24,
                            Version = @"EXHAUST",
                            Ruleset = r,
                            Difficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 8,
                                OverallDifficulty = 8,
                                ApproachRate = 5,
                            },
                        },
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 901050,
                            StarDifficulty = 901050,
                            Version = @"GRAVITY",
                            Ruleset = r,
                            Difficulty = new BeatmapDifficulty
                            {
                                CircleSize = 4,
                                DrainRate = 8.5f,
                                OverallDifficulty = 8.5f,
                                ApproachRate = 5,
                            },
                        },
                    },
                });
            });
        }
    }
}
