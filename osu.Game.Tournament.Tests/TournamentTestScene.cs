// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Users;

namespace osu.Game.Tournament.Tests
{
    public abstract class TournamentTestScene : OsuTestScene
    {
        [Cached]
        protected LadderInfo Ladder { get; private set; } = new LadderInfo();

        [Resolved]
        private RulesetStore rulesetStore { get; set; }

        [Cached]
        protected MatchIPCInfo IPCInfo { get; private set; } = new MatchIPCInfo();

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            Ladder.Ruleset.Value ??= rulesetStore.AvailableRulesets.First();

            TournamentMatch match = CreateSampleMatch();

            Ladder.Rounds.Add(match.Round.Value);
            Ladder.Matches.Add(match);
            Ladder.Teams.Add(match.Team1.Value);
            Ladder.Teams.Add(match.Team2.Value);

            Ladder.CurrentMatch.Value = match;

            Ruleset.BindTo(Ladder.Ruleset);
            Dependencies.CacheAs(new StableInfo(storage));
        }

        public static TournamentMatch CreateSampleMatch() => new TournamentMatch
        {
            Team1 =
            {
                Value = new TournamentTeam
                {
                    Acronym = { Value = "JPN" },
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
                                    BeatmapInfo = CreateSampleBeatmapInfo(),
                                    Score = 12345672,
                                    Seed = { Value = 24 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = CreateSampleBeatmapInfo(),
                                    Score = 1234567,
                                    Seed = { Value = 12 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = CreateSampleBeatmapInfo(),
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
                                    BeatmapInfo = CreateSampleBeatmapInfo(),
                                    Score = 234567,
                                    Seed = { Value = 3 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = CreateSampleBeatmapInfo(),
                                    Score = 234567,
                                    Seed = { Value = 6 },
                                },
                                new SeedingBeatmap
                                {
                                    BeatmapInfo = CreateSampleBeatmapInfo(),
                                    Score = 234567,
                                    Seed = { Value = 12 },
                                }
                            }
                        }
                    },
                    Players =
                    {
                        new User { Username = "Hello", Statistics = new UserStatistics { GlobalRank = 12 } },
                        new User { Username = "Hello", Statistics = new UserStatistics { GlobalRank = 16 } },
                        new User { Username = "Hello", Statistics = new UserStatistics { GlobalRank = 20 } },
                        new User { Username = "Hello", Statistics = new UserStatistics { GlobalRank = 24 } },
                        new User { Username = "Hello", Statistics = new UserStatistics { GlobalRank = 30 } },
                    }
                }
            },
            Team2 =
            {
                Value = new TournamentTeam
                {
                    Acronym = { Value = "USA" },
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

        public static BeatmapInfo CreateSampleBeatmapInfo() =>
            new BeatmapInfo { Metadata = new BeatmapMetadata { Title = "Test Title", Artist = "Test Artist", ID = RNG.Next(0, 1000000) } };

        protected override ITestSceneTestRunner CreateRunner() => new TournamentTestSceneTestRunner();

        public class TournamentTestSceneTestRunner : TournamentGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                BracketLoadTask.ContinueWith(_ => Schedule(() =>
                {
                    // this has to be run here rather than LoadComplete because
                    // TestScene.cs is checking the IsLoaded state (on another thread) and expects
                    // the runner to be loaded at that point.
                    Add(runner = new TestSceneTestRunner.TestRunner());
                }));
            }

            public void RunTestBlocking(TestScene test)
            {
                while (runner?.IsLoaded != true && Host.ExecutionState == ExecutionState.Running)
                    Thread.Sleep(10);

                runner?.RunTestBlocking(test);
            }
        }
    }
}
