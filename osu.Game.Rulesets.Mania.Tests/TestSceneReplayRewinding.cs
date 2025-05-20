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
    }
}
