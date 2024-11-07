// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests
{
    public abstract partial class TournamentTestScene : OsuManualInputManagerTestScene
    {
        [Cached(typeof(IDialogOverlay))]
        protected readonly DialogOverlay DialogOverlay = new DialogOverlay { Depth = float.MinValue };

        [Cached]
        protected LadderInfo Ladder { get; private set; } = new LadderInfo();

        [Cached]
        protected MatchIPCInfo IPCInfo { get; private set; } = new MatchIPCInfo();

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        private TournamentMatch match = null!;

        [BackgroundDependencyLoader]
        private void load(TournamentStorage storage)
        {
            Ladder.Ruleset.Value ??= rulesetStore.AvailableRulesets.First();

            match = CreateSampleMatch();

            Ladder.Rounds.Add(match.Round.Value!);
            Ladder.Matches.Add(match);
            Ladder.Teams.Add(match.Team1.Value!);
            Ladder.Teams.Add(match.Team2.Value!);

            Ruleset.BindTo(Ladder.Ruleset);
            Dependencies.CacheAs(new StableInfo(storage));

            Add(DialogOverlay);
        }

        [SetUpSteps]
        public virtual void SetUpSteps()
        {
            AddStep("set current match", () => Ladder.CurrentMatch.Value = match);
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
                    Seed = { Value = "#12" },
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
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 12345672,
                                    Seed = { Value = 24 },
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 1234567,
                                    Seed = { Value = 12 },
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
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
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 234567,
                                    Seed = { Value = 3 },
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 234567,
                                    Seed = { Value = 6 },
                                },
                                new SeedingBeatmap
                                {
                                    Beatmap = CreateSampleBeatmap(),
                                    Score = 234567,
                                    Seed = { Value = 12 },
                                }
                            }
                        }
                    },
                    Players =
                    {
                        new TournamentUser { Username = "Hello", Rank = 12 },
                        new TournamentUser { Username = "Hello", Rank = 16 },
                        new TournamentUser { Username = "Hello", Rank = 20 },
                        new TournamentUser { Username = "Hello", Rank = 24 },
                        new TournamentUser { Username = "Hello", Rank = 30 },
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
                    Seed = { Value = "#3" },
                    Players =
                    {
                        new TournamentUser { Username = "Hello" },
                        new TournamentUser { Username = "Hello" },
                        new TournamentUser { Username = "Hello" },
                        new TournamentUser { Username = "Hello" },
                        new TournamentUser { Username = "Hello" },
                    }
                }
            },
            Round =
            {
                Value = new TournamentRound { Name = { Value = "Quarterfinals" } },
            }
        };

        public static TournamentBeatmap CreateSampleBeatmap() =>
            new TournamentBeatmap
            {
                Metadata = new BeatmapMetadata
                {
                    Title = "Test Title",
                    Artist = "Test Artist",
                },
                OnlineID = RNG.Next(0, 1000000),
            };

        protected override ITestSceneTestRunner CreateRunner() => new TournamentTestSceneTestRunner();

        public partial class TournamentTestSceneTestRunner : TournamentGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner = null!;

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
