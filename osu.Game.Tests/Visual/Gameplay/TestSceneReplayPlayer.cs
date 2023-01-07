// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneReplayPlayer : RateAdjustedBeatmapTestScene
    {
        protected TestReplayPlayer Player;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Initialise player", () => Player = CreatePlayer(new OsuRuleset()));
            AddStep("Load player", () => LoadScreen(Player));
            AddUntilStep("player loaded", () => Player.IsLoaded);
        }

        [Test]
        public void TestPauseViaSpace()
        {
            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Pause playback with space", () => InputManager.Key(Key.Space));

            AddAssert("player not exited", () => Player.IsCurrentScreen());

            AddUntilStep("Time stopped progressing", () =>
            {
                double current = Player.GameplayClockContainer.CurrentTime;
                bool changed = lastTime != current;
                lastTime = current;

                return !changed;
            });

            AddWaitStep("wait some", 10);

            AddAssert("Time still stopped", () => lastTime == Player.GameplayClockContainer.CurrentTime);
        }

        [Test]
        public void TestPauseViaMiddleMouse()
        {
            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Pause playback with middle mouse", () => InputManager.Click(MouseButton.Middle));

            AddAssert("player not exited", () => Player.IsCurrentScreen());

            AddUntilStep("Time stopped progressing", () =>
            {
                double current = Player.GameplayClockContainer.CurrentTime;
                bool changed = lastTime != current;
                lastTime = current;

                return !changed;
            });

            AddWaitStep("wait some", 10);

            AddAssert("Time still stopped", () => lastTime == Player.GameplayClockContainer.CurrentTime);
        }

        [Test]
        public void TestSeekBackwards()
        {
            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Seek backwards", () =>
            {
                lastTime = Player.GameplayClockContainer.CurrentTime;
                InputManager.Key(Key.Left);
            });

            AddAssert("Jumped backwards", () => Player.GameplayClockContainer.CurrentTime - lastTime < 0);
        }

        [Test]
        public void TestSeekForwards()
        {
            double? lastTime = null;

            AddUntilStep("wait for first hit", () => Player.ScoreProcessor.TotalScore.Value > 0);

            AddStep("Seek forwards", () =>
            {
                lastTime = Player.GameplayClockContainer.CurrentTime;
                InputManager.Key(Key.Right);
            });

            AddAssert("Jumped forwards", () => Player.GameplayClockContainer.CurrentTime - lastTime > 500);
        }

        protected TestReplayPlayer CreatePlayer(Ruleset ruleset)
        {
            Beatmap.Value = CreateWorkingBeatmap(ruleset.RulesetInfo);
            SelectedMods.Value = new[] { ruleset.GetAutoplayMod() };

            return new TestReplayPlayer(false);
        }
    }
}
