// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneReplayRewinding : RateAdjustedBeatmapTestScene
    {
        private ReplayPlayer currentPlayer = null!;

        [Test]
        public void TestRewindingToMiddleOfHoldNote()
        {
            Score score = null!;

            var beatmap = new ManiaBeatmap(new StageDefinition(4))
            {
                HitObjects =
                {
                    new HoldNote
                    {
                        StartTime = 500,
                        EndTime = 1500,
                        Column = 2
                    }
                }
            };

            AddStep(@"create replay", () => score = new Score
            {
                Replay = new Replay
                {
                    Frames =
                    {
                        new ManiaReplayFrame(500, ManiaAction.Key3),
                        new ManiaReplayFrame(1500),
                    }
                },
                ScoreInfo = new ScoreInfo()
            });

            AddStep(@"set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(beatmap));
            AddStep(@"set ruleset", () => Ruleset.Value = beatmap.BeatmapInfo.Ruleset);
            AddStep(@"push player", () => LoadScreen(currentPlayer = new ReplayPlayer(score)));

            AddUntilStep(@"wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep(@"wait for hold to be judged", () => currentPlayer.ChildrenOfType<IFrameStableClock>().Single().CurrentTime, () => Is.GreaterThan(1600));
            AddStep(@"seek to middle of hold note", () => currentPlayer.Seek(1000));
            AddUntilStep(@"wait for gameplay to complete", () => currentPlayer.GameplayState.HasCompleted);
            AddAssert(@"no misses registered", () => currentPlayer.GameplayState.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss), () => Is.Zero);

            AddStep(@"exit player", () => currentPlayer.Exit());
        }

        [Test]
        public void TestCorrectComboAccountingForConcurrentObjects()
        {
            Score score = null!;

            var beatmap = new ManiaBeatmap(new StageDefinition(4))
            {
                HitObjects =
                {
                    new Note
                    {
                        StartTime = 500,
                        Column = 0,
                    },
                    new Note
                    {
                        StartTime = 500,
                        Column = 2,
                    },
                    new HoldNote
                    {
                        StartTime = 1000,
                        EndTime = 1500,
                        Column = 1,
                    }
                }
            };

            AddStep(@"create replay", () => score = new Score
            {
                Replay = new Replay
                {
                    Frames =
                    {
                        new ManiaReplayFrame(500, ManiaAction.Key1, ManiaAction.Key3),
                        new ManiaReplayFrame(520),
                        new ManiaReplayFrame(1000, ManiaAction.Key2),
                        new ManiaReplayFrame(1500),
                    }
                },
                ScoreInfo = new ScoreInfo()
            });

            AddStep(@"set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(beatmap));
            AddStep(@"set ruleset", () => Ruleset.Value = beatmap.BeatmapInfo.Ruleset);
            AddStep(@"push player", () => LoadScreen(currentPlayer = new ReplayPlayer(score)));

            AddUntilStep(@"wait until player is loaded", () => currentPlayer.IsCurrentScreen());
            AddUntilStep(@"wait for objects to be judged", () => currentPlayer.ChildrenOfType<IFrameStableClock>().Single().CurrentTime, () => Is.GreaterThan(1600));
            AddStep(@"stop gameplay", () => currentPlayer.ChildrenOfType<GameplayClockContainer>().Single().Stop());
            AddStep(@"seek to start", () => currentPlayer.Seek(0));
            AddAssert(@"combo is 0", () => currentPlayer.GameplayState.ScoreProcessor.Combo.Value, () => Is.Zero);

            AddStep(@"exit player", () => currentPlayer.Exit());
        }
    }
}
