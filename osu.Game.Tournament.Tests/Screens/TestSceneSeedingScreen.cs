// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.TeamIntro;
using osu.Game.Users;

namespace osu.Game.Tournament.Tests.Screens
{
    public class TestSceneSeedingScreen : LadderTestScene
    {
        [Cached]
        private readonly LadderInfo ladder = new LadderInfo();

        [BackgroundDependencyLoader]
        private void load()
        {
            ladder.CurrentMatch.Value = CreateSampleSeededMatch();

            Add(new SeedingScreen
            {
                FillMode = FillMode.Fit,
                FillAspectRatio = 16 / 9f
            });
        }

        public static TournamentMatch CreateSampleSeededMatch() => new TournamentMatch
        {
            Team1 =
            {
                Value = new TournamentTeam
                {
                    FlagName = { Value = "JP" },
                    FullName = { Value = "Japan" },
                    LastYearPlacing = { Value = 10 },
                    Seed = { Value = "Low" },
                    SeedingResults =
                    {
                        new SeedingResult
                        {
                            Mod = { Value = "NM" },
                            Seed = { Value = 10 },
                            Beatmaps =
                            {
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist" } },
                                    Score = 12345672,
                                    Seed = { Value = 24 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist" } },
                                    Score = 1234567,
                                    Seed = { Value = 12 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist" } },
                                    Score = 1234567,
                                    Seed = { Value = 16 },
                                }
                            }
                        },
                        new SeedingResult
                        {
                            Mod = { Value = "DT" },
                            Seed = { Value = 5 },
                            Beatmaps =
                            {
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist" } },
                                    Score = 234567,
                                    Seed = { Value = 3 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist" } },
                                    Score = 234567,
                                    Seed = { Value = 6 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist" } },
                                    Score = 234567,
                                    Seed = { Value = 12 },
                                }
                            }
                        }
                    },
                    Players =
                    {
                        new User { Username = "Hello", Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 12 } } },
                        new User { Username = "Hello", Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 16 } } },
                        new User { Username = "Hello", Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 20 } } },
                        new User { Username = "Hello", Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 24 } } },
                        new User { Username = "Hello", Statistics = new UserStatistics { Ranks = new UserStatistics.UserRanks { Global = 30 } } },
                    }
                }
            },
            Team2 =
            {
                Value = new TournamentTeam
                {
                    FlagName = { Value = "US" },
                    FullName = { Value = "United States" },
                    Players =
                    {
                        new User { Username = "Hello" },
                        new User { Username = "Hello" },
                        new User { Username = "Hello" },
                        new User { Username = "Hello" },
                        new User { Username = "Hello" },
                    }
                }
            },
            Round =
            {
                Value = new TournamentRound { Name = { Value = "Quarterfinals" } }
            }
        };
    }
}
